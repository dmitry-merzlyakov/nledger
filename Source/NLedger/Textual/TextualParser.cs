// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLedger.Utility;
using NLedger.Times;
using NLedger.TimeLogging;
using NLedger.Querying;
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Commodities;
using NLedger.Expressions;
using NLedger.Items;
using NLedger.Xacts;
using NLedger.Journals;
using NLedger.Values;
using NLedger.Scopus;
using NLedger.Utils;

namespace NLedger.Textual
{
    /// <summary>
    /// Ported from instance_t (textual.cc)
    /// </summary>
    public class TextualParser : Scope
    {
        public const string CommentToken = "comment";
        public const string EndCommentToken = "end comment";
        public const string EndToken = "end";
        public const string PythonToken = "python";
        public const string TestToken = "test";
        public const string EndTestToken = "end test";
        public const string AccountToken = "account";
        public const string AliasToken = "alias";
        public const string ApplyToken = "apply";
        public const string AssertToken = "assert";
        public const string BucketToken = "bucket";
        public const string CheckToken = "check";
        public const string CommodityToken = "commodity";
        public const string DefToken = "def";
        public const string DefineToken = "define";
        public const string ExprToken = "expr";
        public const string EvalToken = "eval";
        public const string IncludeToken = "include";
        public const string ImportToken = "import";
        public const string PayeeToken = "payee";
        public const string TagToken = "tag";
        public const string ValueToken = "value";
        public const string YearToken = "year";

        public const string UUIDTag = "UUID";

        public const string UnspecifiedPayeeKey = "<Unspecified payee>";

        public TextualParser(ParseContextStack contextStack, ParseContext context, TextualParser parent = null, bool noAssertions = false)
        {
            ContextStack = contextStack;
            Context = context;
            Parent = parent;
            ApplyStack = new ApplyStack(parent != null ? parent.ApplyStack : null);
            NoAssertions = noAssertions;
            In = context.Reader;
            TimeLog = new TimeLog(Context);
        }

        public ApplyStack ApplyStack { get; private set; }
        public ParseContextStack ContextStack { get; private set; }
        public ParseContext Context { get; private set; }
        private bool ErrorFlag { get; set; }
        public bool NoAssertions { get; private set; }
        private TextualParser Parent { get; set; }
        private ITextualReader In { get; set; }
        public TimeLog TimeLog { get; private set; }

        public override string Description
        {
            get { return "textual parser"; }
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            return Context.Scope.Lookup(kind, name);
        }

        public Account TopAccount
        {
            get { return ApplyStack.GetApplication<Account>(); }
        }

        /// <summary>
        /// Ported from instance_t::parse
        /// </summary>
        public void Parse()
        {
            Logger.Current.Info(() => String.Format("Parsing file \"{0}\"", Context.PathName));
            var trace = Logger.Current.TraceContext(TimerName.InstanceParse, 1)?.Message(String.Format("Done parsing file {0}", Context.PathName)).Start(); // TRACE_START

            if (In.IsEof())
                return;

            Context.LineNum = 0;
            Context.CurrPos = In.Position;

            ErrorFlag = false;

            while (!In.IsEof())
            {
                try
                {
                    ReadNextDirective(In);
                }
                catch(Exception ex)
                {
                    ErrorFlag = true;
                    string currentContext = ErrorContext.Current.GetContext();

                    foreach(TextualParser instance in this.RecursiveEnum(p => p.Parent))
                        ErrorContext.Current.AddErrorContext(String.Format("In file included from {0}", instance.Context.Location));

                    ErrorContext.Current.AddErrorContext(String.Format("While parsing file {0}", Context.Location));

                    if (CancellationManager.IsCancellationRequested)
                        throw;

                    ErrorContext.Current.WriteError(ErrorContext.Current.GetContext());
                    ErrorContext.Current.WriteError(currentContext);
                    ErrorContext.Current.WriteError(ex);

                    Context.Errors++;

                    if (!String.IsNullOrEmpty(currentContext))
                        Context.Last = currentContext + "\n" + ex.Message;
                    else
                        Context.Last = ex.Message;
                }
            }

            if (ApplyStack.IsFrontType<DateTime?>())
                TimesCommon.Current.Epoch = ApplyStack.Front<DateTime?>().Value;

            ApplyStack.PopFront();

            trace?.Stop(); // TRACE_STOP
        }

        /// <summary>
        /// Ported from std::streamsize instance_t::read_line(char *& line)
        /// </summary>
        private string ReadLine(ITextualReader textualReader)
        {
            if (textualReader.IsEof())
                throw new InvalidOperationException("assert"); // no one should call us in that case

            Context.LineBegPos = Context.CurrPos;

            CancellationManager.CheckForSignal();

            Context.LineBuf = textualReader.ReadLine();
            
            if (Context.LineBuf != null)
            {
                Context.LineNum++;
                Context.CurrPos = textualReader.Position;
            }

            return Context.LineBuf ?? String.Empty;
        }

        /// <summary>
        /// ported from read_next_directive()
        /// </summary>
        private void ReadNextDirective(ITextualReader textualReader)
        {
            string line = ReadLine(textualReader);

            if (line.Length == 0)
                return;

            Char firstChar = line[0];
            if (!Char.IsWhiteSpace(firstChar))
                ErrorFlag = false;

            // Main switch
            if (Char.IsWhiteSpace(firstChar))
            {
                if (!ErrorFlag)
                    throw new ParseError("Unexpected whitespace at beginning of line");
            }                
            else if (firstChar.IsCommentChar())
                return;
            else if (firstChar == '-')
                ReadOptionDirective(line);
            else if (Char.IsDigit(firstChar))
                ReadXactDirective(line, textualReader);
            else if (firstChar == '=')
                ReadAutomatedXactDirective(line, textualReader);
            else if (firstChar == '~')
                ReadPeriodXactDirective(line, textualReader);
            else
            {
                if (line.StartsWith("!") || line.StartsWith("@"))
                    line = line.Remove(0, 1);

                if (!IsGeneralDirective(line, textualReader))
                {
                    if (firstChar == 'i')
                        ReadClockInDirective(line, false);
                    else if (firstChar == 'I')
                        ReadClockInDirective(line, true);
                    else if (firstChar == 'o')
                        ReadClockOutDirective(line, false);
                    else if (firstChar == 'O')
                        ReadClockOutDirective(line, true);
                    else if (firstChar == 'A')
                        ReadDefaultAccountDirective(line.Substring(1));
                    else if (firstChar == 'C')
                        ReadPriceConversionDirective(line);
                    else if (firstChar == 'D')
                        ReadDefaultCommodityDirective(line);
                    else if (firstChar == 'N')
                        ReadNoMarketDirective(line);
                    else if (firstChar == 'P')
                        ReadPriceXactDirective(line);
                    else if (firstChar == 'Y')
                    {
                        string arg = line.Substring(1);
                        if (String.IsNullOrEmpty(arg))
                            throw new ParseError(String.Format("Directive '{0}' requires an argument", line));
                        ReadApplyYearDirective(arg);
                    }
                    // Any other chars are ignored (h, b etc)
                }
            }
        }

        /// <summary>
        /// ported from general_directive()
        /// </summary>
        private bool IsGeneralDirective(string line, ITextualReader textualReader)
        {
            string arg = StringExtensions.NextElement(ref line);

            if (line.StartsWith("!") || line.StartsWith("@"))
                line = line.Remove(0, 1);

            // Ensure there's an argument for all directives that need one.
            if (String.IsNullOrEmpty(arg) &&
                line != CommentToken && line != EndToken && line != PythonToken && line != TestToken && !line.StartsWith("Y"))
                throw new ParseError(String.Format("Directive '{0}' requires an argument", line));

            if (line == AccountToken)
                ReadAccountDirective(arg, textualReader);
            else if (line == AliasToken)
                ReadAliasDirective(arg);
            else if (line == ApplyToken)
                ReadApplyDirective(arg);
            else if (line == AssertToken)
                ReadAssertDirective(arg);
            else if (line == BucketToken)
                ReadDefaultAccountDirective(arg);
            else if (line == CheckToken)
                ReadCheckDirective(arg);
            else if (line == CommentToken)
                ReadCommentDirective(arg, textualReader);
            else if (line == CommodityToken)
                ReadCommodityDirective(arg, textualReader);
            else if (line == DefToken || line == DefineToken)
                ReadEvalDirective(arg);
            else if (line == EndToken)
                ReadEndApplyDirective(arg);
            else if (line == ExprToken || line == EvalToken)
                ReadEvalDirective(arg);
            else if (line == IncludeToken)
                ReadIncludeDirective(arg);
            else if (line == ImportToken)
                ReadImportDirective(arg);
            else if (line == PayeeToken)
                ReadPayeeDirective(arg, textualReader);
            else if (line == PythonToken)
                ReadPythonDirective(arg);
            else if (line == TagToken)
                ReadTagDirective(arg, textualReader);
            else if (line == TestToken)
                ReadCommentDirective(arg, textualReader);
            else if (line == ValueToken)
                ReadValueDirective(arg);
            else if (line == YearToken)
                ReadApplyYearDirective(arg);
            else
            {
                ExprOp op = Lookup(SymbolKindEnum.DIRECTIVE, line);
                if (op != null)
                {
                    CallScope args = new CallScope(this);
                    args.PushBack(Value.StringValue(line));
                    op.AsFunction(args);
                    return true;
                }

                return false;
            }

            return true; // DM - any of options above was processed
        }

        // instance_t::xact_directive
        private void ReadXactDirective(string line, ITextualReader textualReader)
        {
            var trace = Logger.Current.TraceContext(TimerName.Xacts, 1)?.Message("Time spent handling transactions:").Start(); // TRACE_START

            Xact xact = ParseXact(line, textualReader, TopAccount);
            if (xact != null)
            {
                if (Context.Journal.AddXact(xact))
                    Context.Count++;
                else
                    xact.Detach();  // DM - it simulates calling xact_t destructor here if it has not been added to the journal
                // It's perfectly valid for the journal to reject the xact, which it
                // will do if the xact has no substantive effect (for example, a
                // checking xact, all of whose postings have null amounts).
            }
            else
                throw new ParseError(ParseError.ParseError_FailedToParseTransaction);

            trace?.Stop(); // TRACE_STOP
        }

        /// <summary>
        /// Ported from parse_xact()
        /// </summary>
        public Xact ParseXact(string line, ITextualReader textualReader, Account masterAccount)
        {
            var trace = Logger.Current.TraceContext(TimerName.XactText, 1)?.Message("Time spent parsing transaction text:").Start(); // TRACE_START

            Xact xact = new Xact();

            xact.Pos = new ItemPosition();
            xact.Pos.PathName = Context.PathName;
            xact.Pos.BegPos = Context.LineBegPos;
            xact.Pos.BegLine = Context.LineNum;
            xact.Pos.Sequence = Context.Sequence++;

            bool revealContext = true;

            try
            {

                // Parse the date
                string next = StringExtensions.NextElement(ref line);
                string auxDate = StringExtensions.SplitBySeparator(ref line, '=');
                if (!String.IsNullOrEmpty(auxDate))
                    xact.DateAux = TimesCommon.Current.ParseDate(auxDate);
                xact.Date = TimesCommon.Current.ParseDate(line);

                // Parse the optional cleared flag: *
                ParseFlagIfExists(ref next, xact);

                // Parse the optional code: (TEXT)
                if (next.StartsWith("("))
                {
                    int pos = next.IndexOf(')');
                    if (pos > 0)
                    {
                        xact.Code = next.Substring(1, pos - 1);
                        next = next.Remove(0, pos + 1).TrimStart();
                    }
                }

                // Parse the description text
                string xactNote = StringExtensions.SplitBySeparator(ref next, ';', true);
                if (!String.IsNullOrEmpty(next))
                    xact.Payee = Context.Journal.RegisterPayee(next, xact);
                else
                    xact.Payee = UnspecifiedPayeeKey;

                // Parse the xact note
                if (!String.IsNullOrEmpty(xactNote))
                    xact.AppendNote(xactNote, Context.Scope, false);

                trace?.Stop(); // TRACE_STOP

                // Parse all of the posts associated with this xact

                trace = Logger.Current.TraceContext(TimerName.XactDetails, 1)?.Message("Time spent parsing transaction details:").Start(); // TRACE_START

                Post lastPost = null;
                while (textualReader.PeekWhitespaceLine())
                {
                    line = ReadLine(textualReader).TrimStart();
                    if (String.IsNullOrWhiteSpace(line))
                        break;

                    Item item = (Item)lastPost ?? xact;
                    if (line[0] == ';')
                    {
                        // This is a trailing note, and possibly a metadata info tag
                        item.AppendNote(line.Substring(1), Context.Scope, true);
                        item.Flags = item.Flags | SupportsFlagsEnum.ITEM_NOTE_ON_NEXT_LINE;
                        item.Pos.EndPos = (int)Context.CurrPos;
                        item.Pos.EndLine++;
                    }
                    else if (line.StartsWith(AssertToken) || line.StartsWith(CheckToken) || line.StartsWith(ExprToken))
                    {
                        char c = line[0];
                        line = line.Substring(c == 'a' ? 6 : (c == 'c' ? 5 : 4)).TrimStart();
                        Expr expr = new Expr(line);
                        BindScope boundScope = new BindScope(Context.Scope, item);
                        if (c == 'e')
                        {
                            expr.Calc(boundScope);
                        }
                        else if (!expr.Calc(boundScope).AsBoolean)
                        {
                            if (c == 'a')
                            {
                                throw new ParseError(String.Format(ParseError.ParseError_TransactionAssertionFailed, line));
                            }
                            else
                            {
                                Context.Warning(String.Format("Transaction check failed: {0}", line));
                            }
                        }
                    }
                    else
                    {
                        revealContext = false;

                        if (lastPost != null && xact.HasTag(UUIDTag))
                        {
                            string uuid = xact.GetTag(UUIDTag).ToString();
                            string payee;
                            if (Context.Journal.PayeeUUIDMapping.TryGetValue(uuid, out payee))
                                xact.Payee = payee;
                        }

                        Post post = ParsePost(line, xact, masterAccount);
                        if (post != null)
                        {
                            revealContext = true;
                            xact.AddPost(post);
                            lastPost = post;
                        }
                        revealContext = true;
                    }
                }

                xact.Pos.EndPos = (int)Context.CurrPos;
                xact.Pos.EndLine = Context.LineNum;

                foreach (string tag in ApplyStack.GetApplications<string>())
                    xact.ParseTags(tag, Context.Scope, false);

                trace?.Stop();  // TRACE_STOP

                return xact;
            }
            catch
            {
                if (revealContext)
                {
                    ErrorContext.Current.AddErrorContext("While parsing transaction:");
                    ErrorContext.Current.AddErrorContext(ErrorContext.SourceContext(xact.Pos.PathName, xact.Pos.BegPos, Context.CurrPos, "> "));
                }
                throw;
            }
        }

        /// <summary>
        /// Ported from instance_t::parse_post
        /// </summary>
        public Post ParsePost(string line, Xact xact, Account masterAccount, bool deferExpr = false)
        {
            if (String.IsNullOrWhiteSpace(line))
                throw new ArgumentNullException("line");

            var trace = Logger.Current.TraceContext(TimerName.PostDetails, 1)?.Message("Time spent parsing postings:").Start(); // TRACE_START

            Post post = new Post();
            post.Xact = xact; // this could be NULL
            post.Pos.PathName = Context.PathName;
            post.Pos.BegPos = Context.LineBegPos;
            post.Pos.BegLine = Context.LineNum;
            post.Pos.Sequence = Context.Sequence++;

            string buf = line;
            int beg = 0;

            line = line.TrimStart();

            try
            {
                // Parse the state flag

                if (String.IsNullOrEmpty(line))
                    throw new InvalidOperationException("assert");

                ParseFlagIfExists(ref line, post);

                if (xact != null && xact.State != ItemStateEnum.Uncleared && post.State == ItemStateEnum.Uncleared)
                    post.State = xact.State;

                // Parse the account name

                if (String.IsNullOrEmpty(line) || line.StartsWith(";"))
                    throw new ParseError("Posting has no account");

                string next = StringExtensions.NextElement(ref line, true);
                line = line.TrimEnd();

                char firstChar = line.First();
                char lastChar = line.Last();

                if ((firstChar == '[' && lastChar == ']') || (firstChar == '(' && lastChar == ')'))
                {
                    post.Flags = post.Flags | SupportsFlagsEnum.POST_VIRTUAL;
                    Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Parsed a virtual account name", Context.LineNum));

                    if (firstChar == '[')
                    {
                        post.Flags = post.Flags | SupportsFlagsEnum.POST_MUST_BALANCE;
                        Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Posting must balance", Context.LineNum));
                    }
                    line = line.Substring(1, line.Length - 2);
                }
                else if (firstChar == '<' && lastChar == '>')
                {
                    post.Flags = post.Flags | SupportsFlagsEnum.POST_DEFERRED;
                    Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Parsed a deferred account name", Context.LineNum));
                    line = line.Substring(1, line.Length - 2);
                }

                Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Parsed account name {1}", Context.LineNum, line));
                post.Account = Context.Journal.RegisterAccount(line, post, masterAccount);

                // Parse the optional amount

                if (!String.IsNullOrEmpty(next))
                {
                    firstChar = next.First();
                    if (firstChar != ';' && firstChar != '=')
                    {
                        beg = buf.IndexOf(next);

                        if (post.Amount == null)
                            post.Amount = new Amount();

                        if (firstChar != '(')  // indicates a value expression
                            post.Amount.Parse(ref next, AmountParseFlagsEnum.PARSE_NO_REDUCE);
                        else
                            ParseAmountExpression(ref next, Context.Scope, post, amt => post.Amount = amt,
                                AmountParseFlagsEnum.PARSE_NO_REDUCE | AmountParseFlagsEnum.PARSE_SINGLE | AmountParseFlagsEnum.PARSE_NO_ASSIGN,
                                deferExpr, amtExpr => post.AmountExpr = amtExpr);

                        Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: post amount = {1}", Context.LineNum, post.Amount));

                        if (!post.Amount.IsEmpty && post.Amount.HasCommodity)
                        {
                            Context.Journal.RegisterCommodity(post.Amount.Commodity, post);

                            if (!post.Amount.HasAnnotation)
                            {
                                IEnumerable<Tuple<Commodity, Amount>> rates = ApplyStack.GetApplications<Tuple<Commodity, Amount>>();
                                foreach (var rate in rates)
                                {
                                    if (rate.Item1 == post.Amount.Commodity)
                                    {
                                        Annotation details = new Annotation(rate.Item2);
                                        details.IsPriceFixated = true;
                                        post.Amount.Annotate(details);
                                        Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: applied rate = {1}", Context.LineNum, post.Amount));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                next = next.TrimStart();
                if (!String.IsNullOrEmpty(next))
                {
                    // Parse the optional cost (@ PER-UNIT-COST, @@ TOTAL-COST)

                    if (next.StartsWith("@") || next.StartsWith("(@"))
                    {
                        Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Found a price indicator", Context.LineNum));

                        if (next.StartsWith("("))
                        {
                            post.Flags = post.Flags | SupportsFlagsEnum.POST_COST_VIRTUAL;
                            next = next.Remove(0, 1);
                        }

                        bool per_unit = true;
                        next = next.Remove(0, 1);
                        if (next.StartsWith("@"))
                        {
                            per_unit = false;
                            post.Flags = post.Flags | SupportsFlagsEnum.POST_COST_IN_FULL;
                            Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: And it's for a total price", Context.LineNum));
                            next = next.Remove(0, 1);
                        }

                        if (post.Flags.HasFlag(SupportsFlagsEnum.POST_COST_VIRTUAL) && next.StartsWith(")"))
                            next = next.Remove(0, 1);

                        next = next.TrimStart();
                        if (!String.IsNullOrEmpty(next))
                        {
                            post.Cost = new Amount();

                            bool fixed_cost = false;
                            if (next.StartsWith("="))
                            {
                                next = next.Remove(0, 1);
                                fixed_cost = true;
                                if (String.IsNullOrEmpty(next))
                                    throw new ParseError(ParseError.ParseError_PostingIsMissingCostAmount);
                            }

                            beg = buf.IndexOf(next);

                            if (!next.StartsWith("("))     // indicates a value expression
                                post.Cost.Parse(ref next, AmountParseFlagsEnum.PARSE_NO_MIGRATE);
                            else
                                ParseAmountExpression(ref next, Context.Scope, post, amt => post.Cost = amt,
                                    AmountParseFlagsEnum.PARSE_NO_MIGRATE | AmountParseFlagsEnum.PARSE_SINGLE | AmountParseFlagsEnum.PARSE_NO_ASSIGN,
                                    deferExpr, amtExpr => post.AmountExpr = amtExpr);

                            if (post.Cost.Sign < 0)
                                throw new ParseError(ParseError.ParseError_PostingCostMayNotBeNegative);

                            post.Cost.InPlaceUnround();

                            if (per_unit)
                            {
                                // For the sole case where the cost might be uncommoditized,
                                // guarantee that the commodity of the cost after multiplication
                                // is the same as it was before.
                                Commodity costCommodity = post.Cost.Commodity;
                                post.Cost = post.Cost.Multiply(post.Amount);
                                post.Cost.SetCommodity(costCommodity);
                            }
                            else if (post.Amount.Sign < 0)
                            {
                                post.Cost.InPlaceNegate();
                            }

                            if (fixed_cost)
                                post.Flags = post.Flags | SupportsFlagsEnum.POST_COST_FIXATED;

                            post.GivenCost = new Amount(post.Cost);

                            Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Total cost is {1}", Context.LineNum, post.Cost));
                            Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Annotated amount is {1}", Context.LineNum, post.Amount));

                            next = next.TrimStart();
                        }
                        else
                        {
                            throw new ParseError(ParseError.ParseError_ExpectedCostAmount);
                        }
                    }
                }

                // Parse the optional balance assignment

                if (xact != null && !String.IsNullOrEmpty(next) && next.StartsWith("="))
                {
                    Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Found a balance assignment indicator", Context.LineNum));

                    next = next.Remove(0, 1).TrimStart();
                    if (!String.IsNullOrEmpty(next))
                    {
                        post.AssignedAmount = new Amount();

                        beg = buf.IndexOf(next);

                        if (!next.StartsWith("(")) // indicates a value expression
                            post.AssignedAmount.Parse(ref next, AmountParseFlagsEnum.PARSE_NO_MIGRATE);
                        else
                        {
                            ParseAmountExpression(ref next, Context.Scope, post, amount => post.AssignedAmount = amount,
                                AmountParseFlagsEnum.PARSE_SINGLE | AmountParseFlagsEnum.PARSE_NO_MIGRATE);
                        }

                        if (post.AssignedAmount.IsEmpty)
                        {
                            if (post.Amount.IsEmpty)
                                throw new ParseError(ParseError.ParseError_BalanceAssignmentMustEvaluateToConstant);
                            else
                                throw new ParseError(ParseError.ParseError_BalanceAssertionMustEvaluateToConstant);
                        }

                        Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: POST assign: parsed balance amount = {1}", Context.LineNum, post.AssignedAmount));

                        Amount amt = post.AssignedAmount;
                        Value accountTotal = post.Account.Amount().StripAnnotations(new AnnotationKeepDetails());

                        Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: account balance = {1}", Context.LineNum, accountTotal));
                        Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: post amount = {1} (is_zero = {2})", Context.LineNum, amt, amt.IsZero));

                        Balance diff = new Balance(amt);

                        if (accountTotal.Type == ValueTypeEnum.Amount)
                        {
                            diff -= accountTotal.AsAmount;
                            Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Subtracting amount {1} from diff, yielding {2}", Context.LineNum, accountTotal.AsAmount, diff));
                        }
                        else if (accountTotal.Type == ValueTypeEnum.Balance)
                        {
                            diff -= accountTotal.AsBalance;
                            Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Subtracting balance {1} from diff, yielding {2}", Context.LineNum, accountTotal.AsBalance, diff));
                        }

                        Logger.Current.Debug("post.assign", () => String.Format("line {0}: diff = {1}", Context.LineNum, diff));
                        Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: POST assign: diff = {1}", Context.LineNum, diff));

                        // Subtract amounts from previous posts to this account in the xact.
                        foreach (var p in xact.Posts)
                        {
                            if (p.Account == post.Account)
                            {
                                diff -= p.Amount;
                                Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Subtracting {1}, diff = {2}", Context.LineNum, p.Amount, diff));
                            }
                        }

                        // If amt has a commodity, restrict balancing to that. Otherwise, it's the blanket '0' and
                        // check that all of them are zero.
                        if (amt.HasCommodity)
                        {
                            Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Finding commodity {1} ({2}) in balance {3}", Context.LineNum, amt.Commodity, amt, diff));
                            Amount wantedCommodity = diff.CommodityAmount(amt.Commodity);
                            if (Amount.IsNullOrEmpty(wantedCommodity))
                                diff = new Balance(amt - amt); // this is '0' with the correct commodity.
                            else
                                diff = new Balance(wantedCommodity);
                            Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Diff is now {1}", Context.LineNum, diff));
                        }

                        if (Amount.IsNullOrEmpty(post.Amount))
                        {
                            // balance assignment
                            if (!diff.IsZero)
                            {
                                // This will fail if there are more than 1 commodity in diff, which is wanted,
                                // as amount cannot store more than 1 commodity.
                                post.Amount = diff.ToAmount();
                                Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Overwrite null posting", Context.LineNum));
                            }
                        }
                        else
                        {
                            // balance assertion
                            diff -= post.Amount;

                            if (!NoAssertions && !diff.IsZero)
                            {
                                Balance tot = -diff + amt;
                                Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Balance assertion: off by {1} (expected to see {2})", Context.LineNum, diff, tot));
                                throw new ParseError(String.Format(ParseError.ParseError_BalanceAssertionOffBySmthExpectedToSeeSmth, diff.AsString(), tot.AsString()));
                            }
                        }

                        next = next.TrimStart();
                    }
                    else
                    {
                        throw new ParseError(ParseError.ParseError_ExpectedBalanceAssignmentOrAssertionAmount);
                    }
                }

                // Parse the optional note

                if (next.StartsWith(";"))
                {
                    post.AppendNote(next.Remove(0, 1), Context.Scope, true);
                    Logger.Current.Debug(DebugTextualParse, () => String.Format("line {0}: Parsed a posting note", Context.LineNum));
                }
                else
                {
                    // There should be nothing more to read
                    if (!String.IsNullOrWhiteSpace(next))
                        throw new ParseError(String.Format(ParseError.ParseError_UnexpectedCharSmthNoteInlineMathRequiresParentheses, next));
                }


                foreach (string tag in ApplyStack.GetApplications<string>())
                    post.ParseTags(tag, Context.Scope, true);

                trace?.Stop(); // TRACE_STOP

                return post;
            }
            catch
            {
                ErrorContext.Current.AddErrorContext("While parsing posting:");
                ErrorContext.Current.AddErrorContext(ErrorContext.LineContext(buf, beg, buf.Length));
                throw;
            }
        }

        /// <remarks>ported from parse_posts</remarks>
        public bool ParsePosts(ITextualReader textualReader, Account account, XactBase xact, bool deferExpr = false)
        {
            var trace = Logger.Current.TraceContext(TimerName.XactPosts, 1)?.Message("Time spent parsing postings:").Start();  // TRACE_START

            bool added = false;

            while (textualReader.PeekWhitespaceLine())
            {
                string line = ReadLine(textualReader).TrimStart();
                if (!line.StartsWith(";"))
                {
                    Post post = ParsePost(line, null, account, deferExpr);
                    xact.AddPost(post);
                    added = true;
                }
            }

            trace?.Stop();  // TRACE_STOP

            return added;
        }

        /// <remarks>ported from parse_amount_expr</remarks>
        private void ParseAmountExpression(ref string line, Scope scope, Post post, Action<Amount> setAmount, AmountParseFlagsEnum flags = AmountParseFlagsEnum.PARSE_DEFAULT, bool deferExpr = false, Action<Expr> setAmountExpr = null)
        {
            InputTextStream inStream = new InputTextStream(line);
            Expr expr = new Expr(inStream, flags | AmountParseFlagsEnum.PARSE_PARTIAL);
            line = inStream.RemainSource;

            Logger.Current.Debug(DebugTextualParse, () => "Parsed an amount expression");

            if (expr != null)
            {
                if (setAmountExpr != null)
                    setAmountExpr(expr);
                if (!deferExpr)
                    setAmount(post.ResolveExpr(scope, expr));
            }
        }

        private static void ParseFlagIfExists(ref string line, Item item)
        {
            if (line.StartsWith("*") || line.StartsWith("!"))
            {
                item.State = line[0] == '*' ? ItemStateEnum.Cleared : ItemStateEnum.Pending;
                line = line.Remove(0, 1).TrimStart();
                if (item.State == ItemStateEnum.Cleared)
                    Logger.Current.Debug(DebugTextualParse, () => String.Format("line: {0}: Parsed the CLEARED flag", item.Pos.BegLine));
                else
                    Logger.Current.Debug(DebugTextualParse, () => String.Format("line: {0}: Parsed the PENDING flag", item.Pos.BegLine));
            }
        }

        /// <remarks>ported from "value_directive"</remarks>
        private void ReadValueDirective(string line)
        {
            Context.Journal.ValueExpr = new Expr(line);
        }

        /// <remarks>ported from tag_directive</remarks>
        private void ReadTagDirective(string line, ITextualReader textualReader)
        {
            string p = line.TrimStart();
            Context.Journal.RegisterMetadata(p, Value.Empty, null);

            while (textualReader.PeekWhitespaceLine())
            {
                line = ReadLine(textualReader);
                string q = line.TrimStart();
                if (String.IsNullOrEmpty(q))
                    break;

                string b = StringExtensions.NextElement(ref q);
                string keyword = q;
                if (keyword == "assert" || keyword == "check")
                {
                    Context.Journal.TagCheckExprsMap.Add(p, new CheckExprPair(new Expr(b), 
                        keyword == "assert" ? CheckExprKindEnum.EXPR_ASSERTION : CheckExprKindEnum.EXPR_CHECK));
                }
            }
        }

        /// <remarks>ported from python_directive</remarks>
        private void ReadPythonDirective(string arg)
        {
            throw new ParseError(ParseError.ParseError_PythonDirectiveSeenButPythonSupportIsMissing);
        }

        /// <remarks>ported from import_directive</remarks>
        private void ReadImportDirective(string arg)
        {
            throw new ParseError(ParseError.ParseError_ImportDirectiveSeenButPythonSupportIsMissing);
        }

        /// <remarks>ported from payee_directive</remarks>
        private void ReadPayeeDirective(string line, ITextualReader textualReader)
        {
            string payee = Context.Journal.RegisterPayee(line, null);

            while (textualReader.PeekWhitespaceLine())
            {
                line = ReadLine(textualReader);
                string p = line.TrimStart();
                if (String.IsNullOrEmpty(p))
                    break;

                string b = StringExtensions.NextElement(ref p);
                string keyword = p;
                if (String.IsNullOrEmpty(b))
                    throw new ParseError(String.Format(ParseError.ParseError_PayeeDirectiveSmthRequiresAnArgument, keyword));

                if (keyword == "alias")
                    ReadPayeeAliasDirective(payee, b);
                if (keyword == "uuid")
                    ReadPayeeUuidDirective(payee, b);
            }
        }

        // payee_alias_directive
        private void ReadPayeeAliasDirective(string payee, string alias)
        {
            alias = alias.Trim();
            Context.Journal.PayeeAliasMappings.Add(new Tuple<Mask, string>(new Mask(alias), payee));
        }

        // payee_uuid_directive
        private void ReadPayeeUuidDirective(string payee, string uuid)
        {
            uuid = uuid.Trim();
            Context.Journal.PayeeUUIDMapping.Add(uuid, payee);
        }

        /// <summary>
        /// ported from include_directive
        /// </summary>
        private void ReadIncludeDirective(string line)
        {
            string fileName;
            Logger.Current.Debug(DebugTextualInclude, () => "include: " + line);

            if (line[0] != '/' && line[0] != '\\' && line[0] != '~')
            {
                Logger.Current.Debug(DebugTextualInclude, () => "received a relative path");
                Logger.Current.Debug(DebugTextualInclude, () => "parent file path: " + Context.PathName);
                var parent_Path = FileSystem.GetDirectoryName(Context.PathName);

                if (String.IsNullOrWhiteSpace(parent_Path))
                    fileName = FileSystem.Combine(line, ".");
                else
                {
                    fileName = FileSystem.Combine(line, parent_Path);
                    Logger.Current.Debug(DebugTextualInclude, () => "normalized path: " + fileName);
                }
            }
            else
            {
                fileName = line;
            }

            fileName = FileSystem.ResolvePath(fileName);
            Logger.Current.Debug(DebugTextualInclude, () => "resolved path: " + fileName);

            string parentPath = FileSystem.GetParentPath(fileName);
            var glob = Mask.AssignGlob(String.Format("^{0}$", FileSystem.GetFileName(fileName)));

            bool filesFound = false;
            if (FileSystem.DirectoryExists(parentPath))
            {
                foreach (string iter in FileSystem.GetDirectoryFiles(parentPath))
                {
                    // DM - is_regular_file is not needed
                    string fileBase = FileSystem.GetFileName(iter);
                    if (glob.Match(fileBase))
                    {
                        Journal journal = Context.Journal;
                        Account master = TopAccount;
                        Scope scope = Context.Scope;
                        //int errors = Context.Errors;
                        //int count = Context.Count;
                        //int sequence = Context.Sequence;

                        Logger.Current.Debug(DebugTextualInclude, () => "Including: " + iter);
                        Logger.Current.Debug(DebugTextualInclude, () => "Master account: " + master.FullName);

                        ContextStack.Push(iter);

                        ContextStack.GetCurrent().Journal = journal;
                        ContextStack.GetCurrent().Master = master;
                        ContextStack.GetCurrent().Scope = scope;

                        try
                        {
                            var instance = new TextualParser(ContextStack, ContextStack.GetCurrent(), this, NoAssertions);
                            instance.ApplyStack.PushFront("account", master);
                            instance.Parse();
                        }
                        finally
                        {
                            Context.Errors += ContextStack.GetCurrent().Errors;
                            Context.Count += ContextStack.GetCurrent().Count;
                            Context.Sequence += ContextStack.GetCurrent().Sequence;

                            ContextStack.Pop();
                        }

                        filesFound = true;
                    }
                }
            }

            if (!filesFound)
                throw new RuntimeError(String.Format(RuntimeError.ErrorMessageFileToIncludeWasNotFound, fileName));
        }

        /// <summary>
        /// Ported from end_apply_directive
        /// </summary>
        private void ReadEndApplyDirective(string kind)
        {
            string b = !String.IsNullOrEmpty(kind) ? StringExtensions.NextElement(ref kind) : null;
            string name = b ?? String.Empty;

            if (ApplyStack.Size <= 1)
            {
                if (String.IsNullOrEmpty(name))
                    throw new RuntimeError(RuntimeError.ErrorMessageEndOrEndApplyFoundButNoEnclosingApplyDirective);
                else
                    throw new RuntimeError(String.Format(RuntimeError.ErrorMessageEndApplySmthFoundButNoEnclosingApplyDirective, name));
            }

            if (!String.IsNullOrEmpty(name) && name != ApplyStack.FrontLabel())
                throw new RuntimeError(String.Format(RuntimeError.ErrorMessageEndApplySmthDirectiveDoesNotMatchApplySmthDirective, name, ApplyStack.FrontLabel()));

            if (ApplyStack.IsFrontType<DateTime>())
                TimesCommon.Current.Epoch = ApplyStack.Front<DateTime>();

            ApplyStack.PopFront();
        }

        /// <summary>
        /// ported from eval_directive
        /// </summary>
        private void ReadEvalDirective(string line)
        {
            Expr expr = new Expr(line);
            expr.Calc(Context.Scope);
        }

        /// <summary>
        /// ported from commodity_directive
        /// </summary>
        private void ReadCommodityDirective(string line, ITextualReader textualReader)
        {
            string p = line.TrimStart();
            string symbol = Commodity.ParseSymbol(ref p);

            Commodity commodity = CommodityPool.Current.FindOrCreate(symbol);
            if (commodity != null)
            {
                Context.Journal.RegisterCommodity(commodity);

                while (textualReader.PeekWhitespaceLine())
                {
                    line = ReadLine(textualReader);
                    string q = line.TrimStart();
                    if (String.IsNullOrEmpty(q))
                        break;

                    string b = StringExtensions.NextElement(ref q);
                    string keyword = q;
                    // Ensure there's an argument for the directives that need one.
                    if (String.IsNullOrEmpty(b) && keyword != "nomarket" && keyword != "default")
                        throw new ParseError(String.Format(ParseError.ParseError_CommodityDirectiveSmthRequiresAnArgument, keyword));

                    if (keyword == "alias")
                        ReadCommodityAliasDirective(commodity, b);
                    else if (keyword == "value")
                        ReadCommodityValueDirective(commodity, b);
                    else if (keyword == "format")
                        ReadCommodityFormatDirective(commodity, b);
                    else if (keyword == "nomarket")
                        ReadCommodityNoMarketDirective(commodity);
                    else if (keyword == "default")
                        ReadCommodityDefaultDirective(commodity);
                    else if (keyword == "note")
                        commodity.SetNote(b);
                }
            }
        }

        /// <summary>
        /// ported from commodity_alias_directive
        /// </summary>
        private void ReadCommodityAliasDirective(Commodity comm, string alias)
        {
            CommodityPool.Current.Alias(alias.Trim(), comm);
        }

        /// <summary>
        /// ported from commodity_value_directive
        /// </summary>
        private void ReadCommodityValueDirective(Commodity comm, string expr_str)
        {
            comm.ValueExpr = new Expr(expr_str);
        }

        /// <summary>
        /// ported from commodity_format_directive
        /// </summary>
        private void ReadCommodityFormatDirective(Commodity comm, string format)
        {
            format = format.Trim();
            Amount amt = new Amount();
            amt.Parse(ref format);
            amt.Commodity.Flags |= CommodityFlagsEnum.COMMODITY_STYLE_NO_MIGRATE;
            Validator.Verify(() => amt.Valid());
        }

        /// <summary>
        /// ported from commodity_nomarket_directive
        /// </summary>
        private void ReadCommodityNoMarketDirective(Commodity comm)
        {
            comm.Flags |= CommodityFlagsEnum.COMMODITY_NOMARKET;
        }

        /// <summary>
        /// ported from commodity_default_directive
        /// </summary>
        private void ReadCommodityDefaultDirective(Commodity comm)
        {
            CommodityPool.Current.DefaultCommodity = comm;
        }

        /// <summary>
        /// ported from comment_directive
        /// </summary>
        /// <param name="arg"></param>
        private void ReadCommentDirective(string line, ITextualReader textualReader)
        {
            while(!textualReader.IsEof())
            {
                line = ReadLine(textualReader).Trim();
                if (!String.IsNullOrEmpty(line))
                {
                    if (line.StartsWith(EndCommentToken) || line.StartsWith(EndTestToken))
                        break;
                }
            }
        }

        /// <summary>
        /// ported from check_directive
        /// </summary>
        private void ReadCheckDirective(string line)
        {
            Expr expr = new Expr(line);
            if (!expr.Calc(Context.Scope).AsBoolean)
                Context.Warning(String.Format("Check failed: {0}", line));
        }

        /// <summary>
        /// ported from assert_directive
        /// </summary>
        private void ReadAssertDirective(string line)
        {
            Expr expr = new Expr(line);
            if (!expr.Calc(Context.Scope).AsBoolean)
                throw new ParseError(String.Format(ParseError.ParseError_AssertionFailed, line));
        }

        /// <summary>
        /// ported from apply_directive
        /// </summary>
        private void ReadApplyDirective(string line)
        {
            string b = StringExtensions.NextElement(ref line);
            string keyword = line;
            if (keyword == "account")
                ReadApplyAccountDirective(b);
            else if (keyword == "tag")
                ReadApplyTagDirective(b);
            else if (keyword == "fixed" || keyword == "rate")
                ReadApplyRateDirective(b);
            else if (keyword == "year")
                ReadApplyYearDirective(b);
        }

        /// <summary>
        /// ported from apply_account_directive
        /// </summary>
        private void ReadApplyAccountDirective(string line)
        {
            Account acct = TopAccount.FindAccount(line);
            if (acct != null)
                ApplyStack.PushFront("account", acct);
            else
                // dm - NO_ASSERTS is ignored
                throw new InvalidOperationException("assert(\"Failed to create account\" == NULL);");
        }

        /// <summary>
        /// ported from apply_tag_directive
        /// </summary>
        private void ReadApplyTagDirective(string line)
        {
            string tag = line.Trim();

            if (!tag.Contains(':'))
                tag = string.Format(":{0}:", tag);

            ApplyStack.PushFront("tag", tag);
        }

        /// <summary>
        /// ported from apply_rate_directive
        /// </summary>
        private void ReadApplyRateDirective(string line)
        {
            Tuple<Commodity, PricePoint> pricePoint = CommodityPool.Current.ParsePriceDirective(line.Trim(), true, true);
            if (pricePoint != default(Tuple<Commodity, PricePoint>))
                ApplyStack.PushFront("fixed", new Tuple<Commodity, Amount>(pricePoint.Item1, pricePoint.Item2.Price) /* fixed_rate_t */);
            else
                throw new RuntimeError(RuntimeError.ErrorMessageErrorInFixedDirective);
        }

        /// <summary>
        /// ported from apply_year_directive
        /// </summary>
        private void ReadApplyYearDirective(string line)
        {
            try
            {
                int year = int.Parse(line.Trim());
                ApplyStack.PushFront("year", TimesCommon.Current.Epoch);
                Logger.Current.Debug(DebugTimesEpoch, () => String.Format("Setting current year to {0}", year));
                // This must be set to the last day of the year, otherwise partial
                // dates like "11/01" will refer to last year's november, not the
                // current year.
                TimesCommon.Current.Epoch = new DateTime(year, 12, 31);
            }
            catch
            {
                throw new ParseError(String.Format(ParseError.ParseError_ArgumentSmthNotAValidYear, line.Trim()));
            }
        }

        /// <remarks>ported from alias_directive</remarks>
        private void ReadAliasDirective(string line)
        {
            int pos = line.IndexOf('=');
            if (pos >= 0)
            {
                string e = line.Substring(pos + 1).TrimStart();
                line = line.Substring(0, pos).TrimEnd();

                ReadAccountAliasDirective(TopAccount.FindAccount(e), line);
            }
        }

        /// <remarks>ported from account_alias_directive</remarks>
        private void ReadAccountAliasDirective(Account account, string alias)
        {
            // Once we have an alias name (alias) and the target account
            // (account), add a reference to the account in the `account_aliases'
            // map, which is used by the post parser to resolve alias references.
            alias = alias.Trim();
            
            // Ensure that no alias like "alias Foo=Foo" is registered.
            if (alias == account.FullName)
                throw new ParseError(String.Format(ParseError.ParseError_IllegalAliasSmthEqualsSmth, alias, account.FullName));

            Context.Journal.AccountAliases[alias] = account;
        }

        /// <remarks>ported from account_directive</remarks>
        private void ReadAccountDirective(string line, ITextualReader textualReader)
        {
            long begPos = Context.LineBegPos;
            int begLineNum = Context.LineNum;

            string p = line.TrimStart();
            Account account = Context.Journal.RegisterAccount(p, null, TopAccount);
            AutoXact ae = null;

            while (textualReader.PeekWhitespaceLine())
            {
                line = ReadLine(textualReader);
                string q = line.TrimStart();
                if (String.IsNullOrEmpty(q))
                    break;

                string b = StringExtensions.NextElement(ref q);
                string keyword = q;
                // Ensure there's an argument for the directives that need one.
                if (String.IsNullOrEmpty(b) && keyword != "default")
                    throw new ParseError(String.Format(ParseError.ParseError_AccountDirectiveSmthRequiresAnArgument, keyword));

                if (keyword == "alias") 
                    ReadAccountAliasDirective(account, b);
                else if (keyword == "payee") 
                    ReadAccountPayeeDirective(account, b);
                else if (keyword == "value") 
                    ReadAccountValueDirective(account, b);
                else if (keyword == "default") 
                    ReadAccountDefaultDirective(account);
                else  if (keyword == "assert" || keyword == "check")
                {
                    AnnotationKeepDetails keeper = new AnnotationKeepDetails(true, true, true);
                    Expr expr = new Expr("account == \"" + account.FullName + "\"");
                    Predicate pred = new Predicate(expr.Op, keeper);

                    if (ae == null)
                    {
                        ae = new AutoXact(pred);

                        ae.Pos.PathName = Context.PathName;
                        ae.Pos.BegPos = begPos;
                        ae.Pos.BegLine = begLineNum;
                        ae.Pos.Sequence = Context.Sequence++;
                        ae.CheckExprs = new List<CheckExprPair>();
                    }

                    ae.CheckExprs.Add(new CheckExprPair(new Expr(b), keyword == "assert"  ? CheckExprKindEnum.EXPR_ASSERTION : CheckExprKindEnum.EXPR_CHECK));
                }
                else if (keyword == "eval" || keyword == "expr")
                {
                    // jww (2012-02-27): Make account into symbol scopes so that this
                    // can be used to override definitions within the account.
                    BindScope boundScope = new BindScope(Context.Scope, account);
                    new Expr(b).Calc(boundScope);
                } else if (keyword == "note")
                {
                    account.Note = b;
                }
            }

            if (ae != null)
            {
                Context.Journal.AutoXacts.Add(ae);

                ae.Journal = Context.Journal;
                ae.Pos.EndPos = (int)textualReader.Position;
                ae.Pos.EndLine = Context.LineNum;
            }
        }

        /// <remarks>ported from account_payee_directive</remarks>
        private void ReadAccountPayeeDirective(Account account, string payee)
        {
            payee = payee.Trim();
            Context.Journal.PayeesForUnknownAccounts.Add(new Tuple<Mask,Account>(new Mask(payee), account));
        }

        /// <remarks>ported from account_value_directive</remarks>
        private void ReadAccountValueDirective(Account account, string exprStr)
        {
            account.ValueExpr = new Expr(exprStr);
        }

        /// <remarks>ported from account_default_directive</remarks>
        private void ReadAccountDefaultDirective(Account account)
        {
            Context.Journal.Bucket = account;
        }

        /// <remarks>ported from price_xact_directive</remarks>
        private void ReadPriceXactDirective(string line)
        {
            Tuple<Commodity,PricePoint> point = CommodityPool.Current.ParsePriceDirective(line.Substring(1).Trim());
            if (point == default(Tuple<Commodity,PricePoint>))
                throw new ParseError(ParseError.ParseError_PricingEntryFailedToParse);
        }

        /// <remarks>ported from nomarket_directive</remarks>
        private void ReadNoMarketDirective(string line)
        {
            string p = line.Substring(1).Trim();
            string symbol = Commodity.ParseSymbol(ref p);

            Commodity commodity = CommodityPool.Current.FindOrCreate(symbol);
            if (commodity != null)
                commodity.Flags |= CommodityFlagsEnum.COMMODITY_NOMARKET | CommodityFlagsEnum.COMMODITY_KNOWN;
        }

        /// <remarks>ported from default_commodity_directive</remarks>
        private void ReadDefaultCommodityDirective(string line)
        {
            Amount amt = new Amount(line.Substring(1).Trim());
            Validator.Verify(() => amt.Valid());
            CommodityPool.Current.DefaultCommodity = amt.Commodity;
            amt.Commodity.Flags |= CommodityFlagsEnum.COMMODITY_KNOWN;
        }

        /// <remarks>ported from price_conversion_directive</remarks>
        private void ReadPriceConversionDirective(string line)
        {
            int pos = line.IndexOf('=');
            if (pos >= 0)
            {
                string p = line.Substring(pos + 1);
                line = line.Substring(1, pos - 1);
                Amount.ParseConversion(line, p);
            }
        }

        /// <remarks>ported from default_account_directive</remarks>
        private void ReadDefaultAccountDirective(string line)
        {
            Context.Journal.Bucket = TopAccount.FindAccount(line.Trim());
            Context.Journal.Bucket.IsKnownAccount = true;
        }

        private TimeXact CreateClockInOutEvent(string line, bool capitalized)
        {
            string datetime = line.SafeSubstring(2, 19);

            string p = line.SafeSubstring(22).Trim();
            string n = StringExtensions.NextElement(ref p, true);
            string end = !String.IsNullOrEmpty(n) ? StringExtensions.NextElement(ref n, true) : null;

            if (n == ";")
                end = end.Substring(1).TrimStart();
            else
                end = null;

            ItemPosition position = new ItemPosition();
            position.PathName = Context.PathName;
            position.BegPos = Context.LineBegPos;
            position.BegLine = Context.LineNum;
            position.EndPos = (int)Context.CurrPos;
            position.EndLine = Context.LineNum;
            position.Sequence = Context.Sequence++;

            TimeXact evnt = new TimeXact(position, TimesCommon.Current.ParseDateTime(datetime), capitalized,
                !String.IsNullOrEmpty(p) ? TopAccount.FindAccount(p) : null,
                n ?? String.Empty, end ?? String.Empty);

            return evnt;
        }

        /// <remarks>ported from clock_in_directive</remarks>
        private void ReadClockInDirective(string line, bool capitalized)
        {
            TimeLog.ClockIn(CreateClockInOutEvent(line, capitalized));
        }

        /// <remarks>ported from clock_out_directive</remarks>
        private void ReadClockOutDirective(string line, bool capitalized)
        {
            Context.Count += TimeLog.ClockOut(CreateClockInOutEvent(line, capitalized));
        }

        /// <remarks>ported from period_xact_directive</remarks>
        private void ReadPeriodXactDirective(string line, ITextualReader textualReader)
        {
            long pos = Context.LineBegPos;

            bool revealContext = true;

            try
            {
                PeriodXact pe = new PeriodXact(line.Substring(1).TrimStart())
                {
                    Pos = new ItemPosition()
                    {
                        PathName = Context.PathName,
                        BegPos = Context.LineBegPos,
                        BegLine = Context.LineNum,
                        Sequence = Context.Sequence++
                    }
                };

                revealContext = false;

                if (ParsePosts(textualReader, TopAccount, pe))
                {
                    revealContext = true;
                    pe.Journal = Context.Journal;

                    if (pe.FinalizeXact())
                    {
                        Context.Journal.ExtendXact(pe);
                        Context.Journal.PeriodXacts.Add(pe);

                        pe.Pos.EndPos = (int)Context.CurrPos;
                        pe.Pos.EndLine = Context.LineNum;
                    }
                    else
                    {
                        revealContext = true;
                        pe.Journal = null;
                        throw new ParseError(ParseError.ParseError_PeriodTransactionFailedToBalance);
                    }
                }
            }
            catch
            {
                if (revealContext)
                {
                    ErrorContext.Current.AddErrorContext("While parsing periodic transaction:");
                    ErrorContext.Current.AddErrorContext(ErrorContext.SourceContext(Context.PathName, pos, Context.CurrPos, "> "));
                }
                throw;
            }
        }

        /// <remarks>ported from automated_xact_directive</remarks>
        private void ReadAutomatedXactDirective(string line, ITextualReader textualReader)
        {
            long pos = Context.LineBegPos;

            bool revealContext = true;

            try
            {
                Query query = new Query();
                AnnotationKeepDetails keeper = new AnnotationKeepDetails(true, true, true);
                ExprOp expr = query.ParseArgs(Value.Get(Value.StringValue(line.Substring(1).TrimStart()).AsSequence), keeper, false, true);

                if (expr == null)
                    throw new ParseError("Expected predicate after '='");

                AutoXact ae = new AutoXact(new Predicate(expr, keeper))
                {
                    Pos = new ItemPosition()
                    {
                        PathName = Context.PathName,
                        BegPos = Context.LineBegPos,
                        BegLine = Context.LineNum,
                        Sequence = Context.Sequence++
                    }
                };

                Post lastPost = null;

                while (textualReader.PeekWhitespaceLine())
                {
                    string p = ReadLine(textualReader).TrimStart();
                    if (String.IsNullOrEmpty(p))
                        break;

                    int remLen = p.Length;

                    if (p.StartsWith(";"))
                    {
                        Item item = (Item)lastPost ?? ae;

                        // This is a trailing note, and possibly a metadata info tag
                        ae.AppendNote(p.Substring(1), Context.Scope, true);
                        item.Flags |= SupportsFlagsEnum.ITEM_NOTE_ON_NEXT_LINE;
                        item.Pos.EndPos = (int)Context.CurrPos;
                        item.Pos.EndLine++;
                    }
                    else if (p.StartsWith("assert ") || p.StartsWith("check ") || p.StartsWith("expr ") || p.StartsWith("eval "))
                    {
                        string c = p;
                        p = p.Substring(p.IndexOf(' ')).TrimStart();
                        if (ae.CheckExprs == null)
                            ae.CheckExprs = new List<CheckExprPair>();
                        ae.CheckExprs.Add(new CheckExprPair(new Expr(p), c[0] == 'a' ? CheckExprKindEnum.EXPR_ASSERTION :
                            (c[0] == 'c' ? CheckExprKindEnum.EXPR_CHECK : CheckExprKindEnum.EXPR_GENERAL)));
                    }
                    else
                    {
                        revealContext = false;

                        Post post = ParsePost(p, null, TopAccount, true);
                        if (post != null)
                        {
                            revealContext = true;
                            ae.AddPost(post);
                            ae.ActivePost = lastPost = post;
                        }
                        revealContext = true;
                    }
                }

                Context.Journal.AutoXacts.Add(ae);

                ae.Journal = Context.Journal;
                ae.Pos.EndPos = (int)Context.CurrPos;
                ae.Pos.EndLine = Context.LineNum;
            }
            catch
            {
                if (revealContext)
                {
                    ErrorContext.Current.AddErrorContext("While parsing automated transaction:");
                    ErrorContext.Current.AddErrorContext(ErrorContext.SourceContext(Context.PathName, pos, Context.CurrPos, "> "));
                }
                throw;
            }
        }

        /// <remarks>ported from option_directive</remarks>
        private void ReadOptionDirective(string line)
        {
            string p = StringExtensions.NextElement(ref line);
            if (string.IsNullOrEmpty(p))
            {
                int pos = line.IndexOf('=');
                if (pos >= 0)
                {
                    p = line.Substring(pos + 1);
                    line = line.Substring(0, pos);
                }
            }

            if (!Option.ProcessOption(Context.PathName, line.Substring(2), Context.Scope, p, line))
                throw new OptionError(String.Format(OptionError.ErrorMessage_IllegalOption, line.Substring(2)));
        }

        private const string DebugTextualParse = "textual.parse";
        private const string DebugTextualInclude = "textual.include";
        private const string DebugTimesEpoch = "times.epoch";
    }
}

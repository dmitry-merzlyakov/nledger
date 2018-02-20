// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Commodities;
using NLedger.Expressions;
using NLedger.Journals;
using NLedger.Textual;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Scopus
{
    public class Session : SymbolScope
    {
        public const string DefaultLedgerFileName = "ledger";
        public const string DefaultPriceDbFileName = "pricedb";

        public const string OptionCheckPayees = "check_payees";
        public const string OptionDayBreak = "day_break";
        public const string OptionDownload = "download";
        public const string OptionDecimalComma = "decimal_comma";
        public const string OptionTimeColon = "time_colon";
        public const string OptionPriceExp = "price_exp_";
        public const string OptionFile = "file_";
        public const string OptionInputDateFormat = "input_date_format_";
        public const string OptionExplicit = "explicit";
        public const string OptionMasterAccount = "master_account_";
        public const string OptionPedantic = "pedantic";
        public const string OptionPermissive = "permissive";
        public const string OptionPriceDb = "price_db_";
        public const string OptionStrict = "strict";
        public const string OptionValueExpr = "value_expr_";
        public const string OptionRecursiveAliases = "recursive_aliases";
        public const string OptionNoAliases = "no_aliases";

        public const string CurrentSessionKey = "current session";

        public static void SetSessionContext(Session session)
        {
            if (session != null)
            {
                Commodity.Initialize();
                TimesCommon.Current.TimesInitialize();
                Amount.Initialize();
                Amount.ParseConversion("1.0m", "60s");
                Amount.ParseConversion("1.00h", "60m");
            }
            else
            {
                Amount.Shutdown();
                TimesCommon.Current.TimesShutdown();
                Commodity.Shutdown();
            }
        }

        public static void ReleaseSessionContext()
        {
            Amount.Shutdown();
        }

        public Session()
        {
            ParsingContext = new ParseContextStack();
            CreateOptions();
            CreateLookupItems();

            FlushOnNextDataFile = false;
            Journal = new Journal();

            ParsingContext.Push();
        }

        public bool FlushOnNextDataFile { get; set; }
        public Journal Journal { get; private set; }
        public ParseContextStack ParsingContext { get; private set; }
        public Expr ValueExpr { get; private set; }

        public override string Description
        {
            get { return CurrentSessionKey; }
        }

        public Option CheckPayeesHandler { get; private set; }
        public Option DayBreakHandler { get; private set; }
        public Option DownloadHandler { get; private set; }
        public Option DecimalCommaHandler { get; private set; }
        public Option TimeColonHandler { get; private set; }
        public Option PriceExpHandler { get; private set; }
        public FileOption FileHandler { get; private set; }
        public Option InputDateFormatHandler { get; private set; }
        public Option ExplicitHandler { get; private set; }
        public Option MasterAccountHandler { get; private set; }
        public Option PedanticHandler { get; private set; }
        public Option PermissiveHandler { get; private set; }
        public Option PriceDbHandler { get; private set; }
        public Option StrictHandler { get; private set; }
        public Option ValueExprHandler { get; private set; }
        public Option RecursiveAliasesHandler { get; private set; }
        public Option NoAliasesHandler { get; private set; }

        public int ReadData(string masterAccount)
        {
            bool populatedDataFiles = false;

            if (!FileHandler.DataFiles.Any())
            {
                string file = FileSystem.HomePath(DefaultLedgerFileName);                
                if (FileSystem.FileExists(file))
                    FileHandler.DataFiles.Add(file);
                else
                    throw new ParseError(ParseError.ParseError_NoJournalFileWasSpecified);

                populatedDataFiles = true;
            }

            int xactCount = 0;

            Account acct;
            if (String.IsNullOrEmpty(masterAccount))
                acct = Journal.Master;
            else
                acct = Journal.FindAccount(masterAccount);

            string priceDbPath = null;
            if (PriceDbHandler.Handled)
            {
                priceDbPath = FileSystem.ResolvePath(PriceDbHandler.Str());
                if (!FileSystem.FileExists(priceDbPath))
                    throw new ParseError(String.Format(ParseError.ParseError_CouldNotFindSpecifiedPriceDbFile, priceDbPath));
            }
            else
            {
                priceDbPath = FileSystem.HomePath(DefaultPriceDbFileName);
                // TODO - ./.ledgerrc - ?
            }

            if (DayBreakHandler.Handled)
                Journal.DayBreak = true;

            if (RecursiveAliasesHandler.Handled)
                Journal.RecursiveAliases = true;
            if (NoAliasesHandler.Handled)
                Journal.NoAliases = true;

            if (ExplicitHandler.Handled)
                Journal.ForceChecking = true;
            if (CheckPayeesHandler.Handled)
                Journal.CheckPayees = true;

            if (PermissiveHandler.Handled)
                Journal.CheckingStyle = JournalCheckingStyleEnum.CHECK_PERMISSIVE;
            else if (PedanticHandler.Handled)
                Journal.CheckingStyle = JournalCheckingStyleEnum.CHECK_ERROR;
            else if (StrictHandler.Handled)
                Journal.CheckingStyle = JournalCheckingStyleEnum.CHECK_WARNING;

            if (ValueExprHandler.Handled)
                Journal.ValueExpr = new Expr(ValueExprHandler.Str());

            if (!String.IsNullOrEmpty(priceDbPath) && FileSystem.FileExists(priceDbPath))
            {
                ParsingContext.Push(priceDbPath);
                ParsingContext.GetCurrent().Journal = Journal;
                try
                {
                    if (Journal.Read(ParsingContext) > 0)
                        throw new ParseError(ParseError.ParseError_TransactionsNotAllowedInPriceHistoryFile);
                }
                catch
                {
                    ParsingContext.Pop();
                    throw;
                }
                ParsingContext.Pop();
            }

            foreach(string pathName in FileHandler.DataFiles)
            {
                if (pathName == "-" || pathName == "/dev/stdin")  // TODO - verify
                {
                    // To avoid problems with stdin and pipes, etc., we read the entire
                    // file in beforehand into a memory buffer, and then parcel it out
                    // from there.
                    ParsingContext.Push(new TextualReader(FileSystem.GetStdInAsStreamReader()));
                }
                else
                {
                    ParsingContext.Push(pathName);
                }

                ParsingContext.GetCurrent().Journal = Journal;
                ParsingContext.GetCurrent().Master = acct;

                try
                {
                    xactCount += Journal.Read(ParsingContext);
                }
                catch
                {
                    ParsingContext.Pop();
                    throw;
                }
                ParsingContext.Pop();
            }

            Logger.Current.Debug("ledger.read", () => String.Format("xact_count [{0}] == journal->xacts.size() [{1}]", xactCount, Journal.Xacts.Count));
            if (xactCount != Journal.Xacts.Count)
                throw new InvalidOperationException("assert(xact_count == journal->xacts.size())");

            if (populatedDataFiles)
                FileHandler.DataFiles.Clear();

            Validator.Verify(() => Journal.Valid());

            return Journal.Xacts.Count();
        }

        /// <summary>
        /// Ported from journal_t * session_t::read_journal_files
        /// </summary>
        public Journal ReadJournalFiles()
        {
            var info = Logger.Current.InfoContext(TimerName.Journal)?.Message("Read journal file").Start();  // INFO_START

            string masterAccount = null;
            if (MasterAccountHandler.Handled)
                masterAccount = MasterAccountHandler.Str();

            int count = ReadData(masterAccount);

            info?.Finish(); // INFO_FINISH

            Logger.Current.Info(() => String.Format("Found {0} transactions", count));

            return Journal;
        }

        public Journal ReadJournal(string pathName)
        {
            FileHandler.DataFiles.Clear();
            FileHandler.DataFiles.Add(pathName);

            return ReadJournalFiles();
        }

        public Journal ReadJournalFromString(string data)
        {
            FileHandler.DataFiles.Clear();

            ParsingContext.Push(new TextualReader(FileSystem.GetStreamReaderFromString(data)));

            ParsingContext.GetCurrent().Journal = Journal;
            ParsingContext.GetCurrent().Master = Journal.Master;

            try
            {
                Journal.Read(ParsingContext);
            }
            catch
            {
                ParsingContext.Pop();
                throw;
            }
            ParsingContext.Pop();

            return Journal;
        }

        public void CloseJournalFiles()
        {
            Journal = null;
            Amount.Shutdown();

            Journal = new Journal();
            Amount.Initialize();
        }

        public Value FnAccount(CallScope args)
        {
            if (args[0].Type == ValueTypeEnum.String)
                return Value.ScopeValue(Journal.FindAccount(args[0].AsString));
            else if (args[0].Type == ValueTypeEnum.Mask)
                return Value.ScopeValue(Journal.FindAccountRe(args[0].AsMask.ToString()));
            else
                return null;
        }

        public Value FnMin(CallScope args)
        {
            return args[1].IsLessThan(args[0]) ? args[1] : args[0];
        }

        public Value FnMax(CallScope args)
        {
            return args[1].IsGreaterThan(args[0]) ? args[1] : args[0];
        }

        public Value FnInt(CallScope args)
        {
            return Value.Get(args[0].AsLong);
        }

        public Value FnStr(CallScope args)
        {
            return Value.StringValue(args[0].AsString);
        }

        public Value FnLotPrice(CallScope args)
        {
            Amount amt = args.Get<Amount>(0, false);
            if (amt.HasAnnotation && amt.Annotation.Price != null)
                return Value.Get(amt.Annotation.Price);
            else
                return Value.Empty;
        }

        public Value FnLotDate(CallScope args)
        {
            Amount amt = args.Get<Amount>(0, false);
            if (amt.HasAnnotation && amt.Annotation.Date != null)
                return Value.Get(amt.Annotation.Date);
            else
                return Value.Empty;
        }

        public Value FnLotTag(CallScope args)
        {
            Amount amt = args.Get<Amount>(0, false);
            if (amt.HasAnnotation && amt.Annotation.Tag != null)
                return Value.Get(amt.Annotation.Tag);
            else
                return Value.Empty;
        }

        public Option LookupOption(string s)
        {
            return Options.LookupOption(s, this);
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            return LookupItems.Lookup(kind, name, this) ?? base.Lookup(kind, name);
        }        

        public string ReportOptions()
        {
            return Options.Report();
        }

        private void CreateOptions()
        {
            CheckPayeesHandler = Options.Add(new Option(OptionCheckPayees));
            DayBreakHandler = Options.Add(new Option(OptionDayBreak));
            DownloadHandler = Options.Add(new Option(OptionDownload));
            DecimalCommaHandler = Options.Add(new Option(OptionDecimalComma, (o, w) => Commodity.Defaults.DecimalCommaByDefault = true));
            TimeColonHandler = Options.Add(new Option(OptionTimeColon, (o, w) => Commodity.Defaults.TimeColonByDefault = true));
            PriceExpHandler = Options.Add(new Option(OptionPriceExp) { Value = "24" });
            FileHandler = Options.Add(new FileOption() { Parent = this });
            InputDateFormatHandler = Options.Add(new Option(OptionInputDateFormat, (o, w, s) => 
            {
                // This changes static variables inside times.h, which affects the
                // basic date parser.
                TimesCommon.Current.SetInputDateFormat(s);
            }));
            ExplicitHandler = Options.Add(new Option(OptionExplicit));
            MasterAccountHandler = Options.Add(new Option(OptionMasterAccount));
            PedanticHandler = Options.Add(new Option(OptionPedantic));
            PermissiveHandler = Options.Add(new Option(OptionPermissive));
            PriceDbHandler = Options.Add(new Option(OptionPriceDb));
            StrictHandler = Options.Add(new Option(OptionStrict));
            ValueExprHandler = Options.Add(new Option(OptionValueExpr));
            RecursiveAliasesHandler = Options.Add(new Option(OptionRecursiveAliases));
            NoAliasesHandler = Options.Add(new Option(OptionNoAliases));

            Options.AddLookupArgs(OptionDownload, "Q");
            Options.AddLookupArgs(OptionPriceExp, "Z");
            Options.AddLookupOpt(OptionCheckPayees);
            Options.AddLookupOpt(OptionDownload);
            Options.AddLookupOpt(OptionDecimalComma);
            Options.AddLookupOpt(OptionDayBreak);
            Options.AddLookupOpt(OptionExplicit);
            Options.AddLookupOptArgs(OptionFile, "f");
            Options.AddLookupOpt(OptionInputDateFormat);
            Options.AddLookupOptAlt(OptionPriceExp, "leeway_");
            Options.AddLookupOpt(OptionMasterAccount);
            Options.AddLookupOpt(OptionNoAliases);
            Options.AddLookupOpt(OptionPriceDb);
            Options.AddLookupOpt(OptionPriceExp);
            Options.AddLookupOpt(OptionPedantic);
            Options.AddLookupOpt(OptionPermissive);
            Options.AddLookupOpt(OptionRecursiveAliases);
            Options.AddLookupOpt(OptionStrict);
            Options.AddLookupOpt(OptionTimeColon);
            Options.AddLookupOpt(OptionValueExpr);
        }

        private void CreateLookupItems()
        {
            LookupItems.MakeFunctor("account", scope => FnAccount((CallScope)scope));
            LookupItems.MakeFunctor("lot_price", scope => FnLotPrice((CallScope)scope));
            LookupItems.MakeFunctor("lot_date", scope => FnLotDate((CallScope)scope));
            LookupItems.MakeFunctor("lot_tag", scope => FnLotTag((CallScope)scope));
            LookupItems.MakeFunctor("int", scope => FnInt((CallScope)scope));
            LookupItems.MakeFunctor("min", scope => FnMin((CallScope)scope));
            LookupItems.MakeFunctor("max", scope => FnMax((CallScope)scope));
            LookupItems.MakeFunctor("str", scope => FnStr((CallScope)scope));
            // Check if they are trying to access an option's setting or value.
            LookupItems.MakeOptionFunctors(Options);
            LookupItems.MakeOptionHandlers(Options);
        }

        private readonly OptionCollection Options = new OptionCollection();
        private readonly ExprOpCollection LookupItems = new ExprOpCollection();
    }

    public class FileOption : Option
    {
        public FileOption() : base (Session.OptionFile)
        {
            DataFiles = new List<string>();
        }

        public IList<string> DataFiles { get; private set; }

        public new Session Parent
        {
            get { return (Session)base.Parent; }
            set { base.Parent = value; }
        }

        public override void HandlerThunkStr(string whence, string str)
        {
            if (Parent.FlushOnNextDataFile)
            {
                DataFiles.Clear();
                Parent.FlushOnNextDataFile = false;
            }
            DataFiles.Add(str);
        }
    }

}

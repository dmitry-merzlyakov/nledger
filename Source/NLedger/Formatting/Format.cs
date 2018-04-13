// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Formatting
{
    /// <summary>
    /// Ported from format_t
    /// </summary>
    public class Format : ExprBase<string>
    {
        /// <summary>
        /// Ported from format_t::truncate
        /// </summary>
        public static string Truncate(string ustr, int width, int accountAbbrevLength = 0)
        {
            int len = ustr.Length;
            if (width == 0 || len <= width)
                return ustr;

            StringBuilder sb = new StringBuilder();

            FormatElisionStyleEnum style = DefaultStyle;
            if (accountAbbrevLength > 0 && !DefaultStyleChanged)
                style = FormatElisionStyleEnum.ABBREVIATE;

            switch(style)
            {
                case FormatElisionStyleEnum.TRUNCATE_LEADING:
                    // This method truncates at the beginning.
                    sb.Append("..");
                    sb.Append(ustr.Substring(len-(width-2), width-2));
                    break;

                case FormatElisionStyleEnum.TRUNCATE_MIDDLE:
                    // This method truncates in the middle.
                    sb.Append(ustr.Substring(0, (width - 2) / 2));
                    sb.Append("..");
                    sb.Append(ustr.Substring(len - ((width - 2) / 2 + (width - 2) % 2), (width - 2) / 2 + (width - 2) % 2));
                    break;

                case FormatElisionStyleEnum.ABBREVIATE:
                    if (accountAbbrevLength > 0)
                    {
                        // The algorithm here is complex, but aims to preserve the most
                        // information in the most useful places.
                        //
                        // Consider: You have an account name like
                        // 'Assets:Banking:Check:Register'.  This account name, which is
                        // 29 characters long, must be shortened to fit in 20.  How would
                        // you shorten it?
                        //
                        // The approach taken below is to compute the difference, or 9
                        // characters, and then distribute this difference semi-evenly
                        // among first three segments of the account name, by taking
                        // characters until the difference is gone.  Further, earlier
                        // segments will give up more of their share of letters than later
                        // segments, since the later segments usually contain more useful
                        // information.

                        // First, chop up the Unicode string into individual segments.
                        IList<string> parts = ustr.Split(':').ToList();

                        Logger.Current.Debug("format.abbrev", () => String.Format("Account name: {0}", ustr));
                        Logger.Current.Debug("format.abbrev", () => String.Format("Must fit a {0} char string in {1} chars", len, width));

                        // Figure out the lengths of all the parts.  The last part is
                        // always displayed in full, while the former parts are
                        // distributed, with the latter parts being longer than the
                        // former, but with none shorter than account_abbrev_length.
                        IList<int> lens = parts.Select(s => s.Length).ToList();

                        if (Logger.Current.ShowDebug("format.abbrev"))
                            for (int i = 0; i < lens.Count(); i++)
                                Logger.Current.Debug("format.abbrev", () => String.Format("Segment {0} is {1} chars wide", i, lens[i]));

                        // Determine the "overflow", or how many chars in excess we are.
                        int overflow = len - width;
                        Logger.Current.Debug("format.abbrev", () => String.Format("There are {0} chars of overflow", overflow));

                        // Walk through the first n-1 segments, and start subtracting
                        // letters to decrease the overflow.  This is done in multiple
                        // passes until the overflow is gone, or we cannot reduce any
                        // further.  The calculation to find the amount to remove is:
                        //
                        //   overflow * (((len(segment) + counter) * iteration) /
                        //               (len(string) - len(last_segment) - counter))
                        //
                        // Where:
                        //   overflow - the amount that needs to be removed
                        //   counter - starts at n-1 for the first segment, then
                        //             decreases by one until it reaches 0 for the
                        //             last segment (which is never shortened).
                        //             This value is used to weight the shrinkage
                        //             so that earlier segments shrink faster.
                        //   iteration - starts at 1, increase by 1 for every
                        //               iteration of the loop
                        //
                        // In the example above, we have this account name:
                        //
                        //   Assets:Banking:Check:Register
                        //
                        // Therefore, the amount to be removed from Assets is calculated as:
                        //
                        //   9 * (((6 + 3) * 1) / (29 - 8 - 3)) = ceil(4.5) = 5
                        //
                        // However, since removing 5 chars would make the length of the
                        // segment shorter than the default minimum of 2, we can only
                        // remove 4 chars from Assets to reduce the overflow.  And on it
                        // goes.
                        //
                        // The final result will be: As:Ban:Chec:Register

                        int iteration = 1;
                        int lenMinusLast = len - lens.LastOrDefault();
                        while (overflow > 0)
                        {
                            int overflowAtStart = overflow;
                            Logger.Current.Debug("format.abbrev", () => String.Format("Overflow starting at {0} chars", overflow));

                            int counter = lens.Count;
                            int x = 0; // parts iterator

                            for(int i=0; i<lens.Count; i++)
                            {
                                if (--counter == 0 || overflow == 0)
                                    break;
                                Logger.Current.Debug("format.abbrev", () => String.Format("Overflow is {0} chars", overflow));

                                int adjust;

                                if (overflow == 1)
                                    adjust = 1;
                                else
                                    adjust = (int)Math.Ceiling((double)overflow * 
                                        ((double)(lens[i] + counter * 3) * (double)iteration) / ((double)lenMinusLast - (double)counter));

                                Logger.Current.Debug("format.abbrev", () => String.Format("Weight calc: ({0} * ((({1} + {2}) * {3}) / ({4} - {2})))", overflow, lens[i], counter, iteration, lenMinusLast));

                                if (adjust == 0)
                                    adjust = 1;
                                else if (adjust > overflow)
                                    adjust = overflow;

                                Logger.Current.Debug("format.abbrev", () => String.Format("The weighted part is {0} chars", adjust));

                                int slack = lens[i] - Math.Min(lens[i], accountAbbrevLength);

                                if (adjust > slack)
                                    adjust = slack;

                                if (adjust > 0)
                                {
                                    Logger.Current.Debug("format.abbrev", () => String.Format("Reducing segment {0} by {1} chars", i, adjust));
                                    string xs = parts[x];
                                    while (Char.IsWhiteSpace(xs[lens[i] - adjust - 1]) && adjust < lens[i])
                                    {
                                        Logger.Current.Debug("format.abbrev", () => "Segment ends in whitespace, adjusting down");
                                        ++adjust;
                                    }
                                    
                                    lens[i] -= adjust;
                                    Logger.Current.Debug("format.abbrev", () => String.Format("Segment {0} is now {1} chars wide", i, lens[i]));

                                    if (adjust > overflow)
                                        overflow = 0;
                                    else
                                        overflow -= adjust;
                                    Logger.Current.Debug("format.abbrev", () => String.Format("Overflow is now {0} chars", overflow));
                                }
                                ++x;
                            }
                            Logger.Current.Debug("format.abbrev", () => String.Format("Overflow ending this time at {0} chars", overflow));

                            if (overflow == overflowAtStart)
                              break;
                            iteration++;
                        }

                        if (parts.Count != lens.Count)
                            throw new InvalidOperationException();

                        string result = string.Empty;

                        for (int pi = 0; pi < parts.Count; pi++)
                        {
                            if (pi == parts.Count - 1)
                            {
                                result += parts[pi];
                                break;
                            }

                            if (parts[pi].Length > lens[pi])
                                result += parts[pi].Substring(0, lens[pi]) + ":";
                            else
                                result += parts[pi] + ":";
                        }

                        if (overflow > 0)
                        {
                            // Even abbreviated its too big to show the last account, so
                            // abbreviate all but the last and truncate at the beginning.
                            if (result.Length <= width - 2)
                                throw new InvalidOperationException();

                            sb.Append("..");
                            sb.Append(result.Substring(result.Length - (width - 2), width - 2));
                        }
                        else
                        {
                            sb.Append(result);
                        }
                    }
                    else
                    {
                        // fall through...
                        sb.Append(ustr.Substring(0, width - 2));
                        sb.Append("..");
                    }
                    break;

                case FormatElisionStyleEnum.TRUNCATE_TRAILING:
                    // This method truncates at the end (the default).
                    sb.Append(ustr.Substring(0, width - 2));
                    sb.Append("..");
                    break;

                default:
                    throw new InvalidOperationException();
            }

            return sb.ToString();
        }

        public static ExprOp IdentNode(string ident)
        {
            ExprOp node = new ExprOp(OpKindEnum.IDENT);
            node.AsIdent = ident;
            return node;
        }

        public static FormatElisionStyleEnum DefaultStyle
        {
            get { return MainApplicationContext.Current.DefaultStyle; }
            set { MainApplicationContext.Current.DefaultStyle = value; }
        }

        public static bool DefaultStyleChanged
        {
            get { return MainApplicationContext.Current.DefaultStyleChanged; }
            set { MainApplicationContext.Current.DefaultStyleChanged = value; }
        }

        public Format()
        { }

        public Format(string str, Scope context = null)
            : base (context)
        {
            if (!String.IsNullOrEmpty(str))
                ParseFormat(str);
        }

        public Format(string str, Format tmpl)
        {
            ParseFormat(str, tmpl);
        }

        public void ParseFormat(string format, Format tmpl = null)
        {
            Elements = ParseElements(format, tmpl);
            Text = format;
        }

        public override void MarkUncomplited()
        {
            for (FormatElement elem = Elements; elem != null; elem = elem.Next)
            {
                if (elem.Type == FormatElementEnum.EXPR)
                {
                    Expr expr = elem.Data.GetValue<Expr>();
                    expr.MarkUncomplited();
                }
            }
        }

        public FormatElement Elements { get; private set; }

        public override string Dump()
        {
            StringBuilder sb = new StringBuilder();
            for (FormatElement elem = Elements; elem != null; elem = elem.Next)
                sb.Append(elem.Dump());
            return sb.ToString();
        }

        protected override string RealCalc(Scope scope)
        {
            StringBuilder outStr = new StringBuilder();

            for (FormatElement elem = Elements; elem != null; elem = elem.Next)
            {
                string s;

                switch(elem.Type)
                {
                    case FormatElementEnum.STRING:
                        string stringFormat = StringExtensions.GetWidthAlignFormatString(elem.MinWidth, !elem.IsElementAlignLeft);
                        s = String.Format(stringFormat, elem.Data.GetValue<string>());
                        break;

                    case FormatElementEnum.EXPR:
                        Expr expr = new Expr(elem.Data.GetValue<Expr>());
                        try
                        {
                            expr.Compile(scope);

                            Value value;
                            if (expr.IsFunction)
                            {
                                CallScope args = new CallScope(scope);
                                args.PushBack(Value.Get(elem.MaxWidth));
                                value = expr.GetFunction()(args);
                            }
                            else
                            {
                                value = expr.Calc(scope);
                            }
                            Logger.Current.Debug("format.expr", () => String.Format("value = ({0})", value));

                            if (elem.MinWidth > 0)
                                s = value.Print(elem.MinWidth, -1, elem.IsElementAlignLeft ? AmountPrintEnum.AMOUNT_PRINT_NO_FLAGS : AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY);
                            else
                                s = value.ToString();
                        }
                        catch
                        {
                            string currentContext = ErrorContext.Current.GetContext();

                            ErrorContext.Current.AddErrorContext("While calculating format expression:");
                            ErrorContext.Current.AddErrorContext(expr.ContextToStr());

                            if (!String.IsNullOrEmpty(currentContext))
                                ErrorContext.Current.AddErrorContext(currentContext);

                            throw;
                        }
                        break;

                    default:
                        throw new InvalidOperationException("Unknown enum item");
                }

                if (elem.MaxWidth > 0 || elem.MinWidth > 0)
                {
                    string result;

                    if (elem.MaxWidth > 0 && elem.MaxWidth < s.Length)
                    {
                        result = Truncate(s, elem.MaxWidth);
                    }
                    else
                    {
                        result = s;
                        if (elem.MinWidth > s.Length)
                            result = result + new String(' ', s.Length - elem.MinWidth);
                    }

                    outStr.Append(result);
                }
                else
                {
                    outStr.Append(s);
                }
            }

            return outStr.ToString();
        }

        /// <summary>
        /// Ported from format_t::element_t * format_t::parse_elements
        /// </summary>
        private FormatElement ParseElements(string fmt, Format tmpl = null)
        {
            FormatElement result = null;
            FormatElement current = null;
            string q = string.Empty;
            InputTextStream inStream = new InputTextStream(fmt);

            for (char p = inStream.Peek; !inStream.Eof; inStream.Get(), p = inStream.Peek)
            {
                if (p != '%' && p != '\\')
                {
                    q += p;
                    continue;
                }

                if (result  == null)
                {
                    result = new FormatElement();
                    current = result;
                }
                else
                {
                    current.Next = new FormatElement();
                    current = current.Next;
                }

                if (!String.IsNullOrEmpty(q))
                {
                    current.Type = FormatElementEnum.STRING;
                    current.Data.SetValue(q);
                    q = string.Empty;

                    current.Next = new FormatElement();
                    current = current.Next;
                }

                if (p == '\\')
                {
                    inStream.Get(); p = inStream.Peek;
                    current.Type = FormatElementEnum.STRING;
                    switch(p)
                    {
                        case 'b': current.Data.SetValue("\b"); break;
                        case 'f': current.Data.SetValue("\f"); break;
                        case 'n': current.Data.SetValue("\n"); break;
                        case 'r': current.Data.SetValue("\r"); break;
                        case 't': current.Data.SetValue("\t"); break;
                        case 'v': current.Data.SetValue("\v"); break;
                        case '\\': current.Data.SetValue("\\"); break;
                        default: current.Data.SetValue(new string(p, 1)); break;
                    }
                    continue;
                }

                inStream.Get(); p = inStream.Peek;  // cut '%' or '\\'
                while(p == '-')
                {
                    current.IsElementAlignLeft = true;
                    inStream.Get(); p = inStream.Peek;
                }

                current.MinWidth = inStream.ReadInt(0, out p);
                if (p == '.')
                {
                    current.MaxWidth = inStream.ReadInt(0, out p);
                    p = inStream.Peek;
                    if (current.MinWidth == 0)
                        current.MinWidth = current.MaxWidth;
                }

                if (Char.IsLetter(p))
                {
                    string sExpr;
                    if (SingleLetterMapping.TryGetValue(p, out sExpr))
                    {
                        if (sExpr.Contains('$'))
                        {
                            sExpr = sExpr.Replace("$min", current.MinWidth > 0 ? current.MinWidth.ToString() : "-1").
                                          Replace("$max", current.MaxWidth > 0 ? current.MaxWidth.ToString() : "-1").
                                          Replace("$left", current.IsElementAlignLeft ? "false" : "true" );
                            if (sExpr.Contains('$'))
                                throw new InvalidOperationException("Unrecognized format substitution keyword");
                        }
                        current.Type = FormatElementEnum.EXPR;
                        current.Data.SetValue(new Expr(sExpr));
                    }
                    else
                    {
                        throw new FormatError(String.Format(FormatError.ErrorMessageUnrecognizedFormattingCharacterSmth, p));
                    }
                }
                else
                {
                    switch(p)
                    {
                        case '%':
                            current.Type = FormatElementEnum.STRING;
                            current.Data.SetValue("%");
                            break;

                        case '$':
                            {
                                if (tmpl == null)
                                    throw new FormatError(FormatError.ErrorMessagePriorFieldReferenceButNoTemplate);

                                inStream.Get(); p = inStream.Peek;
                                if (p == '0' || (!Char.IsDigit(p) && p != 'A' && p != 'B' && p != 'C' && p != 'D' && p != 'E' && p != 'F'))
                                    throw new FormatError(FormatError.ErrorMessageFieldReferenceMustBeADigitFrom1To9);

                                int index = char.IsDigit(p) ? p - '0' : (p - 'A' + 10);
                                FormatElement tmplElem = tmpl.Elements;

                                for (int i = 1; i<index && tmplElem != null; i++)
                                {
                                    tmplElem = tmplElem.Next;
                                    while (tmplElem != null && tmplElem.Type != FormatElementEnum.EXPR)
                                        tmplElem = tmplElem.Next;
                                }

                                if (tmplElem == null)
                                    throw new FormatError(FormatError.ErrorMessageReferenceToANonExistentPriorField);

                                current.Assign(tmplElem);
                                break;
                            }

                        case '(':
                        case '{':
                            {
                                bool formatAmount = p == '{';

                                current.Type = FormatElementEnum.EXPR;
                                current.Data.SetValue(ParseSingleExpression(inStream));

                                // Wrap the subexpression in calls to justify and scrub
                                if (!formatAmount)
                                    break;

                                ExprOp op = current.Data.GetValue<Expr>().Op;

                                #region Create Expressions
                                ExprOp call2Node = new ExprOp(OpKindEnum.O_CALL);
                                call2Node.Left = IdentNode("justify");

                                ExprOp args3Node = new ExprOp(OpKindEnum.O_CONS);

                                ExprOp call1Node = new ExprOp(OpKindEnum.O_CALL);
                                call1Node.Left = IdentNode("scrub");
                                call1Node.Right = op.Kind == OpKindEnum.O_CONS ? op.Left : op;

                                args3Node.Left = call1Node;

                                ExprOp args2Node = new ExprOp(OpKindEnum.O_CONS);

                                ExprOp arg1Node = new ExprOp(OpKindEnum.VALUE);
                                arg1Node.AsValue = Value.Get(current.MinWidth > 0 ? current.MinWidth : -1);

                                args2Node.Left = arg1Node;

                                ExprOp args1Node = new ExprOp(OpKindEnum.O_CONS);

                                ExprOp arg2Node = new ExprOp(OpKindEnum.VALUE);
                                arg2Node.AsValue = Value.Get(current.MaxWidth > 0 ? current.MaxWidth : -1);

                                args1Node.Left = arg2Node;

                                ExprOp arg3Node = new ExprOp(OpKindEnum.VALUE);
                                arg3Node.AsValue = Value.Get(!current.IsElementAlignLeft);

                                args1Node.Right = arg3Node;

                                args2Node.Right = args1Node;

                                args3Node.Right = args2Node;

                                call2Node.Right = args3Node;
                                #endregion

                                current.MinWidth = 0;
                                current.MaxWidth = 0;

                                string prevExpr = current.Data.GetValue<Expr>().Text;

                                ExprOp colorizeOp = null;
                                if (op.Kind == OpKindEnum.O_CONS && op.HasRight)
                                    colorizeOp = op.Right;

                                if (colorizeOp != null)
                                {
                                    ExprOp call3Node = new ExprOp(OpKindEnum.O_CALL);
                                    call3Node.Left = IdentNode("ansify_if");
                                    ExprOp args4Node = new ExprOp(OpKindEnum.O_CONS);
                                    args4Node.Left = call2Node; // from above
                                    args4Node.Right = colorizeOp;
                                    call3Node.Right = args4Node;

                                    current.Data.SetValue(new Expr(call3Node));
                                }
                                else
                                {
                                    current.Data.SetValue(new Expr(call2Node));
                                }

                                current.Data.GetValue<Expr>().Text = prevExpr;
                                break;
                            }

                        default:
                            throw new FormatError(String.Format(FormatError.ErrorMessageUnrecognizedFormattingCharacterSmth, p));
                    }
                }
            }

            if (!string.IsNullOrEmpty(q))
            {
                if (result == null)
                {
                    result = new FormatElement();
                    current = result;
                }
                else
                {
                    current.Next = new FormatElement();
                    current = current.Next;
                }
                current.Type = FormatElementEnum.STRING;
                current.Data.SetValue(q);
            }

            return result;
        }

        /// <summary>
        /// Ported from parse_single_expression
        /// </summary>
        private Expr ParseSingleExpression(InputTextStream inStream, bool singleExpr = true)
        {
            string p = inStream.RemainSource;
            int pIndex = inStream.Pos;

            Expr expr = new Expr();
            expr.Parse(inStream, singleExpr ? AmountParseFlagsEnum.PARSE_SINGLE : AmountParseFlagsEnum.PARSE_PARTIAL, p);
            
            if (inStream.Eof)
            {
                expr.Text = p;
            }
            else
            {
                expr.Text = p.Substring(0, inStream.Pos - pIndex);

                inStream.Pos--;
                // Don't gobble up any whitespace
                while (char.IsWhiteSpace(inStream.Peek)) inStream.Pos--;
            }
            return expr;
        }

        private readonly IDictionary<char, string> SingleLetterMapping = new Dictionary<char, string>()
        {
            { 'd', "aux_date ? format_date(date) + \"=\" + format_date(aux_date) : format_date(date)" },
            { 'D', "date" },
            { 'S', "filename" },
            { 'B', "beg_pos" },
            { 'b', "beg_line" },
            { 'E', "end_pos" },
            { 'e', "end_line" },
            { 'X', "\"* \" if cleared" },
            { 'Y', "\"* \" if xact.cleared" },
            { 'C', "\"(\" + code + \") \" if code" },
            { 'P', "payee" },
            { 'a', "account" },
            { 'A', "account" },
            { 't', "justify(scrub(display_amount), $min, $max, $left, color)" },
            { 'T', "justify(scrub(display_total), $min, $max, $left, color)" },
            { 'N', "note" },
        };
    }
}

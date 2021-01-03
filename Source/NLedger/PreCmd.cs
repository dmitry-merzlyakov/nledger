// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Formatting;
using NLedger.Querying;
using NLedger.Scopus;
using NLedger.Textual;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    public static class PreCmd
    {
        // parse_command
        public static Value ParseCommand(CallScope args)
        {
            string arg = CallScope.JoinArgs(args);
            if (String.IsNullOrEmpty(arg))
                throw new LogicError(LogicError.ErrorMessageUsageParseText);

            Report report = args.FindScope<Report>();
            StringBuilder sb = new StringBuilder();

            Post post = GetSampleXact(report);

            sb.AppendLine("--- Input expression ---");
            sb.AppendLine(arg);

            sb.AppendLine("--- Text as parsed ---");
            Expr expr = new Expr(arg);
            sb.AppendLine(expr.Print());

            sb.AppendLine("--- Expression tree ---");
            sb.Append(expr.Dump());

            BindScope boundScope = new BindScope(args, post);
            expr.Compile(boundScope);
            sb.AppendLine("--- Compiled tree ---");
            sb.Append(expr.Dump());

            sb.AppendLine("--- Calculated value ---");
            Value result = expr.Calc();
            sb.AppendLine(result.StripAnnotations(report.WhatToKeep()).Dump());

            report.OutputStream.Write(sb.ToString());
            return Value.Empty;
        }

        // eval_command
        public static Value EvalCommand(CallScope args)
        {
            Report report = args.FindScope<Report>();
            Expr expr = new Expr(CallScope.JoinArgs(args));
            Value result = expr.Calc(args).StripAnnotations(report.WhatToKeep());

            if (!Value.IsNullOrEmpty(result))
                report.OutputStream.WriteLine(result.Print());

            return Value.Empty;
        }

        // format_command
        public static Value FormatCommand(CallScope args)
        {
            string arg = CallScope.JoinArgs(args);
            if (String.IsNullOrEmpty(arg))
                throw new LogicError(LogicError.ErrorMessageUsageFormatText);

            Report report = args.FindScope<Report>();
            StringBuilder sb = new StringBuilder();

            Post post = GetSampleXact(report);

            sb.AppendLine("--- Input format string ---");
            sb.AppendLine(arg);
            sb.AppendLine();

            sb.AppendLine("--- Format elements ---");
            Format fmt = new Format(arg);
            sb.AppendLine(fmt.Dump());

            sb.AppendLine("--- Formatted string ---");
            BindScope boundScope = new BindScope(args, post);
            sb.AppendFormat("\"{0}\"", fmt.Calc(boundScope));
            sb.AppendLine();

            report.OutputStream.Write(sb.ToString());
            return Value.Empty;
        }

        // period_command
        public static Value PeriodCommand(CallScope args)
        {
            string arg = CallScope.JoinArgs(args);
            if (String.IsNullOrEmpty(arg))
                throw new LogicError(LogicError.ErrorMessageUsagePeriodText);

            Report report = args.FindScope<Report>();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(TimesCommon.Current.ShowPeriodTokens(arg));

            DateInterval interval = new DateInterval(arg);
            sb.AppendLine(interval.Dump());

            report.OutputStream.Write(sb.ToString());
            return Value.Empty;            
        }

        // query_command
        public static Value QueryCommand(CallScope args)
        {
            Report report = args.FindScope<Report>();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("--- Input arguments ---");
            sb.AppendLine(args.Value().Dump());
            sb.AppendLine();

            Query query = new Query(args.Value(), report.WhatToKeep(), report.CollapseHandler.Handled);
            if (query.HasQuery(QueryKindEnum.QUERY_LIMIT))
            {
                CallScope subArgs = new CallScope(args);
                subArgs.PushBack(Value.StringValue(query.GetQuery(QueryKindEnum.QUERY_LIMIT)));

                report.OutputStream.Write(sb.ToString());
                ParseCommand(subArgs);
            }

            if (query.HasQuery(QueryKindEnum.QUERY_SHOW))
            {
                sb.AppendLine();
                sb.AppendLine("====== Display predicate ======");
                sb.AppendLine();

                CallScope dispSubArgs = new CallScope(args);
                dispSubArgs.PushBack(Value.StringValue(query.GetQuery(QueryKindEnum.QUERY_SHOW)));

                report.OutputStream.Write(sb.ToString());
                ParseCommand(dispSubArgs);
            }

            return Value.Empty;
        }

        public static Post GetSampleXact(Report report)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--- Context is first posting of the following transaction ---");
            sb.AppendLine(SampleXactStr);
            report.OutputStream.Write(sb.ToString());

            ParseContextStack parsingContext = new ParseContextStack();
            parsingContext.Push(new TextualReader(FileSystem.GetStreamReaderFromString(SampleXactStr)));
            parsingContext.GetCurrent().Journal = report.Session.Journal;
            parsingContext.GetCurrent().Scope = report.Session;

            report.Session.Journal.Read(parsingContext);
            report.Session.Journal.ClearXData();

            Xact first = report.Session.Journal.Xacts.First();
            return first.Posts.First();
        }

        private const string SampleXactStr = "2004/05/27 Book Store\n" +
            "    ; This note applies to all postings. :SecondTag:\n" +
            "    Expenses:Books                 20 BOOK @ $10\n" +
            "    ; Metadata: Some Value\n" +
            "    ; Typed:: $100 + $200\n" +
            "    ; :ExampleTag:\n" +
            "    ; Here follows a note describing the posting.\n" +
            "    Liabilities:MasterCard        $-200.00\n";
    }
}

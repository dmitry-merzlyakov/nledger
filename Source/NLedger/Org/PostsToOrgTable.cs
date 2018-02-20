// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Expressions;
using NLedger.Formatting;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Org
{
    public class PostsToOrgTable : PostHandler
    {
        public PostsToOrgTable(Report report, string prependFormat = null)
            : base(null)
        {
            Report = report;
            HeaderPrinted = false;
            FirstReportTitle = true;

            FirstLineFormat = new Format(FirstLineFormatStr);
            NextLineFormat = new Format(NextLineFormatStr);
            AmountLinesFormat = new Format(AmountLineFormatStr);

            if (!String.IsNullOrEmpty(prependFormat))
                PrependFormat = new Format(prependFormat);

            XactsList = new List<Xact>();
            XactsPresentMap = new Dictionary<Xact, bool>();
        }

        public override void Flush()
        {
            Report.OutputStream.Flush();
        }

        /// <summary>
        /// Ported from void posts_to_org_table::operator()(post_t& post)
        /// </summary>
        public override void Handle(Post post)
        {
            if (!post.HasXData || !post.XData.Displayed)
            {
                StringBuilder sb = new StringBuilder();

                BindScope boundScope = new BindScope(Report, post);

                if (!HeaderPrinted)
                {
                    sb.AppendLine("|Date|Code|Payee|X|Account|Amount|Total|Note|");
                    sb.AppendLine("|-|");
                    sb.AppendLine("|||<20>|||<r>|<r>|<20>|");
                    HeaderPrinted = true;
                }

                if (!String.IsNullOrEmpty(ReportTitle))
                {
                    if (FirstReportTitle)
                        FirstReportTitle = false;
                    else
                        sb.AppendLine();

                    ValueScope valScope = new ValueScope(boundScope, Value.StringValue(ReportTitle));
                    Format groupTitleFormat = new Format(Report.GroupTitleFormatHandler.Str());

                    sb.AppendLine("|-|");
                    sb.Append("|" + groupTitleFormat.Calc(valScope));
                    sb.AppendLine("|-|");

                    ReportTitle = String.Empty;
                }

                if (PrependFormat != null)
                    sb.Append("|" + PrependFormat.Calc(boundScope));

                if (LastXact != post.Xact)
                {
                    sb.Append(FirstLineFormat.Calc(boundScope));
                    LastXact = post.Xact;
                }
                else if (LastPost != null && LastPost.GetDate() != post.GetDate())
                {
                    sb.Append(FirstLineFormat.Calc(boundScope));
                }
                else
                {
                    sb.Append(NextLineFormat.Calc(boundScope));
                }

                Value amt = new Expr("display_amount").Calc(boundScope).Simplified();
                Value tot = new Expr("display_total").Calc(boundScope).Simplified();

                if (amt.Type == ValueTypeEnum.Balance || tot.Type == ValueTypeEnum.Balance)
                {
                    Balance amtBal = amt.AsBalance;
                    Balance totBal = tot.AsBalance;

                    var i = amtBal.Amounts.GetIterator();
                    var j = totBal.Amounts.GetIterator();
                    bool first = true;

                    while (!i.IsEnd || !j.IsEnd)
                    {
                        if (first)
                        {
                            first = false;
                            if (!i.IsEnd) i.MoveNext();
                            if (!j.IsEnd) j.MoveNext();
                        }
                        else
                        {
                            SymbolScope callScope = new SymbolScope(boundScope);
                            bool assigned = false;

                            if (!i.IsEnd)
                            {
                                if (i.Current.Value != null)
                                {
                                    Logger.Current.Debug(DebugOrgNextAmount, () => String.Format("next_amount = {0}", i.Current.Value));
                                    callScope.Define(SymbolKindEnum.FUNCTION, "next_amount", ExprOp.WrapValue(Value.Get(i.Current.Value)));
                                    i.MoveNext();
                                    assigned = true;
                                }
                                else
                                {
                                    callScope.Define(SymbolKindEnum.FUNCTION, "next_amount", ExprOp.WrapValue(Value.StringValue(String.Empty)));
                                    i.MoveNext();
                                }
                            }
                            else
                            {
                                callScope.Define(SymbolKindEnum.FUNCTION, "next_amount", ExprOp.WrapValue(Value.StringValue(String.Empty)));
                            }

                            if (!j.IsEnd)
                            {
                                if (j.Current.Value != null)
                                {
                                    Logger.Current.Debug(DebugOrgNextTotal, () => String.Format("next_total = {0}", j.Current.Value));
                                    callScope.Define(SymbolKindEnum.FUNCTION, "next_total", ExprOp.WrapValue(Value.Get(j.Current.Value)));
                                    j.MoveNext();
                                    Logger.Current.Debug(DebugOrgNextTotal, () => String.Format("2.next_total = {0}", callScope.Lookup(SymbolKindEnum.FUNCTION, "next_total").AsValue));
                                    assigned = true;
                                }
                                else
                                {
                                    callScope.Define(SymbolKindEnum.FUNCTION, "next_total", ExprOp.WrapValue(Value.StringValue(String.Empty)));
                                    j.MoveNext();
                                }
                            }
                            else
                            {
                                callScope.Define(SymbolKindEnum.FUNCTION, "next_total", ExprOp.WrapValue(Value.StringValue(String.Empty)));
                            }

                            if (assigned)
                            {
                                AmountLinesFormat.MarkUncomplited();
                                sb.Append(AmountLinesFormat.Calc(callScope));
                            }
                        }
                    }
                }

                post.XData.Displayed = true;
                LastPost = post;

                Report.OutputStream.Write(sb.ToString());
            }
        }

        public override void Clear()
        {
            LastXact = null;
            LastPost = null;
            HeaderPrinted = false;
            FirstReportTitle = true;
            ReportTitle = String.Empty;

            base.Clear();
        }

        protected List<Xact> XactsList { get; private set; }
        protected IDictionary<Xact, bool> XactsPresentMap { get; private set; }

        protected Report Report { get; private set; }
        protected Format FirstLineFormat { get; private set; }
        protected Format NextLineFormat { get; private set; }
        protected Format AmountLinesFormat { get; private set; }
        protected Format PrependFormat { get; private set; }
        protected Xact LastXact { get; private set; }
        protected Post LastPost { get; private set; }
        protected bool HeaderPrinted { get; private set; }
        protected bool FirstReportTitle { get; private set; }
        protected string ReportTitle { get; private set; }

        private const string FirstLineFormatStr =
            "|%(format_date(date))" +
            "|%(code)" +
            "|%(payee)" +
            "|%(cleared ? \"*\" : (pending ? \"!\" : \"\"))" +
            "|%(display_account)" +
            "|%(scrub(top_amount(display_amount)))" +
            "|%(scrub(top_amount(display_total)))" +
            "|%(join(note | xact.note))\n";

        private const string NextLineFormatStr =
            "|" +
            "|" +
            "|%(has_tag(\"Payee\") ? payee : \"\")" +
            "|%(cleared ? \"*\" : (pending ? \"!\" : \"\"))" +
            "|%(display_account)" +
            "|%(scrub(top_amount(display_amount)))" +
            "|%(scrub(top_amount(display_total)))" +
            "|%(join(note | xact.note))\n";

        private const string AmountLineFormatStr =
            "|" +
            "|" +
            "|" +
            "|" +
            "|" +
            "|%(scrub(next_amount))" +
            "|%(scrub(next_total))" +
            "|\n";

        private const string DebugOrgNextAmount = "org.next_amount";
        private const string DebugOrgNextTotal = "org.next_total";

    }
}

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
using NLedger.Chain;
using NLedger.Commodities;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Values;
using NLedger.Xacts;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Filters
{
    // This filter requires that calc_posts be used at some point
    // later in the chain.
    //
    // Ported from changed_value_posts
    public class ChangedValuePost : PostHandler
    {
        public ChangedValuePost(PostHandler handler, Report report, 
            bool forAccountsReports, bool showUnrealized, DisplayFilterPosts displayFilter)
            : base(handler)
        {
            Report = report;
            TotalExpr = report.RevaluedTotalHandler.Handled ? report.RevaluedTotalHandler.Expr : report.DisplayTotalHandler.Expr;
            DisplayTotalExpr = report.DisplayTotalHandler.Expr;
            ChangedValuesOnly = report.RevaluedOnlyHandler.Handled;
            HistoricalPricesOnly = report.HistoricalHandler.Handled;
            ForAccountsReports = forAccountsReports;
            ShowUnrealized = showUnrealized;
            DisplayFilter = displayFilter;
            Temps = new Temporaries();

            string gainsEquityAccountName;
            if (report.UnrealizedGainsHandler.Handled)
                gainsEquityAccountName = report.UnrealizedGainsHandler.Str();
            else
                gainsEquityAccountName = "Equity:Unrealized Gains";

            GainsEquityAccount = report.Session.Journal.Master.FindAccount(gainsEquityAccountName);
            GainsEquityAccount.IsGeneratedAccount = true;

            string lossesEquityAccountName;
            if (report.UnrealizedLossesHandler.Handled)
                lossesEquityAccountName = report.UnrealizedLossesHandler.Str();
            else
                lossesEquityAccountName = "Equity:Unrealized Losses";

            LossesEquityAccount = report.Session.Journal.Master.FindAccount(lossesEquityAccountName);
            LossesEquityAccount.IsGeneratedAccount = true;

            CreateAccounts();
        }

        public Report Report { get; private set; }
        public Expr TotalExpr { get; private set; }
        public Expr DisplayTotalExpr { get; private set; }
        public bool ChangedValuesOnly { get; private set; }
        public bool HistoricalPricesOnly { get; private set; }
        public bool ForAccountsReports { get; private set; }
        public bool ShowUnrealized { get; private set; }
        public Post LastPost { get; private set; }
        public Value LastTotal { get; private set; }
        public Value RepricedTotal { get; private set; }
        public Temporaries Temps { get; private set; }
        public Account RevaluedAccount { get; private set; }
        public Account GainsEquityAccount { get; private set; }
        public Account LossesEquityAccount { get; private set; }

        public DisplayFilterPosts DisplayFilter { get; private set;}

        public void CreateAccounts()
        {
            RevaluedAccount = DisplayFilter != null ? DisplayFilter.RevaluedAccount : Temps.CreateAccount("<Revalued>");
        }

        /// <summary>
        /// Ported from changed_value_posts::flush
        /// </summary>
        public override void Flush()
        {
            if (LastPost != null && LastPost.GetDate() < Report.Terminus.Date)
            {
                if (!HistoricalPricesOnly)
                {
                    if (!ForAccountsReports)
                        OutputIntermediatePrices(LastPost, (Date)Report.Terminus.Date);
                    OutputRevaluation(LastPost, (Date)Report.Terminus.Date);
                }
                LastPost = null;
            }
            base.Flush();
        }

        /// <summary>
        /// Ported from changed_value_posts::output_revaluation
        /// </summary>
        public void OutputRevaluation(Post post, Date date)
        {
            if (date.IsValid())
                post.XData.Date = date;

            try
            {
                BindScope boundScope = new BindScope(Report, post);
                RepricedTotal = TotalExpr.Calc(boundScope);
            }
            finally
            {
                post.XData.Date = default(Date);
            }

            if (!Value.IsNullOrEmpty(LastTotal))
            {
                Value diff = RepricedTotal - LastTotal;
                if (!Value.IsNullOrEmptyOrFalse(diff))
                {
                    Xact xact = Temps.CreateXact();
                    xact.Payee = "Commodities revalued";
                    xact.Date = date.IsValid() ? date : post.ValueDate;

                    if (!ForAccountsReports)
                    {
                        FiltersCommon.HandleValue(
                            /* value=         */ diff,
                            /* account=       */ RevaluedAccount,
                            /* xact=          */ xact,
                            /* temps=         */ Temps,
                            /* handler=       */ (PostHandler)Handler,
                            /* date=          */ xact.Date.Value,
                            /* act_date_p=    */ true,
                            /* total=         */ RepricedTotal);
                    }
                    else if (ShowUnrealized)
                    {
                        FiltersCommon.HandleValue(
                            /* value=         */ diff.Negated(),
                            /* account=       */ 
                                (diff.IsLessThan(Value.Zero) ?
                                 LossesEquityAccount :
                                 GainsEquityAccount),
                            /* xact=          */ xact,
                            /* temps=         */ Temps,
                            /* handler=       */ (PostHandler)Handler,
                            /* date=          */ xact.Date.Value,
                            /* act_date_p=    */ true,
                            /* total=         */ new Value(),
                            /* direct_amount= */ false,
                            /* mark_visited=  */ true);
                    }
                }
            }
        }

        /// <summary>
        /// Ported from changed_value_posts::output_intermediate_prices
        /// </summary>
        public void OutputIntermediatePrices(Post post, Date current)
        {
            // To fix BZ#199, examine the balance of last_post and determine whether the
            // price of that amount changed after its date and before the new post's
            // date.  If so, generate an output_revaluation for that price change.
            // Mostly this is only going to occur if the user has a series of pricing
            // entries, since a posting-based revaluation would be seen here as a post.

            Value displayTotal = Value.Clone(LastTotal);

            if (displayTotal.Type == ValueTypeEnum.Sequence)
            {
                Xact xact = Temps.CreateXact();
                xact.Payee = "Commodities revalued";
                xact.Date = current.IsValid() ? current : post.ValueDate;

                Post temp = Temps.CopyPost(post, xact);
                temp.Flags |= SupportsFlagsEnum.ITEM_GENERATED;

                PostXData xdata = temp.XData;
                if (current.IsValid())
                    xdata.Date = current;

                switch(LastTotal.Type)
                {
                    case ValueTypeEnum.Boolean:
                    case ValueTypeEnum.Integer:
                        LastTotal.InPlaceCast(ValueTypeEnum.Amount);
                        temp.Amount = LastTotal.AsAmount;
                        break;

                    case ValueTypeEnum.Amount:
                        temp.Amount = LastTotal.AsAmount;
                        break;

                    case ValueTypeEnum.Balance:
                    case ValueTypeEnum.Sequence:
                        xdata.CompoundValue = LastTotal;
                        xdata.Compound = true;
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                BindScope innerScope = new BindScope(Report, temp);
                displayTotal = DisplayTotalExpr.Calc(innerScope);
            }

            switch(displayTotal.Type)
            {
                case ValueTypeEnum.Void:
                case ValueTypeEnum.Integer:
                case ValueTypeEnum.Sequence:
                    break;

                case ValueTypeEnum.Amount:
                case ValueTypeEnum.Balance:
                    {
                        if (displayTotal.Type == ValueTypeEnum.Amount)
                            displayTotal.InPlaceCast(ValueTypeEnum.Balance);

                        IDictionary<DateTime, Amount> allPrices = new SortedDictionary<DateTime, Amount>();

                        foreach (KeyValuePair<Commodity, Amount> amtComm in displayTotal.AsBalance.Amounts)
                            amtComm.Key.MapPrices((d, a) => allPrices[d] = a, current, post.ValueDate, true);

                        // Choose the last price from each day as the price to use
                        IDictionary<Date, bool> pricingDates = new SortedDictionary<Date, bool>();

                        foreach (KeyValuePair<DateTime, Amount> price in allPrices.Reverse())
                        {
                            // This insert will fail if a later price has already been inserted
                            // for that date.
                            var priceDate = (Date)price.Key.Date;
                            Logger.Debug("filters.revalued", () => String.Format("re-inserting {0} at {1}", price.Value, priceDate));
                            pricingDates[priceDate] = true;
                        }

                        // Go through the time-sorted prices list, outputting a revaluation for
                        // each price difference.
                        foreach (KeyValuePair<Date, bool> price in pricingDates)
                        {
                            OutputRevaluation(post, price.Key);
                            LastTotal = RepricedTotal;
                        }
                        break;
                    }

                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Ported from void changed_value_posts::operator()(post_t& post)
        /// </summary>
        public override void Handle(Post post)
        {
            if (LastPost != null)
            {
                if (!ForAccountsReports && !HistoricalPricesOnly)
                    OutputIntermediatePrices(LastPost, post.ValueDate);
                OutputRevaluation(LastPost, post.ValueDate);
            }

            if (ChangedValuesOnly)
                post.XData.Displayed = true;

            base.Handle(post);

            BindScope boundScope = new BindScope(Report, post);
            LastTotal = TotalExpr.Calc(boundScope);
            LastPost = post;
        }

        public override void Clear()
        {
            TotalExpr.MarkUncomplited();
            DisplayTotalExpr.MarkUncomplited();

            LastPost = null;
            LastTotal = new Value();

            Temps.Clear();
            CreateAccounts();

            base.Clear();
        }
    }
}

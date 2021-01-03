// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Filters;
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Chain
{
    public static class ChainCommon
    {
        public static PostHandler ChainHandlers(PostHandler handler, Report report, bool forAccountsReport = false)
        {
            handler = ChainPostHandlers(handler, report, forAccountsReport);
            handler = ChainPrePostHandlers(handler, report);
            return handler;
        }

        public static PostHandler ChainPrePostHandlers(PostHandler baseHandler, Report report)
        {
            PostHandler handler = baseHandler;

            // anonymize_posts removes all meaningful information from xact payee's and
            // account names, for the sake of creating useful bug reports.
            if (report.AnonHandler.Handled)
                handler = new AnonymizePosts(handler);

            // This filter_posts will only pass through posts matching the `predicate'.
            if (report.LimitHandler.Handled)
            {
                Logger.Current.Debug("report.predicate", () => String.Format("Report predicate expression = {0}", report.LimitHandler.Str()));
                handler = new FilterPosts(handler, new Predicate(report.LimitHandler.Str(), report.WhatToKeep()), report);
            }

            // budget_posts takes a set of posts from a data file and uses them to
            // generate "budget posts" which balance against the reported posts.
            //
            // forecast_posts is a lot like budget_posts, except that it adds xacts
            // only for the future, and does not balance them against anything but the
            // future balance.

            if (report.BudgetFlags != ReportBudgetFlags.BUDGET_NO_BUDGET)
            {
                BudgetPosts budgetHandler = new BudgetPosts(handler, (Date)report.Terminus.Date, report.BudgetFlags);
                budgetHandler.AddPeriodXacts(report.Session.Journal.PeriodXacts);
                handler = budgetHandler;

                // Apply this before the budget handler, so that only matching posts are
                // calculated toward the budget.  The use of filter_posts above will
                // further clean the results so that no automated posts that don't match
                // the filter get reported.
                if (report.LimitHandler.Handled)
                    handler = new FilterPosts(handler, new Predicate(report.LimitHandler.Str(), report.WhatToKeep()), report);
            }
            else if (report.ForecastWhileHandler.Handled)
            {
                ForecastPosts forecastPosts = new ForecastPosts(handler, new Predicate(report.ForecastWhileHandler.Str(), report.WhatToKeep()), 
                    report, report.ForecastYearsHandler.Handled ? int.Parse(report.ForecastYearsHandler.Value) : 5);
                forecastPosts.AddPeriodXacts(report.Session.Journal.PeriodXacts);
                handler = forecastPosts;

                // See above, under budget_posts.
                if (report.LimitHandler.Handled)
                    handler = new FilterPosts(handler, new Predicate(report.LimitHandler.Str(), report.WhatToKeep()), report);
            }

            return handler;
        }

        public static PostHandler ChainPostHandlers(PostHandler baseHandler, Report report, bool forAccountsReport = false)
        {
            PostHandler handler = baseHandler;
            Predicate displayPredicate = null;
            Predicate onlyPredicate = null;
            DisplayFilterPosts displayFilter = null;

            Expr expr = report.AmountHandler.Expr;
            expr.Context = report;

            report.TotalHandler.Expr.Context = report;
            report.DisplayAmountHandler.Expr.Context = report;
            report.DisplayTotalHandler.Expr.Context = report;

            if (!forAccountsReport)
            {
                // Make sure only forecast postings which match are allowed through
                if (report.ForecastWhileHandler.Handled)
                    handler = new FilterPosts(handler, new Predicate(report.ForecastWhileHandler.Str(), report.WhatToKeep()), report);

                // truncate_xacts cuts off a certain number of _xacts_ from being
                // displayed.  It does not affect calculation.
                if (report.HeadHandler.Handled || report.TailHandler.Handled)
                    handler = new TruncateXacts(handler,
                        report.HeadHandler.Handled ? Int32.Parse(report.HeadHandler.Value) : 0,
                        report.TailHandler.Handled ? Int32.Parse(report.TailHandler.Value) : 0);

                // display_filter_posts adds virtual posts to the list to account
                // for changes in value of commodities, which otherwise would affect
                // the running total unpredictably.
                handler = displayFilter = new DisplayFilterPosts(handler, report, report.RevaluedHandler.Handled && !report.NoRoundingHandler.Handled);

                // filter_posts will only pass through posts matching the
                // `display_predicate'.
                if (report.DisplayHandler.Handled)
                {
                    displayPredicate = new Predicate(report.DisplayHandler.Str(), report.WhatToKeep());
                    handler = new FilterPosts(handler, displayPredicate, report);
                }
            }

            // changed_value_posts adds virtual posts to the list to account for changes
            // in market value of commodities, which otherwise would affect the running
            // total unpredictably.
            if (report.RevaluedHandler.Handled && (!forAccountsReport || report.UnrealizedHandler.Handled))
                handler = new ChangedValuePost(handler, report, forAccountsReport, report.UnrealizedHandler.Handled, displayFilter);

            // calc_posts computes the running total.  When this appears will determine,
            // for example, whether filtered posts are included or excluded from the
            // running total.
            handler = new CalcPosts(handler, expr, !forAccountsReport || (report.RevaluedHandler.Handled && report.UnrealizedHandler.Handled));

            // filter_posts will only pass through posts matching the
            // `secondary_predicate'.
            if (report.OnlyHandler.Handled)
            {
                onlyPredicate = new Predicate(report.OnlyHandler.Str(), report.WhatToKeep());
                handler = new FilterPosts(handler, onlyPredicate, report);
            }

            if (!forAccountsReport)
            {
                // sort_posts will sort all the posts it sees, based on the `sort_order'
                // value expression.
                if (report.SortHandler.Handled)
                {
                    if (report.SortXactsHandler.Handled)
                        handler = new SortXacts(handler, new Expr(report.SortHandler.Str()), report);
                    else
                        handler = new SortPosts(handler, report.SortHandler.Str(), report);
                }

                // collapse_posts causes xacts with multiple posts to appear as xacts
                // with a subtotaled post for each commodity used.
                if (report.CollapseHandler.Handled)
                    handler = new CollapsePosts(handler, report, expr, displayPredicate, onlyPredicate, report.CollapseIfZeroHandler.Handled);

                // subtotal_posts combines all the posts it receives into one subtotal
                // xact, which has one post for each commodity in each account.
                //
                // period_posts is like subtotal_posts, but it subtotals according to time
                // periods rather than totalling everything.
                //
                // day_of_week_posts is like period_posts, except that it reports
                // all the posts that fall on each subsequent day of the week.
                if (report.EquityHandler.Handled)
                    handler = new PostsAsEquity(handler, report, expr);
                else if (report.SubTotalHandler.Handled)
                    handler = new SubtotalPosts(handler, expr);
            }

            if (report.DowHandler.Handled)
                handler = new DayOfWeekPosts(handler, expr);
            else if (report.ByPayeeHandler.Handled)
                handler = new ByPayeePosts(handler, expr);

            // interval_posts groups posts together based on a time period, such as
            // weekly or monthly.
            if (report.PeriodHandler.Handled)
                handler = new IntervalPosts(handler, expr, new DateInterval(report.PeriodHandler.Str()), report.ExactHandler.Handled, report.EmptyHandler.Handled);

            if (report.DateHandler.Handled)
                handler = new TransferDetails(handler, TransferDetailsElementEnum.SET_DATE, report.Session.Journal.Master, new Expr(report.DateHandler.Str()), report);

            if (report.AccountHandler.Handled)
                handler = new TransferDetails(handler, TransferDetailsElementEnum.SET_ACCOUNT, report.Session.Journal.Master, new Expr(report.AccountHandler.Str()), report);
            else if (report.PivotHandler.Handled)
            {
                string pivot = string.Format("\"{0}:\" + tag(\"{0}\")", report.PivotHandler.Str());
                handler = new TransferDetails(handler, TransferDetailsElementEnum.SET_ACCOUNT, report.Session.Journal.Master, new Expr(pivot), report);
            }

            if (report.PayeeHandler.Handled)
                handler = new TransferDetails(handler, TransferDetailsElementEnum.SET_PAYEE, report.Session.Journal.Master, new Expr(report.PayeeHandler.Str()), report);

            // related_posts will pass along all posts related to the post received.  If
            // the `related_all' handler is on, then all the xact's posts are passed;
            // meaning that if one post of an xact is to be printed, all the post for
            // that xact will be printed.
            if (report.RelatedHandler.Handled)
                handler = new RelatedPosts(handler, report.RelatedAllHandler.Handled);

            if (report.InjectHandler.Handled)
                handler = new InjectPosts(handler, report.InjectHandler.Str(), report.Session.Journal.Master);

            return handler;
        }

    }
}

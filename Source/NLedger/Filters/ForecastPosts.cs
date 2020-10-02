// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Xacts;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLedger.Utils;

namespace NLedger.Filters
{
    /// <summary>
    /// Ported from forecast_posts
    /// </summary>
    public class ForecastPosts : GeneratePosts
    {
        public ForecastPosts(PostHandler handler, Predicate pred, Scope context, int forecastYears)
            : base (handler)
        {
            Pred = pred;
            Context = context;
            ForecastYears = forecastYears;
        }

        public Predicate Pred { get; private set; }
        public Scope Context { get; private set; }
        public int ForecastYears { get; private set; }

        public override void AddPost(DateInterval period, Post post)
        {
            DateInterval i = new DateInterval(period);
            if (!i.Start.HasValue && !i.FindPeriod(TimesCommon.Current.CurrentDate))
                return;

            base.AddPost(i, post);

            // Advance the period's interval until it is at or beyond the current
            // date.
            while (i.Start < TimesCommon.Current.CurrentDate)
                ++i;
        }

        /// <summary>
        /// Ported from forecast_posts::flush
        /// </summary>
        public override void Flush()
        {
            IList<Post> passed = new List<Post>();
            Date last = TimesCommon.Current.CurrentDate;

            // If there are period transactions to apply in a continuing series until
            // the forecast condition is met, generate those transactions now.  Note
            // that no matter what, we abandon forecasting beyond the next 5 years.
            //
            // It works like this:
            //
            // Earlier, in forecast_posts::add_period_xacts, we cut up all the periodic
            // transactions into their components postings, so that we have N "periodic
            // postings".  For example, if the user had this:
            //
            // ~ daily
            //   Expenses:Food       $10
            //   Expenses:Auto:Gas   $20
            // ~ monthly
            //   Expenses:Food       $100
            //   Expenses:Auto:Gas   $200
            //
            // We now have 4 periodic postings in `pending_posts'.
            //
            // Each periodic postings gets its own copy of its parent transaction's
            // period, which is modified as we go.  This is found in the second member
            // of the pending_posts_list for each posting.
            //
            // The algorithm below works by iterating through the N periodic postings
            // over and over, until each of them mets the termination critera for the
            // forecast and is removed from the set.

            while (PendingPosts.Any())
            {
                // At each step through the loop, we find the first periodic posting whose
                // period contains the earliest starting date.                
                PendingPostsPair least = PendingPosts.First();
                foreach(PendingPostsPair i in PendingPosts)
                {
                    if (!i.DateInterval.Start.HasValue)
                        throw new InvalidOperationException("Start is empty");
                    if (!least.DateInterval.Start.HasValue)
                        throw new InvalidOperationException("least.Start is empty");

                    if (i.DateInterval.Start < least.DateInterval.Start)
                        least = i;
                }

                // If the next date in the series for this periodic posting is more than 5
                // years beyond the last valid post we generated, drop it from further
                // consideration.
                Date next = least.DateInterval.Next.Value;
                if (next <= least.DateInterval.Start)
                    throw new InvalidOperationException("next <= least.DateInterval.Start");

                if (((next - last).Days) > (365 * ForecastYears))
                {
                    Logger.Current.Debug("filters.forecast", () => String.Format("Forecast transaction exceeds {0} years beyond today", ForecastYears));
                    PendingPosts.Remove(least);
                    continue;
                }

                // `post' refers to the posting defined in the period transaction.  We
                // make a copy of it within a temporary transaction with the payee
                // "Forecast transaction".
                Post post = least.Post;
                Xact xact = Temps.CreateXact();
                xact.Payee = "Forecast transaction";
                xact.Date = next;
                Post temp = Temps.CopyPost(post, xact);

                // Submit the generated posting
                Logger.Current.Debug("filters.forecast", () => String.Format("Forecast transaction: {0} {1} {2}", temp.GetDate(), temp.Account.FullName, temp.Amount));
                base.Handle(temp);

                // If the generated posting matches the user's report query, check whether
                // it also fails to match the continuation condition for the forecast.  If
                // it does, drop this periodic posting from consideration.
                if (temp.HasXData && temp.XData.Matches)
                {
                    Logger.Current.Debug("filters.forecast", () => "  matches report query");
                    BindScope boundScope = new BindScope(Context, temp);
                    if (!Pred.Calc(boundScope).Bool)
                    {
                        Logger.Current.Debug("filters.forecast", () => "  fails to match continuation criteria");
                        PendingPosts.Remove(least);
                        continue;
                    }
                }

                // Increment the 'least', but remove it from pending_posts if it
                // exceeds its own boundaries.
                ++least.DateInterval;
                if (!least.DateInterval.Start.HasValue)
                {
                    PendingPosts.Remove(least);
                    continue;
                }
            }

            base.Flush();
        }

        public override void Clear()
        {
            Pred.MarkUncomplited();
            base.Clear();
        }
    }
}

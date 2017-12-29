// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Chain;
using NLedger.Expressions;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Filters
{
    public class IntervalPosts : SubtotalPosts
    {
        public IntervalPosts(PostHandler handler, Expr amountExpr, DateInterval interval, bool exactPeriods = false, bool generateEmptyPosts = false)
            : base(handler, amountExpr)
        {
            StartInterval = interval;
            Interval = interval;
            ExactPeriods = exactPeriods;
            GenerateEmptyPosts = generateEmptyPosts;

            AllPosts = new List<Post>();
            CreateAccounts();
        }

        public DateInterval StartInterval { get; private set; }
        public DateInterval Interval { get; private set; }
        public Account EmptytAccount { get; private set; }
        public bool ExactPeriods { get; private set; }
        public bool GenerateEmptyPosts { get; private set; }
        public List<Post> AllPosts { get; private set; }

        public void CreateAccounts()
        {
            EmptytAccount = Temps.CreateAccount("<None>");
        }

        public void ReportSubtotal(DateInterval ival)
        {
            if (ExactPeriods)
                base.ReportSubtotal();
            else
                base.ReportSubtotal(null, ival);
        }

        public override void Flush()
        {
            if (Interval.Duration == null)
            {
                base.Flush();
                return;
            }

            // Sort all the postings we saw by date ascending
            // [DM] Enumerable.OrderBy is a stable sort that preserve original positions for equal items
            AllPosts = AllPosts.OrderBy(p => p, new IntervalPostCompare()).ToList();

            // only if the interval has no start use the earliest post
            if (!(Interval.Begin.HasValue && Interval.FindPeriod(Interval.Begin.Value)))
            {
                // Determine the beginning interval by using the earliest post
                if (AllPosts.Any() && !Interval.FindPeriod(AllPosts.First().GetDate()))
                    throw new LogicError(LogicError.ErrorMessageFailedToFindPeriodForIntervalReport);
            }

            // Walk the interval forward reporting all posts within each one
            // before moving on, until we reach the end of all_posts
            bool sawPosts = false;
            for (int i = 0; i < AllPosts.Count; )
            {
                Post post = AllPosts[i];

                Logger.Debug("filters.interval", () => String.Format("Considering post {0} = {1}", post.GetDate(), post.Amount));
                Logger.Debug("filters.interval", () => String.Format("interval is:{0}", DebugInterval(Interval)));

                if (Interval.Finish.HasValue && post.GetDate() >= Interval.Finish.Value)
                    throw new InvalidOperationException("assert(! interval.finish || post->date() < *interval.finish)");

                if (Interval.WithinPeriod(post.GetDate()))
                {
                    Logger.Debug("filters.interval", () => "Calling subtotal_posts::operator()");
                    base.Handle(post);
                    ++i;
                    sawPosts = true;
                }
                else
                {
                    if (sawPosts)
                    {
                        Logger.Debug("filters.interval", () => "Calling subtotal_posts::report_subtotal()");
                        ReportSubtotal(Interval);
                        sawPosts = false;
                    }
                    else if (GenerateEmptyPosts)
                    {
                        // Generate a null posting, so the intervening periods can be
                        // seen when -E is used, or if the calculated amount ends up
                        // being non-zero
                        Xact nullXact = Temps.CreateXact();
                        nullXact.Date = Interval.InclusiveEnd.Value;

                        Post nullPost = Temps.CreatePost(nullXact, EmptytAccount);
                        nullPost.Flags |= SupportsFlagsEnum.POST_CALCULATED;
                        nullPost.Amount = new Amount(0);

                        base.Handle(nullPost);
                        ReportSubtotal(Interval);
                    }

                    Logger.Debug("filters.interval", () => "Advancing interval");
                    ++Interval;
                }
            }

            // If the last postings weren't reported, do so now.
            if (sawPosts)
            {
                Logger.Debug("filters.interval", () => "Calling subtotal_posts::report_subtotal() at end");
                ReportSubtotal(Interval);
            }

            // Tell our parent class to flush
            base.Flush();
        }

        /// <summary>
        /// Ported from debug_interval
        /// </summary>
        private static string DebugInterval(DateInterval ival)
        {
            StringBuilder sb = new StringBuilder();

            if (ival.Start.HasValue)
                sb.AppendFormat("start  = {0}", ival.Start);
            else
                sb.Append("no start");

            if (ival.Finish.HasValue)
                sb.AppendFormat("finish = {0}", ival.Finish);
            else
                sb.Append("no finish");

            return sb.ToString();
        }

        private class IntervalPostCompare : IComparer<Post>
        {
            public int Compare(Post x, Post y)
            {
                return DateTime.Compare(x.GetDate(), y.GetDate());
            }
        }

        public override void Handle(Post post)
        {
            // If there is a duration (such as weekly), we must generate the
            // report in two passes.  Otherwise, we only have to check whether the
            // post falls within the reporting period.

            if (Interval.Duration != null)
                AllPosts.Add(post);
            else if (Interval.FindPeriod(post.GetDate()))
                base.Handle(post);
        }

        public override void Clear()
        {
            Interval = StartInterval;

            base.Clear();
            CreateAccounts();
        }
    }
}

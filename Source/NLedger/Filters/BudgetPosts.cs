// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Chain;
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

namespace NLedger.Filters
{
    /// <summary>
    /// Ported from budget_posts
    /// </summary>
    public class BudgetPosts : GeneratePosts
    {
        public BudgetPosts(PostHandler handler, Date terminus, ReportBudgetFlags flags = ReportBudgetFlags.BUDGET_BUDGETED)
            : base (handler)
        {
            Terminus = terminus;
            Flags = flags;
        }

        public ReportBudgetFlags Flags { get; private set; }
        public Date Terminus { get; private set; }

        /// <summary>
        /// Ported from report_budget_items
        /// </summary>
        public void ReportBudgetItems(Date date)
        {
            // Cleanup pending items that finished before date
            // We have to keep them until the last day they apply because operator() needs them to see if a
            // posting is budgeted or not
            IList<PendingPostsPair> postsToErase = new List<PendingPostsPair>();
            foreach (PendingPostsPair pair in PendingPosts)
            {
                if (pair.DateInterval.Finish.HasValue && !pair.DateInterval.Start.HasValue && pair.DateInterval.Finish < date)
                    postsToErase.Add(pair);
            }
            foreach (PendingPostsPair pair in postsToErase)
                PendingPosts.Remove(pair);

            if (!PendingPosts.Any())
                return;

            bool reported;
            do
            {
                reported = false;

                foreach (PendingPostsPair pair in PendingPosts)
                {
                    if (pair.DateInterval.Finish.HasValue && !pair.DateInterval.Start.HasValue)
                        continue;       // skip expired posts

                    Date? begin = pair.DateInterval.Start;
                    if (!begin.HasValue)
                    {
                        Date? rangeBegin = null;
                        if (pair.DateInterval.Range != null)
                            rangeBegin = pair.DateInterval.Range.Begin;

                        Logger.Current.Debug(DebugBudgetGenerate, () => "Finding period for pending post");
                        if (!pair.DateInterval.FindPeriod(rangeBegin ?? date))
                            continue;
                        if (!pair.DateInterval.Start.HasValue)
                            throw new LogicError(LogicError.ErrorMessageFailedToFindPeriodForPeriodicTransaction);
                        begin = pair.DateInterval.Start;
                    }

                    Logger.Current.Debug(DebugBudgetGenerate, () => String.Format("begin = {0}", begin));
                    Logger.Current.Debug(DebugBudgetGenerate, () => String.Format("date  = {0}", date));
                    if (pair.DateInterval.Finish.HasValue)
                        Logger.Current.Debug(DebugBudgetGenerate, () => String.Format("pair.first.finish = {0}", pair.DateInterval.Finish));

                    if (begin <= date && (!pair.DateInterval.Finish.HasValue || begin < pair.DateInterval.Finish))
                    {
                        Post post = pair.Post;

                        pair.DateInterval++;
                        Logger.Current.Debug(DebugBudgetGenerate, () => "Reporting budget for " + post.ReportedAccount.FullName);

                        Xact xact = Temps.CreateXact();
                        xact.Payee = "Budget transaction";
                        xact.Date = begin.Value;

                        Post temp = Temps.CopyPost(post, xact);
                        temp.Amount.InPlaceNegate();

                        if (Flags.HasFlag(ReportBudgetFlags.BUDGET_WRAP_VALUES))
                        {
                            Value seq = new Value();
                            seq.PushBack(Value.Get(0));
                            seq.PushBack(Value.Get(temp.Amount));

                            temp.XData.CompoundValue = seq;
                            temp.XData.Compound = true;
                        }

                        base.Handle(temp);

                        reported = true;
                    }
                }
            }                
            while (reported);
        }

        public override void Handle(Post post)
        {
            bool postInBudget = false;

            foreach (PendingPostsPair pair in PendingPosts)
            {
                for (Account acct = post.ReportedAccount; acct != null; acct = acct.Parent)
                {
                    if (acct == pair.Post.ReportedAccount)
                    {
                        postInBudget = true;
                        // Report the post as if it had occurred in the parent account.
                        if (post.ReportedAccount != acct)
                            post.ReportedAccount = acct;
                        goto handle;
                    }
                }
            }

            handle:

            if (postInBudget && Flags.HasFlag(ReportBudgetFlags.BUDGET_BUDGETED))
            {
                ReportBudgetItems(post.GetDate());
                base.Handle(post);
            }
            else if (!postInBudget && Flags.HasFlag(ReportBudgetFlags.BUDGET_UNBUDGETED))
            {
                base.Handle(post);
            }
        }

        public override void Flush()
        {
            if (Flags.HasFlag(ReportBudgetFlags.BUDGET_BUDGETED))
                ReportBudgetItems(Terminus);

            base.Flush();
        }

        private const string DebugBudgetGenerate = "budget.generate";
    }
}

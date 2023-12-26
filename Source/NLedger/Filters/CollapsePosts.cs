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
using NLedger.Utils;

namespace NLedger.Filters
{
    /// <summary>
    /// Ported from collapse_posts
    /// </summary>
    public class CollapsePosts : PostHandler
    {
        public CollapsePosts(PostHandler handler, Report report, Expr amountExpr, Predicate displayPredicate, Predicate onlyPredicate, bool onlyCollapseIfZero = false)
            : base(handler)
        {
            AmountExpr = amountExpr;
            DisplayPredicate = displayPredicate;
            OnlyPredicate = onlyPredicate ?? Predicate.EmptyPredicate;
            OnlyCollapseIfZero = onlyCollapseIfZero;
            Report = report;

            Temps = new Temporaries();
            ComponentPosts = new List<Post>();

            CreateAccounts();
        }

        public override void Dispose()
        {
            Temps?.Dispose();
            base.Dispose();
        }

        public Expr AmountExpr { get; private set; }
        public Predicate DisplayPredicate { get; private set; }
        public Predicate OnlyPredicate { get; private set; }
        public Value Subtotal { get; private set; }
        public int Count { get; private set; }
        public Xact LastXact { get; private set; }
        public Post LastPost { get; private set; }
        public Temporaries Temps { get; private set; }
        public Account TotalsAccount { get; private set; }
        public bool OnlyCollapseIfZero { get; private set; }
        public IList<Post> ComponentPosts { get; private set; }
        public Report Report { get; private set; }

        public void CreateAccounts()
        {
            TotalsAccount = Temps.CreateAccount("<Total>");
        }

        public override void Flush()
        {
            ReportSubtotal();
            base.Flush();
        }

        /// <summary>
        /// Ported from collapse_posts::report_subtotal
        /// </summary>
        public void ReportSubtotal()
        {
            if (Count == 0)
                return;

            int displayedCount = 0;
            foreach(Post post in ComponentPosts)
            {
                BindScope boundScope = new BindScope(Report, post);
                if (OnlyPredicate.Calc(boundScope).Bool && DisplayPredicate.Calc(boundScope).Bool)
                    displayedCount++;
            }

            if (displayedCount == 1)
            {
                base.Handle(LastPost);
            }
            else if (OnlyCollapseIfZero && !Subtotal.IsZero)
            {
                foreach (Post post in ComponentPosts)
                    base.Handle(post);
            }
            else
            {
                Date earliestDate = default(Date);
                Date latestDate = default(Date);

                foreach(Post post in ComponentPosts)
                {
                    Date date = post.GetDate();
                    Date valueDate = post.ValueDate;
                    if (!earliestDate.IsValid() || date < earliestDate)
                        earliestDate = date;
                    if (!latestDate.IsValid() || valueDate > latestDate)
                        latestDate = valueDate;
                }

                Xact xact = Temps.CreateXact();
                xact.Payee = LastXact.Payee;
                xact.Date = earliestDate.IsValid() ? earliestDate : LastXact.Date;

                Logger.Current.Debug("filters.collapse", () => String.Format("Pseudo-xact date = {0}", xact.Date));
                Logger.Current.Debug("filters.collapse", () => String.Format("earliest date    = {0}", earliestDate));
                Logger.Current.Debug("filters.collapse", () => String.Format("latest date      = {0}", latestDate));

                FiltersCommon.HandleValue(
                    /* value=      */ Subtotal,
                    /* account=    */ TotalsAccount,
                    /* xact=       */ xact,
                    /* temps=      */ Temps,
                    /* handler=    */ (PostHandler)Handler,
                    /* date=       */ latestDate,
                    /* act_date_p= */ false);
            }

            ComponentPosts.Clear();

            LastXact = null;
            LastPost = null;
            Subtotal = Value.Get(0);
            Count = 0;
        }

        public override void Handle(Post post)
        {
            // If we've reached a new xact, report on the subtotal
            // accumulated thus far.
            if (LastXact != post.Xact && Count > 0)
                ReportSubtotal();

            Subtotal = post.AddToValue(Subtotal, AmountExpr);

            ComponentPosts.Add(post);

            LastXact = post.Xact;
            LastPost = post;
            Count++;
        }

        public override void Clear()
        {
            AmountExpr.MarkUncomplited();
            DisplayPredicate.MarkUncomplited();
            OnlyPredicate.MarkUncomplited();

            Subtotal = new Value();
            Count = 0;
            LastXact = null;
            LastPost = null;

            Temps.Clear();
            CreateAccounts();
            ComponentPosts.Clear();

            base.Clear();
        } 
    }
}

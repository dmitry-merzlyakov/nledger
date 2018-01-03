// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Items;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Accounts
{
    /// <summary>
    /// Ported from account_t/xdata_t/details_t (account.h)
    /// </summary>
    public class AccountXDataDetails
    {
        public AccountXDataDetails()
        {
            Total = new Value();
        }

        public Value Total { get; set; }
        public bool Calculated { get; set; }
        public bool Gathered { get; set; }

        public int PostsCount { get; private set; }
        public int PostsVirtualsCount { get; private set; }
        public int PostsClearedCount { get; private set; }
        public int PostsLast7Count { get; private set; }
        public int PostsLast30Count { get; private set; }
        public int PostsThisMountCount { get; private set; }

        public Date EarliestPost { get; private set; }
        public Date EarliestClearedPost { get; private set; }
        public Date LatestPost { get; private set; }
        public Date LatestClearedPost { get; private set; }

        public DateTime EarliestCheckin { get; private set; }
        public DateTime LatestCheckout { get; private set; }
        public bool LatestCheckoutCleared { get; private set; }

        public IEnumerable<string> Filenames { get { return _Filenames; } }
        public IEnumerable<string> AccountsReferenced { get { return _AccountsReferenced; } }
        public IEnumerable<string> PayeesReferenced { get { return _PayeesReferenced; } }

        public int LastPost { get; set; } // Index in PostsList
        public int  LastReportedPost { get; set; } // Index in PostsList

        public AccountXDataDetails Add(AccountXDataDetails other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            PostsCount += other.PostsCount;
            PostsVirtualsCount += other.PostsVirtualsCount;
            PostsClearedCount += other.PostsClearedCount;
            PostsLast7Count += other.PostsLast7Count;
            PostsLast30Count += other.PostsLast30Count;
            PostsThisMountCount += other.PostsThisMountCount;

            if (!EarliestPost.IsValid() || (other.EarliestPost.IsValid() && other.EarliestPost < EarliestPost))
                EarliestPost = other.EarliestPost;
            if (!EarliestClearedPost.IsValid() || (other.EarliestClearedPost.IsValid() && other.EarliestClearedPost < EarliestClearedPost))
                EarliestClearedPost = other.EarliestClearedPost;

            if (!LatestPost.IsValid() || (other.LatestPost.IsValid() && other.LatestPost > LatestPost))
                LatestPost = other.LatestPost;
            if (!LatestClearedPost.IsValid() || (other.LatestClearedPost.IsValid() && other.LatestClearedPost > LatestClearedPost))
                LatestClearedPost = other.LatestClearedPost;

            _Filenames.UnionWith(other.Filenames);
            _AccountsReferenced.UnionWith(other.AccountsReferenced);
            _PayeesReferenced.UnionWith(other.PayeesReferenced);

            return this;
        }

        public void Update(Post post, bool gatherAll = false)
        {
            if (post == null)
                throw new ArgumentNullException("post");

            PostsCount++;

            if (post.Flags.HasFlag(SupportsFlagsEnum.POST_COST_VIRTUAL))
                PostsVirtualsCount++;

            if (gatherAll && post.HasPos)
                _Filenames.Add(post.Pos.PathName);

            Date date = post.GetDate();

            if (date.Year == TimesCommon.Current.CurrentDate.Year && date.Month == TimesCommon.Current.CurrentDate.Month)
                PostsThisMountCount++;

            if ((TimesCommon.Current.CurrentDate - date).Days <= 30)
                PostsLast30Count++;
            if ((TimesCommon.Current.CurrentDate - date).Days <= 7)
                PostsLast7Count++;

            if (!EarliestPost.IsValid() || date < EarliestPost)
                EarliestPost = date;
            if (!LatestPost.IsValid() || date > LatestPost)
                LatestPost = date;

            if (post.Checkin.HasValue && (EarliestCheckin.IsNotADateTime() || post.Checkin.Value < EarliestCheckin))
                EarliestCheckin = post.Checkin.Value;

            if (post.Checkout.HasValue && (LatestCheckout.IsNotADateTime() || post.Checkout.Value > LatestCheckout))
            {
                LatestCheckout = post.Checkout.Value;
                LatestCheckoutCleared = post.State == ItemStateEnum.Cleared;
            }

            if (post.State == ItemStateEnum.Cleared)
            {
                PostsClearedCount++;

                if (!EarliestClearedPost.IsValid() || date < EarliestClearedPost)
                    EarliestClearedPost = date;
                if (!LatestClearedPost.IsValid() || date > LatestClearedPost)
                    LatestClearedPost = date;
            }

            if (gatherAll)
            {
                _AccountsReferenced.Add(post.Account.FullName);
                _PayeesReferenced.Add(post.Payee);
            }
        }

        private readonly SortedSet<string> _Filenames = new SortedSet<string>();
        private readonly SortedSet<string> _AccountsReferenced = new SortedSet<string>();
        private readonly SortedSet<string> _PayeesReferenced = new SortedSet<string>();
    }
}

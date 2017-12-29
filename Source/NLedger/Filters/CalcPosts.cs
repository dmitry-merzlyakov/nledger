// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Chain;
using NLedger.Expressions;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Filters
{
    public class CalcPosts : PostHandler
    {
        public CalcPosts(PostHandler handler, Expr amountExpr, bool calcRunningTotal = false)
            : base (handler)
        {
            AmountExpr = amountExpr;
            CalcRunningTotal = calcRunningTotal;
        }

        public Post LastPost { get; private set; }
        public Expr AmountExpr { get; private set; }
        public bool CalcRunningTotal { get; private set; }

        /// <summary>
        /// Ported from void calc_posts::operator()(post_t& post)
        /// </summary>
        public override void Handle(Post post)
        {
            PostXData xdata = post.XData;

            if (LastPost != null)
            {
                if (!LastPost.HasXData)
                    throw new InvalidOperationException("no xdata");

                if (CalcRunningTotal)
                    xdata.Total = Value.Clone(LastPost.XData.Total);
                xdata.Count = LastPost.XData.Count + 1;
            }
            else
            {
                xdata.Count = 1;
            }

            xdata.VisitedValue = post.AddToValue(xdata.VisitedValue, AmountExpr);
            xdata.Visited = true;

            Account acct = post.ReportedAccount;
            acct.XData.Visited = true;

            if (CalcRunningTotal)
                xdata.Total = Value.AddOrSetValue(xdata.Total, xdata.VisitedValue);

            base.Handle(post);

            LastPost = post;
        }

        public override void Clear()
        {
            LastPost = null;
            AmountExpr.MarkUncomplited();

            base.Clear();
        }
    }
}

// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Filters
{
    /// <summary>
    /// Ported from by_payee_posts
    /// </summary>
    public class ByPayeePosts : PostHandler
    {
        public ByPayeePosts(PostHandler handler, Expr amountExpr)
            : base(handler)
        {
            AmountExpr = amountExpr;
            PayeeSubtotals = new SortedDictionary<string, SubtotalPosts>();
        }

        public Expr AmountExpr { get; private set; }
        public IDictionary<string, SubtotalPosts> PayeeSubtotals { get; private set; }

        public override void Flush()
        {
            foreach (KeyValuePair<string, SubtotalPosts> pair in PayeeSubtotals)
                pair.Value.ReportSubtotal(pair.Key);

            base.Flush();

            PayeeSubtotals.Clear();
        }

        public override void Handle(Post post)
        {
            SubtotalPosts posts;
            if (!PayeeSubtotals.TryGetValue(post.Payee, out posts))
            {
                posts = new SubtotalPosts((PostHandler)Handler, AmountExpr);
                PayeeSubtotals.Add(post.Payee, posts);
            }

            posts.Handle(post);
        }

        public override void Clear()
        {
            AmountExpr.MarkUncomplited();
            PayeeSubtotals.Clear();

            base.Clear();
        }
    }
}

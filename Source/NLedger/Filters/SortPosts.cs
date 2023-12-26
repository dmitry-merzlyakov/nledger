// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Expressions;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Filters
{
    /// <summary>
    /// Ported from sort_posts
    /// </summary>
    public class SortPosts : PostHandler
    {
        public SortPosts(PostHandler handler, Expr sortOrder, Report report)
            : base(handler)
        {
            SortOrder = sortOrder;
            Posts = new List<Post>();
        }

        public SortPosts(PostHandler handler, string sortOrder, Report report)
            : this(handler, new Expr(sortOrder), report)
        { }

        public Expr SortOrder { get; private set; }
        public List<Post> Posts { get; private set; }
        public Report Report { get; private set; }

        public virtual void PostAccumulatedPosts()
        {
            // [DM] Enumerable.OrderBy is a stable sort that preserve original positions for equal items
            Posts = Posts.OrderBy(p => p, new ComparePosts(SortOrder, Report)).ToList();

            foreach(Post post in Posts)
            {
                post.XData.SortCalc = false;
                base.Handle(post);
            }

            Posts.Clear();
        }

        public override void Flush()
        {
            PostAccumulatedPosts();
            base.Flush();
        }

        public override void Handle(Post post)
        {
            Posts.Add(post);
        }

        public override void Clear()
        {
            Posts.Clear();
            SortOrder.MarkUncomplited();

            base.Clear();
        }
    }
}

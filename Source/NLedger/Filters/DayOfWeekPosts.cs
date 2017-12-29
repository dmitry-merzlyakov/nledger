// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
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
    /// Pored from day_of_week_posts
    /// </summary>
    public class DayOfWeekPosts : SubtotalPosts
    {
        public DayOfWeekPosts(PostHandler handler, Expr amountExpr)
            : base (handler, amountExpr)
        {
            DaysOfTheWeek = new Dictionary<DayOfWeek, IList<Post>>();
            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
                DaysOfTheWeek[dayOfWeek] = new List<Post>();
        }

        public IDictionary<DayOfWeek, IList<Post>> DaysOfTheWeek { get; private set; }

        public override void Flush()
        {
            foreach (IList<Post> posts in DaysOfTheWeek.Values)
            {
                foreach (Post post in posts)
                    base.Handle(post);
                ReportSubtotal("%As");
                posts.Clear();
            }

            base.Flush();
        }

        public override void Handle(Post post)
        {
            DaysOfTheWeek[post.GetDate().DayOfWeek].Add(post);
        }

        public override void Clear()
        {
            foreach (IList<Post> posts in DaysOfTheWeek.Values)
                posts.Clear();

            base.Clear();
        }
    }
}

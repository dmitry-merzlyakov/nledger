// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Filters
{
    public class SortXacts : PostHandler
    {
        public SortXacts(PostHandler handler, Expr sortOrder, Report report)
            : base(null)
        {
            Sorter = new SortPosts(handler, sortOrder, report);
        }

        public SortXacts(PostHandler handler, string sortOrder, Report report)
            : base(null)
        {
            Sorter = new SortPosts(handler, sortOrder, report);
        }

        public SortPosts Sorter { get; private set; }
        public Xact LastXact { get; set; }

        public override void Flush()
        {
            Sorter.Flush();
            base.Flush();
        }

        public override void Handle(Post post)
        {
            if (LastXact != null && post.Xact != LastXact)
                Sorter.PostAccumulatedPosts();

            Sorter.Handle(post);

            LastXact = post.Xact;
        }

        public override void Clear()
        {
            Sorter.Clear();
            LastXact = null;

            base.Clear();
        }
    }
}

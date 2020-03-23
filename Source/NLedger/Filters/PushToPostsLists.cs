// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Filters
{
    public class PushToPostsLists : PostHandler
    {
        public PushToPostsLists(IList<Post> posts) : base(null)
        {
            Posts = posts;
        }

        public IList<Post> Posts { get; private set; }

        public override void Handle(Post post)
        {
            Posts.Add(post);
        }
    }
}

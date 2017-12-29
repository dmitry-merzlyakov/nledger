// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
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
    public class RelatedPosts : PostHandler
    {
        public RelatedPosts(PostHandler handler, bool alsoMatching = false)
            : base (handler)
        {
            Posts = new List<Post>();
            AlsoMatching = alsoMatching;
        }

        public IList<Post> Posts { get; private set; }
        public bool AlsoMatching { get; private set; }

        public override void Flush()
        {
            if (Posts.Any())
            {
                foreach(Post post in Posts)
                {
                    if (post.Xact == null)
                        throw new InvalidOperationException("no xact");
                    foreach(Post rPost in post.Xact.Posts)
                    {
                        PostXData xdata = rPost.XData;
                        if (!xdata.Handled && (!xdata.Received 
                            ? !(rPost.Flags.HasFlag(SupportsFlagsEnum.ITEM_GENERATED) || rPost.Flags.HasFlag(SupportsFlagsEnum.POST_VIRTUAL))
                            : AlsoMatching))
                        {
                            xdata.Handled = true;
                            base.Handle(rPost);
                        }
                    }
                }
            }

            base.Flush();
        }

        public override void Handle(Post post)
        {
            post.XData.Received = true;
            Posts.Add(post);
        }

        public override void Clear()
        {
            Posts.Clear();
            base.Clear();
        }
    }
}

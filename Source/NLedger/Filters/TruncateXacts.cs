// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Filters
{
    /// <summary>
    /// Ported from truncate_xacts
    /// </summary>
    public class TruncateXacts : PostHandler
    {
        public TruncateXacts(PostHandler handler, int headCount, int tailCount)
            : base(handler)
        {
            HeadCount = headCount;
            TailCount = tailCount;

            Posts = new List<Post>();
        }

        public int HeadCount { get; private set; }
        public int TailCount { get; private set; }

        public IList<Post> Posts { get; private set; }
        public bool Completed { get; private set; }
        public int XactsSeen { get; private set; }
        public Xact LastXact { get; private set; }

        /// <summary>
        /// Ported from void truncate_xacts::flush()
        /// </summary>
        public override void Flush()
        {
            if (!Posts.Any())
                return;

            int l = Posts.Select(p => p.Xact).Distinct().Count();

            Xact xact = Posts.First().Xact;

            int i = 0;
            foreach(Post post in Posts)
            {
                if (xact != post.Xact)
                {
                    xact = post.Xact;
                    i++;
                }

                bool print = false;
                if (HeadCount != 0)
                {
                    if (HeadCount > 0 && i < HeadCount)
                        print = true;
                    else if (HeadCount < 0 && i >= -HeadCount)
                        print = true;
                }

                if (!print && TailCount != 0)
                {
                    if (TailCount > 0 && l - i <= TailCount)
                        print = true;
                    else if (TailCount < 0 && l - i > -TailCount)
                        print = true;
                }

                if (print)
                    base.Handle(post);
            }
            Posts.Clear();

            base.Flush();
        }

        public override void Handle(Post post)
        {
            if (Completed)
                return;

            if (LastXact != post.Xact)
            {
                if (LastXact != null)
                    XactsSeen++;
                LastXact  = post.Xact;
            }

            if (TailCount == 0 && HeadCount > 0 && XactsSeen >= HeadCount)
            {
                Flush();
                Completed = true;
                return;
            }

            Posts.Add(post);
        }

        public override void Clear()
        {
            Completed = false;
            Posts.Clear();
            XactsSeen = 0;
            LastXact = null;

            base.Clear();
        }
    }
}

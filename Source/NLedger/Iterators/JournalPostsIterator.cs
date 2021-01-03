// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Journals;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Iterators
{
    public class JournalPostsIterator : IIterator<Post>
    {
        public JournalPostsIterator(Journal journal)
        {
            Reset(journal);
        }

        public XactsIterator Xacts { get; private set; }
        public XactPostsIterator Posts { get; private set; }

        public void Reset(Journal journal)
        {
            Xacts = new XactsIterator(journal);
            Posts = new XactPostsIterator();
        }

        public IEnumerable<Post> Get()
        {
            foreach (Xact xact in Xacts.Get())
            {
                Posts.Reset(xact);
                foreach (Post post in Posts.Get())
                {
                    yield return post;
                }
            }
        }
    }
}

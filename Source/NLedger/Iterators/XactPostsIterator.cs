// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Iterators
{
    public class XactPostsIterator : IIterator<Post>
    {
        public XactPostsIterator()
        { }

        public XactPostsIterator(Xact xact)
        {
            Reset(xact);
        }

        public IEnumerable<Post> Posts { get; private set; }

        public void Reset(Xact xact)
        {
            Posts = new List<Post>(xact.Posts);
        }

        public IEnumerable<Post> Get()
        {
            return Posts ?? Enumerable.Empty<Post>();
        }
    }
}

// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Filters
{
    public class SubtotalPostsTests : TestFixture
    {
        [Fact]
        public void SubtotalPosts_Values_ShouldBeSorted()
        {
            SubtotalPosts subtotalPosts = new SubtotalPosts(new IgnorePosts(), null);
            subtotalPosts.Values.Add("PPP", new SubtotalPosts.AcctValue(null));
            subtotalPosts.Values.Add("ZZZ", new SubtotalPosts.AcctValue(null));
            subtotalPosts.Values.Add("AAA", new SubtotalPosts.AcctValue(null));

            Assert.Equal("AAA", subtotalPosts.Values.ElementAt(0).Key);
            Assert.Equal("PPP", subtotalPosts.Values.ElementAt(1).Key);
            Assert.Equal("ZZZ", subtotalPosts.Values.ElementAt(2).Key);
        }
    }
}

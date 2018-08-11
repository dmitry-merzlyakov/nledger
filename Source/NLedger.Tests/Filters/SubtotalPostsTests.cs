// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Accounts;
using NLedger.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Filters
{
    [TestClass]
    public class SubtotalPostsTests : TestFixture
    {
        [TestMethod]
        public void SubtotalPosts_Values_ShouldBeSorted()
        {
            SubtotalPosts subtotalPosts = new SubtotalPosts(new IgnorePosts(), null);
            subtotalPosts.Values.Add("PPP", new SubtotalPosts.AcctValue(null));
            subtotalPosts.Values.Add("ZZZ", new SubtotalPosts.AcctValue(null));
            subtotalPosts.Values.Add("AAA", new SubtotalPosts.AcctValue(null));

            Assert.AreEqual("AAA", subtotalPosts.Values.ElementAt(0).Key);
            Assert.AreEqual("PPP", subtotalPosts.Values.ElementAt(1).Key);
            Assert.AreEqual("ZZZ", subtotalPosts.Values.ElementAt(2).Key);
        }
    }
}

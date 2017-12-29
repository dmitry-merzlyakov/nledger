// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Filters;
using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Filters
{
    [TestClass]
    public class GeneratePostsTests : TestFixture
    {
        [TestMethod]
        public void GeneratePosts_AddPost_ClonesDateIntervalBeforeAdding()
        {
            DateInterval dateInterval = new DateInterval();
            Post post = new Post();
            GeneratePosts generatePosts = new GeneratePosts(new IgnorePosts());

            generatePosts.AddPost(dateInterval, post);
            dateInterval.Range = new DateSpecifierOrRange(new DateSpecifier((Date)DateTime.Now.Date));

            Assert.AreEqual(1, generatePosts.PendingPosts.Count());
            Assert.IsNull(generatePosts.PendingPosts.ElementAt(0).DateInterval.Range);
            Assert.IsNotNull(dateInterval.Range);  // Changes in added interval does not affect the original interval
        }
    }
}

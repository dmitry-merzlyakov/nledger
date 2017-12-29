// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Expressions;
using NLedger.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Filters
{
    [TestClass]
    public class CalcPostsTests : TestFixture
    {
        [TestMethod]
        public void CalcPosts_Handle_ClonesXDataTotalForCalcRunningTotal()
        {
            // Arrange
            CalcPosts calcPosts = new CalcPosts(null, new Expr("amount"), true);
            Account account = new Account(null, "test");
            Post post1 = new Post() { Amount = new Amount(100), Account = account };
            calcPosts.Handle(post1);  // First pass; post1 is LastPost now
            Post post2 = new Post() { Amount = new Amount(200), Account = account };

            // Action
            calcPosts.Handle(post2);

            // Assert
            Assert.AreNotEqual(post1.XData.Total, post2.XData.Total);

            Assert.AreEqual(100, post1.XData.Total.AsLong);
            Assert.AreEqual(300, post2.XData.Total.AsLong);
        }
    }
}

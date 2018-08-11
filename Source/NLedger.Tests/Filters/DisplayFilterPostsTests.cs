// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Chain;
using NLedger.Filters;
using NLedger.Scopus;
using NLedger.Times;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Filters
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class DisplayFilterPostsTests : TestFixture
    {
        [TestMethod]
        public void DisplayFilterPosts_Constructor_PopulatesProperties()
        {
            Report report = new Report(new Session());
            PostHandler handler = new PostHandler(null);
            
            DisplayFilterPosts displayFilterPosts = new DisplayFilterPosts(handler, report, true);

            Assert.AreEqual(handler, displayFilterPosts.Handler);
            Assert.AreEqual(report, displayFilterPosts.Report);
            Assert.AreEqual(report.DisplayAmountHandler.Expr, displayFilterPosts.DisplayAmountExpr);
            Assert.AreEqual(report.DisplayTotalHandler.Expr, displayFilterPosts.DisplayTotalExpr);
            Assert.IsTrue(displayFilterPosts.ShowRounding);
            Assert.IsNotNull(displayFilterPosts.Temps);
        }

        [TestMethod]
        public void DisplayFilterPosts_Constructor_CreatesTempAccounts()
        {
            Report report = new Report(new Session());
            PostHandler handler = new PostHandler(null);

            DisplayFilterPosts displayFilterPosts = new DisplayFilterPosts(handler, report, true);
            Assert.AreEqual("<Adjustment>", displayFilterPosts.RoundingAccount.Name);
            Assert.AreEqual("<Revalued>", displayFilterPosts.RevaluedAccount.Name);            
        }

    }
}

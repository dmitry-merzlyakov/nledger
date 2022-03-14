// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Filters;
using NLedger.Scopus;
using NLedger.Times;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Filters
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class DisplayFilterPostsTests : TestFixture
    {
        [Fact]
        public void DisplayFilterPosts_Constructor_PopulatesProperties()
        {
            Report report = new Report(new Session());
            PostHandler handler = new PostHandler(null);
            
            DisplayFilterPosts displayFilterPosts = new DisplayFilterPosts(handler, report, true);

            Assert.Equal(handler, displayFilterPosts.Handler);
            Assert.Equal(report, displayFilterPosts.Report);
            Assert.Equal(report.DisplayAmountHandler.Expr, displayFilterPosts.DisplayAmountExpr);
            Assert.Equal(report.DisplayTotalHandler.Expr, displayFilterPosts.DisplayTotalExpr);
            Assert.True(displayFilterPosts.ShowRounding);
            Assert.NotNull(displayFilterPosts.Temps);
        }

        [Fact]
        public void DisplayFilterPosts_Constructor_CreatesTempAccounts()
        {
            Report report = new Report(new Session());
            PostHandler handler = new PostHandler(null);

            DisplayFilterPosts displayFilterPosts = new DisplayFilterPosts(handler, report, true);
            Assert.Equal("<Adjustment>", displayFilterPosts.RoundingAccount.Name);
            Assert.Equal("<Revalued>", displayFilterPosts.RevaluedAccount.Name);            
        }

    }
}

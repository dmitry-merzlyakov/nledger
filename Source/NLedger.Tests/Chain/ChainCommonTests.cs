// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Chain;
using NLedger.Filters;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Chain
{
    [TestClass]
    public class ChainCommonTests : TestFixture
    {
        [TestMethod]
        public void ChainCommon_ChainPostHandlers_AddsSubtotalPostsOnlyIfSubTotalHandlerIsHandled()
        {
            // bool forAccountsReport, bool subTotalHandlerHandled, bool equityHandlerHandled
            Assert.IsFalse(TestChainPostHandlersAddsSubtotalPosts(false, false, false));
            Assert.IsFalse(TestChainPostHandlersAddsSubtotalPosts(false, false, true));
            Assert.IsTrue(TestChainPostHandlersAddsSubtotalPosts(false, true, false));  // The only case when SubtotalPosts handler is added
            Assert.IsFalse(TestChainPostHandlersAddsSubtotalPosts(false, true, true));

            Assert.IsFalse(TestChainPostHandlersAddsSubtotalPosts(true, false, false));
            Assert.IsFalse(TestChainPostHandlersAddsSubtotalPosts(true, false, true));
            Assert.IsFalse(TestChainPostHandlersAddsSubtotalPosts(true, true, false));
            Assert.IsFalse(TestChainPostHandlersAddsSubtotalPosts(true, true, true));
        }

        private bool TestChainPostHandlersAddsSubtotalPosts(bool forAccountsReport, bool subTotalHandlerHandled, bool equityHandlerHandled)
        {
            PostHandler baseHandler = new PostHandler(null);

            Report report = new Report(new Session());
            if (subTotalHandlerHandled)
                report.SubTotalHandler.On("whence");
            if (equityHandlerHandled)
                report.EquityHandler.On("whence");

            PostHandler chainPostHandlers = ChainCommon.ChainPostHandlers(baseHandler, report, forAccountsReport);

            while(chainPostHandlers != null)
            {
                if (chainPostHandlers.GetType() == typeof(SubtotalPosts))
                    return true;
                chainPostHandlers = (PostHandler)chainPostHandlers.Handler;
            }
            return false;
        }
    }
}

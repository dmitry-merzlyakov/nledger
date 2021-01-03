// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Filters;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Chain
{
    public class ChainCommonTests : TestFixture
    {
        [Fact]
        public void ChainCommon_ChainPostHandlers_AddsSubtotalPostsOnlyIfSubTotalHandlerIsHandled()
        {
            // bool forAccountsReport, bool subTotalHandlerHandled, bool equityHandlerHandled
            Assert.False(TestChainPostHandlersAddsSubtotalPosts(false, false, false));
            Assert.False(TestChainPostHandlersAddsSubtotalPosts(false, false, true));
            Assert.True(TestChainPostHandlersAddsSubtotalPosts(false, true, false));  // The only case when SubtotalPosts handler is added
            Assert.False(TestChainPostHandlersAddsSubtotalPosts(false, true, true));

            Assert.False(TestChainPostHandlersAddsSubtotalPosts(true, false, false));
            Assert.False(TestChainPostHandlersAddsSubtotalPosts(true, false, true));
            Assert.False(TestChainPostHandlersAddsSubtotalPosts(true, true, false));
            Assert.False(TestChainPostHandlersAddsSubtotalPosts(true, true, true));
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

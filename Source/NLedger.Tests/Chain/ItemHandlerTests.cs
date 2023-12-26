// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Chain
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext)]
    public class ItemHandlerTests : TestFixture
    {
        [Fact]
        public void ItemHandler_Handle_IgnoreNullHandlers()
        {
            var itemHandler = new ItemHandler<string>();
            itemHandler.Handle("string"); // 'Hanlde' does not cause RTE in case of empty inner handler
        }

        [Fact]
        public void ItemHandler_Handle_ChecksSignal()
        {
            var parentHandler = new ItemHandler<string>();
            var itemHandler = new ItemHandler<string>(parentHandler);

            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.INTERRUPTED;
            Assert.Throws<RuntimeError>(() => itemHandler.Handle("string"));  // Expected RuntimeError because of the cancellation signal
        }

        [Fact]
        public void ItemHandler_Handle_NoSignal()
        {
            var parentHandler = new ItemHandler<string>();
            var itemHandler = new ItemHandler<string>(parentHandler);

            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.NONE_CAUGHT; // Default value
            itemHandler.Handle("string");  // Should not cause an exception because of no signal
        }

    }
}

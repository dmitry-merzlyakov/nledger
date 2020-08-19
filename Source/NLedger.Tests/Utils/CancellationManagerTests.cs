// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utils
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext)]
    public class CancellationManagerTests : TestFixture
    {
        [Fact]
        public void CancellationManager_IsCancellationRequested_ReturnTrueIfNoneCaught()
        {
            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.NONE_CAUGHT;
            Assert.False(CancellationManager.IsCancellationRequested);

            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.INTERRUPTED;
            Assert.True(CancellationManager.IsCancellationRequested);

            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.PIPE_CLOSED;
            Assert.True(CancellationManager.IsCancellationRequested);
        }

        [Fact]
        public void CancellationManager_CheckForSignal_DoesNothingIfNoneCaught()
        {
            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.NONE_CAUGHT;
            CancellationManager.CheckForSignal();
        }

        [Fact]
        public void CancellationManager_CheckForSignal_RaisesRTEIfSomethingCaught()
        {
            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.INTERRUPTED;
            Assert.Throws<RuntimeError>(() => CancellationManager.CheckForSignal());
        }

        [Fact]
        public void CancellationManager_DiscardCancellationRequest_RestoresDefaultSignalValue()
        {
            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.INTERRUPTED;
            CancellationManager.DiscardCancellationRequest();
            Assert.Equal(CaughtSignalEnum.NONE_CAUGHT, MainApplicationContext.Current.CancellationSignal);
        }

    }
}

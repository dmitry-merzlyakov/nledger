// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utils
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext)]
    public class CancellationManagerTests : TestFixture
    {
        [TestMethod]
        public void CancellationManager_IsCancellationRequested_ReturnTrueIfNoneCaught()
        {
            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.NONE_CAUGHT;
            Assert.IsFalse(CancellationManager.IsCancellationRequested);

            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.INTERRUPTED;
            Assert.IsTrue(CancellationManager.IsCancellationRequested);

            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.PIPE_CLOSED;
            Assert.IsTrue(CancellationManager.IsCancellationRequested);
        }

        [TestMethod]
        public void CancellationManager_CheckForSignal_DoesNothingIfNoneCaught()
        {
            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.NONE_CAUGHT;
            CancellationManager.CheckForSignal();
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeError))]
        public void CancellationManager_CheckForSignal_RaisesRTEIfSomethingCaught()
        {
            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.INTERRUPTED;
            CancellationManager.CheckForSignal();
        }

        [TestMethod]
        public void CancellationManager_DiscardCancellationRequest_RestoresDefaultSignalValue()
        {
            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.INTERRUPTED;
            CancellationManager.DiscardCancellationRequest();
            Assert.AreEqual(CaughtSignalEnum.NONE_CAUGHT, MainApplicationContext.Current.CancellationSignal);
        }

    }
}

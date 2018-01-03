// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.IntegrationTests.unit
{
    public static class Boost
    {
        public static void CheckThrow<T>(Action action) where T : Exception
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Assert.AreEqual(typeof(T), ex.GetType());
                return;
            }
            Assert.Fail("No exception");
        }

        public static void CheckThrow<T,R>(Func<R> action) where T : Exception
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Assert.AreEqual(typeof(T), ex.GetType());
                return;
            }
            Assert.Fail("No exception");
        }
    }
}

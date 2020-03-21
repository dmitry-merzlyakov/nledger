// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Utility.Rnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utility.Rnd
{
    [TestClass]
    public class DoubleGeneratorTests
    {
        [TestMethod]
        public void DoubleGenerator_Value_ProvidesSeriesOfDoubleValaues()
        {
            Random random = new Random(200);
            int count = 1000;
            DoubleGenerator dblGen = new DoubleGenerator(random, 2, 3);
            for(int i=0; i<count; i++)
            {
                double val = dblGen.Value();
                Assert.IsTrue(val >= 2);
                Assert.IsTrue(val < 3);
            }

        }
    }
}

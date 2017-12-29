// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
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
    public class BoolGeneratorTests
    {
        [TestMethod]
        public void BoolGenerator_Value_ProvidesSequenceOfBooleanValues()
        {
            Random random = new Random(200);
            BoolGenerator boolGen = new BoolGenerator(random);
            Assert.IsFalse(boolGen.Value());
            Assert.IsTrue(boolGen.Value());
            Assert.IsTrue(boolGen.Value());
            Assert.IsFalse(boolGen.Value());
            Assert.IsTrue(boolGen.Value());
        }
    }
}

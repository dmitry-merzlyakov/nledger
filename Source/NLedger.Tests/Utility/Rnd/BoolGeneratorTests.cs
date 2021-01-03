// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility.Rnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility.Rnd
{
    public class BoolGeneratorTests
    {
        [Fact]
        public void BoolGenerator_Value_ProvidesSequenceOfBooleanValues()
        {
            Random random = new Random(200);
            BoolGenerator boolGen = new BoolGenerator(random);
            Assert.False(boolGen.Value());
            Assert.True(boolGen.Value());
            Assert.True(boolGen.Value());
            Assert.False(boolGen.Value());
            Assert.True(boolGen.Value());
        }
    }
}

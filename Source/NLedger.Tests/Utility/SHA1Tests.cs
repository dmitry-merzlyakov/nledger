// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility
{
    public class SHA1Tests
    {
        [Fact]
        public void SHA1_GetHash_Returns40CharHashString()
        {
            Assert.Equal("4352cc5a03f882f6f159b90a518667bde7200351", SHA1.GetHash("2012/01/01,KFC,$10"));
            Assert.Equal("4d04439fba0c7336377d1191c545efd0cfa15437", SHA1.GetHash("2012/01/02,\"REWE SAGT DANKE  123454321\",10€"));
        }
    }
}

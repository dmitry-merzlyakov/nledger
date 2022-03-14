// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
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
    public class InputTextStreamTests
    {
        [Fact]
        public void InputTextStream_ReadInt_ReadsIntegerValue()
        {
            InputTextStream stream = new InputTextStream("12345)");
            char nextChar;
            int result = stream.ReadInt(0, out nextChar);
            Assert.Equal(12345, result);
            Assert.Equal(')', nextChar);
        }

        [Fact]
        public void InputTextStream_ReadInt_ReturnsDefaultValueIfNoDigits()
        {
            InputTextStream stream = new InputTextStream("()");
            char nextChar;
            int result = stream.ReadInt(25, out nextChar);
            Assert.Equal(25, result);
            Assert.Equal('(', nextChar);
        }

    }
}

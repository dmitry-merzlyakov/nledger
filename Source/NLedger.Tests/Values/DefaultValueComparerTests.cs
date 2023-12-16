// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Values
{
    public class DefaultValueComparerTests : TestFixture
    {
        [Fact]
        public void DefaultValueComparer_Compare_TreatsNullAsEmptyValue()
        {
            Assert.Equal(0, DefaultValueComparer.Instance.Compare(null, null));
            Assert.Equal(0, DefaultValueComparer.Instance.Compare(null, Value.Empty));
            Assert.Equal(0, DefaultValueComparer.Instance.Compare(Value.Empty, null));
            Assert.Equal(0, DefaultValueComparer.Instance.Compare(Value.Empty, Value.Empty));
        }

        [Fact]
        public void DefaultValueComparer_Compare_ComparesStrings()
        {
            var valA = Value.StringValue("A");
            var valB = Value.StringValue("B");
            var valA1 = Value.StringValue("A");

            Assert.Equal(0, DefaultValueComparer.Instance.Compare(valA, valA));
            Assert.Equal(-1, DefaultValueComparer.Instance.Compare(valA, valB));
            Assert.Equal(1, DefaultValueComparer.Instance.Compare(valB, valA));
            Assert.Equal(0, DefaultValueComparer.Instance.Compare(valA, valA1));
        }

    }
}

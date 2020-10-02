// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
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
    public class BoostVariantTests
    {
        [Fact]
        public void BoostVariant_IsEmpty_IndicatesWhetherDataIsAssigned()
        {
            BoostVariant val = new BoostVariant();
            Assert.True(val.IsEmpty);

            val.SetValue(55);
            Assert.False(val.IsEmpty);
        }

        [Fact]
        public void BoostVariant_Type_ReturnsTypeOfDataOrNullIfItIsEmpty()
        {
            BoostVariant val = new BoostVariant();
            Assert.Null(val.Type);

            val.SetValue(55);
            Assert.Equal(typeof(int), val.Type);
        }

        [Fact]
        public void BoostVariant_Value_ReturnsAssignedValueOrNull()
        {
            BoostVariant val = new BoostVariant();
            Assert.Null(val.Value);

            val.SetValue(55);
            Assert.Equal(55, val.Value);
        }

        [Fact]
        public void BoostVariant_GetValue_ReturnsTypedValue()
        {
            BoostVariant val = new BoostVariant();
            val.SetValue(55);
            Assert.Equal(55, val.GetValue<int>());
        }

        [Fact]
        public void BoostVariant_GetValue_FailsIfCastIsImpossible()
        {
            BoostVariant val = new BoostVariant();
            val.SetValue(55);
            Assert.Throws<InvalidCastException>(() => val.GetValue<string>());
        }

        [Fact]
        public void BoostVariant_SetValue_PopulatesTypedValue()
        {
            BoostVariant val = new BoostVariant();
            val.SetValue(55);
            val.SetValue("dummy");
            Assert.Equal("dummy", val.Value);
        }

    }
}

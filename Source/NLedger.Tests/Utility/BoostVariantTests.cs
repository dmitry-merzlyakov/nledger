// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utility
{
    [TestClass]
    public class BoostVariantTests
    {
        [TestMethod]
        public void BoostVariant_IsEmpty_IndicatesWhetherDataIsAssigned()
        {
            BoostVariant val = new BoostVariant();
            Assert.IsTrue(val.IsEmpty);

            val.SetValue(55);
            Assert.IsFalse(val.IsEmpty);
        }

        [TestMethod]
        public void BoostVariant_Type_ReturnsTypeOfDataOrNullIfItIsEmpty()
        {
            BoostVariant val = new BoostVariant();
            Assert.IsNull(val.Type);

            val.SetValue(55);
            Assert.AreEqual(typeof(int), val.Type);
        }

        [TestMethod]
        public void BoostVariant_Value_ReturnsAssignedValueOrNull()
        {
            BoostVariant val = new BoostVariant();
            Assert.IsNull(val.Value);

            val.SetValue(55);
            Assert.AreEqual(55, val.Value);
        }

        [TestMethod]
        public void BoostVariant_GetValue_ReturnsTypedValue()
        {
            BoostVariant val = new BoostVariant();
            val.SetValue(55);
            Assert.AreEqual(55, val.GetValue<int>());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void BoostVariant_GetValue_FailsIfCastIsImpossible()
        {
            BoostVariant val = new BoostVariant();
            val.SetValue(55);
            Assert.AreEqual(55, val.GetValue<string>());
        }

        [TestMethod]
        public void BoostVariant_SetValue_PopulatesTypedValue()
        {
            BoostVariant val = new BoostVariant();
            val.SetValue(55);
            val.SetValue("dummy");
            Assert.AreEqual("dummy", val.Value);
        }

    }
}

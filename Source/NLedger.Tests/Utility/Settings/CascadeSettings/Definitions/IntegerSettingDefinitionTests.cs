// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Utility.Settings.CascadeSettings;
using NLedger.Utility.Settings.CascadeSettings.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utility.Settings.CascadeSettings.Definitions
{
    [TestClass]
    public class IntegerSettingDefinitionTests
    {
        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void IntegerSettingDefinition_Constructor_PopulatesProperties()
        {
            var definition = new IntegerSettingDefinition("test", "desc", 100, SettingScopeEnum.Application);
            Assert.AreEqual("test", definition.Name);
            Assert.AreEqual("desc", definition.Description);
            Assert.AreEqual("100", definition.DefaultValue);
            Assert.AreEqual(SettingScopeEnum.Application, definition.Scope);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void IntegerSettingDefinition_ConvertFromString_ReturnsInteger()
        {
            var definition = new IntegerSettingDefinition("test", "desc", 0);
            Assert.AreEqual(0, definition.ConvertFromString("0"));
            Assert.AreEqual(-101, definition.ConvertFromString("-101"));
            Assert.AreEqual(101, definition.ConvertFromString("101"));
        }

    }
}

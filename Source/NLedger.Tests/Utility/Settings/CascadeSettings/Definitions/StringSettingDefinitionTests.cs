// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
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
    public class StringSettingDefinitionTests
    {
        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void StringSettingDefinition_Constructor_PopulatesProperties()
        {
            var definition = new StringSettingDefinition("test", "desc", "def", SettingScopeEnum.Application);
            Assert.AreEqual("test", definition.Name);
            Assert.AreEqual("desc", definition.Description);
            Assert.AreEqual("def", definition.DefaultValue);
            Assert.AreEqual(SettingScopeEnum.Application, definition.Scope);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void StringSettingDefinition_ConvertFromString_ReturnsOriginalValue()
        {
            var definition = new StringSettingDefinition("test", "desc");
            Assert.AreEqual("value", definition.ConvertFromString("value"));
            Assert.AreEqual(String.Empty, definition.ConvertFromString(String.Empty));
        }
    }
}

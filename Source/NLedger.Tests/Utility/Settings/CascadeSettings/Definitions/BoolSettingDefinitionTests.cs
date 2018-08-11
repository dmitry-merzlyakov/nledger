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
    public class BoolSettingDefinitionTests
    {
        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void BoolSettingDefinition_Constructor_PopulatesProperties()
        {
            var definition = new BoolSettingDefinition("test", "desc", true, SettingScopeEnum.Application);
            Assert.AreEqual("test", definition.Name);
            Assert.AreEqual("desc", definition.Description);
            Assert.AreEqual("True", definition.DefaultValue);
            Assert.AreEqual(SettingScopeEnum.Application, definition.Scope);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void BoolSettingDefinition_GetValues_ReturnsAvailableValues()
        {
            var definition = new BoolSettingDefinition("test", "desc", false);
            var values = definition.GetValues();
            Assert.IsNotNull(values);
            Assert.AreEqual(2, values.Count());
            Assert.AreEqual("False", values.ElementAt(0));
            Assert.AreEqual("True", values.ElementAt(1));
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void BoolSettingDefinition_ConvertFromString_PerformsValidConversions()
        {
            var definition = new BoolSettingDefinition("test", "desc", false);
            Assert.IsFalse(definition.ConvertFromString("false"));
            Assert.IsFalse(definition.ConvertFromString("FALSE"));
            Assert.IsTrue(definition.ConvertFromString("true"));
            Assert.IsTrue(definition.ConvertFromString("TRUE"));
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        [ExpectedException(typeof(FormatException))]
        public void BoolSettingDefinition_ConvertFromString_RaisesFormatException()
        {
            var definition = new BoolSettingDefinition("test", "desc", false);
            definition.ConvertFromString("no");
        }
    }
}

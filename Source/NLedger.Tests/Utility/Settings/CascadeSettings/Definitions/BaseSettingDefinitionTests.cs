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
    public class BaseSettingDefinitionTests
    {
        [TestMethod]
        [TestCategory("CascadeSettings")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BaseSettingDefinition_Constructor_DisallowsEmptyNames()
        {
            new TestBaseSettingDefinition("", "desc", 0, SettingScopeEnum.Application);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void BaseSettingDefinition_Constructor_PopulatesProperties()
        {
            var definition = new TestBaseSettingDefinition("name", "desc", 100, SettingScopeEnum.User);
            Assert.AreEqual("name", definition.Name);
            Assert.AreEqual("desc", definition.Description);
            Assert.AreEqual("100", definition.DefaultValue);
            Assert.AreEqual(SettingScopeEnum.User, definition.Scope);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void BaseSettingDefinition_GetValues_ReturnsEmptyCollectionByDefault()
        {
            var definition = new TestBaseSettingDefinition("name", "desc", 100, SettingScopeEnum.User);
            var defaults = definition.GetValues();
            Assert.IsNotNull(defaults);
            Assert.AreEqual(0, defaults.Count());
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void BaseSettingDefinition_ConvertToString_UsesToStringByDefault()
        {
            var definition = new TestBaseSettingDefinition("name", null, 200, SettingScopeEnum.Application);
            Assert.AreEqual("0", definition.ConvertToString(0));
            Assert.AreEqual("-1", definition.ConvertToString(-1));
            Assert.AreEqual("1", definition.ConvertToString(1));
        }

        private class TestBaseSettingDefinition : BaseSettingDefinition<int>
        {
            public TestBaseSettingDefinition(string name, string description, int defaultValue, SettingScopeEnum scope) 
                : base(name, description, defaultValue, scope)
            { }

            public override int ConvertFromString(string stringValue)
            {
                return Int32.Parse(stringValue);
            }
        }
    }
}

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
using NLedger.Utility.Settings.CascadeSettings.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utility.Settings.CascadeSettings.Sources
{
    [TestClass]
    public class DefaultSettingsSourceTests
    {
        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void DefaultSettingsSource_Constructor_CreateDefinitionDictionary()
        {
            var bool1Def = new BoolSettingDefinition("key1", "desc", true);
            var bool2Def = new BoolSettingDefinition("key2", "desc", false);
            var int1Def = new IntegerSettingDefinition("key3", "desc", 1);
            var int2Def = new IntegerSettingDefinition("key4", "desc", 0);

            var source = new DefaultSettingsSource(new ISettingDefinition[] { bool1Def, bool2Def, int1Def, int2Def });
            Assert.IsNotNull(source.SettingDefinitions);
            Assert.AreEqual(4, source.SettingDefinitions.Count);
            Assert.AreEqual(bool1Def, source.SettingDefinitions["key1"]);
            Assert.AreEqual(bool2Def, source.SettingDefinitions["key2"]);
            Assert.AreEqual(int1Def, source.SettingDefinitions["key3"]);
            Assert.AreEqual(int2Def, source.SettingDefinitions["key4"]);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void DefaultSettingsSource_GetValue_ReturnsDefaultValues()
        {
            var bool1Def = new BoolSettingDefinition("key1", "desc", true);
            var bool2Def = new BoolSettingDefinition("key2", "desc", false);
            var int1Def = new IntegerSettingDefinition("key3", "desc", 1);
            var int2Def = new IntegerSettingDefinition("key4", "desc", 0);

            var source = new DefaultSettingsSource(new ISettingDefinition[] { bool1Def, bool2Def, int1Def, int2Def });

            Assert.AreEqual("True", source.GetValue("key1"));
            Assert.AreEqual("False", source.GetValue("key2"));
            Assert.AreEqual("1", source.GetValue("key3"));
            Assert.AreEqual("0", source.GetValue("key4"));
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void DefaultSettingsSource_Scope_Application()
        {
            var source = new DefaultSettingsSource(new ISettingDefinition[] { });
            Assert.AreEqual(SettingScopeEnum.Application, source.Scope);
        }

    }
}

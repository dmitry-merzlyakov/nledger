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
using NLedger.Utility.Settings.CascadeSettings.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utility.Settings.CascadeSettings.Sources
{
    [TestClass]
    [DeploymentItem(@"Utility\Settings\CascadeSettings\Sources\ConfigurationFileSettingsSource.normal.config.xml")]
    [DeploymentItem(@"Utility\Settings\CascadeSettings\Sources\ConfigurationFileSettingsSource.incomplete.config.xml")]
    public class ConfigurationFileSettingsSourceTests
    {
        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void ConfigurationFileSettingsSource_Constructor_PopulatesProperties()
        {
            var source = new ConfigurationFileSettingsSource("test.config");
            Assert.AreEqual("test.config", source.FilePath);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void ConfigurationFileSettingsSource_FileExists_IndicatesWhetherFileExists()
        {
            var source1 = new ConfigurationFileSettingsSource("test.config");
            Assert.IsFalse(source1.FileExists);

            var source2 = new ConfigurationFileSettingsSource("ConfigurationFileSettingsSource.normal.config.xml");
            Assert.IsTrue(source2.FileExists);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void ConfigurationFileSettingsSource_GetValue_ReturnsExistingValues()
        {
            var source = new ConfigurationFileSettingsSource("ConfigurationFileSettingsSource.normal.config.xml");
            Assert.AreEqual("value1", source.GetValue("key1"));
            Assert.AreEqual("value2", source.GetValue("key2"));
            Assert.IsNull(source.GetValue("unknown-key"));
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void ConfigurationFileSettingsSource_GetValue_ReturnsNullIfFileNotExists()
        {
            var source = new ConfigurationFileSettingsSource("unknown.config.xml");
            Assert.IsNull(source.GetValue("key"));
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void ConfigurationFileSettingsSource_GetValue_ReturnsNullIfFileIncomplete()
        {
            var source = new ConfigurationFileSettingsSource("ConfigurationFileSettingsSource.incomplete.config.xml");
            Assert.IsNull(source.GetValue("key"));
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void ConfigurationFileSettingsSource_Scope_User()
        {
            var source = new ConfigurationFileSettingsSource("some");
            Assert.AreEqual(SettingScopeEnum.User, source.Scope);
        }

    }
}

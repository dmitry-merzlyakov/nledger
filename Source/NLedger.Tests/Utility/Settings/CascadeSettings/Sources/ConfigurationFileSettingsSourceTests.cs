// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility.Settings.CascadeSettings;
using NLedger.Utility.Settings.CascadeSettings.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility.Settings.CascadeSettings.Sources
{
    // [DeploymentItem(@"ConfigurationFileSettingsSource.normal.config.xml")]
    // [DeploymentItem(@"ConfigurationFileSettingsSource.incomplete.config.xml")]
    public class ConfigurationFileSettingsSourceTests
    {
        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void ConfigurationFileSettingsSource_Constructor_PopulatesProperties()
        {
            var source = new ConfigurationFileSettingsSource("test.config");
            Assert.Equal("test.config", source.FilePath);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void ConfigurationFileSettingsSource_FileExists_IndicatesWhetherFileExists()
        {
            var source1 = new ConfigurationFileSettingsSource("test.config");
            Assert.False(source1.FileExists);

            var source2 = new ConfigurationFileSettingsSource("ConfigurationFileSettingsSource.normal.config.xml");
            Assert.True(source2.FileExists);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void ConfigurationFileSettingsSource_GetValue_ReturnsExistingValues()
        {
            var source = new ConfigurationFileSettingsSource("ConfigurationFileSettingsSource.normal.config.xml");
            Assert.Equal("value1", source.GetValue("key1"));
            Assert.Equal("value2", source.GetValue("key2"));
            Assert.Null(source.GetValue("unknown-key"));
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void ConfigurationFileSettingsSource_GetValue_ReturnsNullIfFileNotExists()
        {
            var source = new ConfigurationFileSettingsSource("unknown.config.xml");
            Assert.Null(source.GetValue("key"));
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void ConfigurationFileSettingsSource_GetValue_ReturnsNullIfFileIncomplete()
        {
            var source = new ConfigurationFileSettingsSource("ConfigurationFileSettingsSource.incomplete.config.xml");
            Assert.Null(source.GetValue("key"));
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void ConfigurationFileSettingsSource_Scope_User()
        {
            var source = new ConfigurationFileSettingsSource("some");
            Assert.Equal(SettingScopeEnum.User, source.Scope);
        }

    }
}

// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility.Settings.CascadeSettings;
using NLedger.Utility.Settings.CascadeSettings.Definitions;
using NLedger.Utility.Settings.CascadeSettings.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility.Settings.CascadeSettings
{
    public class SettingValueTests
    {
        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void BaseSettingValue_Constructor_TakesContainerAndDefinition()
        {
            var definition = new IntegerSettingDefinition("test", "desc", 0);
            var container = new CascadeSettingsContainer();

            var settingValue = new SettingValue<int>(container, definition);

            Assert.Equal(container, settingValue.SettingsContainer);
            Assert.Equal(definition, settingValue.Definition);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void BaseSettingValue_Value_ReturnsEffectiveValue()
        {
            var definition = new IntegerSettingDefinition("key1", "desc", 0);
            var source = new CustomDataSource();
            var container = new CascadeSettingsContainer();
            container.AddSource(source);
            var settingValue = new SettingValue<int>(container, definition);

            source.Data["key1"] = "100";

            Assert.Equal(100, settingValue.Value);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void BaseSettingValue_Value_RaisesExceptionIfNotConvertible()
        {
            var definition = new IntegerSettingDefinition("key1", "desc", 0);
            var source = new CustomDataSource();
            var container = new CascadeSettingsContainer();
            container.AddSource(source);
            var settingValue = new SettingValue<int>(container, definition);

            source.Data["key1"] = "text";

            Assert.Throws<InvalidOperationException>(() => settingValue.Value);
        }

    }
}

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

namespace NLedger.Tests.Utility.Settings.CascadeSettings
{
    [TestClass]
    public class SettingValueTests
    {
        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void BaseSettingValue_Constructor_TakesContainerAndDefinition()
        {
            var definition = new IntegerSettingDefinition("test", "desc", 0);
            var container = new CascadeSettingsContainer();

            var settingValue = new SettingValue<int>(container, definition);

            Assert.AreEqual(container, settingValue.SettingsContainer);
            Assert.AreEqual(definition, settingValue.Definition);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void BaseSettingValue_Value_ReturnsEffectiveValue()
        {
            var definition = new IntegerSettingDefinition("key1", "desc", 0);
            var source = new CustomDataSource();
            var container = new CascadeSettingsContainer();
            container.AddSource(source);
            var settingValue = new SettingValue<int>(container, definition);

            source.Data["key1"] = "100";

            Assert.AreEqual(100, settingValue.Value);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseSettingValue_Value_RaisesExceptionIfNotConvertible()
        {
            var definition = new IntegerSettingDefinition("key1", "desc", 0);
            var source = new CustomDataSource();
            var container = new CascadeSettingsContainer();
            container.AddSource(source);
            var settingValue = new SettingValue<int>(container, definition);

            source.Data["key1"] = "text";

            Assert.AreEqual(100, settingValue.Value);
        }

    }
}

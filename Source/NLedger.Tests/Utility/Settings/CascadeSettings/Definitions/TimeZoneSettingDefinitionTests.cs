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
    public class TimeZoneSettingDefinitionTests
    {
        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void TimeZoneSettingDefinition_Constructor_PopulatesProperties()
        {
            var definition = new TimeZoneSettingDefinition("test", "desc", TimeZoneInfo.Local, SettingScopeEnum.Application);
            Assert.AreEqual("test", definition.Name);
            Assert.AreEqual("desc", definition.Description);
            Assert.AreEqual(TimeZoneInfo.Local.Id, definition.DefaultValue);
            Assert.AreEqual(SettingScopeEnum.Application, definition.Scope);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void TimeZoneSettingDefinition_GetValues_ReturnsAvailableTimeZones()
        {
            var definition = new TimeZoneSettingDefinition("test", "desc", TimeZoneInfo.Local);
            var values = definition.GetValues();
            Assert.IsNotNull(values);
            Assert.AreNotEqual(0, values.Count());
            foreach (var name in values)
            {
                Assert.IsNotNull(TimeZoneInfo.FindSystemTimeZoneById(name));
            }
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void TimeZoneSettingDefinition_ConvertToString_ReturnsTzId()
        {
            var definition = new TimeZoneSettingDefinition("test", "desc", TimeZoneInfo.Local);

            var tz = TimeZoneInfo.Local;
            var str = definition.ConvertToString(tz);
            Assert.AreEqual(tz.Id, str);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void TimeZoneSettingDefinition_ConvertFromString_ReturnsTimeZoneInfo()
        {
            var definition = new TimeZoneSettingDefinition("test", "desc", TimeZoneInfo.Local);

            var tz = TimeZoneInfo.Local;
            var converted = definition.ConvertFromString(tz.Id);
            Assert.AreEqual(tz, converted);
        }


    }
}

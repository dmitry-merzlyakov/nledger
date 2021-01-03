// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility.Settings.CascadeSettings;
using NLedger.Utility.Settings.CascadeSettings.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility.Settings.CascadeSettings.Definitions
{
    public class TimeZoneSettingDefinitionTests
    {
        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void TimeZoneSettingDefinition_Constructor_PopulatesProperties()
        {
            var definition = new TimeZoneSettingDefinition("test", "desc", TimeZoneInfo.Local, SettingScopeEnum.Application);
            Assert.Equal("test", definition.Name);
            Assert.Equal("desc", definition.Description);
            Assert.Equal(TimeZoneInfo.Local.Id, definition.DefaultValue);
            Assert.Equal(SettingScopeEnum.Application, definition.Scope);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void TimeZoneSettingDefinition_GetValues_ReturnsAvailableTimeZones()
        {
            var definition = new TimeZoneSettingDefinition("test", "desc", TimeZoneInfo.Local);
            var values = definition.GetValues();
            Assert.NotNull(values);
            Assert.NotEmpty(values);
            foreach (var name in values)
            {
                Assert.NotNull(TimeZoneInfo.FindSystemTimeZoneById(name));
            }
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void TimeZoneSettingDefinition_ConvertToString_ReturnsTzId()
        {
            var definition = new TimeZoneSettingDefinition("test", "desc", TimeZoneInfo.Local);

            var tz = TimeZoneInfo.Local;
            var str = definition.ConvertToString(tz);
            Assert.Equal(tz.Id, str);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void TimeZoneSettingDefinition_ConvertFromString_ReturnsTimeZoneInfo()
        {
            var definition = new TimeZoneSettingDefinition("test", "desc", TimeZoneInfo.Local);

            var tz = TimeZoneInfo.Local;
            var converted = definition.ConvertFromString(tz.Id);
            Assert.Equal(tz, converted);
        }


    }
}

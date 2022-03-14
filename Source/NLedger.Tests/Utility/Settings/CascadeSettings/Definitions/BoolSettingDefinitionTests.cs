// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
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
    public class BoolSettingDefinitionTests
    {
        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void BoolSettingDefinition_Constructor_PopulatesProperties()
        {
            var definition = new BoolSettingDefinition("test", "desc", true, SettingScopeEnum.Application);
            Assert.Equal("test", definition.Name);
            Assert.Equal("desc", definition.Description);
            Assert.Equal("True", definition.DefaultValue);
            Assert.Equal(SettingScopeEnum.Application, definition.Scope);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void BoolSettingDefinition_GetValues_ReturnsAvailableValues()
        {
            var definition = new BoolSettingDefinition("test", "desc", false);
            var values = definition.GetValues();
            Assert.NotNull(values);
            Assert.Equal(2, values.Count());
            Assert.Equal("False", values.ElementAt(0));
            Assert.Equal("True", values.ElementAt(1));
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void BoolSettingDefinition_ConvertFromString_PerformsValidConversions()
        {
            var definition = new BoolSettingDefinition("test", "desc", false);
            Assert.False(definition.ConvertFromString("false"));
            Assert.False(definition.ConvertFromString("FALSE"));
            Assert.True(definition.ConvertFromString("true"));
            Assert.True(definition.ConvertFromString("TRUE"));
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void BoolSettingDefinition_ConvertFromString_RaisesFormatException()
        {
            var definition = new BoolSettingDefinition("test", "desc", false);
            Assert.Throws<FormatException>(() => definition.ConvertFromString("no"));
        }
    }
}

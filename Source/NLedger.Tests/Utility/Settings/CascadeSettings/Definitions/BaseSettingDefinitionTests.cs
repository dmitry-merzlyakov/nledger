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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility.Settings.CascadeSettings.Definitions
{
    public class BaseSettingDefinitionTests
    {
        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void BaseSettingDefinition_Constructor_DisallowsEmptyNames()
        {
            Assert.Throws<ArgumentNullException>(() => new TestBaseSettingDefinition("", "desc", 0, SettingScopeEnum.Application));
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void BaseSettingDefinition_Constructor_PopulatesProperties()
        {
            var definition = new TestBaseSettingDefinition("name", "desc", 100, SettingScopeEnum.User);
            Assert.Equal("name", definition.Name);
            Assert.Equal("desc", definition.Description);
            Assert.Equal("100", definition.DefaultValue);
            Assert.Equal(SettingScopeEnum.User, definition.Scope);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void BaseSettingDefinition_GetValues_ReturnsEmptyCollectionByDefault()
        {
            var definition = new TestBaseSettingDefinition("name", "desc", 100, SettingScopeEnum.User);
            var defaults = definition.GetValues();
            Assert.NotNull(defaults);
            Assert.Empty(defaults);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void BaseSettingDefinition_ConvertToString_UsesToStringByDefault()
        {
            var definition = new TestBaseSettingDefinition("name", null, 200, SettingScopeEnum.Application);
            Assert.Equal("0", definition.ConvertToString(0));
            Assert.Equal("-1", definition.ConvertToString(-1));
            Assert.Equal("1", definition.ConvertToString(1));
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

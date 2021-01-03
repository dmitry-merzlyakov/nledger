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
    public class StringSettingDefinitionTests
    {
        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void StringSettingDefinition_Constructor_PopulatesProperties()
        {
            var definition = new StringSettingDefinition("test", "desc", "def", SettingScopeEnum.Application);
            Assert.Equal("test", definition.Name);
            Assert.Equal("desc", definition.Description);
            Assert.Equal("def", definition.DefaultValue);
            Assert.Equal(SettingScopeEnum.Application, definition.Scope);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void StringSettingDefinition_ConvertFromString_ReturnsOriginalValue()
        {
            var definition = new StringSettingDefinition("test", "desc");
            Assert.Equal("value", definition.ConvertFromString("value"));
            Assert.Equal(String.Empty, definition.ConvertFromString(String.Empty));
        }
    }
}

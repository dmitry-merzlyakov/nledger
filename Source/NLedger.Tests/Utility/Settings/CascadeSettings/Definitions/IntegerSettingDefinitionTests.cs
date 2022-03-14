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
    public class IntegerSettingDefinitionTests
    {
        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void IntegerSettingDefinition_Constructor_PopulatesProperties()
        {
            var definition = new IntegerSettingDefinition("test", "desc", 100, SettingScopeEnum.Application);
            Assert.Equal("test", definition.Name);
            Assert.Equal("desc", definition.Description);
            Assert.Equal("100", definition.DefaultValue);
            Assert.Equal(SettingScopeEnum.Application, definition.Scope);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void IntegerSettingDefinition_ConvertFromString_ReturnsInteger()
        {
            var definition = new IntegerSettingDefinition("test", "desc", 0);
            Assert.Equal(0, definition.ConvertFromString("0"));
            Assert.Equal(-101, definition.ConvertFromString("-101"));
            Assert.Equal(101, definition.ConvertFromString("101"));
        }

    }
}

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
    public class EncodingSettingDefinitionTests
    {
        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void EncodingSettingDefinition_Constructor_PopulatesProperties()
        {
            var definition = new EncodingSettingDefinition("test", "desc", Encoding.UTF32, SettingScopeEnum.Application);
            Assert.Equal("test", definition.Name);
            Assert.Equal("desc", definition.Description);
            Assert.Equal(Encoding.UTF32.HeaderName, definition.DefaultValue);
            Assert.Equal(SettingScopeEnum.Application, definition.Scope);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void EncodingSettingDefinition_GetValues_ReturnsPossibleValues()
        {
            var definition = new EncodingSettingDefinition("test", "desc", Encoding.Default);
            var values = definition.GetValues();
            Assert.NotNull(values);
            Assert.NotEmpty(values);
            foreach(var name in values)
            {
                Assert.NotNull(Encoding.GetEncoding(name));
            }
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void EncodingSettingDefinition_ConvertToString_ReturnsHeaderName()
        {
            var definition = new EncodingSettingDefinition("test", "desc", Encoding.Default);

            var encoding = Encoding.UTF32;
            var str = definition.ConvertToString(encoding);
            Assert.Equal(encoding.HeaderName, str);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void EncodingSettingDefinition_ConvertFromString_ReturnsEncoding()
        {
            var definition = new EncodingSettingDefinition("test", "desc", Encoding.Default);

            var encoding = Encoding.UTF32;
            var converted = definition.ConvertFromString(encoding.HeaderName);
            Assert.Equal(encoding, converted);
        }

    }
}

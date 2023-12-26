// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
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
    public class CustomDataSourceTests
    {
        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void CustomDataSource_Constructor_CreateEmptyDictionary()
        {
            var source = new CustomDataSource();
            Assert.NotNull(source.Data);
            Assert.Equal(0, source.Data.Count);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void CustomDataSource_GetValue_ReturnsValuesFromDictionary()
        {
            var source = new CustomDataSource();
            source.Data["key1"] = "value1";
            source.Data["key2"] = "value2";
            Assert.Equal("value1", source.GetValue("key1"));
            Assert.Equal("value2", source.GetValue("key2"));
            Assert.Null(source.GetValue("key3"));
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void CustomDataSource_Scope_User()
        {
            var source = new CustomDataSource();
            Assert.Equal(SettingScopeEnum.User, source.Scope);
        }

    }
}

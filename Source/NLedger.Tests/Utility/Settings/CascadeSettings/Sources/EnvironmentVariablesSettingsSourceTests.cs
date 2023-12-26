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
    public class EnvironmentVariablesSettingsSourceTests
    {
        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void EnvironmentVariablesSettingsSource_Constructor_PopulatesEnvironmentVariables()
        {
            var source = new EnvironmentVariablesSettingsSource();
            Assert.NotNull(source.EnvironmentVariables);
            Assert.Equal(Environment.GetEnvironmentVariables().Count, source.EnvironmentVariables.Count);
            foreach (var kv in source.EnvironmentVariables)
                Assert.Equal(Environment.GetEnvironmentVariable(kv.Key) ?? String.Empty, kv.Value);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void EnvironmentVariablesSettingsSource_Constructor_CanWorkWithEmptyPrefix()
        {
            var source = new EnvironmentVariablesSettingsSource();
            Assert.NotEqual(0, source.EnvironmentVariables.Count);
            Assert.Equal(source.EffectiveVariables.Count, source.EnvironmentVariables.Count);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void EnvironmentVariablesSettingsSource_Constructor_ManagesPrefix()
        {
            Environment.SetEnvironmentVariable("envvarTestVariable", "test-value");

            var source = new EnvironmentVariablesSettingsSource("envvar");
            Assert.Equal(1, source.EffectiveVariables.Count);
            Assert.Equal("TestVariable", source.EffectiveVariables.Keys.First());
            Assert.Equal("test-value", source.EffectiveVariables.Values.First());
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void EnvironmentVariablesSettingsSource_Scope_Application()
        {
            var source = new EnvironmentVariablesSettingsSource();
            Assert.Equal(SettingScopeEnum.Application, source.Scope);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void EnvironmentVariablesSettingsSource_GetValue_DealsWithEffectiveVariables()
        {
            Environment.SetEnvironmentVariable("envvarTestVariable", "test-value");
            var source = new EnvironmentVariablesSettingsSource("envvar");
            Assert.Equal("test-value", source.GetValue("TestVariable"));
            Assert.Null(source.GetValue("ProgramData"));  // This env variable always exists but filtered out by the prefix
        }

    }
}

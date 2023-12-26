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

namespace NLedger.Tests.Utility.Settings.CascadeSettings
{
    public class CascadeSettingsContainerTests
    {
        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void CascadeSettingsContainer_Constructor_CreatesEmptySources()
        {
            CascadeSettingsContainer container = new CascadeSettingsContainer();
            Assert.NotNull(container.Sources);
            Assert.Equal(0, container.Sources.Count);
            Assert.Equal(SettingScopeEnum.User, container.EffectiveScope);
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void CascadeSettingsContainer_GetEffectiveValue_ReturnsNullIfNoSources()
        {
            CascadeSettingsContainer container = new CascadeSettingsContainer();
            Assert.Null(container.GetEffectiveValue("somekey"));
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void CascadeSettingsContainer_GetEffectiveValue_ReturnsLastValueInSourcesOrder()
        {
            CascadeSettingsContainer container = new CascadeSettingsContainer();
            var sourceA = container.AddSource(new CustomDataSource());
            var sourceB = container.AddSource(new CustomDataSource());

            sourceA.Data["key1"] = "value1A";
            sourceB.Data["key1"] = "value1B";

            sourceA.Data["key2"] = "value2A";

            sourceB.Data["key3"] = "value3B";

            Assert.Equal("value1B", container.GetEffectiveValue("key1"));
            Assert.Equal("value2A", container.GetEffectiveValue("key2"));
            Assert.Equal("value3B", container.GetEffectiveValue("key3"));
            Assert.Null(container.GetEffectiveValue("key4"));     // unknown key
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void CascadeSettingsContainer_GetEffectiveValue_EvaluatesSettingScope()
        {
            CascadeSettingsContainer container = new CascadeSettingsContainer();
            var sourceA = container.AddSource(new CustomDataSource());
            var sourceB = container.AddSource(new CustomDataSource());

            sourceA.Data["key1"] = "value1A";
            sourceB.Data["key1"] = "value2A";

            sourceA.Scope = SettingScopeEnum.Application;
            sourceB.Scope = SettingScopeEnum.User;

            Assert.Equal("value2A", container.GetEffectiveValue("key1"));
            Assert.Equal("value2A", container.GetEffectiveValue("key1", SettingScopeEnum.User));
            Assert.Equal("value1A", container.GetEffectiveValue("key1", SettingScopeEnum.Application));
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void CascadeSettingsContainer_GetEffectiveValue_EvaluatesScopeLimit()
        {
            CascadeSettingsContainer container = new CascadeSettingsContainer();
            var sourceA = container.AddSource(new CustomDataSource());
            var sourceB = container.AddSource(new CustomDataSource());

            sourceA.Data["key1"] = "value1A";
            sourceB.Data["key1"] = "value2A";

            sourceA.Scope = SettingScopeEnum.Application;
            sourceB.Scope = SettingScopeEnum.User;

            container.EffectiveScope = SettingScopeEnum.User;
            Assert.Equal("value2A", container.GetEffectiveValue("key1"));

            container.EffectiveScope = SettingScopeEnum.Application;
            Assert.Equal("value1A", container.GetEffectiveValue("key1"));
        }

        [Fact]
        [Trait("Category", "CascadeSettings")]
        public void CascadeSettingsContainer_AddSource_AddsToSourcesAndReturnsInstance()
        {
            var source = new CustomDataSource();
            var container = new CascadeSettingsContainer();

            var source1 = container.AddSource(source);

            Assert.True(container.Sources.Contains(source));
            Assert.Equal(source, source1);
        }

    }
}

// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Utility.Settings.CascadeSettings;
using NLedger.Utility.Settings.CascadeSettings.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utility.Settings.CascadeSettings
{
    [TestClass]
    public class CascadeSettingsContainerTests
    {
        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void CascadeSettingsContainer_Constructor_CreatesEmptySources()
        {
            CascadeSettingsContainer container = new CascadeSettingsContainer();
            Assert.IsNotNull(container.Sources);
            Assert.AreEqual(0, container.Sources.Count);
            Assert.AreEqual(SettingScopeEnum.User, container.EffectiveScope);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void CascadeSettingsContainer_GetEffectiveValue_ReturnsNullIfNoSources()
        {
            CascadeSettingsContainer container = new CascadeSettingsContainer();
            Assert.IsNull(container.GetEffectiveValue("somekey"));
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void CascadeSettingsContainer_GetEffectiveValue_ReturnsLastValueInSourcesOrder()
        {
            CascadeSettingsContainer container = new CascadeSettingsContainer();
            var sourceA = container.AddSource(new CustomDataSource());
            var sourceB = container.AddSource(new CustomDataSource());

            sourceA.Data["key1"] = "value1A";
            sourceB.Data["key1"] = "value1B";

            sourceA.Data["key2"] = "value2A";

            sourceB.Data["key3"] = "value3B";

            Assert.AreEqual("value1B", container.GetEffectiveValue("key1"));
            Assert.AreEqual("value2A", container.GetEffectiveValue("key2"));
            Assert.AreEqual("value3B", container.GetEffectiveValue("key3"));
            Assert.IsNull(container.GetEffectiveValue("key4"));     // unknown key
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void CascadeSettingsContainer_GetEffectiveValue_EvaluatesSettingScope()
        {
            CascadeSettingsContainer container = new CascadeSettingsContainer();
            var sourceA = container.AddSource(new CustomDataSource());
            var sourceB = container.AddSource(new CustomDataSource());

            sourceA.Data["key1"] = "value1A";
            sourceB.Data["key1"] = "value2A";

            sourceA.Scope = SettingScopeEnum.Application;
            sourceB.Scope = SettingScopeEnum.User;

            Assert.AreEqual("value2A", container.GetEffectiveValue("key1"));
            Assert.AreEqual("value2A", container.GetEffectiveValue("key1", SettingScopeEnum.User));
            Assert.AreEqual("value1A", container.GetEffectiveValue("key1", SettingScopeEnum.Application));
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
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
            Assert.AreEqual("value2A", container.GetEffectiveValue("key1"));

            container.EffectiveScope = SettingScopeEnum.Application;
            Assert.AreEqual("value1A", container.GetEffectiveValue("key1"));
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void CascadeSettingsContainer_AddSource_AddsToSourcesAndReturnsInstance()
        {
            var source = new CustomDataSource();
            var container = new CascadeSettingsContainer();

            var source1 = container.AddSource(source);

            Assert.IsTrue(container.Sources.Contains(source));
            Assert.AreEqual(source, source1);
        }

    }
}

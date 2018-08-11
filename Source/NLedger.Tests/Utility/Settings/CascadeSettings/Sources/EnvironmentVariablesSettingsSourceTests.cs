// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
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

namespace NLedger.Tests.Utility.Settings.CascadeSettings.Sources
{
    [TestClass]
    public class EnvironmentVariablesSettingsSourceTests
    {
        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void EnvironmentVariablesSettingsSource_Constructor_PopulatesEnvironmentVariables()
        {
            var source = new EnvironmentVariablesSettingsSource();
            Assert.IsNotNull(source.EnvironmentVariables);
            Assert.AreEqual(Environment.GetEnvironmentVariables().Count, source.EnvironmentVariables.Count);
            foreach (var kv in source.EnvironmentVariables)
                Assert.AreEqual(Environment.GetEnvironmentVariable(kv.Key) ?? String.Empty, kv.Value);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void EnvironmentVariablesSettingsSource_Constructor_CanWorkWithEmptyPrefix()
        {
            var source = new EnvironmentVariablesSettingsSource();
            Assert.AreNotEqual(0, source.EnvironmentVariables.Count);
            Assert.AreEqual(source.EffectiveVariables.Count, source.EnvironmentVariables.Count);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void EnvironmentVariablesSettingsSource_Constructor_ManagesPrefix()
        {
            Environment.SetEnvironmentVariable("envvarTestVariable", "test-value");

            var source = new EnvironmentVariablesSettingsSource("envvar");
            Assert.AreEqual(1, source.EffectiveVariables.Count);
            Assert.AreEqual("TestVariable", source.EffectiveVariables.Keys.First());
            Assert.AreEqual("test-value", source.EffectiveVariables.Values.First());
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void EnvironmentVariablesSettingsSource_Scope_Application()
        {
            var source = new EnvironmentVariablesSettingsSource();
            Assert.AreEqual(SettingScopeEnum.Application, source.Scope);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void EnvironmentVariablesSettingsSource_GetValue_DealsWithEffectiveVariables()
        {
            Environment.SetEnvironmentVariable("envvarTestVariable", "test-value");
            var source = new EnvironmentVariablesSettingsSource("envvar");
            Assert.AreEqual("test-value", source.GetValue("TestVariable"));
            Assert.IsNull(source.GetValue("ProgramData"));  // This env variable always exists but filtered out by the prefix
        }

    }
}

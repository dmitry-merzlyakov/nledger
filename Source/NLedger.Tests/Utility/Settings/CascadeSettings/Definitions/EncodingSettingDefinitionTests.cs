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
using NLedger.Utility.Settings.CascadeSettings.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utility.Settings.CascadeSettings.Definitions
{
    [TestClass]
    public class EncodingSettingDefinitionTests
    {
        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void EncodingSettingDefinition_Constructor_PopulatesProperties()
        {
            var definition = new EncodingSettingDefinition("test", "desc", Encoding.UTF32, SettingScopeEnum.Application);
            Assert.AreEqual("test", definition.Name);
            Assert.AreEqual("desc", definition.Description);
            Assert.AreEqual(Encoding.UTF32.HeaderName, definition.DefaultValue);
            Assert.AreEqual(SettingScopeEnum.Application, definition.Scope);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void EncodingSettingDefinition_GetValues_ReturnsPossibleValues()
        {
            var definition = new EncodingSettingDefinition("test", "desc", Encoding.Default);
            var values = definition.GetValues();
            Assert.IsNotNull(values);
            Assert.AreNotEqual(0, values.Count());
            foreach(var name in values)
            {
                Assert.IsNotNull(Encoding.GetEncoding(name));
            }
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void EncodingSettingDefinition_ConvertToString_ReturnsHeaderName()
        {
            var definition = new EncodingSettingDefinition("test", "desc", Encoding.Default);

            var encoding = Encoding.UTF32;
            var str = definition.ConvertToString(encoding);
            Assert.AreEqual(encoding.HeaderName, str);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void EncodingSettingDefinition_ConvertFromString_ReturnsEncoding()
        {
            var definition = new EncodingSettingDefinition("test", "desc", Encoding.Default);

            var encoding = Encoding.UTF32;
            var converted = definition.ConvertFromString(encoding.HeaderName);
            Assert.AreEqual(encoding, converted);
        }

    }
}

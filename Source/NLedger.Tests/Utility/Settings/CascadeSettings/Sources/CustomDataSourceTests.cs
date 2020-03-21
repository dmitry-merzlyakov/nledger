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

namespace NLedger.Tests.Utility.Settings.CascadeSettings.Sources
{
    [TestClass]
    public class CustomDataSourceTests
    {
        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void CustomDataSource_Constructor_CreateEmptyDictionary()
        {
            var source = new CustomDataSource();
            Assert.IsNotNull(source.Data);
            Assert.AreEqual(0, source.Data.Count);
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void CustomDataSource_GetValue_ReturnsValuesFromDictionary()
        {
            var source = new CustomDataSource();
            source.Data["key1"] = "value1";
            source.Data["key2"] = "value2";
            Assert.AreEqual("value1", source.GetValue("key1"));
            Assert.AreEqual("value2", source.GetValue("key2"));
            Assert.IsNull(source.GetValue("key3"));
        }

        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void CustomDataSource_Scope_User()
        {
            var source = new CustomDataSource();
            Assert.AreEqual(SettingScopeEnum.User, source.Scope);
        }

    }
}

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
    public class SystemConfigurationSettingsSourceTests
    {
        [TestMethod]
        [TestCategory("CascadeSettings")]
        public void SystemConfigurationSettingsSource_Scope_Application()
        {
            var source = new SystemConfigurationSettingsSource();
            Assert.AreEqual(SettingScopeEnum.Application, source.Scope);
        }
    }
}

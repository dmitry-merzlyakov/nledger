// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace NLedger.IntegrationTests
{
    [TestClass]
    [DeploymentItem(@"test\nledger", @"test\nledger")]
    public class TestSet1_test_nledger
    {
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_nledger_opt_download()
        {
            new TestRunner(@"test\nledger\opt-download.test").Run();
        }
		
    }
}

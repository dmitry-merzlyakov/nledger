// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Xunit;
using System;
namespace NLedger.IntegrationTests
{
    public class TestSet_test_nledger
    {
        public TestSet_test_nledger()
        {
            Extensibility.Python.Platform.PythonConnector.Current.KeepAlive = false;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_nledger_gh_issues_24()
        {
            new TestRunner(@"test/nledger/gh-issues-24.test").Run();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_nledger_gh_issues_5()
        {
            new TestRunner(@"test/nledger/gh-issues-5.test").Run();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_nledger_gh_issues_7()
        {
            new TestRunner(@"test/nledger/gh-issues-7.test").Run();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_nledger_nl_baseline_net_1()
        {
            new TestRunner(@"test/nledger/nl-baseline-net-1.test").Run("dotnet");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_nledger_nl_baseline_net_2()
        {
            new TestRunner(@"test/nledger/nl-baseline-net-2.test").Run("dotnet");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_nledger_nl_baseline_net_3()
        {
            new TestRunner(@"test/nledger/nl-baseline-net-3.test").Run("dotnet");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_nledger_nl_baseline_net_4()
        {
            new TestRunner(@"test/nledger/nl-baseline-net-4.test").Run("dotnet");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_nledger_nl_issues_1()
        {
            new TestRunner(@"test/nledger/nl-issues-1.test").Run();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_nledger_opt_download()
        {
            new TestRunner(@"test/nledger/opt-download.test").Run();
        }

    }
}

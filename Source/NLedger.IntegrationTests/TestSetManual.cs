// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Xunit;
using System;
namespace NLedger.IntegrationTests
{
    public class TestSet_test_manual
    {
        public TestSet_test_manual()
        {
            Extensibility.Python.Platform.PythonConnector.Current.KeepAlive = false;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_manual_transaction_codes_1()
        {
            new TestRunner(@"test/manual/transaction-codes-1.test").Run();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_manual_transaction_codes_2()
        {
            new TestRunner(@"test/manual/transaction-codes-2.test").Run();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_manual_transaction_notes_1()
        {
            new TestRunner(@"test/manual/transaction-notes-1.test").Run();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_manual_transaction_notes_2()
        {
            new TestRunner(@"test/manual/transaction-notes-2.test").Run();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_manual_transaction_notes_3()
        {
            new TestRunner(@"test/manual/transaction-notes-3.test").Run();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_manual_transaction_notes_4()
        {
            new TestRunner(@"test/manual/transaction-notes-4.test").Run();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_manual_transaction_status_1()
        {
            new TestRunner(@"test/manual/transaction-status-1.test").Run();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_manual_transaction_status_2()
        {
            new TestRunner(@"test/manual/transaction-status-2.test").Run();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_manual_transaction_status_3()
        {
            new TestRunner(@"test/manual/transaction-status-3.test").Run();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_manual_transaction_status_4()
        {
            new TestRunner(@"test/manual/transaction-status-4.test").Run();
        }

    }
}

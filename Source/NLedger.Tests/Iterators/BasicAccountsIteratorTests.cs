// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Iterators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Iterators
{
    public class BasicAccountsIteratorTests : TestFixture
    {
        [Fact]
        public void BasicAccountsIterator_Constructor_RequiresAnAccount()
        {
            Account account = new Account(null, "test-account");
            BasicAccountsIterator iterator = new BasicAccountsIterator(account);
            Assert.Equal(account, iterator.Account);
        }

        [Fact]
        public void BasicAccountsIterator_Get_FailsForNullAccount()
        {
            BasicAccountsIterator iterator = new BasicAccountsIterator(null);
            Assert.Throws<ArgumentNullException>(() => iterator.Get().ToList());
        }

        [Fact]
        public void BasicAccountsIterator_Get_ReturnsSingleItemForSingleAccount()
        {
            Account account = new Account(null, "test-account");
            BasicAccountsIterator iterator = new BasicAccountsIterator(account);
            List<Account> result = iterator.Get().ToList();
            Assert.Single(result);
            Assert.Equal(account, result[0]);
        }

        [Fact]
        public void BasicAccountsIterator_Get_ReturnsAllAccountsInTree()
        {
            Account account1 = new Account();
            Account account11 = new Account(account1, "test-account-11");
            Account account12 = new Account(account1, "test-account-12");
            account1.AddAccount(account11);
            account1.AddAccount(account12);

            BasicAccountsIterator iterator = new BasicAccountsIterator(account1);
            List<Account> result = iterator.Get().ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal(account1, result[0]);
            Assert.Equal(account11, result[1]);
            Assert.Equal(account12, result[2]);
        }

        [Fact]
        public void BasicAccountsIterator_Get_ReturnsAllAccountsInThreeLevelTree()
        {
            Account account1 = new Account();
            Account account11 = new Account(account1, "test-account-11");
            Account account12 = new Account(account1, "test-account-12");
            account1.AddAccount(account11);
            account1.AddAccount(account12);
            Account account111 = new Account(account11, "test-account-111");
            Account account121 = new Account(account12, "test-account-121");
            account11.AddAccount(account111);
            account12.AddAccount(account121);

            BasicAccountsIterator iterator = new BasicAccountsIterator(account1);
            List<Account> result = iterator.Get().ToList();

            Assert.Equal(5, result.Count);
            Assert.Equal(account1, result[0]);
            Assert.Equal(account11, result[1]);
            Assert.Equal(account111, result[2]);
            Assert.Equal(account12, result[3]);
            Assert.Equal(account121, result[4]);
        }

    }
}

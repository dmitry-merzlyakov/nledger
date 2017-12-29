// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Accounts;
using NLedger.Iterators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Iterators
{
    [TestClass]
    public class BasicAccountsIteratorTests : TestFixture
    {
        [TestMethod]
        public void BasicAccountsIterator_Constructor_RequiresAnAccount()
        {
            Account account = new Account(null, "test-account");
            BasicAccountsIterator iterator = new BasicAccountsIterator(account);
            Assert.AreEqual(account, iterator.Account);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BasicAccountsIterator_Get_FailsForNullAccount()
        {
            BasicAccountsIterator iterator = new BasicAccountsIterator(null);
            List<Account> result = iterator.Get().ToList();
        }

        [TestMethod]
        public void BasicAccountsIterator_Get_ReturnsSingleItemForSingleAccount()
        {
            Account account = new Account(null, "test-account");
            BasicAccountsIterator iterator = new BasicAccountsIterator(account);
            List<Account> result = iterator.Get().ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(account, result[0]);
        }

        [TestMethod]
        public void BasicAccountsIterator_Get_ReturnsAllAccountsInTree()
        {
            Account account1 = new Account();
            Account account11 = new Account(account1, "test-account-11");
            Account account12 = new Account(account1, "test-account-12");
            account1.AddAccount(account11);
            account1.AddAccount(account12);

            BasicAccountsIterator iterator = new BasicAccountsIterator(account1);
            List<Account> result = iterator.Get().ToList();

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(account1, result[0]);
            Assert.AreEqual(account11, result[1]);
            Assert.AreEqual(account12, result[2]);
        }

        [TestMethod]
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

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(account1, result[0]);
            Assert.AreEqual(account11, result[1]);
            Assert.AreEqual(account111, result[2]);
            Assert.AreEqual(account12, result[3]);
            Assert.AreEqual(account121, result[4]);
        }

    }
}

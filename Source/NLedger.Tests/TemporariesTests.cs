// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Accounts;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests
{
    [TestClass]
    public class TemporariesTests
    {
        [TestMethod]
        public void Temporaries_CopyPost_ClonesOriginalPost()
        {
            var temps = new Temporaries();
            var xact = new Xact();
            var origin = new Post() { Account = new Account() };

            var tempPost = temps.CopyPost(origin, xact); // CopyPost adds ITEM_TEMP flag

            Assert.IsFalse(origin.Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP));
            Assert.IsTrue(tempPost.Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP));

            Assert.AreEqual(temps.LastPost, tempPost);
            Assert.AreNotEqual(temps.LastPost, origin);
        }

        [TestMethod]
        public void Temporaries_CreateAccount_CreatesTempAccount()
        {
            var temps = new Temporaries();

            var tempAccount = temps.CreateAccount("temp-account");

            Assert.IsTrue(tempAccount.IsTempAccount);
            Assert.AreEqual("temp-account", tempAccount.Name);
            Assert.AreEqual(temps.LastAccount, tempAccount);
        }

        [TestMethod]
        public void Temporaries_Dispose_ClearsTemporaries()
        {
            var temps = new Temporaries();
            var xact = new Xact();
            var origin = new Post() { Account = new Account() };

            temps.CreateAccount("temp-account1");
            temps.CreateAccount("temp-account2");
            temps.CopyPost(origin, xact);

            Assert.IsNotNull(temps.LastAccount);
            Assert.IsNotNull(temps.LastPost);

            temps.Dispose();

            Assert.IsNull(temps.LastAccount);
            Assert.IsNull(temps.LastPost);

            temps.Dispose(); // Note - Dispose is tolerant to multiple calls
        }

    }
}

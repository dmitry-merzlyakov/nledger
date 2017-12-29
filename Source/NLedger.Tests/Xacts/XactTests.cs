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
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Xacts
{
    [TestClass]
    public class XactTests : TestFixture
    {
        [TestMethod]
        public void Xact_AddPost_PopulatesXactInPost()
        {
            Xact xact = new Xact();
            Post post = new Post();

            xact.AddPost(post);

            Assert.AreEqual(xact, post.Xact);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Xact_Detach_FailsIfThereIsTempPost()
        {
            Account account = new Account();
            Xact xact = new Xact();
            Post post = new Post(account);
            post.Flags = SupportsFlagsEnum.ITEM_TEMP;
            xact.AddPost(post);

            xact.Detach();
        }

        [TestMethod]
        public void Xact_Detach_RemovesPostsFromAccounts()
        {
            Account account = new Account();
            Xact xact = new Xact();
            Post post = new Post(account);
            xact.AddPost(post);
            account.Posts.Add(post);

            xact.Detach();

            Assert.AreEqual(0, account.Posts.Count);  // Post has been removed
        }

        [TestMethod]
        public void Xact_Detach_DoesNothingIf_ITEM_TEMP()
        {
            Account account = new Account();

            Xact xact = new Xact();
            xact.Flags = SupportsFlagsEnum.ITEM_TEMP;

            Post post = new Post(account);
            post.Flags = SupportsFlagsEnum.ITEM_TEMP;

            xact.AddPost(post);
            account.Posts.Add(post);

            xact.Detach();

            Assert.AreEqual(1, account.Posts.Count);  // Post has not been removed
        }
    }
}

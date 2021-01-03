// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Utility;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Xacts
{
    public class XactTests : TestFixture
    {
        [Fact]
        public void Xact_AddPost_PopulatesXactInPost()
        {
            Xact xact = new Xact();
            Post post = new Post();

            xact.AddPost(post);

            Assert.Equal(xact, post.Xact);
        }

        [Fact]
        public void Xact_Detach_FailsIfThereIsTempPost()
        {
            Account account = new Account();
            Xact xact = new Xact();
            Post post = new Post(account);
            post.Flags = SupportsFlagsEnum.ITEM_TEMP;
            xact.AddPost(post);

            Assert.Throws<InvalidOperationException>(() => xact.Detach());
        }

        [Fact]
        public void Xact_Detach_RemovesPostsFromAccounts()
        {
            Account account = new Account();
            Xact xact = new Xact();
            Post post = new Post(account);
            xact.AddPost(post);
            account.Posts.Add(post);

            xact.Detach();

            Assert.Equal(0, account.Posts.Count);  // Post has been removed
        }

        [Fact]
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

            Assert.Equal(1, account.Posts.Count);  // Post has not been removed
        }

        [Fact]
        public void Xact_Valid_FailsIfNoDateValue()
        {
            Xact xact = new Xact();
            Assert.False(xact.Valid());

            xact.Date = (Date)DateTime.Today;
            Assert.True(xact.Valid());
        }
    }
}

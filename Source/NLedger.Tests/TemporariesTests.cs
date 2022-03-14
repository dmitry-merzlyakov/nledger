// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests
{
    public class TemporariesTests
    {
        [Fact]
        public void Temporaries_CopyPost_ClonesOriginalPost()
        {
            var temps = new Temporaries();
            var xact = new Xact();
            var origin = new Post() { Account = new Account() };

            var tempPost = temps.CopyPost(origin, xact); // CopyPost adds ITEM_TEMP flag

            Assert.False(origin.Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP));
            Assert.True(tempPost.Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP));

            Assert.Equal(temps.LastPost, tempPost);
            Assert.NotEqual(temps.LastPost, origin);
        }

        [Fact]
        public void Temporaries_CreateAccount_CreatesTempAccount()
        {
            var temps = new Temporaries();

            var tempAccount = temps.CreateAccount("temp-account");

            Assert.True(tempAccount.IsTempAccount);
            Assert.Equal("temp-account", tempAccount.Name);
            Assert.Equal(temps.LastAccount, tempAccount);
        }

        [Fact]
        public void Temporaries_Dispose_ClearsTemporaries()
        {
            var temps = new Temporaries();
            var xact = new Xact();
            var origin = new Post() { Account = new Account() };

            temps.CreateAccount("temp-account1");
            temps.CreateAccount("temp-account2");
            temps.CopyPost(origin, xact);

            Assert.NotNull(temps.LastAccount);
            Assert.NotNull(temps.LastPost);

            temps.Dispose();

            Assert.Null(temps.LastAccount);
            Assert.Null(temps.LastPost);

            temps.Dispose(); // Note - Dispose is tolerant to multiple calls
        }

    }
}

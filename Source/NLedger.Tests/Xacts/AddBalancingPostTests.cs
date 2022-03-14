// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Items;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Xacts
{
    public class AddBalancingPostTests : TestFixture
    {
        [Fact]
        public void AddBalancingPost_Constructor_PopulatesXactAndPost()
        {
            Xact xact = new Xact();
            Post post = new Post();

            AddBalancingPost addBalancingPost = new AddBalancingPost(xact, post);

            Assert.True(addBalancingPost.First);
            Assert.Equal(xact, addBalancingPost.Xact);
            Assert.Equal(post, addBalancingPost.NullPost);
        }

        [Fact]
        public void AddBalancingPost_Constructor_CopiesFromAnotherItem()
        {
            Xact xact = new Xact();
            Post post = new Post();
            AddBalancingPost addBalancingPost1 = new AddBalancingPost(xact, post);

            AddBalancingPost addBalancingPost2 = new AddBalancingPost(addBalancingPost1);

            Assert.True(addBalancingPost2.First);
            Assert.Equal(xact, addBalancingPost2.Xact);
            Assert.Equal(post, addBalancingPost2.NullPost);
        }

        [Fact]
        public void AddBalancingPost_Amount_AddsNegatedAmountIfFirst()
        {
            Post post = new Post();
            AddBalancingPost addBalancingPost1 = new AddBalancingPost(new Xact(), post);
            Amount amount = new Amount(10);

            addBalancingPost1.Amount(amount);

            Assert.False(addBalancingPost1.First);
            Assert.Equal(-10, addBalancingPost1.NullPost.Amount.Quantity.ToLong());
            Assert.True(addBalancingPost1.NullPost.Flags.HasFlag(SupportsFlagsEnum.POST_CALCULATED));
        }

        [Fact]
        public void AddBalancingPost_Amount_UpdatesPostAmountIfNotFirst()
        {
            Post post = new Post() { State = ItemStateEnum.Pending };
            AddBalancingPost addBalancingPost1 = new AddBalancingPost(new Xact(), post);
            Amount amount = new Amount(10);
            addBalancingPost1.Amount(amount);

            Amount amount1 = new Amount(5);
            addBalancingPost1.Amount(amount1);

            Post result = addBalancingPost1.Xact.Posts.First();
            Assert.Equal(-5, result.Amount.Quantity.ToLong());
            Assert.True(result.Flags.HasFlag(SupportsFlagsEnum.POST_CALCULATED));
            Assert.True(result.Flags.HasFlag(SupportsFlagsEnum.ITEM_GENERATED));
            Assert.Equal(post.State, addBalancingPost1.NullPost.State);
        }

    }
}

// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Commodities;
using NLedger.Journals;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests
{
    public class PostTests : TestFixture
    {
        [Fact]
        public void Post_ExtendPost_DoesNotExtendCommodityIfNoValueTag()
        {
            Commodity comm = CommodityPool.Current.FindOrCreate("test-comm-post");            
            Post post = new Post() { Amount = new Amount(100, comm), Account = new Account(null, "test") };

            Post.ExtendPost(post, new Journal());

            Assert.Equal(comm, post.Amount.Commodity);
        }

        [Fact]
        public void Post_Constructor_ClonesAmount()
        {
            Post post1 = new Post() { Amount = new Amount(100) };

            Post newPost = new Post(post1);
            newPost.Amount.Multiply(new Amount(10));

            Assert.Equal("100", post1.Amount.ToString());
            Assert.Equal("1000", newPost.Amount.ToString());
        }

        [Fact]
        public void Post_Constructor_ClonesXData()
        {
            Post post1 = new Post();
            Post newPost = new Post(post1);

            post1.XData.Visited = true;

            Assert.True(post1.XData.Visited);
            Assert.False(newPost.XData.Visited);
        }

        [Fact]
        public void Post_Description_ReturnsGeneratedIfNoPos()
        {
            Post post = new Post();
            Assert.False(post.HasPos);
            Assert.Equal(Post.GeneratedPostingKey, post.Description);
        }

        [Fact]
        public void Post_Description_ReturnsPositionIfAvailable()
        {
            Post post = new Post();
            post.Pos.BegLine = 5;

            Assert.True(post.HasPos);
            Assert.Equal("posting at line 5", post.Description);
        }

        [Fact]
        public void Post_Valid_CheckWhetherXactIsPopulated()
        {
            Post post = new Post(new Account(), new Amount(10));
            Xact xact = new Xact();
            xact.Posts.Add(post);

            Assert.False(post.Valid());
            post.Xact = xact;
            Assert.True(post.Valid());
        }

        [Fact]
        public void Post_Valid_CheckWhetherXactRefersToPost()
        {
            Post post = new Post(new Account(), new Amount(10));
            Xact xact = new Xact();
            post.Xact = xact;

            Assert.False(post.Valid());
            xact.Posts.Add(post);
            Assert.True(post.Valid());
        }

        [Fact]
        public void Post_Valid_CheckWhetherAccountIsPopulated()
        {
            Post post = new Post(null, new Amount(10));
            Xact xact = new Xact();
            post.Xact = xact;
            xact.Posts.Add(post);

            Assert.False(post.Valid());
            post.Account = new Account();
            Assert.True(post.Valid());
        }

        [Fact]
        public void Post_Valid_CheckWhetherAmountIsValid()
        {
            Post post = new Post(new Account(), new Amount(10));
            Xact xact = new Xact();
            post.Xact = xact;
            xact.Posts.Add(post);

            Assert.True(post.Valid());
            var quantity = new Amount(10).Quantity.SetPrecision(2048);
            post.Amount = new Amount(quantity, null);
            Assert.False(post.Amount.Valid());
            Assert.False(post.Valid());
        }

    }
}

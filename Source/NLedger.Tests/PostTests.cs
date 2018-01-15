// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Commodities;
using NLedger.Journals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests
{
    [TestClass]
    public class PostTests : TestFixture
    {
        [TestMethod]
        public void Post_ExtendPost_DoesNotExtendCommodityIfNoValueTag()
        {
            Commodity comm = CommodityPool.Current.FindOrCreate("test-comm-post");            
            Post post = new Post() { Amount = new Amount(BigInt.FromInt(100), comm), Account = new Account(null, "test") };

            Post.ExtendPost(post, new Journal());

            Assert.AreEqual(comm, post.Amount.Commodity);
        }

        [TestMethod]
        public void Post_Constructor_ClonesAmount()
        {
            Post post1 = new Post() { Amount = new Amount(100) };

            Post newPost = new Post(post1);
            newPost.Amount.Multiply(new Amount(10));

            Assert.AreEqual("100", post1.Amount.ToString());
            Assert.AreEqual("1000", newPost.Amount.ToString());
        }

        [TestMethod]
        public void Post_Constructor_ClonesXData()
        {
            Post post1 = new Post();
            Post newPost = new Post(post1);

            post1.XData.Visited = true;

            Assert.IsTrue(post1.XData.Visited);
            Assert.IsFalse(newPost.XData.Visited);
        }

        [TestMethod]
        public void Post_Description_ReturnsGeneratedIfNoPos()
        {
            Post post = new Post();
            Assert.IsFalse(post.HasPos);
            Assert.AreEqual(Post.GeneratedPostingKey, post.Description);
        }

        [TestMethod]
        public void Post_Description_ReturnsPositionIfAvailable()
        {
            Post post = new Post();
            post.Pos.BegLine = 5;

            Assert.IsTrue(post.HasPos);
            Assert.AreEqual("posting at line 5", post.Description);
        }

    }
}

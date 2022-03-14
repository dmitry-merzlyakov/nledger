// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Iterators;
using NLedger.Scopus;
using NLedger.Times;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Iterators
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon | ContextInit.SaveCultureInfo)]
    public class GeneratePostsIteratorTests : TestFixture
    {
        protected override void CustomTestInitialize()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [Fact]
        public void GeneratePostsIterator_Constructor_PopulatesProperties()
        {
            Session session = new Session();
            int seed = 100;
            int quantity = 200;

            GeneratePostsIterator genPosts = new GeneratePostsIterator(session, seed, quantity);

            Assert.Equal(session, genPosts.Session);
            Assert.Equal(seed, genPosts.Seed);
            Assert.Equal(quantity, genPosts.Quantity);

            Assert.NotNull(genPosts.StrLenGen);
            Assert.NotNull(genPosts.ThreeGen);
            Assert.NotNull(genPosts.SixGen);
            Assert.NotNull(genPosts.UpcharGen);
            Assert.NotNull(genPosts.DowncharGen);
            Assert.NotNull(genPosts.NumcharGen);
            Assert.NotNull(genPosts.TruthGen);
            Assert.NotNull(genPosts.NegNumberGen);
            Assert.NotNull(genPosts.PosNumberGen);
            Assert.NotNull(genPosts.YearGen);
            Assert.NotNull(genPosts.MonGen);
            Assert.NotNull(genPosts.DayGen);

            Assert.Equal(new DateTime(2288, 2, 19), genPosts.NextDate);
            Assert.Equal(new DateTime(2261, 5, 27), genPosts.NextAuxDate);
        }

        [Fact]
        public void GeneratePostsIterator_GenerateString_CreatesStringWithSpecifiedLength()
        {
            int length = 77;
            GeneratePostsIterator genPosts = new GeneratePostsIterator(new Session(), /* seed */ 100);
            string result = genPosts.GenerateString(length);
            Assert.Equal(length, result.Length);
        }

        [Fact]
        public void GeneratePostsIterator_GenerateString_CreatesStringWithLettersOnlyIfOnlyAlpha()
        {
            int length = 77;
            GeneratePostsIterator genPosts = new GeneratePostsIterator(new Session(), /* seed */ 100);
            string result = genPosts.GenerateString(length, true);
            Assert.True(result.All(c => Char.IsLetter(c)));
        }

        [Fact]
        public void GeneratePostsIterator_GenerateAccount_CreatesAccounNameWithPossibleBrackets()
        {
            // Round brackets
            StringBuilder sb1 = new StringBuilder();
            GeneratePostsIterator genPosts1 = new GeneratePostsIterator(new Session(), 200);
            bool mustBalance1 = genPosts1.GenerateAccount(sb1);
            Assert.Equal("(Q 93:5xq2y5BjY:81:VN n76SLNFXOfO)", sb1.ToString());
            Assert.False(mustBalance1);

            // Square brackets
            StringBuilder sb2 = new StringBuilder();
            GeneratePostsIterator genPosts2 = new GeneratePostsIterator(new Session(), 300);
            bool mustBalance2 = genPosts2.GenerateAccount(sb2);
            Assert.Equal("[Rz4sMk4kvVe53HOGo:i3 0y0hQI rxU2kb9oMO]", sb2.ToString());
            Assert.True(mustBalance2);

            // No brackets
            StringBuilder sb3 = new StringBuilder();
            GeneratePostsIterator genPosts3 = new GeneratePostsIterator(new Session(), 500);
            bool mustBalance3 = genPosts3.GenerateAccount(sb3);
            Assert.Equal("rpBXV13cM:4m", sb3.ToString());
            Assert.True(mustBalance3);
        }

        [Fact]
        public void GeneratePostsIterator_GenerateAccount_CreatesAccounNameWithoutBrackets()
        {
            StringBuilder sb1 = new StringBuilder();
            GeneratePostsIterator genPosts1 = new GeneratePostsIterator(new Session(), 200);
            bool mustBalance1 = genPosts1.GenerateAccount(sb1, true);
            Assert.Equal("x3:5xq2y5BjY:81:VN n7", sb1.ToString());
            Assert.True(mustBalance1);
        }

        [Fact]
        public void GeneratePostsIterator_GenerateCommodity_ReturnsACommodityCode()
        {
            GeneratePostsIterator genPosts = new GeneratePostsIterator(new Session(), 200);
            Assert.Equal("QdzT", genPosts.GenerateCommodity());
            Assert.Equal("EuA", genPosts.GenerateCommodity());
            Assert.Equal("M", genPosts.GenerateCommodity());
            Assert.Equal("nHx", genPosts.GenerateCommodity());
            Assert.Equal("pqG", genPosts.GenerateCommodity());
            Assert.Equal("yl", genPosts.GenerateCommodity());
            Assert.Equal("axBgKs", genPosts.GenerateCommodity());
        }

        [Fact]
        public void GeneratePostsIterator_GenerateCommodity_ExcludeHidesParticularCommodity()
        {
            GeneratePostsIterator genPosts = new GeneratePostsIterator(new Session(), 200);
            Assert.Equal("EuA", genPosts.GenerateCommodity("QdzT"));
            Assert.Equal("nHx", genPosts.GenerateCommodity("M"));
            Assert.Equal("yl", genPosts.GenerateCommodity("pqG"));
        }

        [Fact]
        public void GeneratePostsIterator_GenerateAmount_CreatesAmount()
        {
            StringBuilder sb = new StringBuilder();
            GeneratePostsIterator genPosts = new GeneratePostsIterator(new Session(), 200);
            Assert.Equal("QdzTx 3011.54570292427", genPosts.GenerateAmount(sb));
        }

        [Fact]
        public void GeneratePostsIterator_Get_ReturnsCollectionOfGeneratedPosts()
        {
            StringBuilder sb = new StringBuilder();
            GeneratePostsIterator genPosts = new GeneratePostsIterator(new Session(), 400, 100);
            var posts = genPosts.Get();
            Assert.Equal(100, posts.Count());
        }
    }
}

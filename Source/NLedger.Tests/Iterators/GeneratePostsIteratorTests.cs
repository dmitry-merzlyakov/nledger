// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace NLedger.Tests.Iterators
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon | ContextInit.SaveCultureInfo)]
    public class GeneratePostsIteratorTests : TestFixture
    {
        public override void CustomTestInitialize()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [TestMethod]
        public void GeneratePostsIterator_Constructor_PopulatesProperties()
        {
            Session session = new Session();
            int seed = 100;
            int quantity = 200;

            GeneratePostsIterator genPosts = new GeneratePostsIterator(session, seed, quantity);

            Assert.AreEqual(session, genPosts.Session);
            Assert.AreEqual(seed, genPosts.Seed);
            Assert.AreEqual(quantity, genPosts.Quantity);

            Assert.IsNotNull(genPosts.StrLenGen);
            Assert.IsNotNull(genPosts.ThreeGen);
            Assert.IsNotNull(genPosts.SixGen);
            Assert.IsNotNull(genPosts.UpcharGen);
            Assert.IsNotNull(genPosts.DowncharGen);
            Assert.IsNotNull(genPosts.NumcharGen);
            Assert.IsNotNull(genPosts.TruthGen);
            Assert.IsNotNull(genPosts.NegNumberGen);
            Assert.IsNotNull(genPosts.PosNumberGen);
            Assert.IsNotNull(genPosts.YearGen);
            Assert.IsNotNull(genPosts.MonGen);
            Assert.IsNotNull(genPosts.DayGen);

            Assert.AreEqual(new DateTime(2288, 2, 19), genPosts.NextDate);
            Assert.AreEqual(new DateTime(2261, 5, 27), genPosts.NextAuxDate);
        }

        [TestMethod]
        public void GeneratePostsIterator_GenerateString_CreatesStringWithSpecifiedLength()
        {
            int length = 77;
            GeneratePostsIterator genPosts = new GeneratePostsIterator(new Session(), /* seed */ 100);
            string result = genPosts.GenerateString(length);
            Assert.AreEqual(length, result.Length);
        }

        [TestMethod]
        public void GeneratePostsIterator_GenerateString_CreatesStringWithLettersOnlyIfOnlyAlpha()
        {
            int length = 77;
            GeneratePostsIterator genPosts = new GeneratePostsIterator(new Session(), /* seed */ 100);
            string result = genPosts.GenerateString(length, true);
            Assert.IsTrue(result.All(c => Char.IsLetter(c)));
        }

        [TestMethod]
        public void GeneratePostsIterator_GenerateAccount_CreatesAccounNameWithPossibleBrackets()
        {
            // Round brackets
            StringBuilder sb1 = new StringBuilder();
            GeneratePostsIterator genPosts1 = new GeneratePostsIterator(new Session(), 200);
            bool mustBalance1 = genPosts1.GenerateAccount(sb1);
            Assert.AreEqual("(Q 93:5xq2y5BjY:81:VN n76SLNFXOfO)", sb1.ToString());
            Assert.IsFalse(mustBalance1);

            // Square brackets
            StringBuilder sb2 = new StringBuilder();
            GeneratePostsIterator genPosts2 = new GeneratePostsIterator(new Session(), 300);
            bool mustBalance2 = genPosts2.GenerateAccount(sb2);
            Assert.AreEqual("[Rz4sMk4kvVe53HOGo:i3 0y0hQI rxU2kb9oMO]", sb2.ToString());
            Assert.IsTrue(mustBalance2);

            // No brackets
            StringBuilder sb3 = new StringBuilder();
            GeneratePostsIterator genPosts3 = new GeneratePostsIterator(new Session(), 500);
            bool mustBalance3 = genPosts3.GenerateAccount(sb3);
            Assert.AreEqual("rpBXV13cM:4m", sb3.ToString());
            Assert.IsTrue(mustBalance3);
        }

        [TestMethod]
        public void GeneratePostsIterator_GenerateAccount_CreatesAccounNameWithoutBrackets()
        {
            StringBuilder sb1 = new StringBuilder();
            GeneratePostsIterator genPosts1 = new GeneratePostsIterator(new Session(), 200);
            bool mustBalance1 = genPosts1.GenerateAccount(sb1, true);
            Assert.AreEqual("x3:5xq2y5BjY:81:VN n7", sb1.ToString());
            Assert.IsTrue(mustBalance1);
        }

        [TestMethod]
        public void GeneratePostsIterator_GenerateCommodity_ReturnsACommodityCode()
        {
            GeneratePostsIterator genPosts = new GeneratePostsIterator(new Session(), 200);
            Assert.AreEqual("QdzT", genPosts.GenerateCommodity());
            Assert.AreEqual("EuA", genPosts.GenerateCommodity());
            Assert.AreEqual("M", genPosts.GenerateCommodity());
            Assert.AreEqual("nHx", genPosts.GenerateCommodity());
            Assert.AreEqual("pqG", genPosts.GenerateCommodity());
            Assert.AreEqual("yl", genPosts.GenerateCommodity());
            Assert.AreEqual("axBgKs", genPosts.GenerateCommodity());
        }

        [TestMethod]
        public void GeneratePostsIterator_GenerateCommodity_ExcludeHidesParticularCommodity()
        {
            GeneratePostsIterator genPosts = new GeneratePostsIterator(new Session(), 200);
            Assert.AreEqual("EuA", genPosts.GenerateCommodity("QdzT"));
            Assert.AreEqual("nHx", genPosts.GenerateCommodity("M"));
            Assert.AreEqual("yl", genPosts.GenerateCommodity("pqG"));
        }

        [TestMethod]
        public void GeneratePostsIterator_GenerateAmount_CreatesAmount()
        {
            StringBuilder sb = new StringBuilder();
            GeneratePostsIterator genPosts = new GeneratePostsIterator(new Session(), 200);
            Assert.AreEqual("QdzTx 3011.54570292427", genPosts.GenerateAmount(sb));
        }

        [TestMethod]
        public void GeneratePostsIterator_Get_ReturnsCollectionOfGeneratedPosts()
        {
            StringBuilder sb = new StringBuilder();
            GeneratePostsIterator genPosts = new GeneratePostsIterator(new Session(), 400, 100);
            var posts = genPosts.Get();
            Assert.AreEqual(100, posts.Count());
        }
    }
}

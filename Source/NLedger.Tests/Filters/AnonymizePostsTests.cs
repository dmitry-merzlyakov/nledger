// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Amounts;
using NLedger.Commodities;
using NLedger.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Filters
{
    [TestClass]
    public class AnonymizePostsTests : TestFixture
    {
        [TestMethod]
        public void AnonymizePosts_Constuctor_AcceptsNulls()
        {
            AnonymizePosts anonymizePosts = new AnonymizePosts(null);
            Assert.IsNull(anonymizePosts.Handler);
        }

        [TestMethod]
        public void AnonymizePosts_Constuctor_PopulatesHandler()
        {
            AnonymizePosts parent = new AnonymizePosts(null);
            AnonymizePosts current = new AnonymizePosts(parent);
            Assert.AreEqual(parent, current.Handler);
        }

        [TestMethod]
        public void AnonymizePosts_RenderCommodity_HidesCommodityName()
        {
            string commodityName1 = "comm-name-1";
            string commodityName2 = "comm-name-2";
            Commodity commodity1 = CommodityPool.Current.FindOrCreate(commodityName1);
            Commodity commodity2 = CommodityPool.Current.FindOrCreate(commodityName2);
            Amount amount1 = new Amount(BigInt.FromLong(10), commodity1);
            Amount amount2 = new Amount(BigInt.FromLong(10), commodity2);

            AnonymizePosts anonymizePosts = new AnonymizePosts(null);
            anonymizePosts.RenderCommodity(amount1);
            anonymizePosts.RenderCommodity(amount2);

            Assert.AreEqual("A", amount1.Commodity.Symbol);
            Assert.AreEqual("B", amount2.Commodity.Symbol);

            Assert.AreEqual(commodityName1, anonymizePosts.CommodityIndexMap.Keys.First().BaseSymbol);
            Assert.AreEqual(commodityName2, anonymizePosts.CommodityIndexMap.Keys.Last().BaseSymbol);
        }

    }
}

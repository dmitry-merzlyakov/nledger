// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Commodities;
using NLedger.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Filters
{
    public class AnonymizePostsTests : TestFixture
    {
        [Fact]
        public void AnonymizePosts_Constuctor_AcceptsNulls()
        {
            AnonymizePosts anonymizePosts = new AnonymizePosts(null);
            Assert.Null(anonymizePosts.Handler);
        }

        [Fact]
        public void AnonymizePosts_Constuctor_PopulatesHandler()
        {
            AnonymizePosts parent = new AnonymizePosts(null);
            AnonymizePosts current = new AnonymizePosts(parent);
            Assert.Equal(parent, current.Handler);
        }

        [Fact]
        public void AnonymizePosts_RenderCommodity_HidesCommodityName()
        {
            string commodityName1 = "comm-name-1";
            string commodityName2 = "comm-name-2";
            Commodity commodity1 = CommodityPool.Current.FindOrCreate(commodityName1);
            Commodity commodity2 = CommodityPool.Current.FindOrCreate(commodityName2);
            Amount amount1 = new Amount(10, commodity1);
            Amount amount2 = new Amount(10, commodity2);

            AnonymizePosts anonymizePosts = new AnonymizePosts(null);
            anonymizePosts.RenderCommodity(amount1);
            anonymizePosts.RenderCommodity(amount2);

            Assert.Equal("A", amount1.Commodity.Symbol);
            Assert.Equal("B", amount2.Commodity.Symbol);

            Assert.Equal(commodityName1, anonymizePosts.CommodityIndexMap.Keys.First().BaseSymbol);
            Assert.Equal(commodityName2, anonymizePosts.CommodityIndexMap.Keys.Last().BaseSymbol);
        }

    }
}

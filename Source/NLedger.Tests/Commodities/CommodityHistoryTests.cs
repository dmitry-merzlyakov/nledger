// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Commodities;
using NLedger.Utility.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Commodities
{
    public class CommodityHistoryTests : TestFixture
    {
        [Fact]
        public void CommodityHistory_AddCommodity_AddsACommodity()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("base"));

            commodityHistory.AddCommodity(commodity);

            Assert.True(commodityHistory.PriceGraphAccessor.HasVertex(commodity));
        }

        [Fact]
        public void CommodityHistory_AddPrice_AddsEdgeAndPrice()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            commodityHistory.AddCommodity(commodity1);
            commodityHistory.AddCommodity(commodity2);
            DateTime when = DateTime.Now;
            Amount price = new Amount(10, commodity2);

            commodityHistory.AddPrice(commodity1, when, price);

            var edgeDesc = commodityHistory.PriceGraphAccessor.FindEdgeDescriptor(commodity1, commodity2);
            Assert.Equal(commodity1, edgeDesc.Vertex1);
            Assert.Equal(commodity2, edgeDesc.Vertex2);
            Assert.Equal(1, edgeDesc.Edge.Prices.Count);
            Assert.Equal(price, edgeDesc.Edge.Prices[when]);
        }

        [Fact]
        public void CommodityHistory_RemovePrice_RemovesPriceAndEdge()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            commodityHistory.AddCommodity(commodity1);
            commodityHistory.AddCommodity(commodity2);
            DateTime when = DateTime.Now;
            Amount price = new Amount(10, commodity2);

            commodityHistory.AddPrice(commodity1, when, price);
            commodityHistory.RemovePrice(commodity1, commodity2, when);

            var edgeDesc = commodityHistory.PriceGraphAccessor.FindEdgeDescriptor(commodity1, commodity2);
            Assert.Null(edgeDesc);
        }

        [Fact]
        public void CommodityHistory_MapPrices_IteratesAppropriatePrices()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            commodityHistory.AddCommodity(commodity1);
            commodityHistory.AddCommodity(commodity2);
            DateTime when = DateTime.Now;
            Amount price = new Amount(10, commodity2);
            commodityHistory.AddPrice(commodity1, when, price);

            IList<PricePoint> pricePoints = new List<PricePoint>();
            commodityHistory.MapPrices((d, a) => pricePoints.Add(new PricePoint(d, a)), commodity1, when.AddDays(1));

            Assert.Single(pricePoints);
            Assert.Equal(when, pricePoints.First().When);
            Assert.Equal(price, pricePoints.First().Price);
        }

        [Fact]
        public void CommodityHistory_FindPrice_ReturnsAppropriatePriceForCommodityAndDate()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            commodityHistory.AddCommodity(commodity1);
            commodityHistory.AddCommodity(commodity2);
            DateTime when = DateTime.Now;
            Amount price = new Amount(10, commodity2);
            commodityHistory.AddPrice(commodity1, when, price);

            PricePoint? pricePoint = commodityHistory.FindPrice(commodity1, when.AddDays(1));

            Assert.Equal(when, pricePoint.Value.When);
            Assert.Equal(price, pricePoint.Value.Price);
        }

        [Fact]
        public void CommodityHistory_FindPrice_LooksForShortestPath()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            commodityHistory.AddCommodity(commodity1);
            commodityHistory.AddCommodity(commodity2);
            DateTime when = DateTime.Now;
            Amount price = new Amount(10, commodity2);
            commodityHistory.AddPrice(commodity1, when, price);

            PricePoint? pricePoint = commodityHistory.FindPrice(commodity1, commodity2, when.AddDays(1));

            Assert.Equal(when, pricePoint.Value.When);
            Assert.Equal(price, pricePoint.Value.Price);
        }

        [Fact]
        public void RecentEdgeWeight_Filter_ReturnsFalseIfPricesAreEmpty()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            RecentEdgeWeight recentEdgeWeight = new RecentEdgeWeight(DateTime.Now);
            PriceGraphEdge edge = new PriceGraphEdge();

            var result = recentEdgeWeight.Filter(commodity1, commodity2, edge);

            Assert.False(result);
        }

        [Fact]
        public void RecentEdgeWeight_Filter_ReturnsFalseIfRefTimeLessThanFirstPrice()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            DateTime refTime = new DateTime(2010, 10, 5);
            DateTime firstPriceTime = new DateTime(2010, 10, 15);

            RecentEdgeWeight recentEdgeWeight = new RecentEdgeWeight(refTime);
            PriceGraphEdge edge = new PriceGraphEdge();
            edge.Prices.Add(firstPriceTime, new Amount(10));

            var result = recentEdgeWeight.Filter(commodity1, commodity2, edge);

            Assert.False(result);
        }

        [Fact]
        public void RecentEdgeWeight_Filter_ReturnsFalseIfRefTimeGreaterThanFirstPriceButLessThanOldest()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            DateTime firstPriceTime = new DateTime(2010, 10, 15);
            DateTime refTime = new DateTime(2010, 11, 5);
            DateTime oldest = new DateTime(2010, 12, 15);

            RecentEdgeWeight recentEdgeWeight = new RecentEdgeWeight(refTime, oldest);
            PriceGraphEdge edge = new PriceGraphEdge();
            edge.Prices.Add(firstPriceTime, new Amount(10));

            var result = recentEdgeWeight.Filter(commodity1, commodity2, edge);

            Assert.False(result);
        }

        [Fact]
        public void RecentEdgeWeight_Filter_ReturnsTrueIfRefTimeBiggerThanFirstPrice()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            DateTime firstPriceTime = new DateTime(2010, 10, 15);
            DateTime refTime = new DateTime(2010, 11, 5);

            RecentEdgeWeight recentEdgeWeight = new RecentEdgeWeight(refTime);
            PriceGraphEdge edge = new PriceGraphEdge();
            edge.Prices.Add(firstPriceTime, new Amount(10));

            var result = recentEdgeWeight.Filter(commodity1, commodity2, edge);

            Assert.True(result);
        }

        [Fact]
        public void RecentEdgeWeight_Filter_ReturnsTrueIfRefTimeGreaterThanFirstPriceAndActualPriceGreaterThanOldest()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            DateTime firstPriceTime = new DateTime(2010, 10, 15);
            DateTime oldest = new DateTime(2010, 11, 15);
            DateTime actualPriceTime = new DateTime(2010, 12, 15);
            DateTime refTime = new DateTime(2010, 12, 25);

            RecentEdgeWeight recentEdgeWeight = new RecentEdgeWeight(refTime, oldest);
            PriceGraphEdge edge = new PriceGraphEdge();
            edge.Prices.Add(firstPriceTime, new Amount(10));
            edge.Prices.Add(actualPriceTime, new Amount(10));

            var result = recentEdgeWeight.Filter(commodity1, commodity2, edge);

            Assert.True(result);
        }

        [Fact]
        public void RecentEdgeWeight_Filter_ReturnsTrueAndUpdatesWeight()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            DateTime firstPriceTime = new DateTime(2010, 10, 1);
            DateTime oldest = new DateTime(2010, 11, 2);
            DateTime actualPriceTime = new DateTime(2010, 12, 5);
            DateTime refTime = new DateTime(2011, 02, 02);

            RecentEdgeWeight recentEdgeWeight = new RecentEdgeWeight(refTime, oldest);
            PriceGraphEdge edge = new PriceGraphEdge();
            edge.Prices.Add(firstPriceTime, new Amount(10));
            edge.Prices.Add(actualPriceTime, new Amount(12));

            var result = recentEdgeWeight.Filter(commodity1, commodity2, edge);

            Assert.True(result);
            Assert.Equal(refTime - actualPriceTime, edge.Weight);
            Assert.Equal(actualPriceTime, edge.PricePoint.When);
            Assert.Equal(new Amount(12), edge.Prices[actualPriceTime]);
        }

        public class CommodityHistoryAccessor : CommodityHistory
        {
            public IGraph<Commodity, PriceGraphEdge> PriceGraphAccessor
            {
                get { return PriceGraph; }
            }
        }
    }
}

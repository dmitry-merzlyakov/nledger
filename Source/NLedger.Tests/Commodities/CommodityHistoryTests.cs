// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Amounts;
using NLedger.Commodities;
using NLedger.Utility.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Commodities
{
    [TestClass]
    public class CommodityHistoryTests : TestFixture
    {
        [TestMethod]
        public void CommodityHistory_AddCommodity_AddsACommodity()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("base"));

            commodityHistory.AddCommodity(commodity);

            Assert.IsTrue(commodityHistory.PriceGraphAccessor.HasVertex(commodity));
        }

        [TestMethod]
        public void CommodityHistory_AddPrice_AddsEdgeAndPrice()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            commodityHistory.AddCommodity(commodity1);
            commodityHistory.AddCommodity(commodity2);
            DateTime when = DateTime.Now;
            Amount price = new Amount(BigInt.FromInt(10), commodity2);

            commodityHistory.AddPrice(commodity1, when, price);

            var edgeDesc = commodityHistory.PriceGraphAccessor.FindEdgeDescriptor(commodity1, commodity2);
            Assert.AreEqual(commodity1, edgeDesc.Vertex1);
            Assert.AreEqual(commodity2, edgeDesc.Vertex2);
            Assert.AreEqual(1, edgeDesc.Edge.Prices.Count);
            Assert.AreEqual(price, edgeDesc.Edge.Prices[when]);
        }

        [TestMethod]
        public void CommodityHistory_RemovePrice_RemovesPriceAndEdge()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            commodityHistory.AddCommodity(commodity1);
            commodityHistory.AddCommodity(commodity2);
            DateTime when = DateTime.Now;
            Amount price = new Amount(BigInt.FromInt(10), commodity2);

            commodityHistory.AddPrice(commodity1, when, price);
            commodityHistory.RemovePrice(commodity1, commodity2, when);

            var edgeDesc = commodityHistory.PriceGraphAccessor.FindEdgeDescriptor(commodity1, commodity2);
            Assert.IsNull(edgeDesc);
        }

        [TestMethod]
        public void CommodityHistory_MapPrices_IteratesAppropriatePrices()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            commodityHistory.AddCommodity(commodity1);
            commodityHistory.AddCommodity(commodity2);
            DateTime when = DateTime.Now;
            Amount price = new Amount(BigInt.FromInt(10), commodity2);
            commodityHistory.AddPrice(commodity1, when, price);

            IList<PricePoint> pricePoints = new List<PricePoint>();
            commodityHistory.MapPrices((d, a) => pricePoints.Add(new PricePoint(d, a)), commodity1, when.AddDays(1));

            Assert.AreEqual(1, pricePoints.Count());
            Assert.AreEqual(when, pricePoints.First().When);
            Assert.AreEqual(price, pricePoints.First().Price);
        }

        [TestMethod]
        public void CommodityHistory_FindPrice_ReturnsAppropriatePriceForCommodityAndDate()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            commodityHistory.AddCommodity(commodity1);
            commodityHistory.AddCommodity(commodity2);
            DateTime when = DateTime.Now;
            Amount price = new Amount(BigInt.FromInt(10), commodity2);
            commodityHistory.AddPrice(commodity1, when, price);

            PricePoint? pricePoint = commodityHistory.FindPrice(commodity1, when.AddDays(1));

            Assert.AreEqual(when, pricePoint.Value.When);
            Assert.AreEqual(price, pricePoint.Value.Price);
        }

        [TestMethod]
        public void CommodityHistory_FindPrice_LooksForShortestPath()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            commodityHistory.AddCommodity(commodity1);
            commodityHistory.AddCommodity(commodity2);
            DateTime when = DateTime.Now;
            Amount price = new Amount(BigInt.FromInt(10), commodity2);
            commodityHistory.AddPrice(commodity1, when, price);

            PricePoint? pricePoint = commodityHistory.FindPrice(commodity1, commodity2, when.AddDays(1));

            Assert.AreEqual(when, pricePoint.Value.When);
            Assert.AreEqual(price, pricePoint.Value.Price);
        }

        [TestMethod]
        public void RecentEdgeWeight_Filter_ReturnsFalseIfPricesAreEmpty()
        {
            CommodityHistoryAccessor commodityHistory = new CommodityHistoryAccessor();
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base2"));

            RecentEdgeWeight recentEdgeWeight = new RecentEdgeWeight(DateTime.Now);
            PriceGraphEdge edge = new PriceGraphEdge();

            var result = recentEdgeWeight.Filter(commodity1, commodity2, edge);

            Assert.IsFalse(result);
        }

        [TestMethod]
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

            Assert.IsFalse(result);
        }

        [TestMethod]
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

            Assert.IsFalse(result);
        }

        [TestMethod]
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

            Assert.IsTrue(result);
        }

        [TestMethod]
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

            Assert.IsTrue(result);
        }

        [TestMethod]
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

            Assert.IsTrue(result);
            Assert.AreEqual(refTime - actualPriceTime, edge.Weight);
            Assert.AreEqual(actualPriceTime, edge.PricePoint.When);
            Assert.AreEqual(new Amount(12), edge.Prices[actualPriceTime]);
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

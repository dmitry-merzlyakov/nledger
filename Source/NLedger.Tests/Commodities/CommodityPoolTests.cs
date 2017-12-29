// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Annotate;
using NLedger.Commodities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Commodities
{
    [TestClass]
    public class CommodityPoolTests : TestFixture
    {
        [TestMethod]
        public void CommodityPool_CreateBySymbol_CreatesCommodityAndAddsToPool()
        {
            string symbol = "symbol";
            TestCommodityPool commodityPool = new TestCommodityPool();
            
            Commodity commodity = commodityPool.Create(symbol);

            Assert.IsNotNull(commodity);
            Assert.AreEqual(symbol, commodity.BaseSymbol);
            Assert.AreEqual(symbol, commodity.Symbol);
            Assert.IsNull(commodity.QualifiedSymbol);
            Assert.IsTrue(commodityPool.Commodities.ContainsKey(symbol));
            Assert.AreEqual(commodity, commodityPool.Commodities[symbol]);
            Assert.IsFalse(commodityPool.AnnotatedCommodities.Any());
        }

        [TestMethod]
        public void CommodityPool_CreateBySymbol_AddsQualifiedTextIfNeeded()
        {
            string symbol = "sym&bol";
            string qualifiedSymbol = "\"" + symbol + "\"";

            TestCommodityPool commodityPool = new TestCommodityPool();

            Commodity commodity = commodityPool.Create(symbol);

            Assert.IsNotNull(commodity);
            Assert.AreEqual(symbol, commodity.BaseSymbol);
            Assert.AreEqual(qualifiedSymbol, commodity.Symbol);
            Assert.AreEqual(qualifiedSymbol, commodity.QualifiedSymbol);
        }

        [TestMethod]
        public void CommodityPool_CreateByCommodityAndDetails_CreatesCommodityAndAddsToPool()
        {
            TestCommodityPool commodityPool = new TestCommodityPool();
            string symbol = "symbol";
            Commodity commodity = commodityPool.Create(symbol);
            Annotation details = new Annotation();

            AnnotatedCommodity annotatedCommodity = commodityPool.Create(commodity, details) as AnnotatedCommodity;

            Assert.IsNotNull(annotatedCommodity);
            Assert.AreEqual(details, annotatedCommodity.Details);

            Assert.AreEqual(symbol, annotatedCommodity.BaseSymbol);
            Assert.AreEqual(symbol, annotatedCommodity.Symbol);
            Assert.IsNull(annotatedCommodity.QualifiedSymbol);

            Assert.IsTrue(commodityPool.Commodities.ContainsKey(symbol));
            Assert.AreEqual(commodity, commodityPool.Commodities[symbol]);

            Assert.IsTrue(commodityPool.AnnotatedCommodities.ContainsKey(new Tuple<string, Annotation>(symbol, details)));
            Assert.AreEqual(annotatedCommodity, commodityPool.AnnotatedCommodities[new Tuple<string, Annotation>(symbol, details)]);
        }

        [TestMethod]
        public void CommodityPool_FindBySymbol_ReturnsCommodityIfExists()
        {
            TestCommodityPool commodityPool = new TestCommodityPool();
            string symbol = "symbol";
            string wrongSymbol = "dummy";

            Commodity commodity = commodityPool.Create(symbol);

            Assert.AreEqual(commodity, commodityPool.Find(symbol));
            Assert.IsNull(commodityPool.Find(wrongSymbol));
        }

        [TestMethod]
        public void CommodityPool_FindBySymbolAndDetails_ReturnsAnnotatedCommodityIfExists()
        {
            TestCommodityPool commodityPool = new TestCommodityPool();
            string symbol = "symbol";
            string wrongSymbol = "dummy";
            Annotation details = new Annotation();

            Commodity commodity = commodityPool.Create(symbol);
            AnnotatedCommodity annotatedCommodity = commodityPool.Create(commodity, details) as AnnotatedCommodity;

            Assert.AreEqual(annotatedCommodity, commodityPool.Find(symbol, details));
            Assert.IsNull(commodityPool.Find(wrongSymbol));
        }

        [TestMethod]
        public void CommodityPool_FindOrCreateByCommodityAndDetails_CreatesAnnotatedCommodity()
        {
            TestCommodityPool commodityPool = new TestCommodityPool();
            string symbol = "symbol";
            Annotation details = new Annotation();
            Commodity commodity = commodityPool.Create(symbol);
                      
            AnnotatedCommodity annotatedCommodity = commodityPool.FindOrCreate(commodity, details) as AnnotatedCommodity;

            Assert.IsNotNull(annotatedCommodity);
            Assert.AreEqual(details, annotatedCommodity.Details);

            AnnotatedCommodity annotatedCommodity2 = commodityPool.FindOrCreate(commodity, details) as AnnotatedCommodity;

            Assert.AreEqual(annotatedCommodity, annotatedCommodity2);
        }

        [TestMethod]
        public void CommodityPool_FindWithAnnotation_IgnoresAnnotationFlags()
        {
            TestCommodityPool commodityPool = new TestCommodityPool();
            string symbol = "symbol";
            Annotation details = new Annotation();
            Commodity commodity = commodityPool.Create(symbol);
            AnnotatedCommodity annotatedCommodity = commodityPool.FindOrCreate(commodity, details) as AnnotatedCommodity;

            // The same annotation but with another flags - annotated commodity is found
            Annotation details1 = new Annotation() { IsPriceFixated = true };
            Assert.IsNotNull(commodityPool.Find(symbol, details1));

            // Another annotation - no results
            Annotation details2 = new Annotation() { Tag = "tag" };
            Assert.IsNull(commodityPool.Find(symbol, details2));
        }

        [TestMethod]
        public void CommodityPool_ParsePriceExpression_ReturnsCommodity()
        {
            TestCommodityPool commodityPool = new TestCommodityPool();
            Commodity comm = commodityPool.ParsePriceExpression("CommPoolParseExpr");
            Assert.IsNotNull(comm);
            Assert.AreEqual("CommPoolParseExpr", comm.Symbol);
        }

        public class TestCommodityPool : CommodityPool
        {
            public TestCommodityPool() : base()
            { }
        }
    }
}

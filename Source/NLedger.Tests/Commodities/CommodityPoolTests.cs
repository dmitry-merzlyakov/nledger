// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts;
using NLedger.Annotate;
using NLedger.Commodities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Commodities
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class CommodityPoolTests : TestFixture
    {
        [Fact]
        public void CommodityPool_CreateBySymbol_CreatesCommodityAndAddsToPool()
        {
            string symbol = "symbol";
            TestCommodityPool commodityPool = new TestCommodityPool();
            
            Commodity commodity = commodityPool.Create(symbol);

            Assert.NotNull(commodity);
            Assert.Equal(symbol, commodity.BaseSymbol);
            Assert.Equal(symbol, commodity.Symbol);
            Assert.Null(commodity.QualifiedSymbol);
            Assert.True(commodityPool.Commodities.ContainsKey(symbol));
            Assert.Equal(commodity, commodityPool.Commodities[symbol]);
            Assert.False(commodityPool.AnnotatedCommodities.Any());
        }

        [Fact]
        public void CommodityPool_CreateBySymbol_AddsQualifiedTextIfNeeded()
        {
            string symbol = "sym&bol";
            string qualifiedSymbol = "\"" + symbol + "\"";

            TestCommodityPool commodityPool = new TestCommodityPool();

            Commodity commodity = commodityPool.Create(symbol);

            Assert.NotNull(commodity);
            Assert.Equal(symbol, commodity.BaseSymbol);
            Assert.Equal(qualifiedSymbol, commodity.Symbol);
            Assert.Equal(qualifiedSymbol, commodity.QualifiedSymbol);
        }

        [Fact]
        public void CommodityPool_CreateByCommodityAndDetails_CreatesCommodityAndAddsToPool()
        {
            TestCommodityPool commodityPool = new TestCommodityPool();
            string symbol = "symbol";
            Commodity commodity = commodityPool.Create(symbol);
            Annotation details = new Annotation();

            AnnotatedCommodity annotatedCommodity = commodityPool.Create(commodity, details) as AnnotatedCommodity;

            Assert.NotNull(annotatedCommodity);
            Assert.Equal(details, annotatedCommodity.Details);

            Assert.Equal(symbol, annotatedCommodity.BaseSymbol);
            Assert.Equal(symbol, annotatedCommodity.Symbol);
            Assert.Null(annotatedCommodity.QualifiedSymbol);

            Assert.True(commodityPool.Commodities.ContainsKey(symbol));
            Assert.Equal(commodity, commodityPool.Commodities[symbol]);

            Assert.True(commodityPool.AnnotatedCommodities.ContainsKey(new Tuple<string, Annotation>(symbol, details)));
            Assert.Equal(annotatedCommodity, commodityPool.AnnotatedCommodities[new Tuple<string, Annotation>(symbol, details)]);
        }

        [Fact]
        public void CommodityPool_FindBySymbol_ReturnsCommodityIfExists()
        {
            TestCommodityPool commodityPool = new TestCommodityPool();
            string symbol = "symbol";
            string wrongSymbol = "dummy";

            Commodity commodity = commodityPool.Create(symbol);

            Assert.Equal(commodity, commodityPool.Find(symbol));
            Assert.Null(commodityPool.Find(wrongSymbol));
        }

        [Fact]
        public void CommodityPool_FindBySymbolAndDetails_ReturnsAnnotatedCommodityIfExists()
        {
            TestCommodityPool commodityPool = new TestCommodityPool();
            string symbol = "symbol";
            string wrongSymbol = "dummy";
            Annotation details = new Annotation();

            Commodity commodity = commodityPool.Create(symbol);
            AnnotatedCommodity annotatedCommodity = commodityPool.Create(commodity, details) as AnnotatedCommodity;

            Assert.Equal(annotatedCommodity, commodityPool.Find(symbol, details));
            Assert.Null(commodityPool.Find(wrongSymbol));
        }

        [Fact]
        public void CommodityPool_FindOrCreateByCommodityAndDetails_CreatesAnnotatedCommodity()
        {
            TestCommodityPool commodityPool = new TestCommodityPool();
            string symbol = "symbol";
            Annotation details = new Annotation();
            Commodity commodity = commodityPool.Create(symbol);
                      
            AnnotatedCommodity annotatedCommodity = commodityPool.FindOrCreate(commodity, details) as AnnotatedCommodity;

            Assert.NotNull(annotatedCommodity);
            Assert.Equal(details, annotatedCommodity.Details);

            AnnotatedCommodity annotatedCommodity2 = commodityPool.FindOrCreate(commodity, details) as AnnotatedCommodity;

            Assert.Equal(annotatedCommodity, annotatedCommodity2);
        }

        [Fact]
        public void CommodityPool_FindWithAnnotation_IgnoresAnnotationFlags()
        {
            TestCommodityPool commodityPool = new TestCommodityPool();
            string symbol = "symbol";
            Annotation details = new Annotation();
            Commodity commodity = commodityPool.Create(symbol);
            AnnotatedCommodity annotatedCommodity = commodityPool.FindOrCreate(commodity, details) as AnnotatedCommodity;

            // The same annotation but with another flags - annotated commodity is found
            Annotation details1 = new Annotation() { IsPriceFixated = true };
            Assert.NotNull(commodityPool.Find(symbol, details1));

            // Another annotation - no results
            Annotation details2 = new Annotation() { Tag = "tag" };
            Assert.Null(commodityPool.Find(symbol, details2));
        }

        [Fact]
        public void CommodityPool_ParsePriceExpression_ReturnsCommodity()
        {
            TestCommodityPool commodityPool = new TestCommodityPool();
            Commodity comm = commodityPool.ParsePriceExpression("CommPoolParseExpr");
            Assert.NotNull(comm);
            Assert.Equal("CommPoolParseExpr", comm.Symbol);
        }

        [Fact]
        public void CommodityPool_CommodityQuoteFromScript_AddsCommoditiesToCommand()
        {
            Commodity commodity1 = CommodityPool.Current.Create("QUO1"); commodity1.QualifiedSymbol = "QUO1";
            Commodity commodity2 = CommodityPool.Current.Create("QUO2"); commodity2.QualifiedSymbol = "QUO2";

            TestQuoteProvider quoteProvider = new TestQuoteProvider() { ResultGet = true, ResultResponse = String.Empty };
            MainApplicationContext.Current.SetApplicationServiceProvider(new ApplicationServiceProvider
                (quoteProviderFactory: () => quoteProvider));

            CommodityPool.CommodityQuoteFromScript(commodity1, null);
            Assert.Equal("getquote \"QUO1\" \"\"", quoteProvider.ReceivedCommand);

            CommodityPool.CommodityQuoteFromScript(commodity1, commodity2);
            Assert.Equal("getquote \"QUO1\" \"QUO2\"", quoteProvider.ReceivedCommand);
        }

        [Fact]
        public void CommodityPool_CommodityQuoteFromScript_SetsNoMarkedFlagInCaseOfFailure()
        {
            Commodity commodity1 = CommodityPool.Current.Create("QUO1"); commodity1.QualifiedSymbol = "QUO1";

            TestQuoteProvider quoteProvider = new TestQuoteProvider() { ResultGet = false };
            MainApplicationContext.Current.SetApplicationServiceProvider(new ApplicationServiceProvider
                (quoteProviderFactory: () => quoteProvider));

            var result = CommodityPool.CommodityQuoteFromScript(commodity1, null);
            Assert.Null(result);
            Assert.True(commodity1.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_NOMARKET));
        }

        [Fact]
        public void CommodityPool_CommodityQuoteFromScript_ReturnsPricePointInCaseOfSuccess()
        {
            Commodity commodity1 = CommodityPool.Current.Create("QUO1");
            Commodity commodity2 = CommodityPool.Current.Create("QUO2");

            TestQuoteProvider quoteProvider = new TestQuoteProvider() { ResultGet = true, ResultResponse = "2010/10/10 10:11:12 AAPL $100.00" };
            MainApplicationContext.Current.SetApplicationServiceProvider(new ApplicationServiceProvider
                (quoteProviderFactory: () => quoteProvider));

            var result = CommodityPool.CommodityQuoteFromScript(commodity1, null);
            Assert.NotNull(result);
            Assert.Equal(new DateTime(2010, 10, 10, 10, 11, 12), result.Value.When);
            Assert.Equal("$100", result.Value.Price.ToString());
        }

        public class TestCommodityPool : CommodityPool
        {
            public TestCommodityPool() : base()
            { }
        }

        public class TestQuoteProvider : IQuoteProvider
        {
            public string ReceivedCommand = null;
            public string ResultResponse = null;
            public bool ResultGet = false;

            public bool Get(string command, out string response)
            {
                ReceivedCommand = command;
                response = ResultResponse;
                return ResultGet;
            }
        }
    }
}

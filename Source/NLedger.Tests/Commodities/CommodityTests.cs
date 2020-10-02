// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Commodities;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Commodities
{
    public class CommodityTests : TestFixture
    {
        [Fact]
        public void Commodity_ParseSymbol_ReturnsEmptyStringForEmptyString()
        {
            string s = null;
            Assert.Equal(String.Empty, Commodity.ParseSymbol(ref s));
            s = String.Empty;
            Assert.Equal(String.Empty, Commodity.ParseSymbol(ref s));
        }

        [Fact]
        public void Commodity_ParseSymbol_IgnoresLeadingSpaces()
        {
            string s = "asd";
            Assert.Equal("asd", Commodity.ParseSymbol(ref s));
            s = "  qwe";
            Assert.Equal("qwe", Commodity.ParseSymbol(ref s));
            s = "\tqwe";
            Assert.Equal("qwe", Commodity.ParseSymbol(ref s));
        }

        [Fact]
        public void Commodity_ParseSymbol_HandlesQuotedNames()
        {
            string s = "   \"asd\"qwe";
            Assert.Equal("asd", Commodity.ParseSymbol(ref s));
            Assert.Equal("qwe", s);
        }

        [Fact]
        public void Commodity_ParseSymbol_HandlesNonCkisedQuotes()
        {
            string s = "   \"asdqwe";
            var exception = Assert.Throws<AmountError>(() => Commodity.ParseSymbol(ref s));
            Assert.Equal(AmountError.ErrorMessageNonClosedQuote, exception.Message);
        }

        [Fact]
        public void Commodity_ParseSymbol_TakesNonQuotedName()
        {
            string s = "  ABC DEF";
            Assert.Equal("ABC", Commodity.ParseSymbol(ref s));
            Assert.Equal(" DEF", s);

            s = "  ABC@DEF";
            Assert.Equal("ABC", Commodity.ParseSymbol(ref s));
            Assert.Equal("@DEF", s);

            s = "ABC:DEF";
            Assert.Equal("ABC", Commodity.ParseSymbol(ref s));
            Assert.Equal(":DEF", s);
        }

        [Fact]
        public void Commodity_ParseSymbol_IgnoresReservedTokens()
        {
            string original = "  and DEF";  // 'and' is a reserved token - see Commodity.ReservedTokens
            string s = original;
            Assert.Equal(String.Empty, Commodity.ParseSymbol(ref s));
            Assert.Equal(original, s);
        }

        [Fact]
        public void Commodity_StripAnnotations_ReturnsTheSameInstance()
        {
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Commodity strippedCommodity = commodity.StripAnnotations(new AnnotationKeepDetails());
            Assert.Equal(commodity, strippedCommodity);
        }

        [Fact]
        public void Commodity_CompareByCommodity_ComparesBaseSymbolsIfTheyAreNotEqual()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm2"));

            Amount amount1 = new Amount(0, commodity1);
            Amount amount2 = new Amount(0, commodity2);

            Assert.Equal(-1, Commodity.CompareByCommodity(amount1, amount2));
            Assert.Equal(1, Commodity.CompareByCommodity(amount2, amount1));
        }

        [Fact]
        public void Commodity_CompareByCommodity_ChecksWhetherCommoditiesAreAnnotated()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm1 = new AnnotatedCommodity(commodity1, new Annotation());
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));

            Amount amount1 = new Amount(0, commodity1);
            Amount amount2 = new Amount(0, annComm1);
            Amount amount3 = new Amount(0, commodity2);

            Assert.Equal(-1, Commodity.CompareByCommodity(amount1, amount2));
            Assert.Equal(0, Commodity.CompareByCommodity(amount1, amount3));

            Assert.Equal(1, Commodity.CompareByCommodity(amount2, amount1));
        }

        [Fact]
        public void Commodity_CompareByCommodity_ComparesPrices()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm1 = new AnnotatedCommodity(commodity1, new Annotation());
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm2 = new AnnotatedCommodity(commodity2, new Annotation());

            Amount amount1 = new Amount(0, annComm1);
            Amount amount2 = new Amount(0, annComm2);

            annComm1.Details.Price = new Amount(10);
            Assert.Equal(1, Commodity.CompareByCommodity(amount1, amount2));
            Assert.Equal(-1, Commodity.CompareByCommodity(amount2, amount1));

            annComm1.Details.Price = new Amount(5);
            annComm2.Details.Price = new Amount(10);
            Assert.Equal(-1, Commodity.CompareByCommodity(amount1, amount2));
            Assert.Equal(1, Commodity.CompareByCommodity(amount2, amount1));
        }

        [Fact]
        public void Commodity_CompareByCommodity_ComparesDates()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm1 = new AnnotatedCommodity(commodity1, new Annotation());
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm2 = new AnnotatedCommodity(commodity2, new Annotation());

            Amount amount1 = new Amount(0, annComm1);
            Amount amount2 = new Amount(0, annComm2);

            annComm1.Details.Date = (Date)DateTime.UtcNow.Date;
            Assert.Equal(1, Commodity.CompareByCommodity(amount1, amount2));
            Assert.Equal(-1, Commodity.CompareByCommodity(amount2, amount1));

            annComm1.Details.Date = (Date)DateTime.UtcNow.Date;
            annComm2.Details.Date = (Date)DateTime.UtcNow.Date.AddDays(1);
            Assert.Equal(-1, Commodity.CompareByCommodity(amount1, amount2));
            Assert.Equal(1, Commodity.CompareByCommodity(amount2, amount1));
        }

        [Fact]
        public void Commodity_CompareByCommodity_ComparesTags()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm1 = new AnnotatedCommodity(commodity1, new Annotation());
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm2 = new AnnotatedCommodity(commodity2, new Annotation());

            Amount amount1 = new Amount(0, annComm1);
            Amount amount2 = new Amount(0, annComm2);

            annComm1.Details.Tag = "tag-1";
            Assert.Equal(1, Commodity.CompareByCommodity(amount1, amount2));
            Assert.Equal(-1, Commodity.CompareByCommodity(amount2, amount1));

            annComm1.Details.Tag = "tag-1";
            annComm2.Details.Tag = "tag-2";
            Assert.Equal(-1, Commodity.CompareByCommodity(amount1, amount2));
            Assert.Equal(1, Commodity.CompareByCommodity(amount2, amount1));
        }

        [Fact]
        public void Commodity_CompareByCommodity_PerformsOrdinalComparisonForBaseSymbol()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("€"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("EUR"));

            Amount amount1 = new Amount(0, commodity1);
            Amount amount2 = new Amount(0, commodity2);

            Assert.True(Commodity.CompareByCommodity(amount1, amount2) > 0);
            Assert.True(Commodity.CompareByCommodity(amount2, amount1) < 0);
        }

        [Fact]
        public void Commodity_CompareByCommodity_ReturnsZeroForEqualAnnotatedCommodities()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm1 = new AnnotatedCommodity(commodity1, new Annotation());
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm2 = new AnnotatedCommodity(commodity2, new Annotation());

            Amount amount1 = new Amount(0, annComm1);
            Amount amount2 = new Amount(0, annComm2);

            Assert.Equal(0, Commodity.CompareByCommodity(amount1, amount2));

            annComm1.Details.Price = new Amount(5);
            annComm2.Details.Price = new Amount(5);

            Assert.Equal(0, Commodity.CompareByCommodity(amount1, amount2));

            annComm1.Details.Tag = "tag-1";
            annComm2.Details.Tag = "tag-1";

            Assert.Equal(0, Commodity.CompareByCommodity(amount1, amount2));

            annComm1.Details.Date = (Date)DateTime.UtcNow.Date;
            annComm2.Details.Date = annComm1.Details.Date;

            Assert.Equal(0, Commodity.CompareByCommodity(amount1, amount2));
        }

        [Fact]
        public void Commodity_Equals_IgnoresNulls()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            Assert.False(commodity1.Equals((Commodity)null));
        }

        [Fact]
        public void Commodity_Equals_ComparesBases()
        {
            CommodityBase commBase1 = new CommodityBase("comm1");
            CommodityBase commBase2 = new CommodityBase("comm2");
            Commodity commodity11 = new Commodity(CommodityPool.Current, commBase1);
            Commodity commodity12 = new Commodity(CommodityPool.Current, commBase1);
            Commodity commodity21 = new Commodity(CommodityPool.Current, commBase2);

            Assert.True(commodity11.Equals(commodity11));
            Assert.True(commodity11.Equals(commodity12));
            Assert.False(commodity11.Equals(commodity21));

            // [DM] Hide warning "Comparison made to the same variable"
            #pragma warning disable 1718
            Assert.True(commodity11 == commodity11);
            #pragma warning restore
            Assert.True(commodity11 == commodity12);
            Assert.True(commodity11 != commodity21);

        }

        [Fact]
        public void Commodity_Equals_IgnoresAnnotatedCommodities()
        {
            CommodityBase commBase1 = new CommodityBase("comm1");
            Commodity commodity11 = new Commodity(CommodityPool.Current, commBase1);
            Commodity commodity12 = new Commodity(CommodityPool.Current, commBase1);
            AnnotatedCommodity annCommodity = new AnnotatedCommodity(commodity11, new Annotation(new Amount(10), (Date)DateTime.Now.Date, "tag"));

            Assert.True(commodity11.Equals(commodity11));
            Assert.True(commodity11.Equals(commodity12));
            Assert.False(commodity11.Equals(annCommodity));

            // [DM] Hide warning "Comparison made to the same variable"
            #pragma warning disable 1718
            Assert.True(commodity11 == commodity11);
            #pragma warning restore
            Assert.True(commodity11 == commodity12);
            Assert.True(commodity11 != annCommodity);
        }

        [Fact]
        public void Commodity_Supports_IComparable()
        {
            CommodityBase commBase1 = new CommodityBase("comm1");
            Commodity commodity11 = new Commodity(CommodityPool.Current, commBase1) { QualifiedSymbol = "A1" };
            Commodity commodity12 = new Commodity(CommodityPool.Current, commBase1) { QualifiedSymbol = "A2" };

            IComparable comparable = commodity11 as IComparable;
            Assert.NotNull(comparable);

            var result = comparable.CompareTo(commodity12);
            Assert.Equal(-1, result);
        }

        [Fact]
        public void Commodity_ToString_ReturnsCommodityName()
        {
            CommodityBase commBase1 = new CommodityBase("comm1");
            Commodity commodity11 = new Commodity(CommodityPool.Current, commBase1);
            Assert.Equal("comm1", commodity11.ToString());
        }

        [Fact]
        public void Commodity_Flags_ReturnBaseFlags()
        {
            var flags = CommodityFlagsEnum.COMMODITY_KNOWN | CommodityFlagsEnum.COMMODITY_SAW_ANNOTATED | CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;
            CommodityBase commBase1 = new CommodityBase("comm1") { Flags = flags };
            Commodity commodity11 = new Commodity(CommodityPool.Current, commBase1);
            Assert.Equal(flags, commodity11.Flags);
        }

        [Fact]
        public void Commodity_Flags_ChangesAffectBaseFlags()
        {
            var flags = CommodityFlagsEnum.COMMODITY_KNOWN | CommodityFlagsEnum.COMMODITY_SAW_ANNOTATED;
            CommodityBase commBase1 = new CommodityBase("comm1") { Flags = flags };
            Commodity commodity11 = new Commodity(CommodityPool.Current, commBase1);
            commodity11.Flags |= CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED;
            Assert.Equal(flags | CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED, commBase1.Flags);
        }

        [Fact]
        public void Commodity_CompareByCommodityComparison_ReturnsZeroForEqualAmounts()
        {
            CommodityBase commBase1 = new CommodityBase("comm1");
            CommodityBase commBase2 = new CommodityBase("comm2");
            Commodity commodity1 = new Commodity(CommodityPool.Current, commBase1);
            Commodity commodity2 = new Commodity(CommodityPool.Current, commBase2);

            Amount amt1  = new Amount(10, commodity1);
            Amount amt1a = new Amount(10, commodity1);
            Amount amt2  = new Amount(10, commodity2);

            Assert.Equal( 0, Commodity.CompareByCommodityComparison(amt1, amt1a));
            Assert.Equal(-1, Commodity.CompareByCommodityComparison(amt1, amt2));
            Assert.Equal( 1, Commodity.CompareByCommodityComparison(amt2, amt1));
        }

        [Fact]
        public void Commodity_ExplicitBool_ChecksForNullCommodities()
        {
            Commodity commodity1 = null;
            CommodityBase commBase2 = new CommodityBase("comm1");
            Commodity commodity2 = new Commodity(CommodityPool.Current, commBase2);
            Commodity commodity3 = CommodityPool.Current.NullCommodity;

            Assert.False((bool)commodity1);
            Assert.True((bool)commodity2);
            Assert.False((bool)commodity3);
        }

        [Fact]
        public void Commodity_StripAnnotation_ReturnsAlreadyCreatedCommodityWithReducedAnnotation()
        {
            Amount price = new Amount(110);
            Annotation annotation1 = new Annotation(price, null, null);
            Annotation annotation2 = new Annotation(price, new Date(2017, 10, 10), "tag-string");

            Commodity commodity1 = CommodityPool.Current.FindOrCreate("comm-str-ann-test", annotation1);
            Commodity commodity2 = CommodityPool.Current.FindOrCreate("comm-str-ann-test", annotation1);

            Commodity result = commodity2.StripAnnotations(new AnnotationKeepDetails(keepPrice: true, keepDate: false, keepTag: false));

            Assert.True(commodity2 == result);
        }

        [Fact]
        public void Commodity_Valid_ReturnsFalseIfEmptySymbol()
        {
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase(null));
            Assert.True(String.IsNullOrEmpty(commodity.Symbol));
            Assert.False(commodity == CommodityPool.Current.NullCommodity);
            Assert.False(commodity.Valid());
        }

        [Fact]
        public void Commodity_Valid_ReturnsFalseIfPrecisionIsToBig()
        {
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("cmdtestvalidprec"));
            Assert.True(commodity.Valid());
            commodity.Precision = 17;
            Assert.False(commodity.Valid());
        }
    }
}

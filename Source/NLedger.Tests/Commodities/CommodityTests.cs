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
using NLedger.Annotate;
using NLedger.Commodities;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Commodities
{
    [TestClass]
    public class CommodityTests : TestFixture
    {
        [TestMethod]
        public void Commodity_ParseSymbol_ReturnsEmptyStringForEmptyString()
        {
            string s = null;
            Assert.AreEqual(String.Empty, Commodity.ParseSymbol(ref s));
            s = String.Empty;
            Assert.AreEqual(String.Empty, Commodity.ParseSymbol(ref s));
        }

        [TestMethod]
        public void Commodity_ParseSymbol_IgnoresLeadingSpaces()
        {
            string s = "asd";
            Assert.AreEqual("asd", Commodity.ParseSymbol(ref s));
            s = "  qwe";
            Assert.AreEqual("qwe", Commodity.ParseSymbol(ref s));
            s = "\tqwe";
            Assert.AreEqual("qwe", Commodity.ParseSymbol(ref s));
        }

        [TestMethod]
        public void Commodity_ParseSymbol_HandlesQuotedNames()
        {
            string s = "   \"asd\"qwe";
            Assert.AreEqual("asd", Commodity.ParseSymbol(ref s));
            Assert.AreEqual("qwe", s);
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError), AmountError.ErrorMessageNonClosedQuote)]
        public void Commodity_ParseSymbol_HandlesNonCkisedQuotes()
        {
            string s = "   \"asdqwe";
            Commodity.ParseSymbol(ref s);
        }

        [TestMethod]
        public void Commodity_ParseSymbol_TakesNonQuotedName()
        {
            string s = "  ABC DEF";
            Assert.AreEqual("ABC", Commodity.ParseSymbol(ref s));
            Assert.AreEqual(" DEF", s);

            s = "  ABC@DEF";
            Assert.AreEqual("ABC", Commodity.ParseSymbol(ref s));
            Assert.AreEqual("@DEF", s);

            s = "ABC:DEF";
            Assert.AreEqual("ABC", Commodity.ParseSymbol(ref s));
            Assert.AreEqual(":DEF", s);
        }

        [TestMethod]
        public void Commodity_ParseSymbol_IgnoresReservedTokens()
        {
            string original = "  and DEF";  // 'and' is a reserved token - see Commodity.ReservedTokens
            string s = original;
            Assert.AreEqual(String.Empty, Commodity.ParseSymbol(ref s));
            Assert.AreEqual(original, s);
        }

        [TestMethod]
        public void Commodity_StripAnnotations_ReturnsTheSameInstance()
        {
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Commodity strippedCommodity = commodity.StripAnnotations(new AnnotationKeepDetails());
            Assert.AreEqual(commodity, strippedCommodity);
        }

        [TestMethod]
        public void Commodity_CompareByCommodity_ComparesBaseSymbolsIfTheyAreNotEqual()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm2"));

            Amount amount1 = new Amount(BigInt.FromLong(0), commodity1);
            Amount amount2 = new Amount(BigInt.FromLong(0), commodity2);

            Assert.IsTrue(Commodity.CompareByCommodity(amount1, amount2));
            Assert.IsFalse(Commodity.CompareByCommodity(amount2, amount1));
        }

        [TestMethod]
        public void Commodity_CompareByCommodity_ChecksWhetherCommoditiesAreAnnotated()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm1 = new AnnotatedCommodity(commodity1, new Annotation());
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));

            Amount amount1 = new Amount(BigInt.FromLong(0), commodity1);
            Amount amount2 = new Amount(BigInt.FromLong(0), annComm1);
            Amount amount3 = new Amount(BigInt.FromLong(0), commodity2);

            Assert.IsTrue(Commodity.CompareByCommodity(amount1, amount2));
            Assert.IsFalse(Commodity.CompareByCommodity(amount1, amount3));

            Assert.IsFalse(Commodity.CompareByCommodity(amount2, amount1));
        }

        [TestMethod]
        public void Commodity_CompareByCommodity_ComparesPrices()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm1 = new AnnotatedCommodity(commodity1, new Annotation());
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm2 = new AnnotatedCommodity(commodity2, new Annotation());

            Amount amount1 = new Amount(BigInt.FromLong(0), annComm1);
            Amount amount2 = new Amount(BigInt.FromLong(0), annComm2);

            annComm1.Details.Price = new Amount(10);
            Assert.IsFalse(Commodity.CompareByCommodity(amount1, amount2));
            Assert.IsTrue(Commodity.CompareByCommodity(amount2, amount1));

            annComm1.Details.Price = new Amount(5);
            annComm2.Details.Price = new Amount(10);
            Assert.IsTrue(Commodity.CompareByCommodity(amount1, amount2));
            Assert.IsFalse(Commodity.CompareByCommodity(amount2, amount1));
        }

        [TestMethod]
        public void Commodity_CompareByCommodity_ComparesDates()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm1 = new AnnotatedCommodity(commodity1, new Annotation());
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm2 = new AnnotatedCommodity(commodity2, new Annotation());

            Amount amount1 = new Amount(BigInt.FromLong(0), annComm1);
            Amount amount2 = new Amount(BigInt.FromLong(0), annComm2);

            annComm1.Details.Date = (Date)DateTime.UtcNow.Date;
            Assert.IsFalse(Commodity.CompareByCommodity(amount1, amount2));
            Assert.IsTrue(Commodity.CompareByCommodity(amount2, amount1));

            annComm1.Details.Date = (Date)DateTime.UtcNow.Date;
            annComm2.Details.Date = (Date)DateTime.UtcNow.Date.AddDays(1);
            Assert.IsTrue(Commodity.CompareByCommodity(amount1, amount2));
            Assert.IsFalse(Commodity.CompareByCommodity(amount2, amount1));
        }

        [TestMethod]
        public void Commodity_CompareByCommodity_ComparesTags()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm1 = new AnnotatedCommodity(commodity1, new Annotation());
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            AnnotatedCommodity annComm2 = new AnnotatedCommodity(commodity2, new Annotation());

            Amount amount1 = new Amount(BigInt.FromLong(0), annComm1);
            Amount amount2 = new Amount(BigInt.FromLong(0), annComm2);

            annComm1.Details.Tag = "tag-1";
            Assert.IsFalse(Commodity.CompareByCommodity(amount1, amount2));
            Assert.IsTrue(Commodity.CompareByCommodity(amount2, amount1));

            annComm1.Details.Tag = "tag-1";
            annComm2.Details.Tag = "tag-2";
            Assert.IsTrue(Commodity.CompareByCommodity(amount1, amount2));
            Assert.IsFalse(Commodity.CompareByCommodity(amount2, amount1));
        }

        [TestMethod]
        public void Commodity_CompareByCommodity_PerformsOrdinalComparisonForBaseSymbol()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("€"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("EUR"));

            Amount amount1 = new Amount(BigInt.FromLong(0), commodity1);
            Amount amount2 = new Amount(BigInt.FromLong(0), commodity2);

            Assert.IsFalse(Commodity.CompareByCommodity(amount1, amount2));
            Assert.IsTrue(Commodity.CompareByCommodity(amount2, amount1));
        }

        [TestMethod]
        public void Commodity_Equals_IgnoresNulls()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            Assert.IsFalse(commodity1.Equals((Commodity)null));
        }

        [TestMethod]
        public void Commodity_Equals_ComparesBases()
        {
            CommodityBase commBase1 = new CommodityBase("comm1");
            CommodityBase commBase2 = new CommodityBase("comm2");
            Commodity commodity11 = new Commodity(CommodityPool.Current, commBase1);
            Commodity commodity12 = new Commodity(CommodityPool.Current, commBase1);
            Commodity commodity21 = new Commodity(CommodityPool.Current, commBase2);

            Assert.IsTrue(commodity11.Equals(commodity11));
            Assert.IsTrue(commodity11.Equals(commodity12));
            Assert.IsFalse(commodity11.Equals(commodity21));

            // [DM] Hide warning "Comparison made to the same variable"
            #pragma warning disable 1718
            Assert.IsTrue(commodity11 == commodity11);
            #pragma warning restore
            Assert.IsTrue(commodity11 == commodity12);
            Assert.IsTrue(commodity11 != commodity21);

        }

        [TestMethod]
        public void Commodity_Equals_IgnoresAnnotatedCommodities()
        {
            CommodityBase commBase1 = new CommodityBase("comm1");
            Commodity commodity11 = new Commodity(CommodityPool.Current, commBase1);
            Commodity commodity12 = new Commodity(CommodityPool.Current, commBase1);
            AnnotatedCommodity annCommodity = new AnnotatedCommodity(commodity11, new Annotation(new Amount(10), (Date)DateTime.Now.Date, "tag"));

            Assert.IsTrue(commodity11.Equals(commodity11));
            Assert.IsTrue(commodity11.Equals(commodity12));
            Assert.IsFalse(commodity11.Equals(annCommodity));

            // [DM] Hide warning "Comparison made to the same variable"
            #pragma warning disable 1718
            Assert.IsTrue(commodity11 == commodity11);
            #pragma warning restore
            Assert.IsTrue(commodity11 == commodity12);
            Assert.IsTrue(commodity11 != annCommodity);
        }

        [TestMethod]
        public void Commodity_Supports_IComparable()
        {
            CommodityBase commBase1 = new CommodityBase("comm1");
            Commodity commodity11 = new Commodity(CommodityPool.Current, commBase1) { QualifiedSymbol = "A1" };
            Commodity commodity12 = new Commodity(CommodityPool.Current, commBase1) { QualifiedSymbol = "A2" };

            IComparable comparable = commodity11 as IComparable;
            Assert.IsNotNull(comparable);

            var result = comparable.CompareTo(commodity12);
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void Commodity_ToString_ReturnsCommodityName()
        {
            CommodityBase commBase1 = new CommodityBase("comm1");
            Commodity commodity11 = new Commodity(CommodityPool.Current, commBase1);
            Assert.AreEqual("comm1", commodity11.ToString());
        }

        [TestMethod]
        public void Commodity_Flags_ReturnBaseFlags()
        {
            var flags = CommodityFlagsEnum.COMMODITY_KNOWN | CommodityFlagsEnum.COMMODITY_SAW_ANNOTATED | CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;
            CommodityBase commBase1 = new CommodityBase("comm1") { Flags = flags };
            Commodity commodity11 = new Commodity(CommodityPool.Current, commBase1);
            Assert.AreEqual(flags, commodity11.Flags);
        }

        [TestMethod]
        public void Commodity_Flags_ChangesAffectBaseFlags()
        {
            var flags = CommodityFlagsEnum.COMMODITY_KNOWN | CommodityFlagsEnum.COMMODITY_SAW_ANNOTATED;
            CommodityBase commBase1 = new CommodityBase("comm1") { Flags = flags };
            Commodity commodity11 = new Commodity(CommodityPool.Current, commBase1);
            commodity11.Flags |= CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED;
            Assert.AreEqual(flags | CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED, commBase1.Flags);
        }

        [TestMethod]
        public void Commodity_CompareByCommodityComparison_ReturnsZeroForEqualAmounts()
        {
            CommodityBase commBase1 = new CommodityBase("comm1");
            CommodityBase commBase2 = new CommodityBase("comm2");
            Commodity commodity1 = new Commodity(CommodityPool.Current, commBase1);
            Commodity commodity2 = new Commodity(CommodityPool.Current, commBase2);

            Amount amt1  = new Amount(BigInt.FromLong(10), commodity1);
            Amount amt1a = new Amount(BigInt.FromLong(10), commodity1);
            Amount amt2  = new Amount(BigInt.FromLong(10), commodity2);

            Assert.AreEqual( 0, Commodity.CompareByCommodityComparison(amt1, amt1a));
            Assert.AreEqual(-1, Commodity.CompareByCommodityComparison(amt1, amt2));
            Assert.AreEqual( 1, Commodity.CompareByCommodityComparison(amt2, amt1));
        }

        [TestMethod]
        public void Commodity_ExplicitBool_ChecksForNullCommodities()
        {
            Commodity commodity1 = null;
            CommodityBase commBase2 = new CommodityBase("comm1");
            Commodity commodity2 = new Commodity(CommodityPool.Current, commBase2);
            Commodity commodity3 = CommodityPool.Current.NullCommodity;

            Assert.IsFalse((bool)commodity1);
            Assert.IsTrue((bool)commodity2);
            Assert.IsFalse((bool)commodity3);
        }

        [TestMethod]
        public void Commodity_StripAnnotation_ReturnsAlreadyCreatedCommodityWithReducedAnnotation()
        {
            Amount price = new Amount(110);
            Annotation annotation1 = new Annotation(price, null, null);
            Annotation annotation2 = new Annotation(price, new Date(2017, 10, 10), "tag-string");

            Commodity commodity1 = CommodityPool.Current.FindOrCreate("comm-str-ann-test", annotation1);
            Commodity commodity2 = CommodityPool.Current.FindOrCreate("comm-str-ann-test", annotation1);

            Commodity result = commodity2.StripAnnotations(new AnnotationKeepDetails(keepPrice: true, keepDate: false, keepTag: false));

            Assert.IsTrue(commodity2 == result);
        }

    }
}

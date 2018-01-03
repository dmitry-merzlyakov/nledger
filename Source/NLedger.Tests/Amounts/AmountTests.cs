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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Amounts
{
    [TestClass]
    public class AmountTests : TestFixture
    {
        [TestMethod]
        public void Amount_ParseQuantity_ReturnsEmptyForEmptyString()
        {
            string line = null;
            Assert.AreEqual(String.Empty, Amount.ParseQuantity(ref line));
            line = String.Empty;
            Assert.AreEqual(String.Empty, Amount.ParseQuantity(ref line));
        }

        [TestMethod]
        public void Amount_ParseQuantity_IgnoresInitialWhiteSpaces()
        {
            string line = "12345";
            Assert.AreEqual("12345", Amount.ParseQuantity(ref line));

            line = "   12345";
            Assert.AreEqual("12345", Amount.ParseQuantity(ref line));

            line = "\t12345";
            Assert.AreEqual("12345", Amount.ParseQuantity(ref line));
        }

        [TestMethod]
        public void Amount_ParseQuantity_ReadsDigitsAndOtherChars()
        {
            string line = "12345$ rest of line";
            Assert.AreEqual("12345", Amount.ParseQuantity(ref line));
            Assert.AreEqual("$ rest of line", line);

            line = "-12,345 rest of line";
            Assert.AreEqual("-12,345", Amount.ParseQuantity(ref line));
            Assert.AreEqual(" rest of line", line);

            line = "-12,34.5rest of line";
            Assert.AreEqual("-12,34.5", Amount.ParseQuantity(ref line));
            Assert.AreEqual("rest of line", line);
        }

        [TestMethod]
        public void Amount_ParseQuantity_IgnoresTrailingNonDigitChars()
        {
            string line = "12345, rest of line";
            Assert.AreEqual("12345", Amount.ParseQuantity(ref line));
            Assert.AreEqual(", rest of line", line);

            line = "-12,345. rest of line";
            Assert.AreEqual("-12,345", Amount.ParseQuantity(ref line));
            Assert.AreEqual(". rest of line", line);

            line = "-12,34.5. rest of line";
            Assert.AreEqual("-12,34.5", Amount.ParseQuantity(ref line));
            Assert.AreEqual(". rest of line", line);
        }

        [TestMethod]
        public void Amount_Parse_Integration()
        {
            Amount amount1 = new Amount(default(BigInt), null);
            string line1 = "-10 USD";
            amount1.Parse(ref line1, AmountParseFlagsEnum.PARSE_DEFAULT);
            Assert.AreEqual(BigInt.FromInt(-10), amount1.Quantity);
            Assert.AreEqual("USD", amount1.Commodity.BaseSymbol);
            Assert.AreEqual(String.Empty, line1);

            Amount amount2 = new Amount(default(BigInt), null);
            string line2 = "99 USD";
            amount2.Parse(ref line2, AmountParseFlagsEnum.PARSE_DEFAULT);
            Assert.AreEqual(BigInt.FromInt(99), amount2.Quantity);
            Assert.AreEqual("USD", amount2.Commodity.BaseSymbol);
            Assert.AreEqual(String.Empty, line2);

            Amount amount3 = new Amount(default(BigInt), null);
            string line3 = "STS 99.99";
            amount3.Parse(ref line3, AmountParseFlagsEnum.PARSE_DEFAULT);
            Assert.AreEqual(BigInt.Parse("99.99", 2), amount3.Quantity);  // Precision = 2
            Assert.AreEqual("STS", amount3.Commodity.BaseSymbol);
            Assert.AreEqual(String.Empty, line3);
        }

        [TestMethod]
        public void Amount_StripAnnotations_ReturnsOriginalAmountIfKeepAllIsTrue()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Amount amount = new Amount(BigInt.FromLong(10), comm);
            AnnotationKeepDetails keepDetails = new AnnotationKeepDetails();

            // Commodity is not annotated - it is enough condition to return the original object
            Amount newAmount = amount.StripAnnotations(keepDetails);

            Assert.AreEqual(amount, newAmount);
        }

        [TestMethod]
        public void Amount_StripAnnotations_ReturnsNewAmountForAnnotatedCommodity()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annComm = new AnnotatedCommodity(comm, new Annotation());
            Amount amount = new Amount(BigInt.FromLong(10), annComm);
            AnnotationKeepDetails keepDetails = new AnnotationKeepDetails();

            // The original ampunt has annotated commodity, but "keepDetails" does not specify anything to keep.
            // Therefore, the new amount has not annotated commodity
            Amount newAmount = amount.StripAnnotations(keepDetails);
            Assert.IsFalse(newAmount.Commodity.IsAnnotated);
        }

        [TestMethod]
        public void Amount_FitsInLong_ReturnsTrueIfQuantityCanBeConvertedToLong()
        {
            Amount tooBigNegativeAmount = new Amount(BigInt.Parse("-99999999999999999999"), null);
            Assert.IsFalse(tooBigNegativeAmount.FitsInLong);

            Amount normalAmount = new Amount(BigInt.Parse("99"), null);
            Assert.IsTrue(normalAmount.FitsInLong);

            Amount tooBigPositiveAmount = new Amount(BigInt.Parse("99999999999999999999"), null);
            Assert.IsFalse(tooBigPositiveAmount.FitsInLong);
        }

        [TestMethod]
        public void Amount_IsRealZero_IndicatesThatAmountHasZeroValue()
        {
            // Not applicable - method Sign requires non-empty Quantity. Confirmed by Boost auto-tests
            // Amount amount = new Amount();
            // Assert.IsTrue(amount.IsRealZero);

            Amount amount = new Amount(0);
            Assert.IsTrue(amount.IsRealZero);

            amount = new Amount(BigInt.Parse("0"), null);
            Assert.IsTrue(amount.IsRealZero);
        }

        [TestMethod]
        public void Amount_DivideBy_ReturnsDividedAmount()
        {
            Amount amount10 = new Amount(BigInt.Parse("10", 2), null);
            Amount amount4 = new Amount(BigInt.Parse("4", 2), null);

            Amount amount = amount10.InPlaceDivide(amount4);

            int expectedPrecision = 2 + 2 + Amount.ExtendByDigits;
            Assert.AreEqual(BigInt.Parse("2.5", expectedPrecision), amount.Quantity);
        }

        [TestMethod]
        public void Amount_Multiply_ReturnsMultipliedAmount()
        {
            Amount amount10 = new Amount(BigInt.Parse("10", 2), null);
            Amount amount4 = new Amount(BigInt.Parse("4", 2), null);

            Amount amount = amount10.Multiply(amount4);

            int expectedPrecision = 2 + 2;
            Assert.AreEqual(BigInt.Parse("40", expectedPrecision), amount.Quantity);
        }

        [TestMethod]
        public void Amount_Subtract_ReturnsSubtractedAmountForNoCommodities()
        {
            Amount amount10 = new Amount(10);
            Amount amount8 = new Amount(8);

            Amount amount = amount10.InPlaceSubtract(amount8);

            Assert.AreEqual(2, amount.Quantity.ToLong());
        }

        [TestMethod]
        public void Amount_Subtract_ReturnsSubtractedAmountForTheSameCommodity()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Amount amount10 = new Amount(BigInt.FromLong(10), comm);
            Amount amount8 = new Amount(BigInt.FromLong(8), comm);

            Amount amount = amount10.InPlaceSubtract(amount8);

            Assert.AreEqual(2, amount.Quantity.ToLong());
        }

        [TestMethod]
        public void Amount_Negated_ReturnsInvertedAmount()
        {
            Amount amount = new Amount(8);
            Assert.AreEqual(-8, amount.Negated().Quantity.ToLong());
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_HasAnnotation_FailsIfQuantityIsNotSpecified()
        {
            Amount amount = new Amount();
            Assert.IsFalse(amount.HasAnnotation);
        }

        [TestMethod]
        public void Amount_HasAnnotation_ReturnsFalseIfNoCommodity()
        {
            Amount amount = new Amount(1);
            Assert.IsFalse(amount.HasAnnotation);
        }

        [TestMethod]
        public void Amount_HasAnnotation_ReturnsFalseIfCommodityIsNotAnnotated()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Amount amount = new Amount(BigInt.FromInt(1), comm);
            Assert.IsFalse(amount.HasAnnotation);
        }

        [TestMethod]
        public void Amount_HasAnnotation_ReturnsTrueIfCommodityIsAnnotated()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annComm = new AnnotatedCommodity(comm, new Annotation());
            Amount amount = new Amount(BigInt.FromInt(1), annComm);
            Assert.IsTrue(amount.HasAnnotation);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Amount_HasAnnotation_FailsIfAnnotatedCommodityHasNoDetails()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annComm = new AnnotatedCommodity(comm, null);  /* not sure it is a valid case. TBD */
            Amount amount = new Amount(BigInt.FromInt(1), annComm);
            Assert.IsTrue(amount.HasAnnotation);
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_IsZero_FailsIfQuantityIsEmpty()
        {
            Amount amount = new Amount();
            Assert.IsTrue(amount.IsZero);
        }

        [TestMethod]
        public void Amount_IsZero_ReturnsRealZeroIfNoCommodity()
        {
            Amount amount = new Amount(BigInt.Parse("0.00005", 2), null);  // Notice that precision less than a value.
            Assert.IsFalse(amount.IsZero);
        }

        [TestMethod]
        public void Amount_IsZero_ReturnsPrecisionedZeroIfItHasCommodityAndNoKeepPrecision()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Amount amount = new Amount(BigInt.Parse("0.00005", 2), comm);  // Notice that precision less than a value.
            Assert.IsTrue(amount.IsZero);
        }

        [TestMethod]
        public void Amount_IsZero_PrecisionedZeroUsesCommodityPrecision()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm")) { Precision = 2 };
            Amount amount = new Amount(BigInt.Parse("0.00005", 10), comm);  // Notice commodity's precision has higher priority
            Assert.IsTrue(amount.IsZero);
        }

        [TestMethod]
        public void Amount_IsZero_RoundsToCommodityPrecision()
        {
            Commodity commodityA = new Commodity(CommodityPool.Current, new CommodityBase("AmtNZeroA")) { Precision = 2 };
            // Set a value that less than commodity precision (2) but higher than quantity precision (8)
            Amount amountA = new Amount(BigInt.Parse("0.008", 8), commodityA);
            Assert.IsFalse(amountA.IsZero);  // The value is rounded to 0.01 accorrding to Commodity precision
        }

        [TestMethod]
        public void Amount_SetCommodity_SetsZeroIfQuantityIsEmpty()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Amount amount = new Amount();

            Assert.IsFalse(amount.HasCommodity);
            amount.SetCommodity(comm);
            Assert.IsTrue(amount.HasCommodity);
            Assert.IsTrue(amount.IsZero);
            Assert.IsTrue(amount.IsRealZero);
        }

        [TestMethod]
        public void Amount_ParseConversion_SetsUpConversionCommodities()
        {
            CommodityPool.Cleanup();
            Amount.ParseConversion("1.0m", "60s");

            Commodity larger = CommodityPool.Current.Find("m");
            Assert.AreEqual("m", larger.Symbol);
            Assert.IsNull(larger.Larger);
            Assert.IsNotNull(larger.Smaller);
            Assert.AreEqual("s", larger.Smaller.Commodity.Symbol);
            Assert.AreEqual(60, larger.Smaller.Quantity.ToLong());

            Commodity smaller = CommodityPool.Current.Find("s");
            Assert.AreEqual("s", smaller.Symbol);
            Assert.IsNotNull(smaller.Larger);
            Assert.IsNull(smaller.Smaller);
            Assert.AreEqual("m", smaller.Larger.Commodity.Symbol);
            Assert.AreEqual(60, smaller.Larger.Quantity.ToLong());
        }

        [TestMethod]
        public void Amount_IsLessThan_ChecksWhetherGivenAmountLessThanAnother()
        {
            Amount amount1 = new Amount(10);
            Amount amount2 = new Amount(20);
            Assert.IsTrue(amount1.IsLessThan(amount2));
            Assert.IsFalse(amount2.IsLessThan(amount1));
        }

        [TestMethod]
        public void Amount_IsGreaterThan_ChecksWhetherGivenAmountGreaterThanAnother()
        {
            Amount amount1 = new Amount(10);
            Amount amount2 = new Amount(20);
            Assert.IsFalse(amount1.IsGreaterThan(amount2));
            Assert.IsTrue(amount2.IsGreaterThan(amount1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Amount_Compare_FailsIfAmountIsNull()
        {
            Amount amount1 = new Amount(10);
            amount1.Compare(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_Compare_FailsIfQuantityIsNotSpecified()
        {
            Amount amount1 = new Amount(10);
            amount1.Compare(new Amount());
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_Compare_FailsIfCommoditiesAreDifferent()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));
            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);
            Amount amount2 = new Amount(BigInt.FromInt(12), commodity2);
            amount1.Compare(amount2);
        }

        [TestMethod]
        public void Amount_Compare_ReturnsMinusOneOrZeroOrOne()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);
            Amount amount2 = new Amount(BigInt.FromInt(12), commodity1);

            Assert.AreEqual(-1, amount1.Compare(amount2));
            Assert.AreEqual(0, amount1.Compare(amount1));
            Assert.AreEqual(1, amount2.Compare(amount1));
        }

        [TestMethod]
        public void Amount_Abs_ReturnsNewAmountWithAbsoluteValue()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);
            Amount amount2 = new Amount(BigInt.FromInt(-10), commodity1);

            Amount result1 = amount1.Abs();
            Amount result2 = amount2.Abs();

            Assert.AreEqual(10, result1.Quantity.ToLong());
            Assert.AreEqual(10, result2.Quantity.ToLong());

            Assert.IsTrue(Object.ReferenceEquals(result1, amount1));  // If the value is not changed, return the original object
            Assert.IsFalse(Object.ReferenceEquals(result2, amount2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Amount_Add_NullAmountCausesException()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);
            amount1.InPlaceAdd(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Amount_Add_InvalidAmountCausesException()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);
            Amount amount2 = new Amount(new BigInt(), commodity1);

            Assert.IsFalse(amount2.Valid());
            amount1.InPlaceAdd(amount2);
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_Add_UninitializedFirstAmountCausesException()
        {
            Amount amount1 = new Amount();
            Amount amount2 = new Amount(10);

            Assert.IsTrue(amount2.Valid());
            amount1.InPlaceAdd(amount2);
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_Add_UninitializedSecondAmountCausesException()
        {
            Amount amount1 = new Amount(10);
            Amount amount2 = new Amount();

            Assert.IsTrue(amount2.Valid());
            amount1.InPlaceAdd(amount2);
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_Add_TwoUninitializedAmountsCauseException()
        {
            Amount amount1 = new Amount();
            Amount amount2 = new Amount();

            Assert.IsTrue(amount2.Valid());
            amount1.InPlaceAdd(amount2);
        }

        [TestMethod]
        public void Amount_Add_AddsAmountsWithoutCommodities()
        {
            Amount amount1 = new Amount(10);
            Amount amount2 = new Amount(20);

            Amount result = amount1.InPlaceAdd(amount2);

            Assert.AreEqual(30, result.Quantity.ToLong());
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_Add_ChecksThatBothCommoditiesAreEqual()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));
            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);
            Amount amount2 = new Amount(BigInt.FromInt(20), commodity2);

            amount1.InPlaceAdd(amount2);
        }

        [TestMethod]
        public void Amount_Add_ChangesPrecision()
        {
            BigInt quantity1 = BigInt.FromInt(10);
            quantity1 = quantity1.SetPrecision(2);
            Amount amount1 = new Amount(quantity1, null);

            BigInt quantity2 = BigInt.FromInt(20);
            quantity2 = quantity2.SetPrecision(3);
            Amount amount2 = new Amount(quantity2, null);

            Amount result = amount1.InPlaceAdd(amount2);
            Assert.AreEqual(2, result.Quantity.Precision);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Amount_Subtract_NullAmountCausesException()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);
            amount1.InPlaceSubtract(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Amount_Subtract_InvalidAmountCausesException()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);
            Amount amount2 = new Amount(new BigInt(), commodity1);

            Assert.IsFalse(amount2.Valid());
            amount1.InPlaceSubtract(amount2);
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_Subtract_UninitializedFirstAmountCausesException()
        {
            Amount amount1 = new Amount();
            Amount amount2 = new Amount(10);

            Assert.IsTrue(amount2.Valid());
            amount1.InPlaceSubtract(amount2);
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_Subtract_UninitializedSecondAmountCausesException()
        {
            Amount amount1 = new Amount(10);
            Amount amount2 = new Amount();

            Assert.IsTrue(amount2.Valid());
            amount1.InPlaceSubtract(amount2);
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_Subtract_TwoUninitializedAmountsCauseException()
        {
            Amount amount1 = new Amount();
            Amount amount2 = new Amount();

            Assert.IsTrue(amount2.Valid());
            amount1.InPlaceSubtract(amount2);
        }

        [TestMethod]
        public void Amount_Subtract_SubtractsAmountsWithoutCommodities()
        {
            Amount amount1 = new Amount(30);
            Amount amount2 = new Amount(20);

            Amount result = amount1.InPlaceSubtract(amount2);

            Assert.AreEqual(10, result.Quantity.ToLong());
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_Subtract_ChecksThatBothCommoditiesAreEqual()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));
            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);
            Amount amount2 = new Amount(BigInt.FromInt(20), commodity2);

            amount1.InPlaceSubtract(amount2);
        }

        [TestMethod]
        public void Amount_Subtract_ChangesPrecision()
        {
            BigInt quantity1 = BigInt.FromInt(30);
            quantity1 = quantity1.SetPrecision(2);
            Amount amount1 = new Amount(quantity1, null);

            BigInt quantity2 = BigInt.FromInt(20);
            quantity2 = quantity2.SetPrecision(3);
            Amount amount2 = new Amount(quantity2, null);

            Amount result = amount1.InPlaceSubtract(amount2);
            Assert.AreEqual(2, result.Quantity.Precision);
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_Annotate_FailsIfNoQuantity()
        {
            Amount amount1 = new Amount(new BigInt(), null);
            amount1.Annotate(new Annotation());
        }

        [TestMethod]
        public void Amount_Annotate_DoesNothingIfNoCOmmodity()
        {
            Amount amount1 = new Amount(BigInt.FromInt(10), null);
            amount1.Annotate(new Annotation());
            Assert.IsFalse(amount1.HasAnnotation);
        }

        [TestMethod]
        public void Amount_Annotate_CreatesAnnotatedCommodityForNonAnnotated()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);

            Assert.IsFalse(amount1.Commodity.IsAnnotated);
            amount1.Annotate(new Annotation());
            
            Assert.IsTrue(amount1.HasAnnotation);
            Assert.IsTrue(amount1.Commodity.IsAnnotated);
        }

        [TestMethod]
        public void Amount_Annotate_CreatesNewAnnotatedCommodityForAnnotated()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("aac-base-1"));
            AnnotatedCommodity annotatedCommodity1 = new AnnotatedCommodity(commodity1, new Annotation());
            Amount amount1 = new Amount(BigInt.FromInt(10), annotatedCommodity1);
            Annotation newAnnotation = new Annotation();

            Assert.IsTrue(amount1.Commodity.IsAnnotated);
            amount1.Annotate(newAnnotation);

            Assert.IsTrue(amount1.HasAnnotation);
            Assert.IsTrue(amount1.Commodity.IsAnnotated);

            AnnotatedCommodity resultCommodity = (AnnotatedCommodity)amount1.Commodity;
            Assert.AreEqual(newAnnotation, resultCommodity.Details);
        }

        [TestMethod]
        public void Amount_ClearCommodity_ClearsCommodity()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);

            Assert.IsNotNull(amount1.Commodity);
            amount1.ClearCommodity();
            Assert.AreEqual(CommodityPool.Current.NullCommodity, amount1.Commodity);
        }

        [TestMethod]
        public void Amount_GetInvertedQuantity_ReturnsInvertedQuantityOrZrero()
        {
            Amount amount1 = new Amount(10);
            BigInt result1 = amount1.GetInvertedQuantity();
            Assert.AreEqual(0.1m, result1.ToDecimal());

            Amount amount2 = new Amount();
            BigInt result2 = amount2.GetInvertedQuantity();
            Assert.AreEqual(0m, result2.ToDecimal());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Amount_Merge_FailsWithEmptyArgument()
        {
            Amount amount1 = new Amount();
            amount1.Merge(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Amount_Merge_FailsWithEqualCommodity()
        {
            Amount amount1 = new Amount();
            Amount amount2 = new Amount();
            amount1.Merge(amount2);
        }

        [TestMethod]
        public void Amount_Merge_ReturnsMultipliesValues()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);
            Amount amount2 = new Amount(BigInt.FromInt(20), commodity2);

            Amount result = amount1.Merge(amount2);
            Assert.AreEqual(200, result.Quantity.ToLong());
            Assert.AreEqual(commodity2, result.Commodity);
        }

        [TestMethod]
        public void Amount_ToString_ReturnsPrintedValue()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("ABC"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("CDE"));

            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);
            Amount amount2 = new Amount(BigInt.FromInt(20), commodity2);
            Amount amount3 = new Amount(30);

            Assert.AreEqual("ABC10", amount1.ToString());
            Assert.AreEqual("CDE20", amount2.ToString());
            Assert.AreEqual("30", amount3.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(AmountError))]
        public void Amount_DisplayPrecision_FailsIfNoQuantity()
        {
            Amount amount = new Amount();
            Assert.IsFalse(amount.Quantity.HasValue);
            int precision = amount.DisplayPrecision;
        }

        [TestMethod]
        public void Amount_DisplayPrecision_ReturnsQuantityPrecisionIfNoCommodity()
        {
            BigInt quantity = BigInt.Parse("121", 3);
            Amount amount = new Amount(quantity, null);
            Assert.AreEqual(3, amount.DisplayPrecision);
        }

        [TestMethod]
        public void Amount_DisplayPrecision_ReturnsCommodityPrecisionIfNotKeepPrecision()
        {
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("ABC"));
            commodity.Precision = 3;

            BigInt quantity = BigInt.Parse("121", 4).SetKeepPrecision(false);

            Amount amount = new Amount(quantity, commodity);
            Assert.AreEqual(3, amount.DisplayPrecision);
        }

        [TestMethod]
        public void Amount_DisplayPrecision_ReturnsMaxCommodityOrQuantityPrecision()
        {
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("ABC"));
            commodity.Precision = 3;

            BigInt quantity = BigInt.Parse("121", 4).SetKeepPrecision(true);

            Amount amount = new Amount(quantity, commodity);
            Assert.AreEqual(4, amount.DisplayPrecision);
        }

        [TestMethod]
        public void Amount_Print_UsesQuantityPrint()
        {
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("ABC"));
            commodity.Precision = 3;
            BigInt quantity = BigInt.Parse("121", 4).SetKeepPrecision(true);
            Amount amount = new Amount(quantity, commodity);

            string result = amount.Print();
            Assert.AreEqual("ABC121.000", result);
        }

        [TestMethod]
        public void Amount_IsNullOrEmpty_ReturnsTrueIfAmpuntIsEmpty()
        {
            // null
            Assert.IsTrue(Amount.IsNullOrEmpty(null));

            // empty
            Amount empty = new Amount();
            Assert.IsTrue(Amount.IsNullOrEmpty(empty));
            Assert.IsTrue(empty.IsEmpty);

            // non-empty
            Amount ten = new Amount(10);
            Assert.IsFalse(Amount.IsNullOrEmpty(ten));
            Assert.IsFalse(ten.IsEmpty);
        }

        [TestMethod]
        public void Amount_Print_DoesNotAddSuffixSpaceInCaseCommodityStyleSeparatedNotSpecified()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("TKP1"));
            commodity1.Flags |= CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED;
            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);
            Assert.AreEqual("10TKP1", amount1.Print());
        }

        [TestMethod]
        public void Amount_Print_AddsSuffixSpaceInCaseCommodityStyleSeparatedSpecified()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("TKP1"));
            commodity1.Flags |= CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED | CommodityFlagsEnum.COMMODITY_STYLE_SEPARATED; /* COMMODITY_STYLE_SEPARATED is added */ 
            Amount amount1 = new Amount(BigInt.FromInt(10), commodity1);
            Assert.AreEqual("10 TKP1", amount1.Print()); /* Space between the number and commodity */
        }

        [TestMethod]
        public void Amount_Merge_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(BigInt.FromInt(10).SetKeepPrecision(true), null);
            Amount amt2 = new Amount(BigInt.FromInt(20), commodity1);

            Amount result = amt1.Merge(amt2);
            Assert.IsTrue(result.KeepPrecision);
        }

        [TestMethod]
        public void Amount_Multiply_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(BigInt.FromInt(10).SetKeepPrecision(true), null);
            Amount amt2 = new Amount(BigInt.FromInt(20), commodity1);

            Amount result = amt1.Multiply(amt2);
            Assert.IsTrue(result.KeepPrecision);
        }

        [TestMethod]
        public void Amount_Add_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(BigInt.FromInt(10).SetKeepPrecision(true), null);
            Amount amt2 = new Amount(BigInt.FromInt(20), commodity1);

            Amount result = amt1.InPlaceAdd(amt2);
            Assert.IsTrue(result.KeepPrecision);
        }

        [TestMethod]
        public void Amount_Subtract_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(BigInt.FromInt(10).SetKeepPrecision(true), null);
            Amount amt2 = new Amount(BigInt.FromInt(5), commodity1);

            Amount result = amt1.InPlaceSubtract(amt2);
            Assert.IsTrue(result.KeepPrecision);
        }

        [TestMethod]
        public void Amount_Abs_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(BigInt.FromInt(10).SetKeepPrecision(true), null);

            Amount result = amt1.Abs();
            Assert.IsTrue(result.KeepPrecision);
        }

        [TestMethod]
        public void Amount_Negated_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(BigInt.FromInt(10).SetKeepPrecision(true), null);

            Amount result = amt1.Negated();
            Assert.IsTrue(result.KeepPrecision);
        }

        [TestMethod]
        public void Amount_StripAnnotation_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(BigInt.FromInt(10).SetKeepPrecision(true), null);

            Amount result = amt1.StripAnnotations(new AnnotationKeepDetails());
            Assert.IsTrue(result.KeepPrecision);
        }

        [TestMethod]
        public void Amount_OperatorDivide_DoesNotModifyOperands()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(5);

            Amount result = amountA / amountB;

            Assert.AreEqual(10, amountA.Quantity.ToLong());
            Assert.AreEqual(5, amountB.Quantity.ToLong());
            Assert.AreEqual(2, result.Quantity.ToLong());
        }

        [TestMethod]
        public void Amount_InPlaceDivide_UpdatesFirstOperand()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(5);

            Amount result = amountA.InPlaceDivide(amountB);

            Assert.AreEqual(2, amountA.Quantity.ToLong());
            Assert.AreEqual(5, amountB.Quantity.ToLong());
            Assert.AreEqual(2, result.Quantity.ToLong());
        }


        [TestMethod]
        public void Amount_OperatorMultiply_DoesNotModifyOperands()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(5);

            Amount result = amountA * amountB;

            Assert.AreEqual(10, amountA.Quantity.ToLong());
            Assert.AreEqual(5, amountB.Quantity.ToLong());
            Assert.AreEqual(50, result.Quantity.ToLong());
        }

        [TestMethod]
        public void Amount_Multiply_UpdatesFirstOperand()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(5);

            Amount result = amountA.Multiply(amountB);

            Assert.AreEqual(50, amountA.Quantity.ToLong());
            Assert.AreEqual(5, amountB.Quantity.ToLong());
            Assert.AreEqual(50, result.Quantity.ToLong());
        }

        [TestMethod]
        public void Amount_OperatorSubtract_DoesNotModifyOperands()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(2);

            Amount result = amountA - amountB;

            Assert.AreEqual(10, amountA.Quantity.ToLong());
            Assert.AreEqual(2, amountB.Quantity.ToLong());
            Assert.AreEqual(8, result.Quantity.ToLong());
        }

        [TestMethod]
        public void Amount_InPlaceSubtract_UpdatesFirstOperand()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(2);

            Amount result = amountA.InPlaceSubtract(amountB);

            Assert.AreEqual(8, amountA.Quantity.ToLong());
            Assert.AreEqual(2, amountB.Quantity.ToLong());
            Assert.AreEqual(8, result.Quantity.ToLong());
        }

        [TestMethod]
        public void Amount_OperatorAdd_DoesNotModifyOperands()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(2);

            Amount result = amountA + amountB;

            Assert.AreEqual(10, amountA.Quantity.ToLong());
            Assert.AreEqual(2, amountB.Quantity.ToLong());
            Assert.AreEqual(12, result.Quantity.ToLong());
        }

        [TestMethod]
        public void Amount_InPlaceAdd_UpdatesFirstOperand()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(2);

            Amount result = amountA.InPlaceAdd(amountB);

            Assert.AreEqual(12, amountA.Quantity.ToLong());
            Assert.AreEqual(2, amountB.Quantity.ToLong());
            Assert.AreEqual(12, result.Quantity.ToLong());
        }

        [TestMethod]
        public void Amount_Inverted_ReturnsNewInvertedAmount()
        {
            BigInt bigInt = BigInt.FromInt(10, 2);
            Amount amountA = new Amount(bigInt, null);

            Amount amountB = amountA.Inverted();

            Assert.AreEqual("10", amountA.ToString());
            Assert.AreEqual("0.1", amountB.ToString());
        }

        [TestMethod]
        public void Amount_InPlaceInvert_InvertsItself()
        {
            BigInt bigInt = BigInt.FromInt(10, 2);
            Amount amountA = new Amount(bigInt, null);

            amountA.InPlaceInvert();

            Assert.AreEqual("0.1", amountA.ToString());
        }

        [TestMethod]
        public void Amount_HasCommodity_ChecksNullCommodity()
        {
            BigInt bigInt = BigInt.FromInt(10, 2);
            Amount amountA = new Amount(bigInt, null);

            Assert.IsFalse(amountA.HasCommodity);
            Assert.AreEqual(CommodityPool.Current.NullCommodity, amountA.Commodity);
        }

        [TestMethod]
        public void Amount_Parse_PARSE_NO_MIGRATE_sets_KeepPrecision_True()
        {
            Amount amt1 = new Amount();
            string line1 = "234";
            amt1.Parse(ref line1, AmountParseFlagsEnum.PARSE_NO_MIGRATE);
            Assert.IsTrue(amt1.KeepPrecision);

            Amount amt2 = new Amount();
            string line2 = "234.76";
            amt2.Parse(ref line2, AmountParseFlagsEnum.PARSE_NO_MIGRATE);
            Assert.IsTrue(amt2.KeepPrecision);
        }

        [TestMethod]
        public void Amount_Multiply_ReducesQuantityPrecisionIfHasCommodityAndNoKeepPrecision()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("AMTSTRDQ"));
            comm.Precision = 2;

            BigInt quant = BigInt.Parse("100");
            quant = quant.SetPrecision(10);

            Amount amt1 = new Amount(quant, comm);

            Amount result = amt1.Multiply(amt1);

            Assert.AreEqual(8, result.Quantity.Precision);  // 8 == comm_prec(2) + extend_by_digits(6)
        }

        [TestMethod]
        public void Amount_InPlaceTruncate_RoundsToDisplayPrecision()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("AMTTRCRDS"));
            comm.Precision = 2;

            BigInt quant1 = BigInt.Parse("100.8767");
            Amount amt1 = new Amount(quant1, comm);

            BigInt quant2 = BigInt.Parse("100.1232");
            Amount amt2 = new Amount(quant2, comm);

            Assert.AreEqual(100.88M, amt1.Truncated().Quantity.ToDecimal());
            Assert.AreEqual(100.12M, amt2.Truncated().Quantity.ToDecimal());
        }

    }
}

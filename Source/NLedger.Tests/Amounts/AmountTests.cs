// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Commodities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Amounts
{
    public class AmountTests : TestFixture
    {
        [Fact]
        public void Amount_ParseQuantity_ReturnsEmptyForEmptyString()
        {
            string line = null;
            Assert.Equal(String.Empty, Amount.ParseQuantity(ref line));
            line = String.Empty;
            Assert.Equal(String.Empty, Amount.ParseQuantity(ref line));
        }

        [Fact]
        public void Amount_ParseQuantity_IgnoresInitialWhiteSpaces()
        {
            string line = "12345";
            Assert.Equal("12345", Amount.ParseQuantity(ref line));

            line = "   12345";
            Assert.Equal("12345", Amount.ParseQuantity(ref line));

            line = "\t12345";
            Assert.Equal("12345", Amount.ParseQuantity(ref line));
        }

        [Fact]
        public void Amount_ParseQuantity_ReadsDigitsAndOtherChars()
        {
            string line = "12345$ rest of line";
            Assert.Equal("12345", Amount.ParseQuantity(ref line));
            Assert.Equal("$ rest of line", line);

            line = "-12,345 rest of line";
            Assert.Equal("-12,345", Amount.ParseQuantity(ref line));
            Assert.Equal(" rest of line", line);

            line = "-12,34.5rest of line";
            Assert.Equal("-12,34.5", Amount.ParseQuantity(ref line));
            Assert.Equal("rest of line", line);
        }

        [Fact]
        public void Amount_ParseQuantity_IgnoresTrailingNonDigitChars()
        {
            string line = "12345, rest of line";
            Assert.Equal("12345", Amount.ParseQuantity(ref line));
            Assert.Equal(", rest of line", line);

            line = "-12,345. rest of line";
            Assert.Equal("-12,345", Amount.ParseQuantity(ref line));
            Assert.Equal(". rest of line", line);

            line = "-12,34.5. rest of line";
            Assert.Equal("-12,34.5", Amount.ParseQuantity(ref line));
            Assert.Equal(". rest of line", line);
        }

        [Fact]
        public void Amount_Parse_Integration()
        {
            Amount amount1 = new Amount();
            string line1 = "-10 USD";
            amount1.Parse(ref line1, AmountParseFlagsEnum.PARSE_DEFAULT);
            Assert.Equal(-10, amount1.Quantity.ToLong());
            Assert.Equal("USD", amount1.Commodity.BaseSymbol);
            Assert.Equal(String.Empty, line1);

            Amount amount2 = new Amount();
            string line2 = "99 USD";
            amount2.Parse(ref line2, AmountParseFlagsEnum.PARSE_DEFAULT);
            Assert.Equal(99, amount2.Quantity.ToLong());
            Assert.Equal("USD", amount2.Commodity.BaseSymbol);
            Assert.Equal(String.Empty, line2);

            Amount amount3 = new Amount();
            string line3 = "STS 99.99";
            amount3.Parse(ref line3, AmountParseFlagsEnum.PARSE_DEFAULT);
            Assert.Equal(Quantity.Parse("99.99", 2), amount3.Quantity);  // Precision = 2
            Assert.Equal("STS", amount3.Commodity.BaseSymbol);
            Assert.Equal(String.Empty, line3);
        }

        [Fact]
        public void Amount_StripAnnotations_ReturnsOriginalAmountIfKeepAllIsTrue()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Amount amount = new Amount(10, comm);
            AnnotationKeepDetails keepDetails = new AnnotationKeepDetails();

            // Commodity is not annotated - it is enough condition to return the original object
            Amount newAmount = amount.StripAnnotations(keepDetails);

            Assert.Equal(amount, newAmount);
        }

        [Fact]
        public void Amount_StripAnnotations_ReturnsNewAmountForAnnotatedCommodity()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annComm = new AnnotatedCommodity(comm, new Annotation());
            Amount amount = new Amount(10, annComm);
            AnnotationKeepDetails keepDetails = new AnnotationKeepDetails();

            // The original ampunt has annotated commodity, but "keepDetails" does not specify anything to keep.
            // Therefore, the new amount has not annotated commodity
            Amount newAmount = amount.StripAnnotations(keepDetails);
            Assert.False(newAmount.Commodity.IsAnnotated);
        }

        [Fact]
        public void Amount_FitsInLong_ReturnsTrueIfQuantityCanBeConvertedToLong()
        {
            Amount tooBigNegativeAmount = new Amount(Quantity.Parse("-99999999999999999999"), null);
            Assert.False(tooBigNegativeAmount.FitsInLong);

            Amount normalAmount = new Amount(Quantity.Parse("99"), null);
            Assert.True(normalAmount.FitsInLong);

            Amount tooBigPositiveAmount = new Amount(Quantity.Parse("99999999999999999999"), null);
            Assert.False(tooBigPositiveAmount.FitsInLong);
        }

        [Fact]
        public void Amount_IsRealZero_IndicatesThatAmountHasZeroValue()
        {
            // Not applicable - method Sign requires non-empty Quantity. Confirmed by Boost auto-tests
            // Amount amount = new Amount();
            // Assert.True(amount.IsRealZero);

            Amount amount = new Amount(0);
            Assert.True(amount.IsRealZero);

            amount = new Amount(Quantity.Parse("0"), null);
            Assert.True(amount.IsRealZero);
        }

        [Fact]
        public void Amount_DivideBy_ReturnsDividedAmount()
        {
            Amount amount10 = new Amount(Quantity.Parse("10", 2), null);
            Amount amount4 = new Amount(Quantity.Parse("4", 2), null);

            Amount amount = amount10.InPlaceDivide(amount4);

            int expectedPrecision = 2 + 2 + Amount.ExtendByDigits;
            Assert.Equal(Quantity.Parse("2.5", expectedPrecision), amount.Quantity);
        }

        [Fact]
        public void Amount_Multiply_ReturnsMultipliedAmount()
        {
            Amount amount10 = new Amount(Quantity.Parse("10", 2), null);
            Amount amount4 = new Amount(Quantity.Parse("4", 2), null);

            Amount amount = amount10.Multiply(amount4);

            int expectedPrecision = 2 + 2;
            Assert.Equal(Quantity.Parse("40", expectedPrecision), amount.Quantity);
        }

        [Fact]
        public void Amount_Subtract_ReturnsSubtractedAmountForNoCommodities()
        {
            Amount amount10 = new Amount(10);
            Amount amount8 = new Amount(8);

            Amount amount = amount10.InPlaceSubtract(amount8);

            Assert.Equal(2, amount.Quantity.ToLong());
        }

        [Fact]
        public void Amount_Subtract_ReturnsSubtractedAmountForTheSameCommodity()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Amount amount10 = new Amount(10, comm);
            Amount amount8 = new Amount(8, comm);

            Amount amount = amount10.InPlaceSubtract(amount8);

            Assert.Equal(2, amount.Quantity.ToLong());
        }

        [Fact]
        public void Amount_Negated_ReturnsInvertedAmount()
        {
            Amount amount = new Amount(8);
            Assert.Equal(-8, amount.Negated().Quantity.ToLong());
        }

        [Fact]
        public void Amount_HasAnnotation_FailsIfQuantityIsNotSpecified()
        {
            Amount amount = new Amount();
            Assert.Throws<AmountError>(() => amount.HasAnnotation);
        }

        [Fact]
        public void Amount_HasAnnotation_ReturnsFalseIfNoCommodity()
        {
            Amount amount = new Amount(1);
            Assert.False(amount.HasAnnotation);
        }

        [Fact]
        public void Amount_HasAnnotation_ReturnsFalseIfCommodityIsNotAnnotated()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Amount amount = new Amount(1, comm);
            Assert.False(amount.HasAnnotation);
        }

        [Fact]
        public void Amount_HasAnnotation_ReturnsTrueIfCommodityIsAnnotated()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annComm = new AnnotatedCommodity(comm, new Annotation());
            Amount amount = new Amount(1, annComm);
            Assert.True(amount.HasAnnotation);
        }

        [Fact]
        public void Amount_HasAnnotation_FailsIfAnnotatedCommodityHasNoDetails()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annComm = new AnnotatedCommodity(comm, null);
            Amount amount = new Amount(1, annComm);
            Assert.Throws<InvalidOperationException>(() => amount.HasAnnotation);
        }

        [Fact]
        public void Amount_IsZero_FailsIfQuantityIsEmpty()
        {
            Amount amount = new Amount();
            Assert.Throws<AmountError>(() => amount.IsZero);
        }

        [Fact]
        public void Amount_IsZero_ReturnsRealZeroIfNoCommodity()
        {
            Amount amount = new Amount(Quantity.Parse("0.00005", 2), null);  // Notice that precision less than a value.
            Assert.False(amount.IsZero);
        }

        [Fact]
        public void Amount_IsZero_ReturnsPrecisionedZeroIfItHasCommodityAndNoKeepPrecision()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Amount amount = new Amount(Quantity.Parse("0.00005", 2), comm);  // Notice that precision less than a value.
            Assert.True(amount.IsZero);
        }

        [Fact]
        public void Amount_IsZero_PrecisionedZeroUsesCommodityPrecision()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm")) { Precision = 2 };
            Amount amount = new Amount(Quantity.Parse("0.00005", 10), comm);  // Notice commodity's precision has higher priority
            Assert.True(amount.IsZero);
        }

        [Fact]
        public void Amount_IsZero_RoundsToCommodityPrecision()
        {
            Commodity commodityA = new Commodity(CommodityPool.Current, new CommodityBase("AmtNZeroA")) { Precision = 2 };
            // Set a value that less than commodity precision (2) but higher than quantity precision (8)
            Amount amountA = new Amount(Quantity.Parse("0.008", 8), commodityA);
            Assert.False(amountA.IsZero);  // The value is rounded to 0.01 according to Commodity precision
        }

        [Fact]
        public void Amount_SetCommodity_SetsZeroIfQuantityIsEmpty()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Amount amount = new Amount();

            Assert.False(amount.HasCommodity);
            amount.SetCommodity(comm);
            Assert.True(amount.HasCommodity);
            Assert.True(amount.IsZero);
            Assert.True(amount.IsRealZero);
        }

        [Fact]
        public void Amount_ParseConversion_SetsUpConversionCommodities()
        {
            CommodityPool.Cleanup();
            Amount.ParseConversion("1.0m", "60s");

            Commodity larger = CommodityPool.Current.Find("m");
            Assert.Equal("m", larger.Symbol);
            Assert.Null(larger.Larger);
            Assert.NotNull(larger.Smaller);
            Assert.Equal("s", larger.Smaller.Commodity.Symbol);
            Assert.Equal(60, larger.Smaller.Quantity.ToLong());

            Commodity smaller = CommodityPool.Current.Find("s");
            Assert.Equal("s", smaller.Symbol);
            Assert.NotNull(smaller.Larger);
            Assert.Null(smaller.Smaller);
            Assert.Equal("m", smaller.Larger.Commodity.Symbol);
            Assert.Equal(60, smaller.Larger.Quantity.ToLong());
        }

        [Fact]
        public void Amount_IsLessThan_ChecksWhetherGivenAmountLessThanAnother()
        {
            Amount amount1 = new Amount(10);
            Amount amount2 = new Amount(20);
            Assert.True(amount1.IsLessThan(amount2));
            Assert.False(amount2.IsLessThan(amount1));
        }

        [Fact]
        public void Amount_IsGreaterThan_ChecksWhetherGivenAmountGreaterThanAnother()
        {
            Amount amount1 = new Amount(10);
            Amount amount2 = new Amount(20);
            Assert.False(amount1.IsGreaterThan(amount2));
            Assert.True(amount2.IsGreaterThan(amount1));
        }

        [Fact]
        public void Amount_Compare_FailsIfAmountIsNull()
        {
            Amount amount1 = new Amount(10);
            Assert.Throws<ArgumentException>(() => amount1.Compare(null));
        }

        [Fact]
        public void Amount_Compare_FailsIfQuantityIsNotSpecified()
        {
            Amount amount1 = new Amount(10);
            Assert.Throws<AmountError>(() => amount1.Compare(new Amount()));
        }

        [Fact]
        public void Amount_Compare_FailsIfCommoditiesAreDifferent()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));
            Amount amount1 = new Amount(10, commodity1);
            Amount amount2 = new Amount(12, commodity2);
            Assert.Throws<AmountError>(() => amount1.Compare(amount2));
        }

        [Fact]
        public void Amount_Compare_ReturnsMinusOneOrZeroOrOne()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(10, commodity1);
            Amount amount2 = new Amount(12, commodity1);

            Assert.Equal(-1, amount1.Compare(amount2));
            Assert.Equal(0, amount1.Compare(amount1));
            Assert.Equal(1, amount2.Compare(amount1));
        }

        [Fact]
        public void Amount_Abs_ReturnsNewAmountWithAbsoluteValue()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(10, commodity1);
            Amount amount2 = new Amount(-10, commodity1);

            Amount result1 = amount1.Abs();
            Amount result2 = amount2.Abs();

            Assert.Equal(10, result1.Quantity.ToLong());
            Assert.Equal(10, result2.Quantity.ToLong());

            Assert.True(Object.ReferenceEquals(result1, amount1));  // If the value is not changed, return the original object
            Assert.False(Object.ReferenceEquals(result2, amount2));
        }

        [Fact]
        public void Amount_Add_NullAmountCausesException()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(10, commodity1);
            Assert.Throws<ArgumentException>(() => amount1.InPlaceAdd(null));
        }

        [Fact]
        public void Amount_Add_InvalidAmountCausesException()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(10, commodity1);
            Amount amount2 = new Amount(Quantity.Empty, commodity1);

            Assert.False(amount2.Valid());
            Assert.Throws<AmountError>(() => amount1.InPlaceAdd(amount2));
        }

        [Fact]
        public void Amount_Add_UninitializedFirstAmountCausesException()
        {
            Amount amount1 = new Amount();
            Amount amount2 = new Amount(10);

            Assert.True(amount2.Valid());
            Assert.Throws<AmountError>(() => amount1.InPlaceAdd(amount2));
        }

        [Fact]
        public void Amount_Add_UninitializedSecondAmountCausesException()
        {
            Amount amount1 = new Amount(10);
            Amount amount2 = new Amount();

            Assert.True(amount2.Valid());
            Assert.Throws<AmountError>(() => amount1.InPlaceAdd(amount2));
        }

        [Fact]
        public void Amount_Add_TwoUninitializedAmountsCauseException()
        {
            Amount amount1 = new Amount();
            Amount amount2 = new Amount();

            Assert.True(amount2.Valid());
            Assert.Throws<AmountError>(() => amount1.InPlaceAdd(amount2));
        }

        [Fact]
        public void Amount_Add_AddsAmountsWithoutCommodities()
        {
            Amount amount1 = new Amount(10);
            Amount amount2 = new Amount(20);

            Amount result = amount1.InPlaceAdd(amount2);

            Assert.Equal(30, result.Quantity.ToLong());
        }

        [Fact]
        public void Amount_Add_ChecksThatBothCommoditiesAreEqual()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));
            Amount amount1 = new Amount(10, commodity1);
            Amount amount2 = new Amount(20, commodity2);

            Assert.Throws<AmountError>(() => amount1.InPlaceAdd(amount2));
        }

        [Fact]
        public void Amount_Add_ChangesPrecision()
        {
            var quantity1 = Quantity.FromLong(10);
            quantity1 = quantity1.SetPrecision(2);
            Amount amount1 = new Amount(quantity1, null);

            var quantity2 = Quantity.FromLong(20);
            quantity2 = quantity2.SetPrecision(3);
            Amount amount2 = new Amount(quantity2, null);

            Amount result = amount1.InPlaceAdd(amount2);
            Assert.Equal(2, result.Quantity.Precision);
        }

        [Fact]
        public void Amount_Subtract_NullAmountCausesException()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(10, commodity1);
            Assert.Throws<ArgumentException>(() => amount1.InPlaceSubtract(null));
        }

        [Fact]
        public void Amount_Subtract_InvalidAmountCausesException()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(10, commodity1);
            Amount amount2 = new Amount(Quantity.Empty, commodity1);

            Assert.False(amount2.Valid());
            Assert.Throws<AmountError>(() => amount1.InPlaceSubtract(amount2));
        }

        [Fact]
        public void Amount_Subtract_UninitializedFirstAmountCausesException()
        {
            Amount amount1 = new Amount();
            Amount amount2 = new Amount(10);

            Assert.True(amount2.Valid());
            Assert.Throws<AmountError>(() => amount1.InPlaceSubtract(amount2));
        }

        [Fact]
        public void Amount_Subtract_UninitializedSecondAmountCausesException()
        {
            Amount amount1 = new Amount(10);
            Amount amount2 = new Amount();

            Assert.True(amount2.Valid());
            Assert.Throws<AmountError>(() => amount1.InPlaceSubtract(amount2));
        }

        [Fact]
        public void Amount_Subtract_TwoUninitializedAmountsCauseException()
        {
            Amount amount1 = new Amount();
            Amount amount2 = new Amount();

            Assert.True(amount2.Valid());
            Assert.Throws<AmountError>(() => amount1.InPlaceSubtract(amount2));
        }

        [Fact]
        public void Amount_Subtract_SubtractsAmountsWithoutCommodities()
        {
            Amount amount1 = new Amount(30);
            Amount amount2 = new Amount(20);

            Amount result = amount1.InPlaceSubtract(amount2);

            Assert.Equal(10, result.Quantity.ToLong());
        }

        [Fact]
        public void Amount_Subtract_ChecksThatBothCommoditiesAreEqual()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));
            Amount amount1 = new Amount(10, commodity1);
            Amount amount2 = new Amount(20, commodity2);

            Assert.Throws<AmountError>(() => amount1.InPlaceSubtract(amount2));
        }

        [Fact]
        public void Amount_Subtract_ChangesPrecision()
        {
            var quantity1 = Quantity.FromLong(30);
            quantity1 = quantity1.SetPrecision(2);
            Amount amount1 = new Amount(quantity1, null);

            var quantity2 = Quantity.FromLong(20);
            quantity2 = quantity2.SetPrecision(3);
            Amount amount2 = new Amount(quantity2, null);

            Amount result = amount1.InPlaceSubtract(amount2);
            Assert.Equal(3, result.Quantity.Precision);
        }

        [Fact]
        public void Amount_Annotate_FailsIfNoQuantity()
        {
            Amount amount1 = new Amount(Quantity.Empty, null);
            Assert.Throws<AmountError>(() => amount1.Annotate(new Annotation()));
        }

        [Fact]
        public void Amount_Annotate_DoesNothingIfNoCOmmodity()
        {
            Amount amount1 = new Amount(10, null);
            amount1.Annotate(new Annotation());
            Assert.False(amount1.HasAnnotation);
        }

        [Fact]
        public void Amount_Annotate_CreatesAnnotatedCommodityForNonAnnotated()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(10, commodity1);

            Assert.False(amount1.Commodity.IsAnnotated);
            amount1.Annotate(new Annotation());
            
            Assert.True(amount1.HasAnnotation);
            Assert.True(amount1.Commodity.IsAnnotated);
        }

        [Fact]
        public void Amount_Annotate_CreatesNewAnnotatedCommodityForAnnotated()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("aac-base-1"));
            AnnotatedCommodity annotatedCommodity1 = new AnnotatedCommodity(commodity1, new Annotation());
            Amount amount1 = new Amount(10, annotatedCommodity1);
            Annotation newAnnotation = new Annotation();

            Assert.True(amount1.Commodity.IsAnnotated);
            amount1.Annotate(newAnnotation);

            Assert.True(amount1.HasAnnotation);
            Assert.True(amount1.Commodity.IsAnnotated);

            AnnotatedCommodity resultCommodity = (AnnotatedCommodity)amount1.Commodity;
            Assert.Equal(newAnnotation, resultCommodity.Details);
        }

        [Fact]
        public void Amount_ClearCommodity_ClearsCommodity()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Amount amount1 = new Amount(10, commodity1);

            Assert.NotNull(amount1.Commodity);
            amount1.ClearCommodity();
            Assert.Equal(CommodityPool.Current.NullCommodity, amount1.Commodity);
        }

        [Fact]
        public void Amount_GetInvertedQuantity_ReturnsInvertedQuantityOrZrero()
        {
            Amount amount1 = new Amount(10);
            var result1 = amount1.GetInvertedQuantity();
            Assert.Equal(0.1m, result1.ToDecimal());

            Amount amount2 = new Amount();
            var result2 = amount2.GetInvertedQuantity();
            Assert.Equal(0m, result2.ToDecimal());
        }

        [Fact]
        public void Amount_Merge_FailsWithEmptyArgument()
        {
            Amount amount1 = new Amount();
            Assert.Throws<ArgumentNullException>(() => amount1.Merge(null));
        }

        [Fact]
        public void Amount_Merge_FailsWithEqualCommodity()
        {
            Amount amount1 = new Amount();
            Amount amount2 = new Amount();
            Assert.Throws<InvalidOperationException>(() => amount1.Merge(amount2));
        }

        [Fact]
        public void Amount_Merge_ReturnsMultipliesValues()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Amount amount1 = new Amount(10, commodity1);
            Amount amount2 = new Amount(20, commodity2);

            Amount result = amount1.Merge(amount2);
            Assert.Equal(200, result.Quantity.ToLong());
            Assert.Equal(commodity2, result.Commodity);
        }

        [Fact]
        public void Amount_ToString_ReturnsPrintedValue()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("ABC"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("CDE"));

            Amount amount1 = new Amount(10, commodity1);
            Amount amount2 = new Amount(20, commodity2);
            Amount amount3 = new Amount(30);

            Assert.Equal("ABC10", amount1.ToString());
            Assert.Equal("CDE20", amount2.ToString());
            Assert.Equal("30", amount3.ToString());
        }

        [Fact]
        public void Amount_DisplayPrecision_FailsIfNoQuantity()
        {
            Amount amount = new Amount();
            Assert.False(amount.Quantity.HasValue);
            Assert.Throws<AmountError>(() => amount.DisplayPrecision);
        }

        [Fact]
        public void Amount_DisplayPrecision_ReturnsQuantityPrecisionIfNoCommodity()
        {
            var quantity = Quantity.Parse("121", 3);
            Amount amount = new Amount(quantity, null);
            Assert.Equal(3, amount.DisplayPrecision);
        }

        [Fact]
        public void Amount_DisplayPrecision_ReturnsCommodityPrecisionIfNotKeepPrecision()
        {
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("ABC"));
            commodity.Precision = 3;

            var quantity = Quantity.Parse("121", 4).SetKeepPrecision(false);

            Amount amount = new Amount(quantity, commodity);
            Assert.Equal(3, amount.DisplayPrecision);
        }

        [Fact]
        public void Amount_DisplayPrecision_ReturnsMaxCommodityOrQuantityPrecision()
        {
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("ABC"));
            commodity.Precision = 3;

            var quantity = Quantity.Parse("121", 4).SetKeepPrecision(true);

            Amount amount = new Amount(quantity, commodity);
            Assert.Equal(4, amount.DisplayPrecision);
        }

        [Fact]
        public void Amount_Print_UsesQuantityPrint()
        {
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("ABC"));
            commodity.Precision = 3;
            var quantity = Quantity.Parse("121", 4).SetKeepPrecision(true);
            Amount amount = new Amount(quantity, commodity);

            string result = amount.Print();
            Assert.Equal("ABC121.000", result);
        }

        [Fact]
        public void Amount_IsNullOrEmpty_ReturnsTrueIfAmpuntIsEmpty()
        {
            // null
            Assert.True(Amount.IsNullOrEmpty(null));

            // empty
            Amount empty = new Amount();
            Assert.True(Amount.IsNullOrEmpty(empty));
            Assert.True(empty.IsEmpty);

            // non-empty
            Amount ten = new Amount(10);
            Assert.False(Amount.IsNullOrEmpty(ten));
            Assert.False(ten.IsEmpty);
        }

        [Fact]
        public void Amount_Print_DoesNotAddSuffixSpaceInCaseCommodityStyleSeparatedNotSpecified()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("TKP1"));
            commodity1.Flags |= CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED;
            Amount amount1 = new Amount(10, commodity1);
            Assert.Equal("10TKP1", amount1.Print());
        }

        [Fact]
        public void Amount_Print_AddsSuffixSpaceInCaseCommodityStyleSeparatedSpecified()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("TKP1"));
            commodity1.Flags |= CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED | CommodityFlagsEnum.COMMODITY_STYLE_SEPARATED; /* COMMODITY_STYLE_SEPARATED is added */ 
            Amount amount1 = new Amount(10, commodity1);
            Assert.Equal("10 TKP1", amount1.Print()); /* Space between the number and commodity */
        }

        [Fact]
        public void Amount_Merge_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(Quantity.FromLong(10).SetKeepPrecision(true), null);
            Amount amt2 = new Amount(Quantity.FromLong(20), commodity1);

            Amount result = amt1.Merge(amt2);
            Assert.True(result.KeepPrecision);
        }

        [Fact]
        public void Amount_Multiply_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(Quantity.FromLong(10).SetKeepPrecision(true), null);
            Amount amt2 = new Amount(Quantity.FromLong(20), commodity1);

            Amount result = amt1.Multiply(amt2);
            Assert.True(result.KeepPrecision);
        }

        [Fact]
        public void Amount_Add_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(Quantity.FromLong(10).SetKeepPrecision(true), null);
            Amount amt2 = new Amount(Quantity.FromLong(20), commodity1);

            Amount result = amt1.InPlaceAdd(amt2);
            Assert.True(result.KeepPrecision);
        }

        [Fact]
        public void Amount_Subtract_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(Quantity.FromLong(10).SetKeepPrecision(true), null);
            Amount amt2 = new Amount(Quantity.FromLong(5), commodity1);

            Amount result = amt1.InPlaceSubtract(amt2);
            Assert.True(result.KeepPrecision);
        }

        [Fact]
        public void Amount_Abs_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(Quantity.FromLong(10).SetKeepPrecision(true), null);

            Amount result = amt1.Abs();
            Assert.True(result.KeepPrecision);
        }

        [Fact]
        public void Amount_Negated_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(Quantity.FromLong(10).SetKeepPrecision(true), null);

            Amount result = amt1.Negated();
            Assert.True(result.KeepPrecision);
        }

        [Fact]
        public void Amount_StripAnnotation_HoldsKeepPrecisionFlag()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("AAHKPF1"));
            Amount amt1 = new Amount(Quantity.FromLong(10).SetKeepPrecision(true), null);

            Amount result = amt1.StripAnnotations(new AnnotationKeepDetails());
            Assert.True(result.KeepPrecision);
        }

        [Fact]
        public void Amount_OperatorDivide_DoesNotModifyOperands()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(5);

            Amount result = amountA / amountB;

            Assert.Equal(10, amountA.Quantity.ToLong());
            Assert.Equal(5, amountB.Quantity.ToLong());
            Assert.Equal(2, result.Quantity.ToLong());
        }

        [Fact]
        public void Amount_InPlaceDivide_UpdatesFirstOperand()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(5);

            Amount result = amountA.InPlaceDivide(amountB);

            Assert.Equal(2, amountA.Quantity.ToLong());
            Assert.Equal(5, amountB.Quantity.ToLong());
            Assert.Equal(2, result.Quantity.ToLong());
        }


        [Fact]
        public void Amount_OperatorMultiply_DoesNotModifyOperands()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(5);

            Amount result = amountA * amountB;

            Assert.Equal(10, amountA.Quantity.ToLong());
            Assert.Equal(5, amountB.Quantity.ToLong());
            Assert.Equal(50, result.Quantity.ToLong());
        }

        [Fact]
        public void Amount_Multiply_UpdatesFirstOperand()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(5);

            Amount result = amountA.Multiply(amountB);

            Assert.Equal(50, amountA.Quantity.ToLong());
            Assert.Equal(5, amountB.Quantity.ToLong());
            Assert.Equal(50, result.Quantity.ToLong());
        }

        [Fact]
        public void Amount_OperatorSubtract_DoesNotModifyOperands()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(2);

            Amount result = amountA - amountB;

            Assert.Equal(10, amountA.Quantity.ToLong());
            Assert.Equal(2, amountB.Quantity.ToLong());
            Assert.Equal(8, result.Quantity.ToLong());
        }

        [Fact]
        public void Amount_InPlaceSubtract_UpdatesFirstOperand()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(2);

            Amount result = amountA.InPlaceSubtract(amountB);

            Assert.Equal(8, amountA.Quantity.ToLong());
            Assert.Equal(2, amountB.Quantity.ToLong());
            Assert.Equal(8, result.Quantity.ToLong());
        }

        [Fact]
        public void Amount_OperatorAdd_DoesNotModifyOperands()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(2);

            Amount result = amountA + amountB;

            Assert.Equal(10, amountA.Quantity.ToLong());
            Assert.Equal(2, amountB.Quantity.ToLong());
            Assert.Equal(12, result.Quantity.ToLong());
        }

        [Fact]
        public void Amount_InPlaceAdd_UpdatesFirstOperand()
        {
            Amount amountA = new Amount(10);
            Amount amountB = new Amount(2);

            Amount result = amountA.InPlaceAdd(amountB);

            Assert.Equal(12, amountA.Quantity.ToLong());
            Assert.Equal(2, amountB.Quantity.ToLong());
            Assert.Equal(12, result.Quantity.ToLong());
        }

        [Fact]
        public void Amount_Inverted_ReturnsNewInvertedAmount()
        {
            var bigInt = Quantity.FromLong(10, 2);
            Amount amountA = new Amount(bigInt, null);

            Amount amountB = amountA.Inverted();

            Assert.Equal("10", amountA.ToString());
            Assert.Equal("0.1", amountB.ToString());
        }

        [Fact]
        public void Amount_InPlaceInvert_InvertsItself()
        {
            var bigInt = Quantity.FromLong(10, 2);
            Amount amountA = new Amount(bigInt, null);

            amountA.InPlaceInvert();

            Assert.Equal("0.1", amountA.ToString());
        }

        [Fact]
        public void Amount_HasCommodity_ChecksNullCommodity()
        {
            var bigInt = Quantity.FromLong(10, 2);
            Amount amountA = new Amount(bigInt, null);

            Assert.False(amountA.HasCommodity);
            Assert.Equal(CommodityPool.Current.NullCommodity, amountA.Commodity);
        }

        [Fact]
        public void Amount_Parse_PARSE_NO_MIGRATE_sets_KeepPrecision_True()
        {
            Amount amt1 = new Amount();
            string line1 = "234";
            amt1.Parse(ref line1, AmountParseFlagsEnum.PARSE_NO_MIGRATE);
            Assert.True(amt1.KeepPrecision);

            Amount amt2 = new Amount();
            string line2 = "234.76";
            amt2.Parse(ref line2, AmountParseFlagsEnum.PARSE_NO_MIGRATE);
            Assert.True(amt2.KeepPrecision);
        }

        [Fact]
        public void Amount_Multiply_ReducesQuantityPrecisionIfHasCommodityAndNoKeepPrecision()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("AMTSTRDQ"));
            comm.Precision = 2;

            var quant = Quantity.Parse("100");
            quant = quant.SetPrecision(10);

            Amount amt1 = new Amount(quant, comm);

            Amount result = amt1.Multiply(amt1);

            Assert.Equal(8, result.Quantity.Precision);  // 8 == comm_prec(2) + extend_by_digits(6)
        }

        [Fact]
        public void Amount_InPlaceTruncate_RoundsToDisplayPrecision()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("AMTTRCRDS"));
            comm.Precision = 2;

            var quant1 = Quantity.Parse("100.8767");
            Amount amt1 = new Amount(quant1, comm);

            var quant2 = Quantity.Parse("100.1232");
            Amount amt2 = new Amount(quant2, comm);

            Assert.Equal(100.88M, amt1.Truncated().Quantity.ToDecimal());
            Assert.Equal(100.12M, amt2.Truncated().Quantity.ToDecimal());
        }

        [Fact]
        public void Amount_Valid_ReturnsTrueIfNoQuantityAndCommodity()
        {
            Amount amount = new Amount();
            Assert.False(amount.Quantity.HasValue);
            Assert.False(amount.HasCommodity);
            Assert.True(amount.Valid());
        }

        [Fact]
        public void Amount_Valid_ReturnsFalseIfQuantityIsNotValid()
        {
            Amount amount = new Amount(100);
            Assert.True(amount.Valid());

            var quantity = amount.Quantity.SetPrecision(2048);
            amount = new Amount(quantity, null);
            Assert.False(amount.Quantity.Valid());
            Assert.False(amount.Valid());
        }
    }
}

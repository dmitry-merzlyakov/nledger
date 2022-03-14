// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Commodities;
using NLedger.Utility.BigValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests
{
    // By default, this test uses BigRational arithmetic to validate BigInt.
    // In case you want to validate BigInt with Decimal arithmetic, uncomment the next alias.
    // See Amount.cs for further information about Qunatity Arithmetics in NLedger
    using BigInt = BigInt<BigRational>;
    //using BigInt = BigInt<BigDecimal>;

    public class BigIntTests : TestFixture
    {
        [Fact]
        public void BigInt_ZeroConstant_IsZero()
        {
            Assert.True(BigInt.Zero.HasValue);
            Assert.Equal(0, BigInt.Zero.ToLong());
        }

        [Fact]
        public void BigInt_OneConstant_IsOne()
        {
            Assert.True(BigInt.One.HasValue);
            Assert.Equal(1, BigInt.One.ToLong());
        }

        [Fact]
        public void BigInt_DivisionOperator_ReturnsDividedValue()
        {
            BigInt result = BigInt.FromInt(10) / BigInt.FromInt(5);
            Assert.True(result.HasValue);
            Assert.Equal(2, result.ToLong());
        }

        [Fact]
        public void BigInt_SubtractOperator_ReturnsSubtractedValue()
        {
            BigInt result = BigInt.FromInt(10) - BigInt.FromInt(7);
            Assert.True(result.HasValue);
            Assert.Equal(3, result.ToLong());
        }

        [Fact]
        public void BigInt_MultiplicityOperator_ReturnsMultiplicideValue()
        {
            BigInt result = BigInt.FromInt(10) * BigInt.FromInt(5);
            Assert.True(result.HasValue);
            Assert.Equal(50, result.ToLong());
        }

        [Fact]
        public void BigInt_EqualOperator_ComparesDifferentInstances()
        {
            BigInt value10 = BigInt.FromInt(10);
            BigInt value10_ = BigInt.FromInt(10);
            BigInt value20 = BigInt.FromInt(20);
            BigInt value1 = BigInt.FromInt(1);

            // [DM] Hide warning "Comparison made to the same variable"
            #pragma warning disable 1718
            Assert.True(value10 == value10);
            #pragma warning restore
            Assert.True(value10 == value10_);
            Assert.False(value10 == value20);
            Assert.True(value1 == BigInt.One);
        }

        [Fact]
        public void BigInt_NotEqualOperator_ComparesDifferentInstances()
        {
            BigInt value10 = BigInt.FromInt(10);
            BigInt value10_ = BigInt.FromInt(10);
            BigInt value20 = BigInt.FromInt(20);
            BigInt value1 = BigInt.FromInt(1);

            // [DM] Hide warning "Comparison made to the same variable"
            #pragma warning disable 1718
            Assert.False(value10 != value10);
            #pragma warning restore
            Assert.False(value10 != value10_);
            Assert.True(value10 != value20);
            Assert.False(value1 != BigInt.One);
        }

        [Fact]
        public void BigInt_Parse_ConvertsStringsToBigInts()
        {
            Assert.Equal(BigInt.Zero, BigInt.Parse("0"));
            Assert.Equal(BigInt.One, BigInt.Parse("1"));
            Assert.Equal(BigInt.FromInt(100), BigInt.Parse("100"));
        }

        [Fact]
        public void BigInt_FromInt_ConvertsIntegersToBigInts()
        {
            Assert.Equal(0, BigInt.FromInt(0).ToLong());
            Assert.Equal(1, BigInt.FromInt(1).ToLong());
            Assert.Equal(10, BigInt.FromInt(10).ToLong());
        }

        [Fact]
        public void BigInt_ToLong_PresentsInputValueAsInteger()
        {
            Assert.Equal(0, BigInt.FromInt(0).ToLong());
            Assert.Equal(1, BigInt.FromInt(1).ToLong());
            Assert.Equal(10, BigInt.FromInt(10).ToLong());
        }

        [Fact]
        public void BigInt_ToLong_RoundsFractialNumbers()
        {
            Assert.Equal(1, BigInt.Parse("1.1").ToLong());
            Assert.Equal(2, BigInt.Parse("1.9").ToLong());
        }

        [Fact]
        public void BigInt_Precision_IsKeptAsSpecified()
        {
            Assert.Equal(0, BigInt.FromInt(1).Precision);
            Assert.Equal(10, BigInt.FromInt(1, 10).Precision);
        }

        [Fact]
        public void BigInt_HasValue_ReflectsWhetherValueIsPopulated()
        {
            BigInt uninitialized = default(BigInt);
            Assert.False(uninitialized.HasValue);

            uninitialized = BigInt.FromInt(10);
            Assert.True(uninitialized.HasValue);
        }

        [Fact]
        public void BigInt_Valid_CheckdWhetherPrecisonLessThan1024()
        {
            BigInt value1 = BigInt.FromInt(10);
            Assert.True(value1.Valid());

            BigInt value2 = BigInt.FromInt(10, 100);
            Assert.True(value2.Valid());

            BigInt value3 = BigInt.FromInt(10, 1500);
            Assert.False(value3.Valid());
        }

        [Fact]
        public void BigInt_Negative_ReturnsNegaitveValue()
        {
            Assert.Equal(BigInt.FromInt(0), BigInt.Zero.Negative());
            Assert.Equal(BigInt.FromInt(-1), BigInt.One.Negative());
            Assert.Equal(BigInt.FromInt(-10), BigInt.FromInt(10).Negative());
            Assert.Equal(BigInt.FromInt(10), BigInt.FromInt(-10).Negative());
        }

        [Fact]
        public void BigInt_Abs_ReturnsAbsoluteValue()
        {
            Assert.Equal(BigInt.FromInt(0), BigInt.Zero.Abs());
            Assert.Equal(BigInt.FromInt(1), BigInt.One.Abs());
            Assert.Equal(BigInt.FromInt(10), BigInt.FromInt(10).Abs());
            Assert.Equal(BigInt.FromInt(10), BigInt.FromInt(-10).Abs());
        }

        [Fact]
        public void BigInt_Abs_ReturnsOriginalInstanceIfNotInitialized()
        {
            BigInt uninitialized = default(BigInt);
            BigInt absForUninitialized = uninitialized.Abs();
            Assert.Equal(uninitialized, absForUninitialized);
        }

        [Fact]
        public void BigInt_SetPrecision_ChangesPrecision()
        {
            BigInt value = BigInt.FromInt(10, 20);
            Assert.Equal(20, value.Precision);
            BigInt value1 = value.SetPrecision(30);
            Assert.Equal(20, value.Precision); // Note that the original instance is not changed
            Assert.Equal(30, value1.Precision);
        }

        [Fact]
        public void BigInt_TypedEquals_ComparesDifferentInstances()
        {
            BigInt value10 = BigInt.FromInt(10);
            BigInt value10_ = BigInt.FromInt(10);
            BigInt value20 = BigInt.FromInt(20);
            BigInt value1 = BigInt.FromInt(1);

            Assert.True(value10.Equals(value10));
            Assert.True(value10.Equals(value10_));
            Assert.False(value10.Equals(value20));
            Assert.True(value1.Equals(BigInt.One));
        }

        [Fact]
        public void BigInt_UntypedEquals_ComparesObjectsWithInstances()
        {
            BigInt value10 = BigInt.FromInt(10);
            BigInt value10_ = BigInt.FromInt(10);
            BigInt value20 = BigInt.FromInt(20);
            BigInt value1 = BigInt.FromInt(1);

            Assert.False(value10.Equals((object)null));
            Assert.False(value10.Equals((object)"some text"));
            Assert.True(value10.Equals((object)value10));
            Assert.True(value10.Equals((object)value10_));
            Assert.False(value10.Equals((object)value20));
            Assert.True(value1.Equals((object)BigInt.One));
        }

        [Fact]
        public void BigInt_GetHashCode_ReturnsSameHashForSameObjects()
        {
            BigInt value10 = BigInt.FromInt(10);
            BigInt value10_ = BigInt.FromInt(10);
            BigInt value10_10 = BigInt.FromInt(10, 10);
            BigInt value10_10_ = BigInt.FromInt(10, 10);

            Assert.Equal(value10.GetHashCode(), value10_.GetHashCode());  // Note: precision is ignored
            Assert.Equal(value10_10.GetHashCode(), value10_10_.GetHashCode());
            Assert.Equal(value10.GetHashCode(), value10_10.GetHashCode());
            Assert.Equal(value10.GetHashCode(), value10_10_.GetHashCode());
        }

        [Fact]
        public void BigInt_Equals_ComparesValuesAndIgnoresPrecision()
        {
            BigInt valueA10 = BigInt.FromInt(10);
            BigInt valueA10_ = BigInt.FromInt(10, 5);
            BigInt valueA20 = BigInt.FromInt(20);
            BigInt valueA20_ = BigInt.FromInt(20, 5);

            BigInt valueB10 = BigInt.FromInt(10);
            BigInt valueB10_ = BigInt.FromInt(10, 5);
            BigInt valueB20 = BigInt.FromInt(20);
            BigInt valueB20_ = BigInt.FromInt(20, 5);
            
            Assert.True(valueA10 == valueA10_);
            Assert.True(valueA10 == valueA10_);
            Assert.False(valueA10 == valueA20);
            Assert.False(valueA10 == valueA20_);
            Assert.True(valueA10 == valueB10);
            Assert.True(valueA10 == valueB10_);
            Assert.False(valueA10 == valueA20);
            Assert.False(valueA10 == valueA20_);

            Assert.False(valueA10 != valueA10_);
            Assert.False(valueA10 != valueA10_);
            Assert.True(valueA10 != valueA20);
            Assert.True(valueA10 != valueA20_);
            Assert.False(valueA10 != valueB10);
            Assert.False(valueA10 != valueB10_);
            Assert.True(valueA10 != valueA20);
            Assert.True(valueA10 != valueA20_);

            Assert.True(valueA10.Equals(valueA10_));
            Assert.True(valueA10.Equals(valueA10_));
            Assert.False(valueA10.Equals(valueA20));
            Assert.False(valueA10.Equals(valueA20_));
            Assert.True(valueA10.Equals(valueB10));
            Assert.True(valueA10.Equals(valueB10_));
            Assert.False(valueA10.Equals(valueA20));
            Assert.False(valueA10.Equals(valueA20_));
        }

        [Fact]
        public void BigInt_ToString_ReturnsTextForValues()
        {
            Assert.Equal("0", BigInt.Zero.ToString());
            Assert.Equal("1", BigInt.One.ToString());
            Assert.Equal("10", BigInt.FromInt(10).ToString());
        }

        [Fact]
        public void BigInt_FitsInLong_ReturnsTrueIfValueCanBeConvertedToLong()
        {
            BigInt tooSmall = BigInt.Parse("-99999999999999999999");
            Assert.False(tooSmall.FitsInLong);

            BigInt normal = BigInt.Parse("99");
            Assert.True(normal.FitsInLong);

            BigInt tooBig = BigInt.Parse("99999999999999999999");
            Assert.False(tooBig.FitsInLong);
        }

        [Fact]
        public void BigInt_IsZeroInPrecision_ReturnsTrueForZero()
        {
            BigInt bigInt = BigInt.Parse("0");
            Assert.True(bigInt.IsZeroInPrecision());

            bigInt = BigInt.FromLong(0);
            bigInt.SetPrecision(4);
            Assert.True(bigInt.IsZeroInPrecision());
        }

        [Fact]
        public void BigInt_IsZeroInPrecision_ReturnsTrueForFractionalNumberWhenPrecisionIsZero()
        {
            BigInt bigInt = BigInt.Parse("0.5");
            Assert.True(bigInt.IsZeroInPrecision());
        }

        [Fact]
        public void BigInt_IsZeroInPrecision_ReturnsFalseForFractionalNumberWhenPrecisionIsBig()
        {
            BigInt bigInt = BigInt.Parse("0.5");
            bigInt = bigInt.SetPrecision(1);
            Assert.False(bigInt.IsZeroInPrecision());
        }

        [Fact]
        public void BigInt_IsZeroInPrecisionParameterized_UsesPrecisionParameterInsteadOfOwnOne()
        {
            BigInt bigInt = BigInt.Parse("0.005");
            Assert.False(bigInt.IsZeroInPrecision(4));
            Assert.False(bigInt.IsZeroInPrecision(3));
            Assert.True(bigInt.IsZeroInPrecision(2));
            Assert.True(bigInt.IsZeroInPrecision(1));
            Assert.True(bigInt.IsZeroInPrecision(0));
        }

        [Fact]
        public void BigInt_SetScale_CreatesFractialNumberAccordingToScale()
        {
            BigInt bigInt = BigInt.Parse("12345");
            bigInt = bigInt.SetScale(2);
            Assert.Equal(BigInt.Parse("123.45"), bigInt);
        }

        [Fact]
        public void BigInt_Compare_ReturnsZeroOrMinusOneIfGivenBigIntIsEmpty()
        {
            BigInt bigInt = new BigInt();
            Assert.Equal(-1, bigInt.Compare(BigInt.FromLong(0)));
            Assert.Equal(0, bigInt.Compare(bigInt));
        }

        [Fact]
        public void BigInt_Compare_ReturnsOneIfParamIsEmpty()
        {
            BigInt bigInt1 = BigInt.FromLong(10);
            BigInt bigInt2 = new BigInt();
            Assert.Equal(1, bigInt1.Compare(bigInt2));
        }

        [Fact]
        public void BigInt_Compare_ReturnsZeroIfValuesAreEqual()
        {
            BigInt bigInt1 = BigInt.FromLong(10);
            BigInt bigInt2 = BigInt.FromLong(10);
            Assert.Equal(0, bigInt1.Compare(bigInt2));
        }

        [Fact]
        public void BigInt_Compare_ReturnsMinusOneIfParamIsGreater()
        {
            BigInt bigInt1 = BigInt.FromLong(5);
            BigInt bigInt2 = BigInt.FromLong(10);
            Assert.Equal(-1, bigInt1.Compare(bigInt2));
        }

        [Fact]
        public void BigInt_Compare_ReturnsMinusOneIfParamIsLess()
        {
            BigInt bigInt1 = BigInt.FromLong(50);
            BigInt bigInt2 = BigInt.FromLong(10);
            Assert.Equal(1, bigInt1.Compare(bigInt2));
        }

        [Fact]
        public void BigInt_Print_DefaultDigitSeparatorIsComma()
        {
            BigInt bigInt1 = BigInt.FromLong(1000);
            Assert.Equal("1000", bigInt1.Print(0));
        }

        [Fact]
        public void BigInt_Print_DefaultDecimalMarkIsPoint()
        {
            BigInt bigInt1 = BigInt.FromLong(100);
            Assert.Equal("100.0", bigInt1.Print(1, 1));
        }

        [Fact]
        public void BigInt_Print_PrecisionCanRoundTheValue()
        {
            BigInt bigInt1 = BigInt.Parse("100.1234", 4);
            Assert.Equal("100.1234", bigInt1.Print(6, 4));
            Assert.Equal("100.1234", bigInt1.Print(5, 4));
            Assert.Equal("100.1234", bigInt1.Print(4, 4));
            Assert.Equal("100.123",  bigInt1.Print(3, 4));
            Assert.Equal("100.12",   bigInt1.Print(2, 4));
            Assert.Equal("100.1",    bigInt1.Print(1, 4));
            Assert.Equal("100",      bigInt1.Print(0, 4));
        }

        [Fact]
        public void BigInt_Print_ZerosSpecHasLessPriorityThanPrecision()
        {
            BigInt bigInt1 = BigInt.Parse("100.1234", 4);
            Assert.Equal("100.1234", bigInt1.Print(4, 6));
            Assert.Equal("100.1234", bigInt1.Print(4, 5));
            Assert.Equal("100.1234", bigInt1.Print(4, 4));
            Assert.Equal("100.1234", bigInt1.Print(4, 3));
            Assert.Equal("100.1234", bigInt1.Print(4, 2));
            Assert.Equal("100.1234", bigInt1.Print(4, 1));
            Assert.Equal("100.1234", bigInt1.Print(4, 0));
        }

        [Fact]
        public void BigInt_Print_CommodityHasToHaveStyleThousandsToAffectDigitSeparator()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            commodity2.Flags = CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS | CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;
            BigInt bigInt1 = BigInt.FromLong(1000);

            Assert.Equal("1000", bigInt1.Print(0, 0));
            Assert.Equal("1000", bigInt1.Print(0, 0, commodity1));
            Assert.Equal("1.000", bigInt1.Print(0, 0, commodity2));
        }

        [Fact]
        public void BigInt_Print_CommodityAffectsDecimalMark()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            commodity2.Flags = CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS | CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;
            BigInt bigInt1 = BigInt.FromLong(1000);

            Assert.Equal("1000.00", bigInt1.Print(2, 2));
            Assert.Equal("1000.00", bigInt1.Print(2, 2, commodity1));
            Assert.Equal("1.000,00", bigInt1.Print(2, 2, commodity2));
        }

        [Fact]
        public void BigInt_Print_NoDigitSeparatorsIfCommodityIsNull()
        {
            Assert.Equal("1000", BigInt.FromLong(1000).Print(0, 2));
        }

        [Fact]
        public void BigInt_Print_NoPrefixZerosForHundreds()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            commodity2.Flags = CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS | CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;
            BigInt bigInt1 = BigInt.FromLong(100);

            Assert.Equal("100.00", bigInt1.Print(2, 2));
            Assert.Equal("100.00", bigInt1.Print(2, 2, commodity1));
            Assert.Equal("100,00", bigInt1.Print(2, 2, commodity2));
        }

        [Fact]
        public void BigInt_Print_DigitSeparatorIsCommaIfCommodityDoesNotHave_COMMODITY_STYLE_DECIMAL_COMMA()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            comm.Flags = CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS; /* no COMMODITY_STYLE_DECIMAL_COMMA */
            Assert.Equal("1,000", BigInt.FromLong(1000).Print(0, 2, comm));
        }

    }
}

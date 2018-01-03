// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Commodities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests
{
    [TestClass]
    public class BigIntTests : TestFixture
    {
        [TestMethod]
        public void BigInt_ZeroConstant_IsZero()
        {
            Assert.IsTrue(BigInt.Zero.HasValue);
            Assert.AreEqual(0, BigInt.Zero.ToLong());
        }

        [TestMethod]
        public void BigInt_OneConstant_IsOne()
        {
            Assert.IsTrue(BigInt.One.HasValue);
            Assert.AreEqual(1, BigInt.One.ToLong());
        }

        [TestMethod]
        public void BigInt_DivisionOperator_ReturnsDividedValue()
        {
            BigInt result = BigInt.FromInt(10) / BigInt.FromInt(5);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(2, result.ToLong());
        }

        [TestMethod]
        public void BigInt_SubtractOperator_ReturnsSubtractedValue()
        {
            BigInt result = BigInt.FromInt(10) - BigInt.FromInt(7);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(3, result.ToLong());
        }

        [TestMethod]
        public void BigInt_MultiplicityOperator_ReturnsMultiplicideValue()
        {
            BigInt result = BigInt.FromInt(10) * BigInt.FromInt(5);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(50, result.ToLong());
        }

        [TestMethod]
        public void BigInt_EqualOperator_ComparesDifferentInstances()
        {
            BigInt value10 = BigInt.FromInt(10);
            BigInt value10_ = BigInt.FromInt(10);
            BigInt value20 = BigInt.FromInt(20);
            BigInt value1 = BigInt.FromInt(1);

            Assert.IsTrue(value10 == value10);
            Assert.IsTrue(value10 == value10_);
            Assert.IsFalse(value10 == value20);
            Assert.IsTrue(value1 == BigInt.One);
        }

        [TestMethod]
        public void BigInt_NotEqualOperator_ComparesDifferentInstances()
        {
            BigInt value10 = BigInt.FromInt(10);
            BigInt value10_ = BigInt.FromInt(10);
            BigInt value20 = BigInt.FromInt(20);
            BigInt value1 = BigInt.FromInt(1);

            Assert.IsFalse(value10 != value10);
            Assert.IsFalse(value10 != value10_);
            Assert.IsTrue(value10 != value20);
            Assert.IsFalse(value1 != BigInt.One);
        }

        [TestMethod]
        public void BigInt_Parse_ConvertsStringsToBigInts()
        {
            Assert.AreEqual(BigInt.Zero, BigInt.Parse("0"));
            Assert.AreEqual(BigInt.One, BigInt.Parse("1"));
            Assert.AreEqual(BigInt.FromInt(100), BigInt.Parse("100"));
        }

        [TestMethod]
        public void BigInt_FromInt_ConvertsIntegersToBigInts()
        {
            Assert.AreEqual(0, BigInt.FromInt(0).ToLong());
            Assert.AreEqual(1, BigInt.FromInt(1).ToLong());
            Assert.AreEqual(10, BigInt.FromInt(10).ToLong());
        }

        [TestMethod]
        public void BigInt_ToLong_PresentsInputValueAsInteger()
        {
            Assert.AreEqual(0, BigInt.FromInt(0).ToLong());
            Assert.AreEqual(1, BigInt.FromInt(1).ToLong());
            Assert.AreEqual(10, BigInt.FromInt(10).ToLong());
        }

        [TestMethod]
        public void BigInt_ToLong_RoundsFractialNumbers()
        {
            Assert.AreEqual(1, BigInt.Parse("1.1").ToLong());
            Assert.AreEqual(2, BigInt.Parse("1.9").ToLong());
        }

        [TestMethod]
        public void BigInt_Precision_IsKeptAsSpecified()
        {
            Assert.AreEqual(0, BigInt.FromInt(1).Precision);
            Assert.AreEqual(10, BigInt.FromInt(1, 10).Precision);
        }

        [TestMethod]
        public void BigInt_HasValue_ReflectsWhetherValueIsPopulated()
        {
            BigInt uninitialized = default(BigInt);
            Assert.IsFalse(uninitialized.HasValue);

            uninitialized = BigInt.FromInt(10);
            Assert.IsTrue(uninitialized.HasValue);
        }

        [TestMethod]
        public void BigInt_Valid_CheckdWhetherPrecisonLessThan2014()
        {
            BigInt value1 = BigInt.FromInt(10);
            Assert.IsTrue(value1.Valid());

            BigInt value2 = BigInt.FromInt(10, 100);
            Assert.IsTrue(value2.Valid());

            BigInt value3 = BigInt.FromInt(10, 1500);
            Assert.IsFalse(value3.Valid());
        }

        [TestMethod]
        public void BigInt_Negative_ReturnsNegaitveValue()
        {
            Assert.AreEqual(BigInt.FromInt(0), BigInt.Zero.Negative());
            Assert.AreEqual(BigInt.FromInt(-1), BigInt.One.Negative());
            Assert.AreEqual(BigInt.FromInt(-10), BigInt.FromInt(10).Negative());
            Assert.AreEqual(BigInt.FromInt(10), BigInt.FromInt(-10).Negative());
        }

        [TestMethod]
        public void BigInt_Abs_ReturnsAbsoluteValue()
        {
            Assert.AreEqual(BigInt.FromInt(0), BigInt.Zero.Abs());
            Assert.AreEqual(BigInt.FromInt(1), BigInt.One.Abs());
            Assert.AreEqual(BigInt.FromInt(10), BigInt.FromInt(10).Abs());
            Assert.AreEqual(BigInt.FromInt(10), BigInt.FromInt(-10).Abs());
        }

        [TestMethod]
        public void BigInt_Abs_ReturnsOriginalInstanceIfNotInitialized()
        {
            BigInt uninitialized = default(BigInt);
            BigInt absForUninitialized = uninitialized.Abs();
            Assert.AreEqual(uninitialized, absForUninitialized);
        }

        [TestMethod]
        public void BigInt_SetPrecision_ChangesPrecision()
        {
            BigInt value = BigInt.FromInt(10, 20);
            Assert.AreEqual(20, value.Precision);
            BigInt value1 = value.SetPrecision(30);
            Assert.AreEqual(20, value.Precision); // Note that the original instance is not changed
            Assert.AreEqual(30, value1.Precision);
        }

        [TestMethod]
        public void BigInt_TypedEquals_ComparesDifferentInstances()
        {
            BigInt value10 = BigInt.FromInt(10);
            BigInt value10_ = BigInt.FromInt(10);
            BigInt value20 = BigInt.FromInt(20);
            BigInt value1 = BigInt.FromInt(1);

            Assert.IsTrue(value10.Equals(value10));
            Assert.IsTrue(value10.Equals(value10_));
            Assert.IsFalse(value10.Equals(value20));
            Assert.IsTrue(value1.Equals(BigInt.One));
        }

        [TestMethod]
        public void BigInt_UntypedEquals_ComparesObjectsWithInstances()
        {
            BigInt value10 = BigInt.FromInt(10);
            BigInt value10_ = BigInt.FromInt(10);
            BigInt value20 = BigInt.FromInt(20);
            BigInt value1 = BigInt.FromInt(1);

            Assert.IsFalse(value10.Equals((object)null));
            Assert.IsFalse(value10.Equals((object)"some text"));
            Assert.IsTrue(value10.Equals((object)value10));
            Assert.IsTrue(value10.Equals((object)value10_));
            Assert.IsFalse(value10.Equals((object)value20));
            Assert.IsTrue(value1.Equals((object)BigInt.One));
        }

        [TestMethod]
        public void BigInt_GetHashCode_ReturnsSameHashForSameObjects()
        {
            BigInt value10 = BigInt.FromInt(10);
            BigInt value10_ = BigInt.FromInt(10);
            BigInt value10_10 = BigInt.FromInt(10, 10);
            BigInt value10_10_ = BigInt.FromInt(10, 10);

            Assert.AreEqual(value10.GetHashCode(), value10_.GetHashCode());  // Note: precision is ignored
            Assert.AreEqual(value10_10.GetHashCode(), value10_10_.GetHashCode());
            Assert.AreEqual(value10.GetHashCode(), value10_10.GetHashCode());
            Assert.AreEqual(value10.GetHashCode(), value10_10_.GetHashCode());
        }

        [TestMethod]
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
            
            Assert.IsTrue(valueA10 == valueA10_);
            Assert.IsTrue(valueA10 == valueA10_);
            Assert.IsFalse(valueA10 == valueA20);
            Assert.IsFalse(valueA10 == valueA20_);
            Assert.IsTrue(valueA10 == valueB10);
            Assert.IsTrue(valueA10 == valueB10_);
            Assert.IsFalse(valueA10 == valueA20);
            Assert.IsFalse(valueA10 == valueA20_);

            Assert.IsFalse(valueA10 != valueA10_);
            Assert.IsFalse(valueA10 != valueA10_);
            Assert.IsTrue(valueA10 != valueA20);
            Assert.IsTrue(valueA10 != valueA20_);
            Assert.IsFalse(valueA10 != valueB10);
            Assert.IsFalse(valueA10 != valueB10_);
            Assert.IsTrue(valueA10 != valueA20);
            Assert.IsTrue(valueA10 != valueA20_);

            Assert.IsTrue(valueA10.Equals(valueA10_));
            Assert.IsTrue(valueA10.Equals(valueA10_));
            Assert.IsFalse(valueA10.Equals(valueA20));
            Assert.IsFalse(valueA10.Equals(valueA20_));
            Assert.IsTrue(valueA10.Equals(valueB10));
            Assert.IsTrue(valueA10.Equals(valueB10_));
            Assert.IsFalse(valueA10.Equals(valueA20));
            Assert.IsFalse(valueA10.Equals(valueA20_));
        }

        [TestMethod]
        public void BigInt_ToString_ReturnsTextForValues()
        {
            Assert.AreEqual("0", BigInt.Zero.ToString());
            Assert.AreEqual("1", BigInt.One.ToString());
            Assert.AreEqual("10", BigInt.FromInt(10).ToString());
        }

        [TestMethod]
        public void BigInt_FitsInLong_ReturnsTrueIfValueCanBeConvertedToLong()
        {
            BigInt tooSmall = BigInt.Parse("-99999999999999999999");
            Assert.IsFalse(tooSmall.FitsInLong);

            BigInt normal = BigInt.Parse("99");
            Assert.IsTrue(normal.FitsInLong);

            BigInt tooBig = BigInt.Parse("99999999999999999999");
            Assert.IsFalse(tooBig.FitsInLong);
        }

        [TestMethod]
        public void BigInt_IsZeroInPrecision_ReturnsTrueForZero()
        {
            BigInt bigInt = BigInt.Parse("0");
            Assert.IsTrue(bigInt.IsZeroInPrecision());

            bigInt = BigInt.FromLong(0);
            bigInt.SetPrecision(4);
            Assert.IsTrue(bigInt.IsZeroInPrecision());
        }

        [TestMethod]
        public void BigInt_IsZeroInPrecision_ReturnsTrueForFractionalNumberWhenPrecisionIsZero()
        {
            BigInt bigInt = BigInt.Parse("0.5");
            Assert.IsTrue(bigInt.IsZeroInPrecision());
        }

        [TestMethod]
        public void BigInt_IsZeroInPrecision_ReturnsFalseForFractionalNumberWhenPrecisionIsBig()
        {
            BigInt bigInt = BigInt.Parse("0.5");
            bigInt = bigInt.SetPrecision(1);
            Assert.IsFalse(bigInt.IsZeroInPrecision());
        }

        [TestMethod]
        public void BigInt_IsZeroInPrecisionParameterized_UsesPrecisionParameterInsteadOfOwnOne()
        {
            BigInt bigInt = BigInt.Parse("0.005");
            Assert.IsFalse(bigInt.IsZeroInPrecision(4));
            Assert.IsFalse(bigInt.IsZeroInPrecision(3));
            Assert.IsTrue(bigInt.IsZeroInPrecision(2));
            Assert.IsTrue(bigInt.IsZeroInPrecision(1));
            Assert.IsTrue(bigInt.IsZeroInPrecision(0));
        }

        [TestMethod]
        public void BigInt_SetScale_CreatesFractialNumberAccordingToScale()
        {
            BigInt bigInt = BigInt.Parse("12345");
            bigInt = bigInt.SetScale(2);
            Assert.AreEqual(BigInt.Parse("123.45"), bigInt);
        }

        [TestMethod]
        public void BigInt_Compare_ReturnsZeroOrMinusOneIfGivenBigIntIsEmpty()
        {
            BigInt bigInt = new BigInt();
            Assert.AreEqual(-1, bigInt.Compare(BigInt.FromLong(0)));
            Assert.AreEqual(0, bigInt.Compare(bigInt));
        }

        [TestMethod]
        public void BigInt_Compare_ReturnsOneIfParamIsEmpty()
        {
            BigInt bigInt1 = BigInt.FromLong(10);
            BigInt bigInt2 = new BigInt();
            Assert.AreEqual(1, bigInt1.Compare(bigInt2));
        }

        [TestMethod]
        public void BigInt_Compare_ReturnsZeroIfValuesAreEqual()
        {
            BigInt bigInt1 = BigInt.FromLong(10);
            BigInt bigInt2 = BigInt.FromLong(10);
            Assert.AreEqual(0, bigInt1.Compare(bigInt2));
        }

        [TestMethod]
        public void BigInt_Compare_ReturnsMinusOneIfParamIsGreater()
        {
            BigInt bigInt1 = BigInt.FromLong(5);
            BigInt bigInt2 = BigInt.FromLong(10);
            Assert.AreEqual(-1, bigInt1.Compare(bigInt2));
        }

        [TestMethod]
        public void BigInt_Compare_ReturnsMinusOneIfParamIsLess()
        {
            BigInt bigInt1 = BigInt.FromLong(50);
            BigInt bigInt2 = BigInt.FromLong(10);
            Assert.AreEqual(1, bigInt1.Compare(bigInt2));
        }

        [TestMethod]
        public void BigInt_Print_DefaultDigitSeparatorIsComma()
        {
            BigInt bigInt1 = BigInt.FromLong(1000);
            Assert.AreEqual("1000", bigInt1.Print(0));
        }

        [TestMethod]
        public void BigInt_Print_DefaultDecimalMarkIsPoint()
        {
            BigInt bigInt1 = BigInt.FromLong(100);
            Assert.AreEqual("100.0", bigInt1.Print(1, 1));
        }

        [TestMethod]
        public void BigInt_Print_PrecisionCanRoundTheValue()
        {
            BigInt bigInt1 = BigInt.Parse("100.1234", 4);
            Assert.AreEqual("100.1234", bigInt1.Print(6, 4));
            Assert.AreEqual("100.1234", bigInt1.Print(5, 4));
            Assert.AreEqual("100.1234", bigInt1.Print(4, 4));
            Assert.AreEqual("100.123",  bigInt1.Print(3, 4));
            Assert.AreEqual("100.12",   bigInt1.Print(2, 4));
            Assert.AreEqual("100.1",    bigInt1.Print(1, 4));
            Assert.AreEqual("100",      bigInt1.Print(0, 4));
        }

        [TestMethod]
        public void BigInt_Print_ZerosSpecHasLessPriorityThanPrecision()
        {
            BigInt bigInt1 = BigInt.Parse("100.1234", 4);
            Assert.AreEqual("100.1234", bigInt1.Print(4, 6));
            Assert.AreEqual("100.1234", bigInt1.Print(4, 5));
            Assert.AreEqual("100.1234", bigInt1.Print(4, 4));
            Assert.AreEqual("100.1234", bigInt1.Print(4, 3));
            Assert.AreEqual("100.1234", bigInt1.Print(4, 2));
            Assert.AreEqual("100.1234", bigInt1.Print(4, 1));
            Assert.AreEqual("100.1234", bigInt1.Print(4, 0));
        }

        [TestMethod]
        public void BigInt_Print_CommodityHasToHaveStyleThousandsToAffectDigitSeparator()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            commodity2.Flags = CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS | CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;
            BigInt bigInt1 = BigInt.FromLong(1000);

            Assert.AreEqual("1000", bigInt1.Print(0, 0));
            Assert.AreEqual("1000", bigInt1.Print(0, 0, commodity1));
            Assert.AreEqual("1.000", bigInt1.Print(0, 0, commodity2));
        }

        [TestMethod]
        public void BigInt_Print_CommodityAffectsDecimalMark()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            commodity2.Flags = CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS | CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;
            BigInt bigInt1 = BigInt.FromLong(1000);

            Assert.AreEqual("1000.00", bigInt1.Print(2, 2));
            Assert.AreEqual("1000.00", bigInt1.Print(2, 2, commodity1));
            Assert.AreEqual("1.000,00", bigInt1.Print(2, 2, commodity2));
        }

        [TestMethod]
        public void BigInt_Print_NoDigitSeparatorsIfCommodityIsNull()
        {
            Assert.AreEqual("1000", BigInt.FromLong(1000).Print(0, 2));
        }

        [TestMethod]
        public void BigInt_Print_NoPrefixZerosForHundreds()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            commodity2.Flags = CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS | CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;
            BigInt bigInt1 = BigInt.FromLong(100);

            Assert.AreEqual("100.00", bigInt1.Print(2, 2));
            Assert.AreEqual("100.00", bigInt1.Print(2, 2, commodity1));
            Assert.AreEqual("100,00", bigInt1.Print(2, 2, commodity2));
        }

        [TestMethod]
        public void BigInt_Print_DigitSeparatorIsCommaIfCommodityDoesNotHave_COMMODITY_STYLE_DECIMAL_COMMA()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            comm.Flags = CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS; /* no COMMODITY_STYLE_DECIMAL_COMMA */
            Assert.AreEqual("1,000", BigInt.FromLong(1000).Print(0, 2, comm));
        }

    }
}

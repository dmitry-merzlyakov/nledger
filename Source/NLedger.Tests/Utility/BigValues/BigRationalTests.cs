// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Utility.BigValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utility.BigValues
{
    [TestClass]
    public class BigRationalTests
    {
        public static readonly string VeryBigNumber = "1234567890123456789012345678901234567890";
        public static readonly string DoubleVeryBig = "2469135780246913578024691357802469135780";
        public static readonly string MultiplNumber = "1524157875323883675049535156256668194500533455762536198787501905199875019052100";

        [TestMethod]
        public void BigRational_ToString_SupportsSpecificFormatForIntegralPart()
        {
            BigRational bigRational1 = BigRational.Create(12345m);
            Assert.AreEqual("12345", bigRational1.ToString("R", null));
            Assert.AreEqual("12345/1", bigRational1.ToString("B", null));
            Assert.AreEqual("12345", bigRational1.ToString("0", null));
            Assert.AreEqual("12,345", bigRational1.ToString("#,##0", null));
        }

        [TestMethod]
        public void BigRational_ToString_SupportsSpecificFormatForFractionalPart()
        {
            BigRational bigRational1 = BigRational.Create(12345m);
            Assert.AreEqual("12345.00", bigRational1.ToString("0.00", null));
            Assert.AreEqual("12345", bigRational1.ToString("0.##", null));
            Assert.AreEqual("12345.0", bigRational1.ToString("0.0#", null));
            Assert.AreEqual("12345", bigRational1.ToString("0.##", null));

            BigRational bigRational2 = BigRational.Create(12345.1m);
            Assert.AreEqual("12345.10", bigRational2.ToString("0.00", null));
            Assert.AreEqual("12345.1", bigRational2.ToString("0.##", null));
            Assert.AreEqual("12345.1", bigRational2.ToString("0.0#", null));
            Assert.AreEqual("12345.1", bigRational2.ToString("0.##", null));

            BigRational bigRational3 = BigRational.Create(12345.9m);
            Assert.AreEqual("12345.90", bigRational3.ToString("0.00", null));
            Assert.AreEqual("12345.9", bigRational3.ToString("0.##", null));
            Assert.AreEqual("12345.9", bigRational3.ToString("0.0#", null));
            Assert.AreEqual("12345.9", bigRational3.ToString("0.##", null));
        }

        [TestMethod]
        public void BigRational_ToString_SupportsRoundingOfValues()
        {
            BigRational bigRational2 = BigRational.Create(12345.1m);
            Assert.AreEqual("12345", bigRational2.ToString("0", null));

            BigRational bigRational3 = BigRational.Create(12345.9m);
            Assert.AreEqual("12346", bigRational3.ToString("0", null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BigRational_Round_DoesNotAllowNegativeDecimals()
        {
            BigRational bigRational = BigRational.Create(1.0m);
            bigRational.Round(out bigRational, - 1);
        }

        [TestMethod]
        public void BigRational_Round_DoesNothingIfNoFractionalPart()
        {
            BigRational bigRational1 = BigRational.Create(100.0m);
            Assert.AreEqual(bigRational1, bigRational1.Round(0));
            Assert.AreEqual(bigRational1, bigRational1.Round(1));
            Assert.AreEqual(bigRational1, bigRational1.Round(2));

            BigRational bigRational2 = BigRational.Create(1.0m);
            Assert.AreEqual(bigRational2, bigRational2.Round(0));
            Assert.AreEqual(bigRational2, bigRational2.Round(1));
            Assert.AreEqual(bigRational2, bigRational2.Round(2));

            BigRational bigRational3 = BigRational.Create(0m);
            Assert.AreEqual(bigRational3, bigRational3.Round(0));
            Assert.AreEqual(bigRational3, bigRational3.Round(1));
            Assert.AreEqual(bigRational3, bigRational3.Round(2));

            BigRational bigRational4 = BigRational.Create(-1.0m);
            Assert.AreEqual(bigRational4, bigRational4.Round(0));
            Assert.AreEqual(bigRational4, bigRational4.Round(1));
            Assert.AreEqual(bigRational4, bigRational4.Round(2));

            BigRational bigRational5 = BigRational.Create(-100.0m);
            Assert.AreEqual(bigRational5, bigRational5.Round(0));
            Assert.AreEqual(bigRational5, bigRational5.Round(1));
            Assert.AreEqual(bigRational5, bigRational5.Round(2));
        }

        [TestMethod]
        public void BigRational_Round_DoesNothingIfPrecisionIsHigher()
        {
            BigRational bigRational1 = BigRational.Create(100.1m);
            Assert.AreEqual(bigRational1, bigRational1.Round(1));
            Assert.AreEqual(bigRational1, bigRational1.Round(2));
            Assert.AreEqual(bigRational1, bigRational1.Round(3));

            BigRational bigRational2 = BigRational.Create(100.12m);
            Assert.AreEqual(bigRational2, bigRational2.Round(2));
            Assert.AreEqual(bigRational2, bigRational2.Round(3));

            BigRational bigRational3 = BigRational.Create(-100.12m);
            Assert.AreEqual(bigRational3, bigRational3.Round(2));
            Assert.AreEqual(bigRational3, bigRational3.Round(3));

            BigRational bigRational4 = BigRational.Create(-100.1m);
            Assert.AreEqual(bigRational4, bigRational4.Round(1));
            Assert.AreEqual(bigRational4, bigRational4.Round(2));
            Assert.AreEqual(bigRational4, bigRational4.Round(3));
        }

        [TestMethod]
        public void BigRational_Round_DoesItToLowest()
        {
            BigRational bigRational1 = BigRational.Create(100.123m);
            Assert.AreEqual(BigRational.Create(100m), bigRational1.Round(0));
            Assert.AreEqual(BigRational.Create(100.1m), bigRational1.Round(1));
            Assert.AreEqual(BigRational.Create(100.12m), bigRational1.Round(2));
            Assert.AreEqual(BigRational.Create(100.123m), bigRational1.Round(3));
            Assert.AreEqual(BigRational.Create(100.123m), bigRational1.Round(4));

            BigRational bigRational2 = BigRational.Create(-100.123m);
            Assert.AreEqual(BigRational.Create(-100m), bigRational2.Round(0));
            Assert.AreEqual(BigRational.Create(-100.1m), bigRational2.Round(1));
            Assert.AreEqual(BigRational.Create(-100.12m), bigRational2.Round(2));
            Assert.AreEqual(BigRational.Create(-100.123m), bigRational2.Round(3));
            Assert.AreEqual(BigRational.Create(-100.123m), bigRational2.Round(4));
        }

        [TestMethod]
        public void BigRational_Round_DoesItToHighest()
        {
            BigRational bigRational1 = BigRational.Create(100.987m);
            Assert.AreEqual(BigRational.Create(101m), bigRational1.Round(0));
            Assert.AreEqual(BigRational.Create(101m), bigRational1.Round(1));
            Assert.AreEqual(BigRational.Create(100.99m), bigRational1.Round(2));
            Assert.AreEqual(BigRational.Create(100.987m), bigRational1.Round(3));
            Assert.AreEqual(BigRational.Create(100.987m), bigRational1.Round(4));

            BigRational bigRational2 = BigRational.Create(-100.987m);
            Assert.AreEqual(BigRational.Create(-101m), bigRational2.Round(0));
            Assert.AreEqual(BigRational.Create(-101m), bigRational2.Round(1));
            Assert.AreEqual(BigRational.Create(-100.99m), bigRational2.Round(2));
            Assert.AreEqual(BigRational.Create(-100.987m), bigRational2.Round(3));
            Assert.AreEqual(BigRational.Create(-100.987m), bigRational2.Round(4));
        }

        [TestMethod]
        public void BigRational_Round_DoesItToEven()
        {
            BigRational bigRational1 = BigRational.Create(100.152535455565758595m);
            Assert.AreEqual(BigRational.Create(100m), bigRational1.Round(0));
            Assert.AreEqual(BigRational.Create(100.2m), bigRational1.Round(1));
            Assert.AreEqual(BigRational.Create(100.15m), bigRational1.Round(2));
            Assert.AreEqual(BigRational.Create(100.153m), bigRational1.Round(3));
            Assert.AreEqual(BigRational.Create(100.1525m), bigRational1.Round(4));
            Assert.AreEqual(BigRational.Create(100.15254m), bigRational1.Round(5));
            Assert.AreEqual(BigRational.Create(100.152535m), bigRational1.Round(6));
            Assert.AreEqual(BigRational.Create(100.1525355m), bigRational1.Round(7));
            Assert.AreEqual(BigRational.Create(100.15253546m), bigRational1.Round(8));
            Assert.AreEqual(BigRational.Create(100.152535456m), bigRational1.Round(9));
            Assert.AreEqual(BigRational.Create(100.1525354556m), bigRational1.Round(10));
            Assert.AreEqual(BigRational.Create(100.15253545557m), bigRational1.Round(11));
            Assert.AreEqual(BigRational.Create(100.152535455566m), bigRational1.Round(12));
            Assert.AreEqual(BigRational.Create(100.1525354555658m), bigRational1.Round(13));
            Assert.AreEqual(BigRational.Create(100.15253545556576m), bigRational1.Round(14));
            Assert.AreEqual(BigRational.Create(100.152535455565759m), bigRational1.Round(15));
            Assert.AreEqual(BigRational.Create(100.1525354555657586m), bigRational1.Round(16));
            Assert.AreEqual(BigRational.Create(100.15253545556575860m), bigRational1.Round(17));
            Assert.AreEqual(BigRational.Create(100.152535455565758595m), bigRational1.Round(18));
            Assert.AreEqual(BigRational.Create(100.152535455565758595m), bigRational1.Round(19));

            BigRational bigRational2 = BigRational.Create(-100.152535455565758595m);
            Assert.AreEqual(BigRational.Create(-100m), bigRational2.Round(0));
            Assert.AreEqual(BigRational.Create(-100.2m), bigRational2.Round(1));
            Assert.AreEqual(BigRational.Create(-100.15m), bigRational2.Round(2));
            Assert.AreEqual(BigRational.Create(-100.153m), bigRational2.Round(3));
            Assert.AreEqual(BigRational.Create(-100.1525m), bigRational2.Round(4));
            Assert.AreEqual(BigRational.Create(-100.15254m), bigRational2.Round(5));
            Assert.AreEqual(BigRational.Create(-100.152535m), bigRational2.Round(6));
            Assert.AreEqual(BigRational.Create(-100.1525355m), bigRational2.Round(7));
            Assert.AreEqual(BigRational.Create(-100.15253546m), bigRational2.Round(8));
            Assert.AreEqual(BigRational.Create(-100.152535456m), bigRational2.Round(9));
            Assert.AreEqual(BigRational.Create(-100.1525354556m), bigRational2.Round(10));
            Assert.AreEqual(BigRational.Create(-100.15253545557m), bigRational2.Round(11));
            Assert.AreEqual(BigRational.Create(-100.152535455566m), bigRational2.Round(12));
            Assert.AreEqual(BigRational.Create(-100.1525354555658m), bigRational2.Round(13));
            Assert.AreEqual(BigRational.Create(-100.15253545556576m), bigRational2.Round(14));
            Assert.AreEqual(BigRational.Create(-100.152535455565759m), bigRational2.Round(15));
            Assert.AreEqual(BigRational.Create(-100.1525354555657586m), bigRational2.Round(16));
            Assert.AreEqual(BigRational.Create(-100.15253545556575860m), bigRational2.Round(17));
            Assert.AreEqual(BigRational.Create(-100.152535455565758595m), bigRational2.Round(18));
            Assert.AreEqual(BigRational.Create(-100.152535455565758595m), bigRational2.Round(19));
        }

        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void BigRational_Constructor_TwoBigInteger_ProducesDividedByZeroIfDenominatorIsZero()
        {
            new BigRational((BigInteger)1, (BigInteger)0);
        }

        [TestMethod]
        public void BigRational_Constructor_TwoBigInteger_HandlesZeroNumenator()
        {
            var value = new BigRational((BigInteger)0, (BigInteger)100);
            Assert.AreEqual("0/1", value.ToString("B", null));
        }

        [TestMethod]
        public void BigRational_Constructor_TwoBigInteger_HandlesNegativeDenumenator()
        {
            var value1 = new BigRational((BigInteger)100, -(BigInteger)100);
            Assert.AreEqual("-1/1", value1.ToString("B", null));

            var value2 = new BigRational(-(BigInteger)100, -(BigInteger)100);
            Assert.AreEqual("1/1", value2.ToString("B", null));
        }

        [TestMethod]
        public void BigRational_Constructor_TwoBigInteger_SimplifiesFraction()
        {
            var value1 = new BigRational((BigInteger)20, (BigInteger)10);
            Assert.AreEqual("2/1", value1.ToString("B", null));

            var value2 = new BigRational((BigInteger)10, (BigInteger)10);
            Assert.AreEqual("1/1", value2.ToString("B", null));

            var value3 = new BigRational((BigInteger)4, (BigInteger)6);
            Assert.AreEqual("2/3", value3.ToString("B", null));
        }

        [TestMethod]
        public void BigRational_Constructor_OneBigInteger_AddsDenominatorEqualOne()
        {
            var value1 = new BigRational((BigInteger)20);
            Assert.AreEqual("20/1", value1.ToString("B", null));

            var value2 = new BigRational(-(BigInteger)10);
            Assert.AreEqual("-10/1", value2.ToString("B", null));
        }

        [TestMethod]
        public void BigRational_Abs_ReturnsAbsoluteValue()
        {
            var value1 = new BigRational((BigInteger)10);
            Assert.AreEqual("10/1", value1.Abs().ToString("B", null));

            var value2 = new BigRational(-(BigInteger)10);
            Assert.AreEqual("10/1", value2.Abs().ToString("B", null));

            var value3 = BigRational.Zero;
            Assert.AreEqual("0/1", value3.Abs().ToString("B", null));

            var value4 = BigRational.Create(VeryBigNumber);
            Assert.AreEqual($"{VeryBigNumber}/1", value4.Abs().ToString("B", null));

            var value5 = BigRational.Create($"-{VeryBigNumber}");
            Assert.AreEqual($"{VeryBigNumber}/1", value4.Abs().ToString("B", null));
        }

        [TestMethod]
        public void BigRational_Add_ReturnsAdditionOfValues()
        {
            Assert.AreEqual("0/1", (BigRational.Create(0).Add(BigRational.Create(0))).ToString("B", null));
            Assert.AreEqual("2/1", (BigRational.Create(0).Add(BigRational.Create(2))).ToString("B", null));
            Assert.AreEqual("2/1", (BigRational.Create(2).Add(BigRational.Create(0))).ToString("B", null));
            Assert.AreEqual("-2/1", (BigRational.Create(0).Add(BigRational.Create(-2))).ToString("B", null));
            Assert.AreEqual("-2/1", (BigRational.Create(-2).Add(BigRational.Create(0))).ToString("B", null));
            Assert.AreEqual("5/1", (BigRational.Create(2).Add(BigRational.Create(3))).ToString("B", null));
            Assert.AreEqual("5/1", (BigRational.Create(3).Add(BigRational.Create(2))).ToString("B", null));
            Assert.AreEqual("1/1", (BigRational.Create(3).Add(BigRational.Create(-2))).ToString("B", null));
            Assert.AreEqual("-1/1", (BigRational.Create(-3).Add(BigRational.Create(2))).ToString("B", null));
            Assert.AreEqual("-6/1", (BigRational.Create(-3).Add(BigRational.Create(-3))).ToString("B", null));

            Assert.AreEqual($"{DoubleVeryBig}/1", (BigRational.Create(VeryBigNumber).Add(BigRational.Create(VeryBigNumber))).ToString("B", null));
            Assert.AreEqual($"-{DoubleVeryBig}/1", (BigRational.Create($"-{VeryBigNumber}").Add(BigRational.Create($"-{VeryBigNumber}"))).ToString("B", null));
            Assert.AreEqual($"0/1", (BigRational.Create(VeryBigNumber).Add(BigRational.Create($"-{VeryBigNumber}"))).ToString("B", null));
        }

        [TestMethod]
        public void BigRational_Ceiling_ReturnsOriginalValueIfFractionIsZero()
        {
            Assert.AreEqual("0/1", (BigRational.Create(0).Ceiling()).ToString("B", null));
            Assert.AreEqual("10/1", (BigRational.Create(10).Ceiling()).ToString("B", null));
            Assert.AreEqual("-10/1", (BigRational.Create(-10).Ceiling()).ToString("B", null));
        }

        [TestMethod]
        public void BigRational_Ceiling_ReturnsSmallestIntegralValueThatIsGreaterThanSpecified()
        {
            Assert.AreEqual("1/1", (BigRational.Create(0.3m).Ceiling()).ToString("B", null));
            Assert.AreEqual("1/1", (BigRational.Create(0.5m).Ceiling()).ToString("B", null));
            Assert.AreEqual("1/1", (BigRational.Create(0.7m).Ceiling()).ToString("B", null));
            Assert.AreEqual("11/1", (BigRational.Create(10.5m).Ceiling()).ToString("B", null));

            Assert.AreEqual("0/1", (BigRational.Create(-0.3m).Ceiling()).ToString("B", null));
            Assert.AreEqual("0/1", (BigRational.Create(-0.5m).Ceiling()).ToString("B", null));
            Assert.AreEqual("0/1", (BigRational.Create(-0.7m).Ceiling()).ToString("B", null));
            Assert.AreEqual("-10/1", (BigRational.Create(-10.5m).Ceiling()).ToString("B", null));
        }

        [TestMethod]
        public void BigRational_CompareTo_HandlesSmallerEqualAndHigherValuesToCompare()
        {
            Assert.AreEqual(0, BigRational.Create(0).CompareTo(BigRational.Create(0)));

            Assert.AreEqual(1, BigRational.Create(1).CompareTo(BigRational.Create(0)));
            Assert.AreEqual(-1, BigRational.Create(0).CompareTo(BigRational.Create(1)));
            Assert.AreEqual(1, BigRational.Create(10).CompareTo(BigRational.Create(5)));
            Assert.AreEqual(-1, BigRational.Create(5).CompareTo(BigRational.Create(10)));

            Assert.AreEqual(-1, BigRational.Create(-1).CompareTo(BigRational.Create(0)));
            Assert.AreEqual(1, BigRational.Create(0).CompareTo(BigRational.Create(-1)));
            Assert.AreEqual(-1, BigRational.Create(-10).CompareTo(BigRational.Create(5)));
            Assert.AreEqual(1, BigRational.Create(5).CompareTo(BigRational.Create(-10)));

            Assert.AreEqual(-1, BigRational.Create(1.1m).CompareTo(BigRational.Create(1.2m)));
            Assert.AreEqual(1, BigRational.Create(1.2m).CompareTo(BigRational.Create(1.1m)));
            Assert.AreEqual(0, BigRational.Create(1.2m).CompareTo(BigRational.Create(1.2m)));

            Assert.AreEqual(-1, BigRational.Create(-1.1m).CompareTo(BigRational.Create(-0.2m)));
            Assert.AreEqual(1, BigRational.Create(-1.2m).CompareTo(BigRational.Create(-1.4m)));
            Assert.AreEqual(0, BigRational.Create(-1.2m).CompareTo(BigRational.Create(-1.2m)));
        }

        [TestMethod]
        public void BigRational_ConvertibleToLong_IndicatesWhetherStoredValueCanBeConvertedToLong()
        {
            Assert.IsTrue(BigRational.Create(0).ConvertibleToLong());
            Assert.IsTrue(BigRational.Create(100).ConvertibleToLong());
            Assert.IsTrue(BigRational.Create(-100).ConvertibleToLong());
            Assert.IsTrue(BigRational.Create(long.MaxValue).ConvertibleToLong());
            Assert.IsTrue(BigRational.Create(long.MinValue).ConvertibleToLong());
            Assert.IsFalse(BigRational.Create(((decimal)long.MaxValue) + 1).ConvertibleToLong());
            Assert.IsFalse(BigRational.Create(((decimal)long.MinValue) - 1).ConvertibleToLong());
            Assert.IsFalse(BigRational.Create(1.5m).ConvertibleToLong());
            Assert.IsFalse(BigRational.Create(-1.5m).ConvertibleToLong());
        }

        [TestMethod]
        public void BigRational_Divide_DividesBigRationals()
        {
            Assert.AreEqual("0/1", (BigRational.Create(0).Divide(BigRational.Create(2))).ToString("B", null));
            Assert.AreEqual("0/1", (BigRational.Create(0).Divide(BigRational.Create(-2))).ToString("B", null));
            Assert.AreEqual("2/3", (BigRational.Create(2).Divide(BigRational.Create(3))).ToString("B", null));
            Assert.AreEqual("3/2", (BigRational.Create(3).Divide(BigRational.Create(2))).ToString("B", null));
            Assert.AreEqual("-3/2", (BigRational.Create(3).Divide(BigRational.Create(-2))).ToString("B", null));
            Assert.AreEqual("-1/1", (BigRational.Create(-3).Divide(BigRational.Create(3))).ToString("B", null));
            Assert.AreEqual("1/1", (BigRational.Create(-3).Divide(BigRational.Create(-3))).ToString("B", null));

            Assert.AreEqual($"1/1", (BigRational.Create(VeryBigNumber).Divide(BigRational.Create(VeryBigNumber))).ToString("B", null));
            Assert.AreEqual($"{VeryBigNumber}/1", (BigRational.Create($"{DoubleVeryBig}").Divide(BigRational.Create(2))).ToString("B", null));
            Assert.AreEqual($"-2/1", (BigRational.Create($"{DoubleVeryBig}").Divide(BigRational.Create($"-{VeryBigNumber}"))).ToString("B", null));
        }

        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void BigRational_Divide_ProducesDivisionAtZero()
        {
            BigRational.Create(10).Divide(BigRational.Create(0));
        }

        [TestMethod]
        public void BigRational_Equals_ComparesBigRationals()
        {
            Assert.IsFalse(BigRational.Create(10).Equals(null));
            Assert.IsFalse(BigRational.Create(10).Equals(10.0m));  // Because of attempt to compare BigRational adn Decimal

            Assert.IsTrue(BigRational.Create(10).Equals(BigRational.Create(10)));
            Assert.IsTrue(BigRational.Create(-10).Equals(BigRational.Create(-10)));

            Assert.IsTrue(BigRational.Create(0).Equals(BigRational.Create(0)));
            Assert.IsFalse(BigRational.Create(0).Equals(BigRational.Create(1)));
            Assert.IsFalse(BigRational.Create(1).Equals(BigRational.Create(0)));
            Assert.IsFalse(BigRational.Create(0).Equals(BigRational.Create(-1)));
            Assert.IsFalse(BigRational.Create(-1).Equals(BigRational.Create(0)));

            Assert.IsTrue(BigRational.Create($"{DoubleVeryBig}").Equals(BigRational.Create($"{DoubleVeryBig}")));
            Assert.IsFalse(BigRational.Create($"-{DoubleVeryBig}").Equals(BigRational.Create($"{DoubleVeryBig}")));
        }

        [TestMethod]
        public void BigRational_GetHashCode_ReturnsRelevantHashCode()
        {
            Assert.AreEqual(BigRational.Create(10).GetHashCode(), BigRational.Create(10).GetHashCode());
            Assert.AreNotEqual(BigRational.Create(10).GetHashCode(), BigRational.Create(-10).GetHashCode());
            Assert.AreEqual(BigRational.Create(-10).GetHashCode(), BigRational.Create(-10).GetHashCode());
            Assert.AreEqual(BigRational.Create($"{DoubleVeryBig}").GetHashCode(), BigRational.Create($"{DoubleVeryBig}").GetHashCode());
            Assert.AreNotEqual(BigRational.Create($"{DoubleVeryBig}").GetHashCode(), BigRational.Create($"{VeryBigNumber}").GetHashCode());
        }

        [TestMethod]
        public void BigRational_Floor_ReturnsOriginalValueIfFractionIsZero()
        {
            Assert.AreEqual("0/1", (BigRational.Create(0).Floor()).ToString("B", null));
            Assert.AreEqual("10/1", (BigRational.Create(10).Floor()).ToString("B", null));
            Assert.AreEqual("-10/1", (BigRational.Create(-10).Floor()).ToString("B", null));
        }

        [TestMethod]
        public void BigRational_Floor_ReturnsLargestIntegralValueThatIsLessThanSpecified()
        {
            Assert.AreEqual("0/1", (BigRational.Create(0.3m).Floor()).ToString("B", null));
            Assert.AreEqual("0/1", (BigRational.Create(0.5m).Floor()).ToString("B", null));
            Assert.AreEqual("0/1", (BigRational.Create(0.7m).Floor()).ToString("B", null));
            Assert.AreEqual("10/1", (BigRational.Create(10.5m).Floor()).ToString("B", null));

            Assert.AreEqual("-1/1", (BigRational.Create(-0.3m).Floor()).ToString("B", null));
            Assert.AreEqual("-1/1", (BigRational.Create(-0.5m).Floor()).ToString("B", null));
            Assert.AreEqual("-1/1", (BigRational.Create(-0.7m).Floor()).ToString("B", null));
            Assert.AreEqual("-11/1", (BigRational.Create(-10.5m).Floor()).ToString("B", null));
        }

        [TestMethod]
        public void BigRational_FromDouble_CreatesBigRationalFromDouble()
        {
            BigRational result;

            BigRational.Zero.FromDouble(out result, 0);
            Assert.AreEqual("0/1", result.ToString("B", null));

            BigRational.Zero.FromDouble(out result, 1);
            Assert.AreEqual("1/1", result.ToString("B", null));

            BigRational.Zero.FromDouble(out result, 0.5);
            Assert.AreEqual("1/2", result.ToString("B", null));

            BigRational.Zero.FromDouble(out result, -0.5);
            Assert.AreEqual("-1/2", result.ToString("B", null));
        }

        [TestMethod]
        public void BigRational_FromDecimal_CreatesBigRationalFromDecimal()
        {
            // Note: BigRational.Create(decimal) is an wrapper of FromDecimal
            Assert.AreEqual("0/1", BigRational.Create(0).ToString("B", null));

            Assert.AreEqual("1/1", BigRational.Create(1).ToString("B", null));
            Assert.AreEqual("-1/1", BigRational.Create(-1).ToString("B", null));
            Assert.AreEqual("12345/1", BigRational.Create(12345).ToString("B", null));
            Assert.AreEqual("-12345/1", BigRational.Create(-12345).ToString("B", null));
            Assert.AreEqual("79228162514264337593543950335/1", BigRational.Create(Decimal.MaxValue).ToString("B", null));
            Assert.AreEqual("-79228162514264337593543950335/1", BigRational.Create(Decimal.MinValue).ToString("B", null));

            Assert.AreEqual("1/2", BigRational.Create(0.5m).ToString("B", null));
            Assert.AreEqual("-1/2", BigRational.Create(-0.5m).ToString("B", null));
            Assert.AreEqual("3/2", BigRational.Create(1.5m).ToString("B", null));
            Assert.AreEqual("-3/2", BigRational.Create(-1.5m).ToString("B", null));
        }

        [TestMethod]
        public void BigRational_FromLong_CreatesBigRationalFromLong()
        {
            BigRational result;

            BigRational.Zero.FromLong(out result, 0);
            Assert.AreEqual("0/1", result.ToString("B", null));

            BigRational.Zero.FromLong(out result, 1);
            Assert.AreEqual("1/1", result.ToString("B", null));

            BigRational.Zero.FromLong(out result, long.MaxValue);
            Assert.AreEqual("9223372036854775807/1", result.ToString("B", null));

            BigRational.Zero.FromLong(out result, long.MinValue);
            Assert.AreEqual("-9223372036854775808/1", result.ToString("B", null));
        }

        [TestMethod]
        public void BigRational_IsZero_IndicatesWhetherGivenValueIsZero()
        {
            Assert.IsTrue(BigRational.Zero.IsZero());
            Assert.IsTrue(BigRational.Create(0).IsZero());
            Assert.IsFalse(BigRational.Create(1).IsZero());
            Assert.IsFalse(BigRational.Create(-1).IsZero());
            Assert.IsFalse(BigRational.Create(0.5m).IsZero());
            Assert.IsFalse(BigRational.Create(-0.5m).IsZero());
        }

        [TestMethod]
        public void BigRational_Multiply_MultipliesBigRationals()
        {
            Assert.AreEqual("0/1", (BigRational.Create(0).Multiply(BigRational.Create(0))).ToString("B", null));
            Assert.AreEqual("0/1", (BigRational.Create(0).Multiply(BigRational.Create(2))).ToString("B", null));
            Assert.AreEqual("0/1", (BigRational.Create(2).Multiply(BigRational.Create(0))).ToString("B", null));
            Assert.AreEqual("0/1", (BigRational.Create(0).Multiply(BigRational.Create(-2))).ToString("B", null));
            Assert.AreEqual("0/1", (BigRational.Create(-2).Multiply(BigRational.Create(0))).ToString("B", null));
            Assert.AreEqual("6/1", (BigRational.Create(2).Multiply(BigRational.Create(3))).ToString("B", null));
            Assert.AreEqual("6/1", (BigRational.Create(3).Multiply(BigRational.Create(2))).ToString("B", null));
            Assert.AreEqual("-6/1", (BigRational.Create(3).Multiply(BigRational.Create(-2))).ToString("B", null));
            Assert.AreEqual("-6/1", (BigRational.Create(-3).Multiply(BigRational.Create(2))).ToString("B", null));
            Assert.AreEqual("9/1", (BigRational.Create(-3).Multiply(BigRational.Create(-3))).ToString("B", null));

            Assert.AreEqual($"{DoubleVeryBig}/1", (BigRational.Create(VeryBigNumber).Multiply(BigRational.Create(2))).ToString("B", null));
            Assert.AreEqual($"{MultiplNumber}/1", (BigRational.Create($"-{VeryBigNumber}").Multiply(BigRational.Create($"-{VeryBigNumber}"))).ToString("B", null));
            Assert.AreEqual($"-{MultiplNumber}/1", (BigRational.Create(VeryBigNumber).Multiply(BigRational.Create($"-{VeryBigNumber}"))).ToString("B", null));

            Assert.AreEqual("-10/1", (BigRational.Create(2.5m).Multiply(BigRational.Create(-4))).ToString("B", null));
        }

        [TestMethod]
        public void BigRational_Negate_ReturnsInvertedValue()
        {
            Assert.AreEqual("0/1", BigRational.Create(0).Negate().ToString("B", null));
            Assert.AreEqual("-2/1", BigRational.Create(2).Negate().ToString("B", null));
            Assert.AreEqual("2/1", BigRational.Create(-2).Negate().ToString("B", null));
            Assert.AreEqual("-1/2", BigRational.Create(0.5m).Negate().ToString("B", null));
            Assert.AreEqual("1/2", BigRational.Create(-0.5m).Negate().ToString("B", null));
        }

        [TestMethod]
        public void BigRational_Parse_ReturnsBigRationalFromString()
        {
            Assert.AreEqual("0/1", BigRational.Create("0").ToString("B", null));
            Assert.AreEqual("0/1", BigRational.Create("0.").ToString("B", null));
            Assert.AreEqual("0/1", BigRational.Create(".0").ToString("B", null));
            Assert.AreEqual("0/1", BigRational.Create(".").ToString("B", null));

            Assert.AreEqual("10/1", BigRational.Create("10").ToString("B", null));
            Assert.AreEqual("10/1", BigRational.Create("10.").ToString("B", null));
            Assert.AreEqual("-10/1", BigRational.Create("-10").ToString("B", null));
            Assert.AreEqual("-10/1", BigRational.Create("-10.").ToString("B", null));

            Assert.AreEqual("1/10", BigRational.Create("0.1").ToString("B", null));
            Assert.AreEqual("1/10", BigRational.Create(".1").ToString("B", null));
            Assert.AreEqual("1/10", BigRational.Create(".10").ToString("B", null));
            Assert.AreEqual("-1/10", BigRational.Create("-0.1").ToString("B", null));
            Assert.AreEqual("-1/10", BigRational.Create("-.1").ToString("B", null));
            Assert.AreEqual("-1/10", BigRational.Create("-.10").ToString("B", null));

            Assert.AreEqual("3/2", BigRational.Create("1.5").ToString("B", null));
            Assert.AreEqual("-3/2", BigRational.Create("-1.5").ToString("B", null));
        }

        [TestMethod]
        public void BigRational_Scale_ReturnsScaledBigRational()
        {
            BigRational result;

            BigRational.Create(0).Scale(out result, 0);
            Assert.AreEqual("0/1", result.ToString("B", null));

            BigRational.Create(0).Scale(out result, 1);
            Assert.AreEqual("0/1", result.ToString("B", null));

            BigRational.Create(10).Scale(out result, 0);
            Assert.AreEqual("10/1", result.ToString("B", null));

            BigRational.Create(10).Scale(out result, 1);
            Assert.AreEqual("1/1", result.ToString("B", null));

            BigRational.Create(10).Scale(out result, 2);
            Assert.AreEqual("1/10", result.ToString("B", null));

            BigRational.Create(10).Scale(out result, 3);
            Assert.AreEqual("1/100", result.ToString("B", null));
        }

        [TestMethod]
        public void BigRational_Sign_ReturnsSignOfBigRational()
        {
            Assert.AreEqual(0, BigRational.Create(0).Sign());
            Assert.AreEqual(1, BigRational.Create(10).Sign());
            Assert.AreEqual(-1, BigRational.Create(-10).Sign());
            Assert.AreEqual(1, BigRational.Create(0.2m).Sign());
            Assert.AreEqual(-1, BigRational.Create(-0.2m).Sign());
        }

        [TestMethod]
        public void BigRational_Subtract_ReturnsSubtractionOfValues()
        {
            Assert.AreEqual("0/1", (BigRational.Create(0).Subtract(BigRational.Create(0))).ToString("B", null));
            Assert.AreEqual("-2/1", (BigRational.Create(0).Subtract(BigRational.Create(2))).ToString("B", null));
            Assert.AreEqual("2/1", (BigRational.Create(2).Subtract(BigRational.Create(0))).ToString("B", null));
            Assert.AreEqual("2/1", (BigRational.Create(0).Subtract(BigRational.Create(-2))).ToString("B", null));
            Assert.AreEqual("-2/1", (BigRational.Create(-2).Subtract(BigRational.Create(0))).ToString("B", null));
            Assert.AreEqual("-1/1", (BigRational.Create(2).Subtract(BigRational.Create(3))).ToString("B", null));
            Assert.AreEqual("1/1", (BigRational.Create(3).Subtract(BigRational.Create(2))).ToString("B", null));
            Assert.AreEqual("5/1", (BigRational.Create(3).Subtract(BigRational.Create(-2))).ToString("B", null));
            Assert.AreEqual("-5/1", (BigRational.Create(-3).Subtract(BigRational.Create(2))).ToString("B", null));
            Assert.AreEqual("0/1", (BigRational.Create(-3).Subtract(BigRational.Create(-3))).ToString("B", null));

            Assert.AreEqual($"0/1", (BigRational.Create(VeryBigNumber).Subtract(BigRational.Create(VeryBigNumber))).ToString("B", null));
            Assert.AreEqual($"-{VeryBigNumber}/1", (BigRational.Create($"-{DoubleVeryBig}").Subtract(BigRational.Create($"-{VeryBigNumber}"))).ToString("B", null));
            Assert.AreEqual($"-{VeryBigNumber}/1", (BigRational.Create(VeryBigNumber).Subtract(BigRational.Create($"{DoubleVeryBig}"))).ToString("B", null));
        }

        [TestMethod]
        public void BigRational_ToDecimal_ReturnsDecimalFromBigRational()
        {
            Assert.AreEqual(0.0m, BigRational.Create("0").ToDecimal());
            Assert.AreEqual(1.0m, BigRational.Create("1").ToDecimal());
            Assert.AreEqual(-1.0m, BigRational.Create("-1").ToDecimal());
            Assert.AreEqual(123.45m, BigRational.Create("123.45").ToDecimal());
            Assert.AreEqual(-123.45m, BigRational.Create("-123.45").ToDecimal());
            Assert.AreEqual(Decimal.MaxValue, BigRational.Create($"{Decimal.MaxValue}").ToDecimal());
            Assert.AreEqual(Decimal.MinValue, BigRational.Create($"{Decimal.MinValue}").ToDecimal());
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void BigRational_ToDecimal_ProducesOverflowExceptionIfValueIsTooBig()
        {
            BigRational bigValue = BigRational.Create(VeryBigNumber);
            bigValue.ToDecimal();
        }

        [TestMethod]
        public void BigRational_ToLong_ReturnsLongFromBigRational()
        {
            Assert.AreEqual(0, BigRational.Create("0").ToLong());
            Assert.AreEqual(1, BigRational.Create("1").ToLong());
            Assert.AreEqual(-1, BigRational.Create("-1").ToLong());
            Assert.AreEqual(123, BigRational.Create("123.45").ToLong());  // Notice that the value is rounded
            Assert.AreEqual(-123, BigRational.Create("-123.45").ToLong());
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void BigRational_ToLong_ProducesOverflowExceptionIfValueIsTooBig()
        {
            BigRational.Create($"{Decimal.MaxValue}").ToLong();
        }

    }
}

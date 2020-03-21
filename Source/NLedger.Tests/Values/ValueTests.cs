// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using NLedger.Amounts;
using NLedger.Values;
using NLedger.Times;
using NLedger.Utility;
using System.Text.RegularExpressions;
using NLedger.Scopus;
using NLedger.Expressions;
using NLedger.Commodities;
using NLedger.Utils;

namespace NLedger.Tests.Values
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon )]
    public class ValueTests : TestFixture
    {
        [TestMethod]
        public void Value_Get_Returns_Empty_For_Null()
        {
            Assert.IsTrue(Value.IsNullOrEmpty(Value.Get<object>(null)));
        }

        [TestMethod]
        public void Value_Get_Returns_Empty_For_EmptyStrings()
        {
            Assert.IsTrue(Value.IsNullOrEmpty(Value.Get<string>(null)));
            Assert.IsTrue(Value.IsNullOrEmpty(Value.Get(String.Empty)));
        }

        [TestMethod]
        public void Value_Get_Returns_Empty_For_DefaultValues()
        {
            Assert.IsTrue(Value.IsNullOrEmpty(Value.Get("")));
            Assert.IsTrue(Value.IsNullOrEmpty(Value.Get(default(Mask))));
        }

        [TestMethod]
        public void Value_Get_Returns_Normal_Values()
        {
            Assert.AreEqual("   ", Value.Get("   ").ToString());
            Assert.AreEqual("abc", Value.Get("abc").ToString());
            Assert.AreEqual("true", Value.Get(true).ToString());
            Assert.AreEqual("1", Value.Get(1).ToString());
        }

        [TestMethod]
        public void Value_Get_Returns_String_WhenLiteralIsTrue()
        {
            Value val = Value.Get("my-string", true);
            Assert.AreEqual(ValueTypeEnum.String, val.Type);
            Assert.AreEqual("my-string", val.ToString());
        }

        [TestMethod]
        public void Value_Get_Returns_Amount_WhenLiteralIsFalse()
        {
            Value val1 = Value.Get("123", false);
            Value val2 = Value.Get("1234");
            Assert.AreEqual(ValueTypeEnum.Amount, val1.Type);
            Assert.AreEqual(ValueTypeEnum.String, val2.Type);
            Assert.AreEqual("123", val1.ToString());
            Assert.AreEqual("1234", val2.ToString());
        }

        [TestMethod]
        public void Value_AddOrSetValue_ClonesRightValueIfLeftIsEmpty()
        {
            Value val1 = new Value();
            Value val2 = Value.Get(2);

            Value val3 = Value.AddOrSetValue(val1, val2);

            Assert.AreEqual(2, val3.AsLong);
            Assert.IsFalse(Object.ReferenceEquals(val3, val2));
        }

        [TestMethod]
        public void Value_PushBack_AddsItemsToTheEnd()
        {
            Value val1 = new Value();

            val1.PushBack(Value.Get(1));
            val1.PushBack(Value.Get(2));
            val1.PushBack(Value.Get(3));

            Assert.AreEqual(1, val1.AsSequence[0].AsLong);
            Assert.AreEqual(2, val1.AsSequence[1].AsLong);
            Assert.AreEqual(3, val1.AsSequence[2].AsLong);
        }

        [TestMethod]
        public void Value_Print_AlignsRightBySpecialStyle()
        {
            Value val1 = Value.Get(1234);

            Assert.AreEqual("1234", val1.Print());
            Assert.AreEqual("1234      ", val1.Print(10));
            Assert.AreEqual("      1234", val1.Print(10, -1, AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY));  // Expected right justification
        }

        [TestMethod]
        public void Value_StringValue_ReturnsAmountByAsAmount()
        {
            Value val1 = Value.Get("1234.567");
            Amount result = val1.AsAmount;
            Assert.AreEqual("1234.567", result.Quantity.Print(3, 3));
        }

        [TestMethod]
        public void Value_AnyValue_ConvertsToSequence()
        {
            object origVal = new object();

            Value val = Value.Get(origVal);
            Assert.AreEqual(ValueTypeEnum.Any, val.Type);

            IList<Value> valSeq = val.AsSequence;
            Assert.AreEqual(1, valSeq.Count);

            Value valFinal = valSeq[0];
            Assert.AreEqual(ValueTypeEnum.Any, valFinal.Type);
            Assert.AreEqual(origVal, valFinal.AsAny());
        }

        [TestMethod]
        public void Value_Clone_CreatesANewEmptyValueForEmptyValue()
        {
            Value origVal = Value.Empty;

            Value val = Value.Clone(origVal);

            Assert.IsTrue(val.Type == ValueTypeEnum.Void);
            Assert.AreNotSame(val, origVal);
        }

        [TestMethod]
        public void Value_IsNullOrEmptyOrFalse_ReturnsTrueForNullEmptyFalseValues()
        {
            Assert.IsTrue(Value.IsNullOrEmptyOrFalse(null));
            Assert.IsTrue(Value.IsNullOrEmptyOrFalse(Value.Empty));
            Assert.IsTrue(Value.IsNullOrEmptyOrFalse(new Value()));
            Assert.IsTrue(Value.IsNullOrEmptyOrFalse(Value.Get(false)));
            Assert.IsFalse(Value.IsNullOrEmptyOrFalse(Value.Get(1)));
        }

        [TestMethod]
        public void Value_Negated_ClonesOriginalValue()
        {
            Amount amount = new Amount(100);
            Value value = Value.Get(amount);

            Value negated = value.Negated();

            Assert.AreEqual(100, value.AsAmount.Quantity.ToLong());
            Assert.AreEqual(-100, negated.AsAmount.Quantity.ToLong());
        }

        [TestMethod]
        public void Value_DateTimeValue_AsString_ReturnsProperlyFormattedString()
        {
            DateTime dateTime = new DateTime(2012, 12, 22, 2, 3, 4);
            Value value = Value.Get(dateTime);
            Assert.AreEqual("2012/12/22 02:03:04", value.AsString);
        }

        [TestMethod]
        public void Value_DateValue_AsString_ReturnsProperlyFormattedString()
        {
            Date date = new Date(2012, 12, 22);
            Value value = Value.Get(date);
            Assert.AreEqual("2012/12/22", value.AsString);
        }

        [TestMethod]
        public void Value_Get_ForStringAndLiteralTrue_ReturnsStringValueForAnyStringsEvenEmpty()
        {
            Value value1 = Value.Get(null, true);
            Assert.AreEqual(ValueTypeEnum.String, value1.Type);
            Assert.AreEqual(String.Empty, value1.AsString);

            Value value2 = Value.Get("", true);
            Assert.AreEqual(ValueTypeEnum.String, value2.Type);
            Assert.AreEqual(String.Empty, value2.AsString);

            Value value3 = Value.Get("text", true);
            Assert.AreEqual(ValueTypeEnum.String, value3.Type);
            Assert.AreEqual("text", value3.AsString);
        }

        [TestMethod]
        public void Value_AsLong_ForStringValues_ReturnsZeriForEmptyString()
        {
            Assert.AreEqual(0, Value.StringValue("").AsLong);
            Assert.AreEqual(0, Value.StringValue("0").AsLong);
            Assert.AreEqual(100, Value.StringValue("100").AsLong);
        }

        [TestMethod]
        public void Value_AsDate_ConvertsPossibleValuesToDate()
        {
            Assert.AreEqual(new Date(2015, 10, 22), Value.Get(new Date(2015, 10, 22)).AsDate);
            Assert.AreEqual(new Date(2015, 10, 22), Value.Get(new DateTime(2015, 10, 22)).AsDate);
            Assert.AreEqual(new Date(2015, 10, 22), Value.StringValue("2015/10/22").AsDate);
        }

        [TestMethod]
        public void Value_ToString_ConvertsToStringAndUsesAsString()
        {
            Assert.AreEqual("something", Value.StringValue("something").ToString());
            Assert.AreEqual("2015/10/22",  Value.Get(new Date(2015, 10, 22)).ToString());
            Assert.AreEqual("2015/10/22 00:00:00", Value.Get(new DateTime(2015, 10, 22)).ToString());
            Assert.AreEqual("true", Value.Get(true).ToString());
            Assert.AreEqual("false", Value.Get(false).ToString());
            Assert.AreEqual("1", Value.Get((Amount)1).ToString());
        }

        [TestMethod]
        public void Value_InPlaceCast_ChangesEmptyValueToIntegerItself()
        {
            Value val1 = new Value();
            val1.InPlaceCast(ValueTypeEnum.Integer);
            Assert.AreEqual(ValueTypeEnum.Integer, val1.Type);
        }

        [TestMethod]
        public void Value_InPlaceCast_ChangesEmptyValueToAmountItself()
        {
            Value val1 = new Value();
            val1.InPlaceCast(ValueTypeEnum.Amount);
            Assert.AreEqual(ValueTypeEnum.Amount, val1.Type);
        }

        [TestMethod]
        public void Value_InPlaceCast_ChangesEmptyValueToStringItself()
        {
            Value val1 = new Value();
            val1.InPlaceCast(ValueTypeEnum.String);
            Assert.AreEqual(ValueTypeEnum.String, val1.Type);
        }

        [TestMethod]
        public void Value_InPlaceCast_ChangesStringValueToIntegerItself()
        {
            Value val1 = Value.StringValue("1");
            val1.InPlaceCast(ValueTypeEnum.Integer);
            Assert.AreEqual(ValueTypeEnum.Integer, val1.Type);
        }

        [TestMethod]
        public void Value_InPlaceCast_ChangesStringValueToBooleanItself()
        {
            Value val1 = Value.StringValue("1");
            val1.InPlaceCast(ValueTypeEnum.Boolean);
            Assert.AreEqual(ValueTypeEnum.Boolean, val1.Type);
        }

        [TestMethod]
        public void Value_InPlaceCast_ChangesStringValueToDateItself()
        {
            Value val1 = Value.StringValue("2017/10/22");
            val1.InPlaceCast(ValueTypeEnum.Date);
            Assert.AreEqual(ValueTypeEnum.Date, val1.Type);
        }

        [TestMethod]
        public void Value_InPlaceCast_ChangesStringValueToDateTimeItself()
        {
            Value val1 = Value.StringValue("2017/10/22 10:10:10");
            val1.InPlaceCast(ValueTypeEnum.DateTime);
            Assert.AreEqual(ValueTypeEnum.DateTime, val1.Type);
        }

        [TestMethod]
        public void Value_InPlaceCast_ChangesStringValueToAmountItself()
        {
            Value val1 = Value.StringValue("10 EUR");
            val1.InPlaceCast(ValueTypeEnum.Amount);
            Assert.AreEqual(ValueTypeEnum.Amount, val1.Type);
        }

        [TestMethod]
        public void Value_InPlaceCast_ChangesStringValueToStringItself()
        {
            Value val1 = Value.StringValue("text");
            val1.InPlaceCast(ValueTypeEnum.String);
            Assert.AreEqual(ValueTypeEnum.String, val1.Type);
        }

        [TestMethod]
        public void Value_InPlaceCast_ChangesStringValueToMaskItself()
        {
            Value val1 = Value.StringValue("text");
            val1.InPlaceCast(ValueTypeEnum.Mask);
            Assert.AreEqual(ValueTypeEnum.Mask, val1.Type);
        }

        [TestMethod]
        public void Value_Dump_Amount_NoSquaresIfRelaxed()
        {
            Amount amount = new Amount(20);
            Value val1 = Value.Get(amount);
            string result = val1.Dump(true);
            Assert.AreEqual("20", result);
        }

        [TestMethod]
        public void Value_Dump_Amount_NoSquaresIfRelaxedByDefault()
        {
            Amount amount = new Amount(20);
            Value val1 = Value.Get(amount);
            string result = val1.Dump();
            Assert.AreEqual("20", result);
        }

        [TestMethod]
        public void Value_Dump_Amount_AddsSquaresIfNotRelaxed()
        {
            Amount amount = new Amount(20);
            Value val1 = Value.Get(amount);
            string result = val1.Dump(false);
            Assert.AreEqual("{20}", result);
        }

        [TestMethod]
        public void Value_Dump_String_AddsQuotes()
        {
            Value val1 = Value.StringValue("val");
            string result = val1.Dump(false);
            Assert.AreEqual("\"val\"", result);
        }

        [TestMethod]
        public void Value_Label_PrintsValueTypeInfo()
        {
            Assert.AreEqual("an uninitialized value", new Value().Label());
            Assert.AreEqual("a boolean", Value.Get(true).Label());
            Assert.AreEqual("a date/time", Value.Get(DateTime.Now).Label());
            Assert.AreEqual("a date", Value.Get(new Date()).Label());
            Assert.AreEqual("an integer", Value.Get(10).Label());
            Assert.AreEqual("an amount", Value.Get(new Amount()).Label());
            Assert.AreEqual("a balance", Value.Get(new Balance()).Label());
            Assert.AreEqual("a string", Value.StringValue("text").Label());
            Assert.AreEqual("a regexp", Value.Get(new Mask()).Label());
            Assert.AreEqual("a sequence", Value.Get(new List<Value>()).Label());
            Assert.AreEqual("a scope", Value.Get(new ValueScope(null, Value.Zero)).Label());
            Assert.AreEqual("an expr", Value.Get(new ExprOp()).Label());
            Assert.AreEqual("an object", Value.Get(new object()).Label());
        }

        [TestMethod]
        public void Value_IsRealZero_ReturnsTrueForDefaultDate()
        {
            Value val = Value.Get(default(Date));
            Assert.IsTrue(val.IsRealZero);
        }

        [TestMethod]
        public void Value_IsEqualTo_ComparesTwoDates()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new Date(2017, 10, 10));
            Assert.IsTrue(val1.IsEqualTo(val2));
        }

        [TestMethod]
        [ExpectedException(typeof(ValueError))]
        public void Value_IsEqualTo_ComparesOnlyDates()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new DateTime(2017, 10, 10));
            Assert.IsTrue(val1.IsEqualTo(val2));
        }

        [TestMethod]
        public void Value_IsLessThan_ComparesTwoDates()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new Date(2018, 10, 10));
            Assert.IsTrue(val1.IsLessThan(val2));
        }

        [TestMethod]
        [ExpectedException(typeof(ValueError))]
        public void Value_IsLessThan_ComparesOnlyDates()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new DateTime(2018, 10, 10));
            Assert.IsTrue(val1.IsLessThan(val2));
        }

        [TestMethod]
        public void Value_IsGreaterThan_ComparesTwoDates()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new Date(2015, 10, 10));
            Assert.IsTrue(val1.IsGreaterThan(val2));
        }

        [TestMethod]
        [ExpectedException(typeof(ValueError))]
        public void Value_IsGreaterThan_ComparesOnlyDates()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new DateTime(2015, 10, 10));
            Assert.IsTrue(val1.IsGreaterThan(val2));
        }

        [TestMethod]
        public void Value_InPlaceAdd_HandlesDateAndInteger()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(10);
            Assert.AreEqual(new Date(2017, 10, 20), val1.InPlaceAdd(val2).AsDate);
        }

        [TestMethod]
        public void Value_InPlaceAdd_HandlesDateAndAmount()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new Amount(10));
            Assert.AreEqual(new Date(2017, 10, 20), val1.InPlaceAdd(val2).AsDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ValueError))]
        public void Value_InPlaceAdd_HandlesDateWithOnlyIntegerAndAmount()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new DateTime(2017, 10, 1));
            val1.InPlaceAdd(val2);
        }

        [TestMethod]
        public void Value_SubtractValueStorage_HandlesDateAndInteger()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(8);
            Assert.AreEqual(new Date(2017, 10, 2), val1.InPlaceSubtract(val2).AsDate);
        }

        [TestMethod]
        public void Value_SubtractValueStorage_HandlesDateAndAmount()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new Amount(8));
            Assert.AreEqual(new Date(2017, 10, 2), val1.InPlaceSubtract(val2).AsDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ValueError))]
        public void Value_SubtractValueStorage_HandlesDateWithOnluIntegerAndAmount()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new DateTime(2017, 10, 1));
            val1.InPlaceSubtract(val2);
        }

        [TestMethod]
        public void Value_Amount_Bool_CallsAmountIsNonZero()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("ValAmtBool1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("ValAmtBool2")) { Precision = 5 };

            Amount amt1 = new Amount(Quantity.Parse("0.05"), commodity1);
            Amount amt2 = new Amount(Quantity.Parse("0.05"), commodity2);

            Value val1 = Value.Get(amt1);
            Value val2 = Value.Get(amt2);

            Assert.IsFalse(val1.Bool);      // Because Commodity's precision is 0
            Assert.IsTrue(val2.Bool);       // Because Commodity's precision is 5
        }

        [TestMethod]
        public void Value_Print_Date_ProducesOutputInWrittenFormat()
        {
            Value val = Value.Get(new Date(2010, 10, 22));
            string output = val.Print();
            Assert.AreEqual("2010/10/22", output);
        }

        [TestMethod]
        public void Value_Print_DateTime_ProducesOutputInWrittenFormat()
        {
            Value val = Value.Get(new DateTime(2010, 10, 22));
            string output = val.Print();
            Assert.AreEqual("2010/10/22 00:00:00", output);
        }

        [TestMethod]
        public void Value_Bool_ChecksBalanceIsZero()
        {
            Commodity commodityA = new Commodity(CommodityPool.Current, new CommodityBase("valNZeroA"));
            Commodity commodityB = new Commodity(CommodityPool.Current, new CommodityBase("valNZeroB"));

            // Commodity precision is "0"; add values that are less than commodity precision
            Amount amountA = new Amount(Quantity.Parse("0.1"), commodityA);
            Amount amountB = new Amount(Quantity.Parse("0.1"), commodityB);

            Balance balance = new Balance();
            balance.Add(amountA);
            balance.Add(amountB);

            Value val = Value.Get(balance);

            Assert.IsFalse((bool)balance);
            Assert.IsFalse(val.Bool);
        }

        [TestMethod]
        [ExpectedException(typeof(AssertionFailedError))]
        public void Value_Get_AmountMustBeValid()
        {
            Validator.IsVerifyEnabled = true;

            Amount amount = new Amount(10);
            var quantity = amount.Quantity.SetPrecision(2048);
            amount = new Amount(quantity, null);

            Assert.IsFalse(amount.Valid());
            Value.Get(amount);
        }

        [TestMethod]
        [ExpectedException(typeof(AssertionFailedError))]
        public void Value_Get_BalanceMustBeValid()
        {
            Validator.IsVerifyEnabled = true;

            Balance balance = new Balance();

            Amount amount = new Amount(10);
            var quantity = amount.Quantity.SetPrecision(2048);
            amount = new Amount(quantity, null);
            balance.Add(amount);

            Assert.IsFalse(amount.Valid());
            Assert.IsFalse(balance.Valid());
            Value.Get(balance);
        }

    }
}

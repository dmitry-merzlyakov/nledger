// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
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
using Xunit;

namespace NLedger.Tests.Values
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon )]
    public class ValueTests : TestFixture
    {
        [Fact]
        public void Value_Get_Returns_Empty_For_Null()
        {
            Assert.True(Value.IsNullOrEmpty(Value.Get<object>(null)));
        }

        [Fact]
        public void Value_Get_Returns_Empty_For_EmptyStrings()
        {
            Assert.True(Value.IsNullOrEmpty(Value.Get<string>(null)));
            Assert.True(Value.IsNullOrEmpty(Value.Get(String.Empty)));
        }

        [Fact]
        public void Value_Get_Returns_Empty_For_DefaultValues()
        {
            Assert.True(Value.IsNullOrEmpty(Value.Get("")));
            Assert.True(Value.IsNullOrEmpty(Value.Get(default(Mask))));
        }

        [Fact]
        public void Value_Get_Returns_Normal_Values()
        {
            Assert.Equal("   ", Value.Get("   ").ToString());
            Assert.Equal("abc", Value.Get("abc").ToString());
            Assert.Equal("true", Value.Get(true).ToString());
            Assert.Equal("1", Value.Get(1).ToString());
        }

        [Fact]
        public void Value_Get_Returns_String_WhenLiteralIsTrue()
        {
            Value val = Value.Get("my-string", true);
            Assert.Equal(ValueTypeEnum.String, val.Type);
            Assert.Equal("my-string", val.ToString());
        }

        [Fact]
        public void Value_Get_Returns_Amount_WhenLiteralIsFalse()
        {
            Value val1 = Value.Get("123", false);
            Value val2 = Value.Get("1234");
            Assert.Equal(ValueTypeEnum.Amount, val1.Type);
            Assert.Equal(ValueTypeEnum.String, val2.Type);
            Assert.Equal("123", val1.ToString());
            Assert.Equal("1234", val2.ToString());
        }

        [Fact]
        public void Value_AddOrSetValue_ClonesRightValueIfLeftIsEmpty()
        {
            Value val1 = new Value();
            Value val2 = Value.Get(2);

            Value val3 = Value.AddOrSetValue(val1, val2);

            Assert.Equal(2, val3.AsLong);
            Assert.False(Object.ReferenceEquals(val3, val2));
        }

        [Fact]
        public void Value_PushBack_AddsItemsToTheEnd()
        {
            Value val1 = new Value();

            val1.PushBack(Value.Get(1));
            val1.PushBack(Value.Get(2));
            val1.PushBack(Value.Get(3));

            Assert.Equal(1, val1.AsSequence[0].AsLong);
            Assert.Equal(2, val1.AsSequence[1].AsLong);
            Assert.Equal(3, val1.AsSequence[2].AsLong);
        }

        [Fact]
        public void Value_Print_AlignsRightBySpecialStyle()
        {
            Value val1 = Value.Get(1234);

            Assert.Equal("1234", val1.Print());
            Assert.Equal("1234      ", val1.Print(10));
            Assert.Equal("      1234", val1.Print(10, -1, AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY));  // Expected right justification
        }

        [Fact]
        public void Value_StringValue_ReturnsAmountByAsAmount()
        {
            Value val1 = Value.Get("1234.567");
            Amount result = val1.AsAmount;
            Assert.Equal("1234.567", result.Quantity.Print(3, 3));
        }

        [Fact]
        public void Value_AnyValue_ConvertsToSequence()
        {
            object origVal = new object();

            Value val = Value.Get(origVal);
            Assert.Equal(ValueTypeEnum.Any, val.Type);

            IList<Value> valSeq = val.AsSequence;
            Assert.Equal(1, valSeq.Count);

            Value valFinal = valSeq[0];
            Assert.Equal(ValueTypeEnum.Any, valFinal.Type);
            Assert.Equal(origVal, valFinal.AsAny());
        }

        [Fact]
        public void Value_Clone_CreatesANewEmptyValueForEmptyValue()
        {
            Value origVal = Value.Empty;

            Value val = Value.Clone(origVal);

            Assert.True(val.Type == ValueTypeEnum.Void);
            Assert.NotSame(val, origVal);
        }

        [Fact]
        public void Value_IsNullOrEmptyOrFalse_ReturnsTrueForNullEmptyFalseValues()
        {
            Assert.True(Value.IsNullOrEmptyOrFalse(null));
            Assert.True(Value.IsNullOrEmptyOrFalse(Value.Empty));
            Assert.True(Value.IsNullOrEmptyOrFalse(new Value()));
            Assert.True(Value.IsNullOrEmptyOrFalse(Value.Get(false)));
            Assert.False(Value.IsNullOrEmptyOrFalse(Value.Get(1)));
        }

        [Fact]
        public void Value_Negated_ClonesOriginalValue()
        {
            Amount amount = new Amount(100);
            Value value = Value.Get(amount);

            Value negated = value.Negated();

            Assert.Equal(100, value.AsAmount.Quantity.ToLong());
            Assert.Equal(-100, negated.AsAmount.Quantity.ToLong());
        }

        [Fact]
        public void Value_DateTimeValue_AsString_ReturnsProperlyFormattedString()
        {
            DateTime dateTime = new DateTime(2012, 12, 22, 2, 3, 4);
            Value value = Value.Get(dateTime);
            Assert.Equal("2012/12/22 02:03:04", value.AsString);
        }

        [Fact]
        public void Value_DateValue_AsString_ReturnsProperlyFormattedString()
        {
            Date date = new Date(2012, 12, 22);
            Value value = Value.Get(date);
            Assert.Equal("2012/12/22", value.AsString);
        }

        [Fact]
        public void Value_Get_ForStringAndLiteralTrue_ReturnsStringValueForAnyStringsEvenEmpty()
        {
            Value value1 = Value.Get(null, true);
            Assert.Equal(ValueTypeEnum.String, value1.Type);
            Assert.Equal(String.Empty, value1.AsString);

            Value value2 = Value.Get("", true);
            Assert.Equal(ValueTypeEnum.String, value2.Type);
            Assert.Equal(String.Empty, value2.AsString);

            Value value3 = Value.Get("text", true);
            Assert.Equal(ValueTypeEnum.String, value3.Type);
            Assert.Equal("text", value3.AsString);
        }

        [Fact]
        public void Value_AsLong_ForStringValues_ReturnsZeriForEmptyString()
        {
            Assert.Equal(0, Value.StringValue("").AsLong);
            Assert.Equal(0, Value.StringValue("0").AsLong);
            Assert.Equal(100, Value.StringValue("100").AsLong);
        }

        [Fact]
        public void Value_AsDate_ConvertsPossibleValuesToDate()
        {
            Assert.Equal(new Date(2015, 10, 22), Value.Get(new Date(2015, 10, 22)).AsDate);
            Assert.Equal(new Date(2015, 10, 22), Value.Get(new DateTime(2015, 10, 22)).AsDate);
            Assert.Equal(new Date(2015, 10, 22), Value.StringValue("2015/10/22").AsDate);
        }

        [Fact]
        public void Value_ToString_ConvertsToStringAndUsesAsString()
        {
            Assert.Equal("something", Value.StringValue("something").ToString());
            Assert.Equal("2015/10/22",  Value.Get(new Date(2015, 10, 22)).ToString());
            Assert.Equal("2015/10/22 00:00:00", Value.Get(new DateTime(2015, 10, 22)).ToString());
            Assert.Equal("true", Value.Get(true).ToString());
            Assert.Equal("false", Value.Get(false).ToString());
            Assert.Equal("1", Value.Get((Amount)1).ToString());
        }

        [Fact]
        public void Value_InPlaceCast_ChangesEmptyValueToIntegerItself()
        {
            Value val1 = new Value();
            val1.InPlaceCast(ValueTypeEnum.Integer);
            Assert.Equal(ValueTypeEnum.Integer, val1.Type);
        }

        [Fact]
        public void Value_InPlaceCast_ChangesEmptyValueToAmountItself()
        {
            Value val1 = new Value();
            val1.InPlaceCast(ValueTypeEnum.Amount);
            Assert.Equal(ValueTypeEnum.Amount, val1.Type);
        }

        [Fact]
        public void Value_InPlaceCast_ChangesEmptyValueToStringItself()
        {
            Value val1 = new Value();
            val1.InPlaceCast(ValueTypeEnum.String);
            Assert.Equal(ValueTypeEnum.String, val1.Type);
        }

        [Fact]
        public void Value_InPlaceCast_ChangesStringValueToIntegerItself()
        {
            Value val1 = Value.StringValue("1");
            val1.InPlaceCast(ValueTypeEnum.Integer);
            Assert.Equal(ValueTypeEnum.Integer, val1.Type);
        }

        [Fact]
        public void Value_InPlaceCast_ChangesStringValueToBooleanItself()
        {
            Value val1 = Value.StringValue("1");
            val1.InPlaceCast(ValueTypeEnum.Boolean);
            Assert.Equal(ValueTypeEnum.Boolean, val1.Type);
        }

        [Fact]
        public void Value_InPlaceCast_ChangesStringValueToDateItself()
        {
            Value val1 = Value.StringValue("2017/10/22");
            val1.InPlaceCast(ValueTypeEnum.Date);
            Assert.Equal(ValueTypeEnum.Date, val1.Type);
        }

        [Fact]
        public void Value_InPlaceCast_ChangesStringValueToDateTimeItself()
        {
            Value val1 = Value.StringValue("2017/10/22 10:10:10");
            val1.InPlaceCast(ValueTypeEnum.DateTime);
            Assert.Equal(ValueTypeEnum.DateTime, val1.Type);
        }

        [Fact]
        public void Value_InPlaceCast_ChangesStringValueToAmountItself()
        {
            Value val1 = Value.StringValue("10 EUR");
            val1.InPlaceCast(ValueTypeEnum.Amount);
            Assert.Equal(ValueTypeEnum.Amount, val1.Type);
        }

        [Fact]
        public void Value_InPlaceCast_ChangesStringValueToStringItself()
        {
            Value val1 = Value.StringValue("text");
            val1.InPlaceCast(ValueTypeEnum.String);
            Assert.Equal(ValueTypeEnum.String, val1.Type);
        }

        [Fact]
        public void Value_InPlaceCast_ChangesStringValueToMaskItself()
        {
            Value val1 = Value.StringValue("text");
            val1.InPlaceCast(ValueTypeEnum.Mask);
            Assert.Equal(ValueTypeEnum.Mask, val1.Type);
        }

        [Fact]
        public void Value_Dump_Amount_NoSquaresIfRelaxed()
        {
            Amount amount = new Amount(20);
            Value val1 = Value.Get(amount);
            string result = val1.Dump(true);
            Assert.Equal("20", result);
        }

        [Fact]
        public void Value_Dump_Amount_NoSquaresIfRelaxedByDefault()
        {
            Amount amount = new Amount(20);
            Value val1 = Value.Get(amount);
            string result = val1.Dump();
            Assert.Equal("20", result);
        }

        [Fact]
        public void Value_Dump_Amount_AddsSquaresIfNotRelaxed()
        {
            Amount amount = new Amount(20);
            Value val1 = Value.Get(amount);
            string result = val1.Dump(false);
            Assert.Equal("{20}", result);
        }

        [Fact]
        public void Value_Dump_String_AddsQuotes()
        {
            Value val1 = Value.StringValue("val");
            string result = val1.Dump(false);
            Assert.Equal("\"val\"", result);
        }

        [Fact]
        public void Value_Label_PrintsValueTypeInfo()
        {
            Assert.Equal("an uninitialized value", new Value().Label());
            Assert.Equal("a boolean", Value.Get(true).Label());
            Assert.Equal("a date/time", Value.Get(DateTime.Now).Label());
            Assert.Equal("a date", Value.Get(new Date()).Label());
            Assert.Equal("an integer", Value.Get(10).Label());
            Assert.Equal("an amount", Value.Get(new Amount()).Label());
            Assert.Equal("a balance", Value.Get(new Balance()).Label());
            Assert.Equal("a string", Value.StringValue("text").Label());
            Assert.Equal("a regexp", Value.Get(new Mask()).Label());
            Assert.Equal("a sequence", Value.Get(new List<Value>()).Label());
            Assert.Equal("a scope", Value.Get(new ValueScope(null, Value.Zero)).Label());
            Assert.Equal("an expr", Value.Get(new ExprOp()).Label());
            Assert.Equal("an object", Value.Get(new object()).Label());
        }

        [Fact]
        public void Value_IsRealZero_ReturnsTrueForDefaultDate()
        {
            Value val = Value.Get(default(Date));
            Assert.True(val.IsRealZero);
        }

        [Fact]
        public void Value_IsEqualTo_ComparesTwoDates()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new Date(2017, 10, 10));
            Assert.True(val1.IsEqualTo(val2));
        }

        [Fact]
        public void Value_IsEqualTo_ComparesOnlyDates()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new DateTime(2017, 10, 10));
            Assert.Throws<ValueError>(() => val1.IsEqualTo(val2));
        }

        [Fact]
        public void Value_IsLessThan_ComparesTwoDates()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new Date(2018, 10, 10));
            Assert.True(val1.IsLessThan(val2));
        }

        [Fact]
        public void Value_IsLessThan_ComparesOnlyDates()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new DateTime(2018, 10, 10));
            Assert.Throws<ValueError>(() => val1.IsLessThan(val2));
        }

        [Fact]
        public void Value_IsGreaterThan_ComparesTwoDates()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new Date(2015, 10, 10));
            Assert.True(val1.IsGreaterThan(val2));
        }

        [Fact]
        public void Value_IsGreaterThan_ComparesOnlyDates()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new DateTime(2015, 10, 10));
            Assert.Throws<ValueError>(() => val1.IsGreaterThan(val2));
        }

        [Fact]
        public void Value_InPlaceAdd_HandlesDateAndInteger()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(10);
            Assert.Equal(new Date(2017, 10, 20), val1.InPlaceAdd(val2).AsDate);
        }

        [Fact]
        public void Value_InPlaceAdd_HandlesDateAndAmount()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new Amount(10));
            Assert.Equal(new Date(2017, 10, 20), val1.InPlaceAdd(val2).AsDate);
        }

        [Fact]
        public void Value_InPlaceAdd_HandlesDateWithOnlyIntegerAndAmount()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new DateTime(2017, 10, 1));
            Assert.Throws<ValueError>(() => val1.InPlaceAdd(val2));
        }

        [Fact]
        public void Value_SubtractValueStorage_HandlesDateAndInteger()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(8);
            Assert.Equal(new Date(2017, 10, 2), val1.InPlaceSubtract(val2).AsDate);
        }

        [Fact]
        public void Value_SubtractValueStorage_HandlesDateAndAmount()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new Amount(8));
            Assert.Equal(new Date(2017, 10, 2), val1.InPlaceSubtract(val2).AsDate);
        }

        [Fact]
        public void Value_SubtractValueStorage_HandlesDateWithOnluIntegerAndAmount()
        {
            Value val1 = Value.Get(new Date(2017, 10, 10));
            Value val2 = Value.Get(new DateTime(2017, 10, 1));
            Assert.Throws<ValueError>(() => val1.InPlaceSubtract(val2));
        }

        [Fact]
        public void Value_Amount_Bool_CallsAmountIsNonZero()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("ValAmtBool1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("ValAmtBool2")) { Precision = 5 };

            Amount amt1 = new Amount(Quantity.Parse("0.05"), commodity1);
            Amount amt2 = new Amount(Quantity.Parse("0.05"), commodity2);

            Value val1 = Value.Get(amt1);
            Value val2 = Value.Get(amt2);

            Assert.False(val1.Bool);      // Because Commodity's precision is 0
            Assert.True(val2.Bool);       // Because Commodity's precision is 5
        }

        [Fact]
        public void Value_Print_Date_ProducesOutputInWrittenFormat()
        {
            Value val = Value.Get(new Date(2010, 10, 22));
            string output = val.Print();
            Assert.Equal("2010/10/22", output);
        }

        [Fact]
        public void Value_Print_DateTime_ProducesOutputInWrittenFormat()
        {
            Value val = Value.Get(new DateTime(2010, 10, 22));
            string output = val.Print();
            Assert.Equal("2010/10/22 00:00:00", output);
        }

        [Fact]
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

            Assert.False((bool)balance);
            Assert.False(val.Bool);
        }

        [Fact]
        public void Value_Get_AmountMustBeValid()
        {
            Validator.IsVerifyEnabled = true;

            Amount amount = new Amount(10);
            var quantity = amount.Quantity.SetPrecision(2048);
            amount = new Amount(quantity, null);

            Assert.False(amount.Valid());
            Assert.Throws<AssertionFailedError>(() => Value.Get(amount));
        }

        [Fact]
        public void Value_Get_BalanceMustBeValid()
        {
            Validator.IsVerifyEnabled = true;

            Balance balance = new Balance();

            Amount amount = new Amount(10);
            var quantity = amount.Quantity.SetPrecision(2048);
            amount = new Amount(quantity, null);
            balance.Add(amount);

            Assert.False(amount.Valid());
            Assert.False(balance.Valid());
            Assert.Throws<AssertionFailedError>(() => Value.Get(balance));
        }

    }
}

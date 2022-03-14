// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Commodities;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Values
{
    public class ValueStorageTests : TestFixture
    {
        [Fact]
        public void ValueStorageExtensions_Create_CreatesStorageInstancesByTypes()
        {
            IValueStorage value;

            value = ValueStorageExtensions.Create(true);
            Assert.True(value is BooleanValueStorage);
            Assert.True(value.AsBoolean);

            value = ValueStorageExtensions.Create(new DateTime(2010, 10, 10));
            Assert.True(value is DateTimeValueStorage);
            Assert.Equal(new DateTime(2010, 10, 10), value.AsDateTime);

            value = ValueStorageExtensions.Create((int)100);
            Assert.True(value is IntegerValueStorage);
            Assert.Equal(100, value.AsLong);

            value = ValueStorageExtensions.Create((long)100);
            Assert.True(value is IntegerValueStorage);
            Assert.Equal(100, value.AsLong);

            Amount amount = new Amount(200);
            value = ValueStorageExtensions.Create(amount);
            Assert.True(value is AmountValueStorage);
            Assert.Equal(amount, value.AsAmount);

            Balance balance = new Balance(amount);
            value = ValueStorageExtensions.Create(balance);
            Assert.True(value is BalanceValueStorage);
            Assert.Equal(balance, value.AsBalance);

            string s = "test-string";
            value = ValueStorageExtensions.Create(s);
            Assert.True(value is StringValueStorage);
            Assert.Equal(s, value.AsString);

            Mask regex = new Mask(".");
            value = ValueStorageExtensions.Create(regex);
            Assert.True(value is MaskValueStorage);
            Assert.Equal(regex, value.AsMask);

            IList<Value> values = new List<Value>();
            value = ValueStorageExtensions.Create(values);
            Assert.True(value is SequenceValueStorage);
            Assert.Equal(values.Count, value.AsSequence.Count);
        }

        /* Added Any storage
        [Fact]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValueStorageExtensions_Create_FailsForUnexpectedTypes()
        {
            ValueStorageExtensions.Create(1.1);  // Double is unknonw type
        }
         */

        [Fact]
        public void BooleanValueStorage_Constructor_PopulatesTypeAndValue()
        {
            BooleanValueStorage storage = new BooleanValueStorage(false);
            Assert.Equal(ValueTypeEnum.Boolean, storage.Type);
            Assert.False(storage.Val);

            storage = new BooleanValueStorage(true);
            Assert.Equal(ValueTypeEnum.Boolean, storage.Type);
            Assert.True(storage.Val);
        }

        [Fact]
        public void BooleanValueStorage_AsString_ReturnsTrueOrFalse()
        {
            Assert.Equal("false", new BooleanValueStorage(false).AsString);
            Assert.Equal("true", new BooleanValueStorage(true).AsString);
        }

        [Fact]
        public void DateTimeValueStorage_Constructor_PopulatesTypeAndValue()
        {
            DateTime date = DateTime.UtcNow;
            DateTimeValueStorage storage = new DateTimeValueStorage(date);
            Assert.Equal(ValueTypeEnum.DateTime, storage.Type);
            Assert.Equal(date, storage.Val);
        }

        [Fact]
        public void DateTimeValueStorage_AsLong_ReturnsTicks()
        {
            DateTime date = DateTime.UtcNow;
            DateTimeValueStorage storage = new DateTimeValueStorage(date);
            Assert.Equal(date.Ticks, storage.AsLong);
        }

        [Fact]
        public void DateTimeValueStorage_AsAmount_ReturnsTicks()
        {
            DateTime date = DateTime.UtcNow;
            DateTimeValueStorage storage = new DateTimeValueStorage(date);
            Assert.Equal(date.Ticks, storage.AsAmount.Quantity.ToLong());
        }

        [Fact]
        public void DateTimeValueStorage_Add_SupportsIntegerAndAmount()
        {
            DateTime date = new DateTime(2015, 10, 10);
            DateTimeValueStorage storage1 = new DateTimeValueStorage(date);
            IntegerValueStorage storage2 = new IntegerValueStorage(1000);
            Assert.Equal(date.AddSeconds(1000).Ticks, storage1.Add(storage2).AsLong);

            storage1 = new DateTimeValueStorage(date);
            AmountValueStorage storage3 = new AmountValueStorage(new Amount(2000));
            Assert.Equal(date.AddSeconds(2000).Ticks, storage1.Add(storage3).AsLong);
        }

        [Fact]
        public void IntegerValueStorage_Add_AddsTwoIntegers()
        {
            IntegerValueStorage storage1 = new IntegerValueStorage(100);
            IntegerValueStorage storage2 = new IntegerValueStorage(200);
            Assert.Equal(300, storage1.Add(storage2).AsLong);
        }

        [Fact]
        public void IntegerValueStorage_Add_AddsAmountWithoutCommodityAndChangesToAmount()
        {
            IntegerValueStorage storage1 = new IntegerValueStorage(100);
            AmountValueStorage storage2 = new AmountValueStorage(new Amount(200));

            IValueStorage result = storage1.Add(storage2);
            Assert.Equal(300, result.AsLong);
            Assert.Equal(ValueTypeEnum.Amount, result.Type);
        }

        [Fact]
        public void IntegerValueStorage_Add_AddsAmountWithCommodityAndChangesToBalance()
        {
            string commodityName = "test-commodity";
            Commodity commodity = CommodityPool.Current.Find(commodityName) ?? CommodityPool.Current.Create(commodityName);

            IntegerValueStorage storage1 = new IntegerValueStorage(100);
            AmountValueStorage storage2 = new AmountValueStorage(new Amount(200, commodity));

            IValueStorage result = storage1.Add(storage2);
            Assert.Equal(ValueTypeEnum.Balance, result.Type);
        }

        [Fact]
        public void ValueStorageExtensions_CreateFromObject_ClonesAmounts()
        {
            Amount amount = new Amount(10);
            var storage = ValueStorageExtensions.CreateFromObject(amount);

            Amount newAmount = storage.AsAmount;
            newAmount.Multiply(new Amount(15));

            Assert.Equal(150, newAmount.Quantity.ToLong());
            Assert.Equal(10, amount.Quantity.ToLong());
        }

        [Fact]
        public void ValueStorageExtensions_CreateFromObject_ClonesBalances()
        {
            Balance balance = new Balance();
            var storage = ValueStorageExtensions.CreateFromObject(balance);

            Balance newBalance = storage.AsBalance;
            newBalance.Add(new Amount(15));

            Assert.Single(newBalance.Amounts);
            Assert.Empty(balance.Amounts);
        }

        [Fact]
        public void ValueStorageExtensions_CreateFromObject_ClonesSequences()
        {
            IList<Value> sequence = new List<Value>();
            var storage = ValueStorageExtensions.CreateFromObject(sequence);

            IList<Value> newSequence = storage.AsSequence;
            newSequence.Add(Value.One);

            Assert.Single(newSequence);
            Assert.Empty(sequence);
        }

        [Fact]
        public void SequenceValueStorage_Negate_CallsNegatedForChildren()
        {
            Amount amount = new Amount(100);
            IList<Value> sequence = new List<Value>();
            sequence.Add(Value.Get(amount));
            var storage = ValueStorageExtensions.CreateFromObject(sequence);

            storage.Negate();

            Amount newAmount = storage.AsSequence.First().AsAmount;

            Assert.Equal(-100, newAmount.Quantity.ToLong());
            Assert.Equal(100, amount.Quantity.ToLong());
        }

        [Fact]
        public void SequenceValueStorage_Create_SupportsNullableDateTime()
        {
            DateTime? date = DateTime.Now;
            var storage = ValueStorageExtensions.CreateFromObject(date);
            Assert.True(storage.Type == ValueTypeEnum.DateTime);
            Assert.Equal(date.Value, storage.AsDateTime);
        }

        [Fact]
        public void Value_Get_SupportsEmptyNullableDateTime1()
        {
            DateTime? date = null;
            var val = Value.Get(date);
            Assert.True(Value.IsNullOrEmpty(val));
            Assert.True(val.Type == ValueTypeEnum.Void);
        }


    }
}

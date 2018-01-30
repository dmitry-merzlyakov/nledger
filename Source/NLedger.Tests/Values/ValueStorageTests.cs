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
using NLedger.Commodities;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Values
{
    [TestClass]
    public class ValueStorageTests : TestFixture
    {
        [TestMethod]
        public void ValueStorageExtensions_Create_CreatesStorageInstancesByTypes()
        {
            IValueStorage value;

            value = ValueStorageExtensions.Create(true);
            Assert.IsTrue(value is BooleanValueStorage);
            Assert.AreEqual(true, value.AsBoolean);

            value = ValueStorageExtensions.Create(new DateTime(2010, 10, 10));
            Assert.IsTrue(value is DateTimeValueStorage);
            Assert.AreEqual(new DateTime(2010, 10, 10), value.AsDateTime);

            value = ValueStorageExtensions.Create((int)100);
            Assert.IsTrue(value is IntegerValueStorage);
            Assert.AreEqual(100, value.AsLong);

            value = ValueStorageExtensions.Create((long)100);
            Assert.IsTrue(value is IntegerValueStorage);
            Assert.AreEqual(100, value.AsLong);

            Amount amount = new Amount(200);
            value = ValueStorageExtensions.Create(amount);
            Assert.IsTrue(value is AmountValueStorage);
            Assert.AreEqual(amount, value.AsAmount);

            Balance balance = new Balance(amount);
            value = ValueStorageExtensions.Create(balance);
            Assert.IsTrue(value is BalanceValueStorage);
            Assert.AreEqual(balance, value.AsBalance);

            string s = "test-string";
            value = ValueStorageExtensions.Create(s);
            Assert.IsTrue(value is StringValueStorage);
            Assert.AreEqual(s, value.AsString);

            Mask regex = new Mask(".");
            value = ValueStorageExtensions.Create(regex);
            Assert.IsTrue(value is MaskValueStorage);
            Assert.AreEqual(regex, value.AsMask);

            IList<Value> values = new List<Value>();
            value = ValueStorageExtensions.Create(values);
            Assert.IsTrue(value is SequenceValueStorage);
            Assert.AreEqual(values.Count, value.AsSequence.Count);
        }

        /* Added Any storage
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValueStorageExtensions_Create_FailsForUnexpectedTypes()
        {
            ValueStorageExtensions.Create(1.1);  // Double is unknonw type
        }
         */

        [TestMethod]
        public void BooleanValueStorage_Constructor_PopulatesTypeAndValue()
        {
            BooleanValueStorage storage = new BooleanValueStorage(false);
            Assert.AreEqual(ValueTypeEnum.Boolean, storage.Type);
            Assert.IsFalse(storage.Val);

            storage = new BooleanValueStorage(true);
            Assert.AreEqual(ValueTypeEnum.Boolean, storage.Type);
            Assert.IsTrue(storage.Val);
        }

        [TestMethod]
        public void BooleanValueStorage_AsString_ReturnsTrueOrFalse()
        {
            Assert.AreEqual("false", new BooleanValueStorage(false).AsString);
            Assert.AreEqual("true", new BooleanValueStorage(true).AsString);
        }

        [TestMethod]
        public void DateTimeValueStorage_Constructor_PopulatesTypeAndValue()
        {
            DateTime date = DateTime.UtcNow;
            DateTimeValueStorage storage = new DateTimeValueStorage(date);
            Assert.AreEqual(ValueTypeEnum.DateTime, storage.Type);
            Assert.AreEqual(date, storage.Val);
        }

        [TestMethod]
        public void DateTimeValueStorage_AsLong_ReturnsTicks()
        {
            DateTime date = DateTime.UtcNow;
            DateTimeValueStorage storage = new DateTimeValueStorage(date);
            Assert.AreEqual(date.Ticks, storage.AsLong);
        }

        [TestMethod]
        public void DateTimeValueStorage_AsAmount_ReturnsTicks()
        {
            DateTime date = DateTime.UtcNow;
            DateTimeValueStorage storage = new DateTimeValueStorage(date);
            Assert.AreEqual(date.Ticks, storage.AsAmount.Quantity.ToLong());
        }

        [TestMethod]
        public void DateTimeValueStorage_Add_SupportsIntegerAndAmount()
        {
            DateTime date = new DateTime(2015, 10, 10);
            DateTimeValueStorage storage1 = new DateTimeValueStorage(date);
            IntegerValueStorage storage2 = new IntegerValueStorage(1000);
            Assert.AreEqual(date.AddSeconds(1000).Ticks, storage1.Add(storage2).AsLong);

            storage1 = new DateTimeValueStorage(date);
            AmountValueStorage storage3 = new AmountValueStorage(new Amount(2000));
            Assert.AreEqual(date.AddSeconds(2000).Ticks, storage1.Add(storage3).AsLong);
        }

        [TestMethod]
        public void IntegerValueStorage_Add_AddsTwoIntegers()
        {
            IntegerValueStorage storage1 = new IntegerValueStorage(100);
            IntegerValueStorage storage2 = new IntegerValueStorage(200);
            Assert.AreEqual(300, storage1.Add(storage2).AsLong);
        }

        [TestMethod]
        public void IntegerValueStorage_Add_AddsAmountWithoutCommodityAndChangesToAmount()
        {
            IntegerValueStorage storage1 = new IntegerValueStorage(100);
            AmountValueStorage storage2 = new AmountValueStorage(new Amount(200));

            IValueStorage result = storage1.Add(storage2);
            Assert.AreEqual(300, result.AsLong);
            Assert.AreEqual(ValueTypeEnum.Amount, result.Type);
        }

        [TestMethod]
        public void IntegerValueStorage_Add_AddsAmountWithCommodityAndChangesToBalance()
        {
            string commodityName = "test-commodity";
            Commodity commodity = CommodityPool.Current.Find(commodityName) ?? CommodityPool.Current.Create(commodityName);

            IntegerValueStorage storage1 = new IntegerValueStorage(100);
            AmountValueStorage storage2 = new AmountValueStorage(new Amount(200, commodity));

            IValueStorage result = storage1.Add(storage2);
            Assert.AreEqual(ValueTypeEnum.Balance, result.Type);
        }

        [TestMethod]
        public void ValueStorageExtensions_CreateFromObject_ClonesAmounts()
        {
            Amount amount = new Amount(10);
            var storage = ValueStorageExtensions.CreateFromObject(amount);

            Amount newAmount = storage.AsAmount;
            newAmount.Multiply(new Amount(15));

            Assert.AreEqual(150, newAmount.Quantity.ToLong());
            Assert.AreEqual(10, amount.Quantity.ToLong());
        }

        [TestMethod]
        public void ValueStorageExtensions_CreateFromObject_ClonesBalances()
        {
            Balance balance = new Balance();
            var storage = ValueStorageExtensions.CreateFromObject(balance);

            Balance newBalance = storage.AsBalance;
            newBalance.Add(new Amount(15));

            Assert.AreEqual(1, newBalance.Amounts.Count());
            Assert.AreEqual(0, balance.Amounts.Count());
        }

        [TestMethod]
        public void ValueStorageExtensions_CreateFromObject_ClonesSequences()
        {
            IList<Value> sequence = new List<Value>();
            var storage = ValueStorageExtensions.CreateFromObject(sequence);

            IList<Value> newSequence = storage.AsSequence;
            newSequence.Add(Value.One);

            Assert.AreEqual(1, newSequence.Count());
            Assert.AreEqual(0, sequence.Count());
        }

        [TestMethod]
        public void SequenceValueStorage_Negate_CallsNegatedForChildren()
        {
            Amount amount = new Amount(100);
            IList<Value> sequence = new List<Value>();
            sequence.Add(Value.Get(amount));
            var storage = ValueStorageExtensions.CreateFromObject(sequence);

            storage.Negate();

            Amount newAmount = storage.AsSequence.First().AsAmount;

            Assert.AreEqual(-100, newAmount.Quantity.ToLong());
            Assert.AreEqual(100, amount.Quantity.ToLong());
        }

        [TestMethod]
        public void SequenceValueStorage_Create_SupportsNullableDateTime()
        {
            DateTime? date = DateTime.Now;
            var storage = ValueStorageExtensions.CreateFromObject(date);
            Assert.IsTrue(storage.Type == ValueTypeEnum.DateTime);
            Assert.AreEqual(date.Value, storage.AsDateTime);
        }

        [TestMethod]
        public void Value_Get_SupportsEmptyNullableDateTime1()
        {
            DateTime? date = null;
            var val = Value.Get(date);
            Assert.IsTrue(Value.IsNullOrEmpty(val));
            Assert.IsTrue(val.Type == ValueTypeEnum.Void);
        }


    }
}

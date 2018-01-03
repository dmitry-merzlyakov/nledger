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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests
{
    [TestClass]
    public class BalanceTests : TestFixture
    {
        [TestMethod]
        public void Balance_IsEmpty_IndicatesThatBalanceHasNoAmounts()
        {
            Balance balance = new Balance();
            Assert.IsTrue(balance.IsEmpty);

            balance.Add(new Amount(1));
            Assert.IsFalse(balance.IsEmpty);
        }

        [TestMethod]
        public void Balance_IsSingleAmount_IndicatesThatBalanceHasOneAmounts()
        {
            Balance balance = new Balance();
            Assert.IsFalse(balance.IsSingleAmount);

            balance.Add(new Amount(1));
            Assert.IsTrue(balance.IsSingleAmount);

            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            balance.Add(new Amount(BigInt.FromLong(1), comm));
            Assert.IsFalse(balance.IsSingleAmount);
        }

        [TestMethod]
        public void Balance_IsRealZero_IndicatesThatBalanceIsEmptyOrHasAllEmptyAmounts()
        {
            Balance balance = new Balance();
            Assert.IsTrue(balance.IsRealZero);

            balance.Add(new Amount(0));
            Assert.IsTrue(balance.IsRealZero);

            balance.Add(new Amount(1));
            Assert.IsFalse(balance.IsRealZero);
        }

        [TestMethod]
        public void Balance_SingleAmount_RetursSingleAmountIfItIsReallySingle()
        {
            Amount amount = new Amount(1);
            Balance balance = new Balance(amount);
            Assert.AreEqual(amount, balance.SingleAmount);
        }

        [TestMethod]
        public void Balance_Add_IgnoresZeroAmounts()
        {
            Balance balance = new Balance();
            balance.Add(new Amount(0));
            Assert.IsTrue(balance.IsEmpty);
        }

        [TestMethod]
        public void Balance_Add_AddsNonZeroAmounts()
        {
            Balance balance = new Balance();
            balance.Add(new Amount(1));
            Assert.IsTrue(balance.IsSingleAmount);
        }

        [TestMethod]
        public void Balance_Add_IncorporatesAmountsWithTheSameCommodity()
        {
            Balance balance = new Balance();
            balance.Add(new Amount(1));
            balance.Add(new Amount(2));
            Assert.IsTrue(balance.IsSingleAmount);
            Assert.AreEqual(3, balance.SingleAmount.Quantity.ToLong());
        }

        [TestMethod]
        public void Balance_Add_CopiesAddedAmounts()
        {
            Commodity comm1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            Commodity comm2 = new Commodity(CommodityPool.Current, new CommodityBase("comm2"));

            Amount amount1 = new Amount(BigInt.FromInt(1), comm1);
            Amount amount2 = new Amount(BigInt.FromInt(2), comm2);

            Balance balance = new Balance();
            balance.Add(amount1);
            balance.Add(amount2);

            amount1.InPlaceAdd(new Amount(10));
            amount2.InPlaceAdd(new Amount(10));

            Assert.AreEqual(1, balance.Amounts.ElementAt(0).Value.Quantity.ToLong());
            Assert.AreEqual(2, balance.Amounts.ElementAt(1).Value.Quantity.ToLong());
            Assert.AreEqual(11, amount1.Quantity.ToLong());
            Assert.AreEqual(12, amount2.Quantity.ToLong());
        }

        [TestMethod]
        public void Balance_Subtract_IgnoresZeroAmounts()
        {
            Balance balance = new Balance();
            balance.Subtract(new Amount(0));
            Assert.IsTrue(balance.IsEmpty);
        }

        [TestMethod]
        public void Balance_Subtract_SubtractsAnExistingAmount()
        {
            Balance balance = new Balance();
            balance.Add(new Amount(5));
            balance.Subtract(new Amount(2));
            Assert.IsTrue(balance.IsSingleAmount);
            Assert.AreEqual(3, balance.SingleAmount.Quantity.ToLong());
        }

        [TestMethod]
        public void Balance_Subtract_RemovesZeroResults()
        {
            Balance balance = new Balance();
            balance.Add(new Amount(5));
            balance.Subtract(new Amount(5));
            Assert.IsTrue(balance.IsEmpty);
        }

        [TestMethod]
        public void Balance_Subtract_AddsInvertedAmount()
        {
            Balance balance = new Balance();
            balance.Subtract(new Amount(5));
            Assert.IsTrue(balance.IsSingleAmount);
            Assert.AreEqual(-5, balance.SingleAmount.Quantity.ToLong());
        }

        [TestMethod]
        public void Balance_CommodityAmount_EmptyBalanceReturnsNullForGivenCommodity()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Balance balance = new Balance();
            Assert.IsNull(balance.CommodityAmount(comm));
        }

        [TestMethod]
        public void Balance_CommodityAmount_LooksForAddedAmountWithTheSameCommodity()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromLong(5), comm));

            Amount amount = balance.CommodityAmount(comm);
            Assert.AreEqual(5, amount.Quantity.ToLong());
        }

        [TestMethod]
        public void Balance_CommodityAmount_ReturnsExistingSingleCommodity()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromLong(5), comm));

            Amount amount = balance.CommodityAmount();
            Assert.AreEqual(5, amount.Quantity.ToLong());
        }

        [TestMethod]
        [ExpectedException(typeof(BalanceError))]
        public void Balance_Multiply_RequiresAmounObject()
        {
            Balance balance = new Balance();
            balance.Multiply(new Amount()); // Non-initialized amount
        }

        [TestMethod]
        public void Balance_Multiply_ReturnsCurrentBalanceIfItIsZero()
        {
            Balance balance = new Balance();
            Assert.IsTrue(balance.IsRealZero);
            Balance result = balance.Multiply(new Amount(10));
            Assert.AreEqual(result, balance);
        }

        [TestMethod]
        public void Balance_Multiply_ReturnsNewEmptyBalanceIfAmountIsZero()
        {
            Balance balance = new Balance(new Amount(10));  // Non-empty balance
            Balance result = balance.Multiply(new Amount(0));
            Assert.AreNotEqual(result, balance);
            Assert.IsTrue(result.IsRealZero);
        }

        [TestMethod]
        public void Balance_Multiply_MultipliesAmountsIfGivenAmountHasNoCommodity()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromInt(10), commodity1));
            balance.Add(new Amount(BigInt.FromInt(20), commodity2));

            Balance result = balance.Multiply(new Amount(5));

            Assert.AreEqual(50, balance.Amounts.First().Value.Quantity.ToLong());
            Assert.AreEqual(100, balance.Amounts.Last().Value.Quantity.ToLong());
        }

        [TestMethod]
        [ExpectedException(typeof(BalanceError))]
        public void Balance_Multiply_ExpectsTheSameCommodityForSingleAmountBalance()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromInt(10), commodity1));

            Balance result = balance.Multiply(new Amount(BigInt.FromInt(10), commodity2));
        }

        [TestMethod]
        [ExpectedException(typeof(BalanceError))]
        public void Balance_Multiply_RequiresNoCommoditiesForAmountIfBalanceIsMultiAmount()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromInt(10), commodity1));
            balance.Add(new Amount(BigInt.FromInt(10), commodity2));

            Balance result = balance.Multiply(new Amount(BigInt.FromInt(10), commodity2));
        }

        [TestMethod]
        public void Balance_Multiply_MultipliesSingleAmountBalance()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));

            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromInt(10), commodity1));
            balance.Add(new Amount(BigInt.FromInt(20), commodity1));

            Balance result = balance.Multiply(new Amount(BigInt.FromInt(10), commodity1));

            Assert.AreEqual(300, balance.Amounts.First().Value.Quantity.ToLong());
        }

        [TestMethod]
        [ExpectedException(typeof(BalanceError))]
        public void Balance_Divide_RequiresAmounObject()
        {
            Balance balance = new Balance();
            balance.Divide(new Amount()); // Non-initialized amount
        }

        [TestMethod]
        public void Balance_Divide_ReturnsCurrentBalanceIfItIsZero()
        {
            Balance balance = new Balance();
            Assert.IsTrue(balance.IsRealZero);
            Balance result = balance.Divide(new Amount(10));
            Assert.AreEqual(result, balance);
        }

        [TestMethod]
        [ExpectedException(typeof(BalanceError))]
        public void Balance_Divide_ReturnsExceptionIfAmountIsZero()
        {
            Balance balance = new Balance(new Amount(10));  // Non-empty balance
            Balance result = balance.Divide(new Amount(0));
        }

        [TestMethod]
        public void Balance_Divide_MultipliesAmountsIfGivenAmountHasNoCommodity()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromInt(10), commodity1));
            balance.Add(new Amount(BigInt.FromInt(20), commodity2));

            Balance result = balance.Divide(new Amount(5));

            Assert.AreEqual(2, balance.Amounts.First().Value.Quantity.ToLong());
            Assert.AreEqual(4, balance.Amounts.Last().Value.Quantity.ToLong());
        }

        [TestMethod]
        [ExpectedException(typeof(BalanceError))]
        public void Balance_Divide_ExpectsTheSameCommodityForSingleAmountBalance()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromInt(10), commodity1));

            Balance result = balance.Divide(new Amount(BigInt.FromInt(10), commodity2));
        }

        [TestMethod]
        [ExpectedException(typeof(BalanceError))]
        public void Balance_Divide_RequiresNoCommoditiesForAmountIfBalanceIsMultiAmount()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromInt(10), commodity1));
            balance.Add(new Amount(BigInt.FromInt(10), commodity2));

            Balance result = balance.Divide(new Amount(BigInt.FromInt(10), commodity2));
        }

        [TestMethod]
        public void Balance_Divide_DividessSingleAmountBalance()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));

            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromInt(10), commodity1));
            balance.Add(new Amount(BigInt.FromInt(20), commodity1));

            Balance result = balance.Divide(new Amount(BigInt.FromInt(10), commodity1));

            Assert.AreEqual(3, balance.Amounts.First().Value.Quantity.ToLong());
        }

        [TestMethod]
        [ExpectedException(typeof(BalanceError))]
        public void Balance_EqualsAmount_RequiresNonEmptyAmount()
        {
            new Balance().Equals(new Amount());
        }

        [TestMethod]
        public void Balance_EqualsAmount_ReturnsIsEmptyIfAmountIsZero()
        {
            Assert.IsTrue(new Balance().Equals(new Amount(0)));
        }

        [TestMethod]
        public void Balance_EqualsAmount_ReturnsFalseIfBalanceIsMultiAmount()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));

            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromInt(10), commodity1));
            balance.Add(new Amount(BigInt.FromInt(20), commodity1));

            Assert.IsFalse(balance.Equals(new Amount(10)));
        }

        [TestMethod]
        public void Balance_EqualsAmount_ComparesSingleAmountWithAmount()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));

            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromInt(10), commodity1));
            balance.Add(new Amount(BigInt.FromInt(20), commodity1));

            Assert.IsTrue(balance.Equals(new Amount(BigInt.FromLong(30), commodity1)));
        }

        [TestMethod]
        public void Balance_EqualsBalance_ComparesBalanceAmountsToBeEqual()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));

            Balance balance1 = new Balance();
            balance1.Add(new Amount(BigInt.FromInt(10), commodity1));
            balance1.Add(new Amount(BigInt.FromInt(20), commodity1));

            Balance balance2 = new Balance();
            balance2.Add(new Amount(BigInt.FromInt(10), commodity1));
            balance2.Add(new Amount(BigInt.FromInt(20), commodity1));

            Assert.IsTrue(balance1.Equals(balance2));
        }

        [TestMethod]
        public void Balance_IsLessThan_ComparesAllAmounts()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromInt(10), commodity1));
            balance.Add(new Amount(BigInt.FromInt(20), commodity2));

            Assert.IsTrue(balance.IsLessThan(new Amount(30)));
            Assert.IsFalse(balance.IsLessThan(new Amount(10)));
        }

        [TestMethod]
        public void Balance_IsGreaterThan_ComparesAllAmounts()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.FromInt(10), commodity1));
            balance.Add(new Amount(BigInt.FromInt(20), commodity2));

            Assert.IsFalse(balance.IsGreaterThan(new Amount(30)));
            Assert.IsTrue(balance.IsGreaterThan(new Amount(10)));
        }

        [TestMethod]
        public void Balance_Constructor_ClonesAmounts()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Amount amount1 = new Amount(BigInt.FromInt(200), commodity1);
            Amount amount2 = new Amount(BigInt.FromInt(300), commodity2);

            Balance balance = new Balance();
            balance.Add(amount1);
            balance.Add(amount2);

            Balance clonedBalance = new Balance(balance);
            clonedBalance.Amounts.ElementAt(0).Value.Multiply(new Amount(5));
            clonedBalance.Amounts.ElementAt(1).Value.Multiply(new Amount(7));

            Assert.AreEqual(2, clonedBalance.Amounts.Count());
            Assert.AreEqual(1000, clonedBalance.Amounts.ElementAt(0).Value.Quantity.ToLong());
            Assert.AreEqual(2100, clonedBalance.Amounts.ElementAt(1).Value.Quantity.ToLong());

            Assert.AreEqual(2, balance.Amounts.Count());
            Assert.AreEqual(200, balance.Amounts.ElementAt(0).Value.Quantity.ToLong());
            Assert.AreEqual(300, balance.Amounts.ElementAt(1).Value.Quantity.ToLong());
        }

        [TestMethod]
        public void Balance_Add_Balance_AddsAllAmounts()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            // First balance

            Amount amount1 = new Amount(BigInt.FromInt(200), commodity1);
            Amount amount2 = new Amount(BigInt.FromInt(300), commodity2);

            Balance balance1 = new Balance();
            balance1.Add(amount1);
            balance1.Add(amount2);

            // Second balance

            Amount amount3 = new Amount(BigInt.FromInt(400), commodity1);
            Amount amount4 = new Amount(BigInt.FromInt(500), commodity2);

            Balance balance2 = new Balance();
            balance2.Add(amount3);
            balance2.Add(amount4);

            // Action

            balance1.Add(balance2);

            // Assert

            Assert.AreEqual(2, balance1.Amounts.Count());
            Assert.AreEqual(600, balance1.Amounts.ElementAt(0).Value.Quantity.ToLong());
            Assert.AreEqual(800, balance1.Amounts.ElementAt(1).Value.Quantity.ToLong());
        }

        [TestMethod]
        public void Balance_Subtract_Balance_SubtractsAllAmounts()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            // First balance

            Amount amount1 = new Amount(BigInt.FromInt(200), commodity1);
            Amount amount2 = new Amount(BigInt.FromInt(300), commodity2);

            Balance balance1 = new Balance();
            balance1.Add(amount1);
            balance1.Add(amount2);

            // Second balance

            Amount amount3 = new Amount(BigInt.FromInt(50), commodity1);
            Amount amount4 = new Amount(BigInt.FromInt(70), commodity2);

            Balance balance2 = new Balance();
            balance2.Add(amount3);
            balance2.Add(amount4);

            // Action

            balance1.Subtract(balance2);

            // Assert

            Assert.AreEqual(2, balance1.Amounts.Count());
            Assert.AreEqual(150, balance1.Amounts.ElementAt(0).Value.Quantity.ToLong());
            Assert.AreEqual(230, balance1.Amounts.ElementAt(1).Value.Quantity.ToLong());
        }

        [TestMethod]
        public void Balance_Print_PerformsLeftJustificationInCaseOfNo_AMOUNT_PRINT_RIGHT_JUSTIFY()
        {
            Balance balance = new Balance();
            balance.Add((Amount)1);
            var result = balance.Print(10);
            Assert.AreEqual("1         ", result);
        }

        [TestMethod]
        public void Balance_Print_PerformsRightJustificationInCaseOf_AMOUNT_PRINT_RIGHT_JUSTIFY()
        {
            Balance balance = new Balance();
            balance.Add((Amount)1);
            var result = balance.Print(10, 10, AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY);
            Assert.AreEqual("         1", result);
        }

        [TestMethod]
        public void Balance_ToString_CallsPrint()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("balToStrA"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("balToStrB"));

            Balance balance = new Balance();
            balance.Add(new Amount(BigInt.Parse("1.22"), commodity1));
            balance.Add(new Amount(BigInt.Parse("2.44"), commodity2));

            Assert.AreEqual("balToStrA1  \r\nbalToStrB2", balance.ToString());
        }

        [TestMethod]
        public void Balance_IsNonZero_ReturnsFalseIfEmpty()
        {
            Balance balance = new Balance();
            Assert.IsTrue(balance.IsEmpty);
            Assert.IsFalse(balance.IsNonZero);
        }

        [TestMethod]
        public void Balance_IsNonZero_ReturnsFalseIfAllAmountsAreZero()
        {
            Commodity commodityA = new Commodity(CommodityPool.Current, new CommodityBase("balNZeroA"));
            Commodity commodityB = new Commodity(CommodityPool.Current, new CommodityBase("balNZeroB"));

            // Commodity precision is "0"; add values that are less than commodity precision
            Amount amountA = new Amount(BigInt.Parse("0.1"), commodityA);
            Amount amountB = new Amount(BigInt.Parse("0.1"), commodityB);

            Balance balance = new Balance();
            balance.Add(amountA);
            balance.Add(amountB);

            Assert.IsFalse(amountA.IsNonZero);
            Assert.IsFalse(amountB.IsNonZero);
            Assert.IsFalse(balance.IsEmpty);

            Assert.IsFalse(balance.IsNonZero);
        }

        [TestMethod]
        public void Balance_IsNonZero_ReturnsTrueIfOneOfAmountsIsNonZero()
        {
            Commodity commodityA = new Commodity(CommodityPool.Current, new CommodityBase("balNZeroA"));
            Commodity commodityB = new Commodity(CommodityPool.Current, new CommodityBase("balNZeroB"));

            // Commodity precision is "0"; add values that are less than commodity precision
            Amount amountA = new Amount(BigInt.Parse("0.1"), commodityA);
            Amount amountB = new Amount(BigInt.Parse("1.0"), commodityB);

            Balance balance = new Balance();
            balance.Add(amountA);
            balance.Add(amountB);

            Assert.IsFalse(amountA.IsNonZero);
            Assert.IsTrue(amountB.IsNonZero);  // Here you are
            Assert.IsFalse(balance.IsEmpty);

            Assert.IsTrue(balance.IsNonZero);  // Results in True
        }

    }
}

// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Commodities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests
{
    public class BalanceTests : TestFixture
    {
        [Fact]
        public void Balance_IsEmpty_IndicatesThatBalanceHasNoAmounts()
        {
            Balance balance = new Balance();
            Assert.True(balance.IsEmpty);

            balance.Add(new Amount(1));
            Assert.False(balance.IsEmpty);
        }

        [Fact]
        public void Balance_IsSingleAmount_IndicatesThatBalanceHasOneAmounts()
        {
            Balance balance = new Balance();
            Assert.False(balance.IsSingleAmount);

            balance.Add(new Amount(1));
            Assert.True(balance.IsSingleAmount);

            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            balance.Add(new Amount(1, comm));
            Assert.False(balance.IsSingleAmount);
        }

        [Fact]
        public void Balance_IsRealZero_IndicatesThatBalanceIsEmptyOrHasAllEmptyAmounts()
        {
            Balance balance = new Balance();
            Assert.True(balance.IsRealZero);

            balance.Add(new Amount(0));
            Assert.True(balance.IsRealZero);

            balance.Add(new Amount(1));
            Assert.False(balance.IsRealZero);
        }

        [Fact]
        public void Balance_SingleAmount_RetursSingleAmountIfItIsReallySingle()
        {
            Amount amount = new Amount(1);
            Balance balance = new Balance(amount);
            Assert.Equal(amount, balance.SingleAmount);
        }

        [Fact]
        public void Balance_Add_IgnoresZeroAmounts()
        {
            Balance balance = new Balance();
            balance.Add(new Amount(0));
            Assert.True(balance.IsEmpty);
        }

        [Fact]
        public void Balance_Add_AddsNonZeroAmounts()
        {
            Balance balance = new Balance();
            balance.Add(new Amount(1));
            Assert.True(balance.IsSingleAmount);
        }

        [Fact]
        public void Balance_Add_IncorporatesAmountsWithTheSameCommodity()
        {
            Balance balance = new Balance();
            balance.Add(new Amount(1));
            balance.Add(new Amount(2));
            Assert.True(balance.IsSingleAmount);
            Assert.Equal(3, balance.SingleAmount.Quantity.ToLong());
        }

        [Fact]
        public void Balance_Add_CopiesAddedAmounts()
        {
            Commodity comm1 = new Commodity(CommodityPool.Current, new CommodityBase("comm1"));
            Commodity comm2 = new Commodity(CommodityPool.Current, new CommodityBase("comm2"));

            Amount amount1 = new Amount(1, comm1);
            Amount amount2 = new Amount(2, comm2);

            Balance balance = new Balance();
            balance.Add(amount1);
            balance.Add(amount2);

            amount1.InPlaceAdd(new Amount(10));
            amount2.InPlaceAdd(new Amount(10));

            Assert.Equal(1, balance.Amounts.ElementAt(0).Value.Quantity.ToLong());
            Assert.Equal(2, balance.Amounts.ElementAt(1).Value.Quantity.ToLong());
            Assert.Equal(11, amount1.Quantity.ToLong());
            Assert.Equal(12, amount2.Quantity.ToLong());
        }

        [Fact]
        public void Balance_Subtract_IgnoresZeroAmounts()
        {
            Balance balance = new Balance();
            balance.Subtract(new Amount(0));
            Assert.True(balance.IsEmpty);
        }

        [Fact]
        public void Balance_Subtract_SubtractsAnExistingAmount()
        {
            Balance balance = new Balance();
            balance.Add(new Amount(5));
            balance.Subtract(new Amount(2));
            Assert.True(balance.IsSingleAmount);
            Assert.Equal(3, balance.SingleAmount.Quantity.ToLong());
        }

        [Fact]
        public void Balance_Subtract_RemovesZeroResults()
        {
            Balance balance = new Balance();
            balance.Add(new Amount(5));
            balance.Subtract(new Amount(5));
            Assert.True(balance.IsEmpty);
        }

        [Fact]
        public void Balance_Subtract_AddsInvertedAmount()
        {
            Balance balance = new Balance();
            balance.Subtract(new Amount(5));
            Assert.True(balance.IsSingleAmount);
            Assert.Equal(-5, balance.SingleAmount.Quantity.ToLong());
        }

        [Fact]
        public void Balance_CommodityAmount_EmptyBalanceReturnsNullForGivenCommodity()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Balance balance = new Balance();
            Assert.Null(balance.CommodityAmount(comm));
        }

        [Fact]
        public void Balance_CommodityAmount_LooksForAddedAmountWithTheSameCommodity()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Balance balance = new Balance();
            balance.Add(new Amount(5, comm));

            Amount amount = balance.CommodityAmount(comm);
            Assert.Equal(5, amount.Quantity.ToLong());
        }

        [Fact]
        public void Balance_CommodityAmount_ReturnsExistingSingleCommodity()
        {
            Commodity comm = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            Balance balance = new Balance();
            balance.Add(new Amount(5, comm));

            Amount amount = balance.CommodityAmount();
            Assert.Equal(5, amount.Quantity.ToLong());
        }

        [Fact]
        public void Balance_Multiply_RequiresAmounObject()
        {
            Balance balance = new Balance();
            Assert.Throws<BalanceError>(() => balance.Multiply(new Amount())); // Non-initialized amount);
        }

        [Fact]
        public void Balance_Multiply_ReturnsCurrentBalanceIfItIsZero()
        {
            Balance balance = new Balance();
            Assert.True(balance.IsRealZero);
            Balance result = balance.Multiply(new Amount(10));
            Assert.Equal(result, balance);
        }

        [Fact]
        public void Balance_Multiply_ReturnsNewEmptyBalanceIfAmountIsZero()
        {
            Balance balance = new Balance(new Amount(10));  // Non-empty balance
            Balance result = balance.Multiply(new Amount(0));
            Assert.NotEqual(result, balance);
            Assert.True(result.IsRealZero);
        }

        [Fact]
        public void Balance_Multiply_MultipliesAmountsIfGivenAmountHasNoCommodity()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(10, commodity1));
            balance.Add(new Amount(20, commodity2));

            Balance result = balance.Multiply(new Amount(5));

            Assert.Equal(50, balance.Amounts.First().Value.Quantity.ToLong());
            Assert.Equal(100, balance.Amounts.Last().Value.Quantity.ToLong());
        }

        [Fact]
        public void Balance_Multiply_ExpectsTheSameCommodityForSingleAmountBalance()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(10, commodity1));

            Assert.Throws<BalanceError>(() => balance.Multiply(new Amount(10, commodity2)));
        }

        [Fact]
        public void Balance_Multiply_RequiresNoCommoditiesForAmountIfBalanceIsMultiAmount()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(10, commodity1));
            balance.Add(new Amount(10, commodity2));

            Assert.Throws<BalanceError>(() => balance.Multiply(new Amount(10, commodity2)));
        }

        [Fact]
        public void Balance_Multiply_MultipliesSingleAmountBalance()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));

            Balance balance = new Balance();
            balance.Add(new Amount(10, commodity1));
            balance.Add(new Amount(20, commodity1));

            Balance result = balance.Multiply(new Amount(10, commodity1));

            Assert.Equal(300, balance.Amounts.First().Value.Quantity.ToLong());
        }

        [Fact]
        public void Balance_Divide_RequiresAmounObject()
        {
            Balance balance = new Balance();
            Assert.Throws<BalanceError>(() => balance.Divide(new Amount())); // Non-initialized amount
        }

        [Fact]
        public void Balance_Divide_ReturnsCurrentBalanceIfItIsZero()
        {
            Balance balance = new Balance();
            Assert.True(balance.IsRealZero);
            Balance result = balance.Divide(new Amount(10));
            Assert.Equal(result, balance);
        }

        [Fact]
        public void Balance_Divide_ReturnsExceptionIfAmountIsZero()
        {
            Balance balance = new Balance(new Amount(10));  // Non-empty balance
            Assert.Throws<BalanceError>(() => balance.Divide(new Amount(0)));
        }

        [Fact]
        public void Balance_Divide_MultipliesAmountsIfGivenAmountHasNoCommodity()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(10, commodity1));
            balance.Add(new Amount(20, commodity2));

            Balance result = balance.Divide(new Amount(5));

            Assert.Equal(2, balance.Amounts.First().Value.Quantity.ToLong());
            Assert.Equal(4, balance.Amounts.Last().Value.Quantity.ToLong());
        }

        [Fact]
        public void Balance_Divide_ExpectsTheSameCommodityForSingleAmountBalance()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(10, commodity1));

            Assert.Throws<BalanceError>(() => balance.Divide(new Amount(10, commodity2)));
        }

        [Fact]
        public void Balance_Divide_RequiresNoCommoditiesForAmountIfBalanceIsMultiAmount()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(10, commodity1));
            balance.Add(new Amount(10, commodity2));

            Assert.Throws<BalanceError>(() => balance.Divide(new Amount(10, commodity2)));
        }

        [Fact]
        public void Balance_Divide_DividessSingleAmountBalance()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));

            Balance balance = new Balance();
            balance.Add(new Amount(10, commodity1));
            balance.Add(new Amount(20, commodity1));

            Balance result = balance.Divide(new Amount(10, commodity1));

            Assert.Equal(3, balance.Amounts.First().Value.Quantity.ToLong());
        }

        [Fact]
        public void Balance_EqualsAmount_RequiresNonEmptyAmount()
        {
            Assert.Throws<BalanceError>(() => new Balance().Equals(new Amount()));
        }

        [Fact]
        public void Balance_EqualsAmount_ReturnsIsEmptyIfAmountIsZero()
        {
            Assert.True(new Balance().Equals(new Amount(0)));
        }

        [Fact]
        public void Balance_EqualsAmount_ReturnsFalseIfBalanceIsMultiAmount()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));

            Balance balance = new Balance();
            balance.Add(new Amount(10, commodity1));
            balance.Add(new Amount(20, commodity1));

            Assert.False(balance.Equals(new Amount(10)));
        }

        [Fact]
        public void Balance_EqualsAmount_ComparesSingleAmountWithAmount()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));

            Balance balance = new Balance();
            balance.Add(new Amount(10, commodity1));
            balance.Add(new Amount(20, commodity1));

            Assert.True(balance.Equals(new Amount(30, commodity1)));
        }

        [Fact]
        public void Balance_EqualsBalance_ComparesBalanceAmountsToBeEqual()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));

            Balance balance1 = new Balance();
            balance1.Add(new Amount(10, commodity1));
            balance1.Add(new Amount(20, commodity1));

            Balance balance2 = new Balance();
            balance2.Add(new Amount(10, commodity1));
            balance2.Add(new Amount(20, commodity1));

            Assert.True(balance1.Equals(balance2));
        }

        [Fact]
        public void Balance_IsLessThan_ComparesAllAmounts()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(10, commodity1));
            balance.Add(new Amount(20, commodity2));

            Assert.True(balance.IsLessThan(new Amount(30)));
            Assert.False(balance.IsLessThan(new Amount(10)));
        }

        [Fact]
        public void Balance_IsGreaterThan_ComparesAllAmounts()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Balance balance = new Balance();
            balance.Add(new Amount(10, commodity1));
            balance.Add(new Amount(20, commodity2));

            Assert.False(balance.IsGreaterThan(new Amount(30)));
            Assert.True(balance.IsGreaterThan(new Amount(10)));
        }

        [Fact]
        public void Balance_Constructor_ClonesAmounts()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            Amount amount1 = new Amount(200, commodity1);
            Amount amount2 = new Amount(300, commodity2);

            Balance balance = new Balance();
            balance.Add(amount1);
            balance.Add(amount2);

            Balance clonedBalance = new Balance(balance);
            clonedBalance.Amounts.ElementAt(0).Value.Multiply(new Amount(5));
            clonedBalance.Amounts.ElementAt(1).Value.Multiply(new Amount(7));

            Assert.Equal(2, clonedBalance.Amounts.Count());
            Assert.Equal(1000, clonedBalance.Amounts.ElementAt(0).Value.Quantity.ToLong());
            Assert.Equal(2100, clonedBalance.Amounts.ElementAt(1).Value.Quantity.ToLong());

            Assert.Equal(2, balance.Amounts.Count());
            Assert.Equal(200, balance.Amounts.ElementAt(0).Value.Quantity.ToLong());
            Assert.Equal(300, balance.Amounts.ElementAt(1).Value.Quantity.ToLong());
        }

        [Fact]
        public void Balance_Add_Balance_AddsAllAmounts()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            // First balance

            Amount amount1 = new Amount(200, commodity1);
            Amount amount2 = new Amount(300, commodity2);

            Balance balance1 = new Balance();
            balance1.Add(amount1);
            balance1.Add(amount2);

            // Second balance

            Amount amount3 = new Amount(400, commodity1);
            Amount amount4 = new Amount(500, commodity2);

            Balance balance2 = new Balance();
            balance2.Add(amount3);
            balance2.Add(amount4);

            // Action

            balance1.Add(balance2);

            // Assert

            Assert.Equal(2, balance1.Amounts.Count());
            Assert.Equal(600, balance1.Amounts.ElementAt(0).Value.Quantity.ToLong());
            Assert.Equal(800, balance1.Amounts.ElementAt(1).Value.Quantity.ToLong());
        }

        [Fact]
        public void Balance_Subtract_Balance_SubtractsAllAmounts()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("base-1"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("base-2"));

            // First balance

            Amount amount1 = new Amount(200, commodity1);
            Amount amount2 = new Amount(300, commodity2);

            Balance balance1 = new Balance();
            balance1.Add(amount1);
            balance1.Add(amount2);

            // Second balance

            Amount amount3 = new Amount(50, commodity1);
            Amount amount4 = new Amount(70, commodity2);

            Balance balance2 = new Balance();
            balance2.Add(amount3);
            balance2.Add(amount4);

            // Action

            balance1.Subtract(balance2);

            // Assert

            Assert.Equal(2, balance1.Amounts.Count());
            Assert.Equal(150, balance1.Amounts.ElementAt(0).Value.Quantity.ToLong());
            Assert.Equal(230, balance1.Amounts.ElementAt(1).Value.Quantity.ToLong());
        }

        [Fact]
        public void Balance_Print_PerformsLeftJustificationInCaseOfNo_AMOUNT_PRINT_RIGHT_JUSTIFY()
        {
            Balance balance = new Balance();
            balance.Add((Amount)1);
            var result = balance.Print(10);
            Assert.Equal("1         ", result);
        }

        [Fact]
        public void Balance_Print_PerformsRightJustificationInCaseOf_AMOUNT_PRINT_RIGHT_JUSTIFY()
        {
            Balance balance = new Balance();
            balance.Add((Amount)1);
            var result = balance.Print(10, 10, AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY);
            Assert.Equal("         1", result);
        }

        [Fact]
        public void Balance_ToString_CallsPrint()
        {
            Commodity commodity1 = new Commodity(CommodityPool.Current, new CommodityBase("balToStrA"));
            Commodity commodity2 = new Commodity(CommodityPool.Current, new CommodityBase("balToStrB"));

            Balance balance = new Balance();
            balance.Add(new Amount(Quantity.Parse("1.22"), commodity1));
            balance.Add(new Amount(Quantity.Parse("2.44"), commodity2));

            Assert.Equal("balToStrA1  \nbalToStrB2", balance.ToString().RemoveCarriageReturns());
        }

        [Fact]
        public void Balance_IsNonZero_ReturnsFalseIfEmpty()
        {
            Balance balance = new Balance();
            Assert.True(balance.IsEmpty);
            Assert.False(balance.IsNonZero);
        }

        [Fact]
        public void Balance_IsNonZero_ReturnsFalseIfAllAmountsAreZero()
        {
            Commodity commodityA = new Commodity(CommodityPool.Current, new CommodityBase("balNZeroA"));
            Commodity commodityB = new Commodity(CommodityPool.Current, new CommodityBase("balNZeroB"));

            // Commodity precision is "0"; add values that are less than commodity precision
            Amount amountA = new Amount(Quantity.Parse("0.1"), commodityA);
            Amount amountB = new Amount(Quantity.Parse("0.1"), commodityB);

            Balance balance = new Balance();
            balance.Add(amountA);
            balance.Add(amountB);

            Assert.False(amountA.IsNonZero);
            Assert.False(amountB.IsNonZero);
            Assert.False(balance.IsEmpty);

            Assert.False(balance.IsNonZero);
        }

        [Fact]
        public void Balance_IsNonZero_ReturnsTrueIfOneOfAmountsIsNonZero()
        {
            Commodity commodityA = new Commodity(CommodityPool.Current, new CommodityBase("balNZeroA"));
            Commodity commodityB = new Commodity(CommodityPool.Current, new CommodityBase("balNZeroB"));

            // Commodity precision is "0"; add values that are less than commodity precision
            Amount amountA = new Amount(Quantity.Parse("0.1"), commodityA);
            Amount amountB = new Amount(Quantity.Parse("1.0"), commodityB);

            Balance balance = new Balance();
            balance.Add(amountA);
            balance.Add(amountB);

            Assert.False(amountA.IsNonZero);
            Assert.True(amountB.IsNonZero);  // Here you are
            Assert.False(balance.IsEmpty);

            Assert.True(balance.IsNonZero);  // Results in True
        }

        [Fact]
        public void Balance_Valid_ReturnsFalseIfAmountIsNotValid()
        {
            Balance balance = new Balance();
            Assert.True(balance.Valid());

            Amount amount = new Amount(10);
            var quantity = amount.Quantity.SetPrecision(2048);
            amount = new Amount(quantity, null);
            balance.Add(amount);

            Assert.False(amount.Valid());
            Assert.False(balance.Valid());
        }
    }
}

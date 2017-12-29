// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Amounts;
using NLedger.Utility;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests
{
    [TestClass]
    public class CompareItemsTests : TestFixture
    {
        [TestMethod]
        public void CompareItems_SortValueIsLessThan_IgnoresBalancesAndReturnsZero()
        {
            Balance bal1 = new Balance(new Amount(10));
            Balance bal2 = new Balance(new Amount(10));

            IList<Tuple<Value, bool>> list1 = new List<Tuple<Value, bool>>() { new Tuple<Value, bool>(Value.Get(bal1), false) };
            IList<Tuple<Value, bool>> list2 = new List<Tuple<Value, bool>>() { new Tuple<Value, bool>(Value.Get(bal2), false) };

            Assert.AreEqual(0, CompareItems<Post>.SortValueIsLessThan(list1, list2));
            Assert.AreEqual(0, CompareItems<Post>.SortValueIsLessThan(list2, list1));
        }

        [TestMethod]
        public void CompareItems_SortValueIsLessThan_ReturnsOneIfFirstLessThanSecondOrMinusOneOtherwise()
        {
            Date date1 = new Date(2010, 10, 10);
            Date date2 = new Date(2010, 10, 15);

            IList<Tuple<Value, bool>> list1 = new List<Tuple<Value, bool>>() { new Tuple<Value, bool>(Value.Get(date1), false) };
            IList<Tuple<Value, bool>> list2 = new List<Tuple<Value, bool>>() { new Tuple<Value, bool>(Value.Get(date2), false) };

            Assert.AreEqual(1, CompareItems<Post>.SortValueIsLessThan(list1, list2));
            Assert.AreEqual(-1, CompareItems<Post>.SortValueIsLessThan(list2, list1));
        }

        [TestMethod]
        public void CompareItems_SortValueIsLessThan_ReturnsMinusOneIfFirstLessThanSecondOrOneOtherwiseWhenInverted()
        {
            Date date1 = new Date(2010, 10, 10);
            Date date2 = new Date(2010, 10, 15);

            IList<Tuple<Value, bool>> list1 = new List<Tuple<Value, bool>>() { new Tuple<Value, bool>(Value.Get(date1), true) };
            IList<Tuple<Value, bool>> list2 = new List<Tuple<Value, bool>>() { new Tuple<Value, bool>(Value.Get(date2), true) };

            Assert.AreEqual(-1, CompareItems<Post>.SortValueIsLessThan(list1, list2));
            Assert.AreEqual(1, CompareItems<Post>.SortValueIsLessThan(list2, list1));
        }

        [TestMethod]
        public void CompareItems_SortValueIsLessThan_ReturnsZeroIfValuesAreEqual()
        {
            Date date1 = new Date(2010, 10, 10);
            Date date2 = new Date(2010, 10, 10);

            IList<Tuple<Value, bool>> list1 = new List<Tuple<Value, bool>>() { new Tuple<Value, bool>(Value.Get(date1), false) };
            IList<Tuple<Value, bool>> list2 = new List<Tuple<Value, bool>>() { new Tuple<Value, bool>(Value.Get(date2), false) };

            Assert.AreEqual(0, CompareItems<Post>.SortValueIsLessThan(list1, list2));
            Assert.AreEqual(0, CompareItems<Post>.SortValueIsLessThan(list2, list1));
        }

    }
}

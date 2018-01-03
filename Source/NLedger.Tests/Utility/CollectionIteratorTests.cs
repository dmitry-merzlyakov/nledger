// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utility
{
    [TestClass]
    public class CollectionIteratorTests
    {
        [TestMethod]
        public void CollectionIteratorFactory_GetIterator_ReturnsIterator()
        {
            var iter = ListOfTenIntegers.GetIterator();
            Assert.AreEqual(1, iter.Current);
            Assert.IsFalse(iter.IsEnd);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CollectionIterator_Constructor_DoesNotAllowNulls()
        {
            new CollectionIterator<int>(null);
        }

        [TestMethod]
        public void CollectionIterator_Constructor_SetsToFirstPosition()
        {
            CollectionIterator<int> iterator = new CollectionIterator<int>(ListOfTenIntegers);
            Assert.AreEqual(1, iterator.Current);
            Assert.IsFalse(iterator.IsEnd);
        }

        [TestMethod]
        public void CollectionIterator_Current_ReturnsCurrentItem()
        {
            CollectionIterator<int> iterator = new CollectionIterator<int>(ListOfTenIntegers);
            for (int i = 1; i <= 10; i++)
            {
                Assert.AreEqual(i, iterator.Current);
                iterator.MoveNext();
            }
        }

        [TestMethod]
        public void CollectionIterator_Current_BecomesNullAtTheEnd()
        {
            CollectionIterator<int> iterator = new CollectionIterator<int>(ListOfTenIntegers);
            for (int i = 1; i <= 10; i++)
                iterator.MoveNext();
            Assert.AreEqual(default(int), iterator.Current);
        }

        [TestMethod]
        public void CollectionIterator_IsEnd_IndicatesThatIteratorReachesTheEndOfSequence()
        {
            CollectionIterator<int> iterator = new CollectionIterator<int>(ListOfTenIntegers);
            for (int i = 1; i <= 10; i++)
            {
                Assert.IsFalse(iterator.IsEnd);
                iterator.MoveNext();
            }
            Assert.IsTrue(iterator.IsEnd);
        }

        [TestMethod]
        public void CollectionIterator_MoveNext_MovesCurrentPositionToNextItem()
        {
            CollectionIterator<int> iterator = new CollectionIterator<int>(ListOfTenIntegers);
            for (int i = 1; i <= 10; i++)
            {
                Assert.AreEqual(i, iterator.Current);
                Assert.IsFalse(iterator.IsEnd);
                iterator.MoveNext();
            }
            Assert.AreEqual(default(int), iterator.Current);
            Assert.IsTrue(iterator.IsEnd);
        }

        private static IList<int> ListOfTenIntegers = new List<int>(new int[]{1,2,3,4,5,6,7,8,9,10});
    }
}

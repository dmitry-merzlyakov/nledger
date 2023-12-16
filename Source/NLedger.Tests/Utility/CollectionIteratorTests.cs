// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility
{
    public class CollectionIteratorTests
    {
        [Fact]
        public void CollectionIteratorFactory_GetIterator_ReturnsIterator()
        {
            var iter = ListOfTenIntegers.GetIterator();
            Assert.Equal(1, iter.Current);
            Assert.False(iter.IsEnd);
        }

        [Fact]
        public void CollectionIterator_Constructor_DoesNotAllowNulls()
        {
            Assert.Throws<ArgumentNullException>(() => new CollectionIterator<int>(null));
        }

        [Fact]
        public void CollectionIterator_Constructor_SetsToFirstPosition()
        {
            CollectionIterator<int> iterator = new CollectionIterator<int>(ListOfTenIntegers);
            Assert.Equal(1, iterator.Current);
            Assert.False(iterator.IsEnd);
        }

        [Fact]
        public void CollectionIterator_Current_ReturnsCurrentItem()
        {
            CollectionIterator<int> iterator = new CollectionIterator<int>(ListOfTenIntegers);
            for (int i = 1; i <= 10; i++)
            {
                Assert.Equal(i, iterator.Current);
                iterator.MoveNext();
            }
        }

        [Fact]
        public void CollectionIterator_Current_BecomesNullAtTheEnd()
        {
            CollectionIterator<int> iterator = new CollectionIterator<int>(ListOfTenIntegers);
            for (int i = 1; i <= 10; i++)
                iterator.MoveNext();
            Assert.Equal(default(int), iterator.Current);
        }

        [Fact]
        public void CollectionIterator_IsEnd_IndicatesThatIteratorReachesTheEndOfSequence()
        {
            CollectionIterator<int> iterator = new CollectionIterator<int>(ListOfTenIntegers);
            for (int i = 1; i <= 10; i++)
            {
                Assert.False(iterator.IsEnd);
                iterator.MoveNext();
            }
            Assert.True(iterator.IsEnd);
        }

        [Fact]
        public void CollectionIterator_MoveNext_MovesCurrentPositionToNextItem()
        {
            CollectionIterator<int> iterator = new CollectionIterator<int>(ListOfTenIntegers);
            for (int i = 1; i <= 10; i++)
            {
                Assert.Equal(i, iterator.Current);
                Assert.False(iterator.IsEnd);
                iterator.MoveNext();
            }
            Assert.Equal(default(int), iterator.Current);
            Assert.True(iterator.IsEnd);
        }

        private static IList<int> ListOfTenIntegers = new List<int>(new int[]{1,2,3,4,5,6,7,8,9,10});
    }
}

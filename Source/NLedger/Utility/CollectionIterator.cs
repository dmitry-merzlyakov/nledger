// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public static class CollectionIteratorFactory
    {
        public static CollectionIterator<T> GetIterator<T>(this ICollection<T> list)
        {
            return new CollectionIterator<T>(list);
        }
    }

    public class CollectionIterator<T> : IDisposable
    {
        public CollectionIterator(ICollection<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("enumerable");

            Enumerator = list.GetEnumerator();
            IsEnd = !Enumerator.MoveNext();
        }

        public T Current
        {
            get { return Enumerator.Current; }
        }

        public bool IsEnd { get; private set; }

        public bool MoveNext()
        {
            if (!IsEnd)
                IsEnd = !Enumerator.MoveNext();

            return !IsEnd;
        }

        public void Dispose()
        {
            Enumerator.Dispose();
        }

        private readonly IEnumerator<T> Enumerator;
    }
}

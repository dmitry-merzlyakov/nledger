// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public interface IMultiMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, ISet<TValue>>>
    {
        void Add(TKey key, TValue value);
        bool ContainsValue(TKey key, TValue value);
        void Remove(TKey key, TValue value);
        void Merge(IMultiMap<TKey, TValue> toMergeWith);
        ISet<TValue> GetValues(TKey key, bool returnEmptySet = true);
    }
}

// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public sealed class MultiMap<TKey, TValue> : Dictionary<TKey, ISet<TValue>>, IMultiMap<TKey, TValue>
    {
        public void Add(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            ISet<TValue> container = null;
            if (!this.TryGetValue(key, out container))
            {
                container = new HashSet<TValue>();
                base.Add(key, container);
            }
            container.Add(value);
        }

        public bool ContainsValue(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            bool contains = false;
            ISet<TValue> values = null;
            if (this.TryGetValue(key, out values))
                contains = values.Contains(value);

            return contains;
        }

        public void Remove(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            ISet<TValue> container = null;
            if (this.TryGetValue(key, out container))
            {
                container.Remove(value);
                if (container.Count <= 0)
                    this.Remove(key);
            }
        }

        public void Merge(IMultiMap<TKey, TValue> toMergeWith)
        {
            if (toMergeWith == null)
                return;

            foreach (KeyValuePair<TKey, ISet<TValue>> pair in toMergeWith)
            {
                foreach (TValue value in pair.Value)
                    this.Add(pair.Key, value);
            }
        }

        public ISet<TValue> GetValues(TKey key, bool returnEmptySet = true)
        {
            ISet<TValue> values = null;
            if (!base.TryGetValue(key, out values) && returnEmptySet)
                values = Empty;

            return values;
        }

        private readonly ISet<TValue> Empty = new HashSet<TValue>();
    }
}

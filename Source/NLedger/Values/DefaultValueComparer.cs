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

namespace NLedger.Values
{
    /// <summary>
    /// [DM] Default comparer for value objects that is used for sorted dictionaries
    /// to provide correct equality comparison and ordering
    /// </summary>
    public sealed class DefaultValueComparer : IComparer<Value>
    {
        public static readonly DefaultValueComparer Instance = new DefaultValueComparer();

        public int Compare(Value x, Value y)
        {
            x = x ?? Value.Empty;
            y = y ?? Value.Empty;

            if (x.IsLessThan(y))
                return -1;
            else if (x.IsGreaterThan(y))
                return 1;
            else
                return 0;
        }
    }
}

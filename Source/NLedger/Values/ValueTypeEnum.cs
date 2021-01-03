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
    public enum ValueTypeEnum
    {
        Void,                       // a null value (i.e., uninitialized)
        Boolean,                    // a boolean
        Date,                       // a date
        DateTime,                   // a date and time
        Integer,                    // a signed integer value
        Amount,                     // a ledger::amount_t
        Balance,                    // a ledger::balance_t
        String,                     // a string object
        Mask,                       // a regular expression mask
        Sequence,                   // a vector of value_t objects
        Scope,                      // a pointer to a scope
        Any                         // a pointer to an arbitrary object
    }
}

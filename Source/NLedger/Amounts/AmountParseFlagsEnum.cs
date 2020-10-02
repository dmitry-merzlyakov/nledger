// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Amounts
{
    /// <summary>
    /// Ported from: parse_flags_enum_t (amount.h)
    /// </summary>
    [Flags]
    public enum AmountParseFlagsEnum
    {
        PARSE_DEFAULT = 0x00,
        PARSE_PARTIAL = 0x01,
        PARSE_SINGLE = 0x02,
        PARSE_NO_MIGRATE = 0x04,
        PARSE_NO_REDUCE = 0x08,
        PARSE_NO_ASSIGN = 0x10,
        PARSE_NO_ANNOT = 0x20,
        PARSE_OP_CONTEXT = 0x40,
        PARSE_SOFT_FAIL = 0x80
    }
}

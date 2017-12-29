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

namespace NLedger
{
    [Flags]
    public enum SupportsFlagsEnum
    {
        ITEM_NORMAL = 0x00,             // no flags at all, a basic posting
        ITEM_GENERATED = 0x01,          // posting was not found in a journal
        ITEM_TEMP = 0x02,               // posting is a managed temporary
        ITEM_NOTE_ON_NEXT_LINE = 0x04,   // did we see a note on the next line?

        // TODO - REFACTOR!!!
        POST_VIRTUAL =         0x0010, // the account was specified with (parens)
        POST_MUST_BALANCE =    0x0020, // posting must balance in the transaction
        POST_CALCULATED =      0x0040, // posting's amount was calculated
        POST_COST_CALCULATED = 0x0080, // posting's cost was calculated
        POST_COST_IN_FULL =    0x0100, // cost specified using @@
        POST_COST_FIXATED =    0x0200, // cost is fixed using = indicator
        POST_COST_VIRTUAL =    0x0400, // cost is virtualized: (@)
        POST_ANONYMIZED =      0x0800, // a temporary, anonymous posting
        POST_DEFERRED =        0x1000  // the account was specified with <angles>

    }
}

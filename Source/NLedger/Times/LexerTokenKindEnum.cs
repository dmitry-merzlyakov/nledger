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

namespace NLedger.Times
{
    public enum LexerTokenKindEnum
    {
        UNKNOWN,

        TOK_DATE,
        TOK_INT,
        TOK_SLASH,
        TOK_DASH,
        TOK_DOT,

        TOK_A_MONTH,
        TOK_A_WDAY,

        TOK_AGO,
        TOK_HENCE,
        TOK_SINCE,
        TOK_UNTIL,
        TOK_IN,
        TOK_THIS,
        TOK_NEXT,
        TOK_LAST,
        TOK_EVERY,

        TOK_TODAY,
        TOK_TOMORROW,
        TOK_YESTERDAY,

        TOK_YEAR,
        TOK_QUARTER,
        TOK_MONTH,
        TOK_WEEK,
        TOK_DAY,

        TOK_YEARLY,
        TOK_QUARTERLY,
        TOK_BIMONTHLY,
        TOK_MONTHLY,
        TOK_BIWEEKLY,
        TOK_WEEKLY,
        TOK_DAILY,

        TOK_YEARS,
        TOK_QUARTERS,
        TOK_MONTHS,
        TOK_WEEKS,
        TOK_DAYS,

        END_REACHED
    }
}

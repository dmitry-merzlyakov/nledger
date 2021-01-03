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

namespace NLedger.Scopus
{
    [Flags]
    public enum ReportBudgetFlags
    {
        BUDGET_NO_BUDGET   = 0x00,
        BUDGET_BUDGETED    = 0x01,
        BUDGET_UNBUDGETED  = 0x02,
        BUDGET_WRAP_VALUES = 0x04
    }
}

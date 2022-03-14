// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Times;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Xacts
{
    /// <summary>
    /// Ported from period_xact_t (xact.h)
    /// </summary>
    public class PeriodXact : XactBase
    {
        public const string GeneratedPeriodicTransactionKey = "generated periodic transaction";

        public PeriodXact()
        { }

        public PeriodXact(string period)
        {
            Period = new DateInterval(period);
            PeriodSting = period;
        }

        public DateInterval Period { get; private set; }
        public string PeriodSting { get; private set; }

        public override string Description
        {
            get { return HasPos ? String.Format("transaction at line {0}", Pos.BegLine) : GeneratedPeriodicTransactionKey; }
        }
    }
}

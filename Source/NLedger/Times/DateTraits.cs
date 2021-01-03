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
    public struct DateTraits
    {
        public DateTraits(bool hasYear = false, bool hasMonth = false, bool hasDay = false) : this()
        {
            HasYear = hasYear;
            HasMonth = hasMonth;
            HasDay = hasDay;
        }

        public bool HasYear { get; private set; }
        public bool HasMonth { get; private set; }
        public bool HasDay { get; private set; }
    }
}

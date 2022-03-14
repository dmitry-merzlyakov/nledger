// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Drafts
{
    // draft_t::xact_template_t::post_template_t
    public class DraftXactPostTemplate
    {
        public bool From { get; set; }
        public Mask AccountMask { get; set; }
        public Amount Amount { get; set; }
        public string CostOperator { get; set; }
        public Amount Cost { get; set; }
    }
}

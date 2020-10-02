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

namespace NLedger.Utils
{
    /// <summary>
    /// Ported from enum log_level_t
    /// </summary>
    public enum LogLevelEnum
    {
        LOG_OFF = 0,
        LOG_CRIT,
        LOG_FATAL,
        LOG_ASSERT,
        LOG_ERROR,
        LOG_VERIFY,
        LOG_WARN,
        LOG_INFO,
        LOG_EXCEPT,
        LOG_DEBUG,
        LOG_TRACE,
        LOG_ALL
    }
}

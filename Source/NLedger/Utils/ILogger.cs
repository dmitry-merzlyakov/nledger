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

namespace NLedger.Utils
{
    /// <summary>
    /// The interface that represent all logging functions used in Ledger code.
    /// </summary>
    public interface ILogger
    {
        LogLevelEnum LogLevel { get; set; }
        string LogCategory { get; set; }
        int TraceLevel { get; set; }

        bool ShowDebug(string cat);
        bool ShowInfo();

        void Debug(string cat, Func<string> msg);
        void Info(Func<string> msg);

        /// <summary>
        /// Returns timing context for TRACE_START/TRACE_STOP/TRACE_FINISH
        /// Might by NULL if TRACE is not enabled
        /// </summary>
        ITimerContext TraceContext(string name, int lvl);

        /// <summary>
        /// Returns timing context for INFO_START/INFO_STOP/INFO_FINISH
        /// Might by NULL if INFO is not enabled
        /// </summary>
        ITimerContext InfoContext(string name);
    }
}

// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
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
    /// Ported from enum caught_signal_t
    /// </summary>
    public enum CaughtSignalEnum
    {
        NONE_CAUGHT,
        INTERRUPTED,
        PIPE_CLOSED
    }

    public static class CancellationManager
    {
        public static bool IsCancellationRequested
        {
            get { return CaughtSignal != CaughtSignalEnum.NONE_CAUGHT; }
        }

        /// <summary>
        /// Ported from extern caught_signal_t caught_signal
        /// </summary>
        public static CaughtSignalEnum CaughtSignal
        {
            get { return MainApplicationContext.Current.CancellationSignal; }
        }

        /// <summary>
        /// Ported from inline void check_for_signal()
        /// </summary>
        public static void CheckForSignal()
        {
            switch (CaughtSignal)
            {
                case CaughtSignalEnum.NONE_CAUGHT:
                    break;
                case CaughtSignalEnum.INTERRUPTED:
                    throw new RuntimeError("Interrupted by user (use Control-D to quit)");
                case CaughtSignalEnum.PIPE_CLOSED:
                    throw new RuntimeError("Pipe terminated");
                default:
                    throw new InvalidOperationException(String.Format("Unknown signal: {0}", CaughtSignal));
            }
        }

        public static void DiscardCancellationRequest()
        {
            MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.NONE_CAUGHT;
        }
    }
}

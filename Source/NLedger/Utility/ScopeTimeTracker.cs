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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public class ScopeTimeTracker : IDisposable
    {
        public ScopeTimeTracker(Action<TimeSpan> setResult)
        {
            if (setResult == null)
                throw new ArgumentNullException(nameof(setResult));

            SetResult = setResult;
            Stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (Stopwatch.IsRunning)
            {
                Stopwatch.Stop();
                SetResult(Stopwatch.Elapsed);
            }
        }

        private readonly Action<TimeSpan> SetResult;
        private readonly Stopwatch Stopwatch;        
    }
}

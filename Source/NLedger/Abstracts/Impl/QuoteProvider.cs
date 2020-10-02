// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Abstracts.Impl
{
    /// <summary>
    /// Default implementation of a quote provider that executes a script
    /// in the same manner as Ledger does it.
    /// </summary>
    public sealed class QuoteProvider : IQuoteProvider
    {
        public bool Get(string command, out string response)
        {
            if (String.IsNullOrWhiteSpace(command))
                throw new ArgumentNullException("command");

            var getQuotePath = VirtualEnvironment.GetEnvironmentVariable("GETQUOTEPATH");
            return MainApplicationContext.Current.ApplicationServiceProvider.ProcessManager.ExecuteShellCommand(command, getQuotePath, out response) == 0;
        }
    }
}

// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NLedger.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            // System.Diagnostics.Debugger.Launch(); // This debugging option might be useful in case of troubleshooting of NLTest issues

            var context = new MainApplicationContext();
            new NLedgerConfiguration().ConfigureConsole(context);

            var main = new Main(context);

            var argString = CommandLineArgs.GetArguments(args); // This way is preferrable because of double quotas that are missed by using args
            Environment.ExitCode = main.Execute(argString);
        }
    }
}

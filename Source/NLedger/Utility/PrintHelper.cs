// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    // Helper class that is intended to validate .Net Extension capabilities
    public static class PrintHelper
    {
        public static void Print(string arg)
        {
            MainApplicationContext.Current?.ApplicationServiceProvider.VirtualConsoleProvider.ConsoleOutput.WriteLine(arg);
        }

        public static void Print(string arg0, string arg1)
        {
            MainApplicationContext.Current?.ApplicationServiceProvider.VirtualConsoleProvider.ConsoleOutput.WriteLine($"{arg0} {arg1}");
        }
    }
}

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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Abstracts.Impl
{
    /// <summary>
    /// Default implementation of Virtual Console provider
    /// </summary>
    public sealed class VirtualConsoleProvider : IVirtualConsoleProvider
    {
        public VirtualConsoleProvider(TextReader consoleInput = null, TextWriter consoleOutput = null, TextWriter consoleError = null)
        {
            ConsoleInput = consoleInput ?? Console.In;
            ConsoleOutput = consoleOutput ?? Console.Out;
            ConsoleError = consoleError ?? Console.Error;
        }

        public TextWriter ConsoleError { get; private set; }
        public TextReader ConsoleInput { get; private set; }
        public TextWriter ConsoleOutput { get; private set; }

        public int WindowWidth
        {
            get { return Console.WindowWidth; }
        }

        public void AddHistory(string readLineName, string str)
        {
            // #readline-library - add_history
        }

        public int HistoryExpand(string readLineName, string str, ref string output)
        {
            // #readline-library - history_expand
            return 0; // "If no expansions took place"
        }
    }
}

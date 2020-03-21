// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests
{
    public sealed class TestConsoleProvider : IVirtualConsoleProvider
    {
        public TestConsoleProvider(TextReader consoleInput = null, TextWriter consoleOutput = null, TextWriter consoleError = null)
        {
            ConsoleInput = consoleInput;
            ConsoleOutput = consoleOutput;
            ConsoleError = consoleError;
        }

        public TextWriter ConsoleError { get; private set; }
        public TextReader ConsoleInput { get; private set; }
        public TextWriter ConsoleOutput { get; private set; }
        public int WindowWidth
        {
            get { return 0; }
        }
        public void AddHistory(string readLineName, string str)
        { }
        public int HistoryExpand(string readLineName, string str, ref string output)
        {
            return 0;
        }
    }
}

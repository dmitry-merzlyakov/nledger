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

namespace NLedger.IntegrationTests
{
    public sealed class TestCase
    {
        public TestCase (string fileName, int startLine, int endLine, string commandLine, string expectedOutput, string expectedError)
        {
            if (String.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");
            if (String.IsNullOrWhiteSpace(commandLine))
                throw new ArgumentNullException("commandLine");
            expectedOutput = expectedOutput ?? String.Empty;

            if (startLine < 0)
                throw new ArgumentException("startLine");
            if (endLine < 0)
                throw new ArgumentException("endLine");
            if (endLine < startLine)
                throw new ArgumentException("endLine less than startLine");

            FileName = fileName;
            StartLine = startLine;
            EndLine = endLine;
            CommandLine = commandLine;
            ExpectedOutput = expectedOutput;
            ExpectedError = expectedError;
        }

        public string FileName { get; private set; }

        public int StartLine { get; private set; }
        public int EndLine { get; private set; }

        public string CommandLine { get; private set; }
        public string ExpectedOutput { get; private set; }
        public string ExpectedError { get; private set; }
    }
}

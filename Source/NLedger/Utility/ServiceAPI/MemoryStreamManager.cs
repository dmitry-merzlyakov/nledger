// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.ServiceAPI
{
    public sealed class MemoryStreamManager : IDisposable
    {
        public MemoryStreamManager(string inputText = null)
        {
            ConsoleInput = new StringReader(inputText ?? String.Empty);
            ConsoleOutput = new StringWriter();
            ConsoleError = new StringWriter();
        }

        public TextWriter ConsoleError { get; private set; }
        public TextReader ConsoleInput { get; private set; }
        public TextWriter ConsoleOutput { get; private set; }

        public void Attach(Func<TextWriter, BaseAnsiTextWriter> ansiTextWriterFactory)
        {
            if (ansiTextWriterFactory == null)
                throw new ArgumentNullException(nameof(ansiTextWriterFactory));

            ConsoleOutput = ansiTextWriterFactory(ConsoleOutput);
            ConsoleError = ansiTextWriterFactory(ConsoleError);
        }

        public string GetOutputText()
        {
            return ConsoleOutput.ToString();
        }

        public string GetErrorText()
        {
            return ConsoleError.ToString();
        }

        public void Dispose()
        {
            ConsoleInput.Dispose();
            ConsoleOutput.Dispose();
            ConsoleError.Dispose();
        }
    }
}

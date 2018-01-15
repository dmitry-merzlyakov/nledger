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

namespace NLedger.Abstracts
{
    /// <summary>
    /// A system service that is responsible to run an external process and 
    /// provides input/output communications with it
    /// </summary>
    public interface IProcessManager
    {
        /// <summary>
        /// Runs an executable file that does not require an input but possibly generates some output
        /// </summary>
        /// <param name="fileName">The name (and optional path) to an executable file</param>
        /// <param name="arguments">Optional collection of arguments</param>
        /// <param name="workingDirectory">Optional path to working directory</param>
        /// <param name="output">The output that the running file generates</param>
        /// <returns>Exit code that returns the file</returns>
        int Execute(string fileName, string arguments, string workingDirectory, out string output);
    }
}

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
        /// <returns>Exit code that the process returns</returns>
        int Execute(string fileName, string arguments, string workingDirectory, out string output);

        /// <summary>
        /// Runs an executable file that requires an input but does not generate any output
        /// </summary>
        /// <param name="fileName">The name (and optional path) to an executable file</param>
        /// <param name="arguments">Optional collection of arguments</param>
        /// <param name="workingDirectory">Optional path to working directory</param>
        /// <param name="input">The input that will be send to the process</param>
        /// <returns>Exit code that the process returns</returns>
        int Execute(string fileName, string arguments, string workingDirectory, string input, bool noTimeout = false);

        /// <summary>
        /// Executes a shell command with arguments
        /// </summary>
        /// <param name="command">Shell command with arguments.</param>
        /// <param name="workingDirectory">Optional path to working directory</param>
        /// <param name="output">The output that the command generates</param>
        /// <returns>Exit code that the process returns</returns>
        int ExecuteShellCommand(string command, string workingDirectory, out string output);

        /// <summary>
        /// Starts a process that is associated with a particular file type. 
        /// E.g. opens a default browser for an HTML file.
        /// </summary>
        /// <remarks>
        /// In case of any error while starting a process, it returns False result and puts an error message to stderr.
        /// </remarks>
        /// <param name="fileName">Path to an existing file that has a proper file type association (Open With...)</param>
        /// <param name="arguments">Optional arguments.</param>
        /// <returns>True if the process is started successfully or False otherwise.</returns>
        bool Start(string fileName, string arguments = null);
    }
}

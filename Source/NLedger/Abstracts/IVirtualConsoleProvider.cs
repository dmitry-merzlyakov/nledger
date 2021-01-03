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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Abstracts
{
    /// <summary>
    /// A provider that virtualizes a console for the current thread
    /// </summary>
    public interface IVirtualConsoleProvider
    {
        /// <summary>
        /// Standard input stream
        /// </summary>
        TextReader ConsoleInput { get; }

        /// <summary>
        /// Standard output stream
        /// </summary>
        TextWriter ConsoleOutput { get; }

        /// <summary>
        /// Standard error stream
        /// </summary>
        TextWriter ConsoleError { get; }

        /// <summary>
        /// Console window width. Zero if not available.
        /// </summary>
        int WindowWidth { get; }

        /// <summary>
        /// Adds to console readline history
        /// </summary>
        /// <remarks>
        /// Reflects Readline library functions (in case the console supports them)
        /// See https://tiswww.case.edu/php/chet/readline/history.html
        /// </remarks>
        /// <param name="readLineName">Optional key that filters history</param>
        /// <param name="str">Text that needs to be added to history</param>
        void AddHistory(string readLineName, string str);

        /// <summary>
        /// Expands console readline history
        /// </summary>
        /// <remarks>
        /// Reflects Readline library functions (in case the console supports them)
        /// See https://tiswww.case.edu/php/chet/readline/history.html
        /// </remarks>
        /// <param name="readLineName">Optional key that filters history</param>
        /// <param name="str">Text to expact</param>
        /// <param name="output">Result</param>
        /// <returns>
        /// Note: return Zero if history is not available
        /// 0 - If no expansions took place(or, if the only change in the text was the removal of escape characters preceding the history expansion character);
        /// 1 - if expansions did take place; 
        /// -1 - if there was an error in expansion; 
        /// 2 - if the returned line should be displayed, but not executed, as with the :p modifier
        /// </returns>
        int HistoryExpand(string readLineName, string str, ref string output);
    }
}

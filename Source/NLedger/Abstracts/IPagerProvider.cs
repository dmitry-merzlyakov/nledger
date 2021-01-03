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
    /// This interface virtualizes access to a pager
    /// </summary>
    public interface IPagerProvider
    {
        /// <summary>
        /// Returns an output stream that will be routed to a pager.
        /// </summary>
        /// <remarks>
        /// The output stream will be closed by NLedger when it finishes sending data.
        /// </remarks>
        /// <param name="pagerPath">Path to a pager</param>
        /// <returns>TextWriter that holds the output for the pager.</returns>
        TextWriter GetPager(string pagerPath);

        /// <summary>
        /// Returns a default pager path
        /// </summary>
        /// <returns>Pager path if it is specified or an empty string otherwise</returns>
        string GetDefaultPagerPath();
    }
}

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
    /// The service provider that returns stock quotes
    /// </summary>
    public interface IQuoteProvider
    {
        /// <summary>
        /// Returns a stock quote for a specified commodity
        /// </summary>
        /// <param name="command">A commant that requests a quote in Ledger format (getquote "[commodity]" "[optional exchange commodity]")</param>
        /// <param name="response">The requested quote in Price Directive format ([date] [commodity] [price])</param>
        /// <returns>True if request is finished successfully or False otherwise</returns>
        bool Get(string command, out string response);
    }
}

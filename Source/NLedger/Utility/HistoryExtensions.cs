// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public class HistoryExtensions
    {
        public static int HistoryExpand(string str, ref string output)
        {
            // see http://www.delorie.com/gnu/docs/readline/rlman_28.html
            // See https://cnswww.cns.cwru.edu/php/chet/readline/history.html
            // See http://stackoverflow.com/questions/2024170/is-there-a-net-library-similar-to-gnu-readline
            return 0; // TODO
        }

        public static void AddHistory(string str)
        {
            // TODO
        }
    }
}

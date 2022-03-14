// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public static class VirtualPager
    {
        public static IPagerProvider PagerProvider
        {
            get { return MainApplicationContext.Current.ApplicationServiceProvider.PagerProvider; }
        }

        public static TextWriter GetPager(string pagerPath)
        {
            return PagerProvider.GetPager(pagerPath);
        }

        public static string GetDefaultPagerPath()
        {
            return PagerProvider.GetDefaultPagerPath();
        }
    }
}

// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Commodities;
using NLedger.Formatting;
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    public sealed class MainApplicationContext
    {
        public static MainApplicationContext Current => CurrentInstance;

        public static void Initialize()
        {
            CurrentInstance = new MainApplicationContext();
        }

        public static void Cleanup()
        {
            CurrentInstance = null;
        }

        // For GlobalScope
        public bool ArgsOnly { get; set; }
        public string InitFile { get; set; }

        // For CommodityPool
        public CommodityPool CommodityPool
        {
            get { return _CommodityPool ?? (_CommodityPool = new CommodityPool()); }
            set { _CommodityPool = value; }
        }

        // For FileSystem
        public bool IsAtty { get; set; } = true;

        // For Times
        public TimesCommon TimesCommon { get; set; } = new TimesCommon();

        // For Scope
        public Scope DefaultScope { get; set; }
        public Scope EmptyScope { get; set; }

        // For Logger
        public Logger Logger { get; set; }

        // For Format
        public FormatElisionStyleEnum DefaultStyle { get; set; }
        public bool DefaultStyleChanged { get; set; }

        // For datetime extensions
        public string TimeZoneId { get; set; }
        public TimeZoneInfo TimeZone { get; set; }

        [ThreadStatic]
        private static MainApplicationContext CurrentInstance;

        private CommodityPool _CommodityPool;
    }
}

// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts;
using NLedger.Abstracts.Impl;
using NLedger.Commodities;
using NLedger.Formatting;
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Utils;
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

        // For Logger & Validator
        public ILogger Logger { get; set; } = new Logger();
        public bool IsVerifyEnabled { get; set; }

        // For Format
        public FormatElisionStyleEnum DefaultStyle { get; set; }
        public bool DefaultStyleChanged { get; set; }

        // For datetime extensions
        public string TimeZoneId { get; set; }
        public TimeZoneInfo TimeZone { get; set; }

        // Abstract Application Services
        public IQuoteProvider QuoteProvider => _QuoteProvider.Value;
        public IProcessManager ProcessManager => _ProcessManager.Value;
        public IManPageProvider ManPageProvider => _ManPageProvider.Value;

        public void SetQuoteProvider(Func<IQuoteProvider> quoteProviderFactory)
        {
            _QuoteProvider = new Lazy<IQuoteProvider>(quoteProviderFactory);
        }

        [ThreadStatic]
        private static MainApplicationContext CurrentInstance;

        private CommodityPool _CommodityPool;
        private Lazy<IQuoteProvider> _QuoteProvider = new Lazy<IQuoteProvider>(() => new QuoteProvider());
        private Lazy<IProcessManager> _ProcessManager = new Lazy<IProcessManager>(() => new ProcessManager());
        private Lazy<IManPageProvider> _ManPageProvider = new Lazy<IManPageProvider>(() => new ManPageProvider());
    }
}

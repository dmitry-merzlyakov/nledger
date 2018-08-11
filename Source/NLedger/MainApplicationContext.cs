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
        public TimeZoneInfo TimeZone { get; set; }

        // For Error Context
        public ErrorContext ErrorContext { get; private set; } = new ErrorContext();

        // Cancellation Management
        private volatile CaughtSignalEnum _CancellationSignal;
        public CaughtSignalEnum CancellationSignal
        {
            get { return _CancellationSignal; }
            set { _CancellationSignal = value; }
        }

        // Default Pager Name
        public string DefaultPager { get; set; }

        // Environment Variables
        public IDictionary<string,string> EnvironmentVariables
        {
            get { return _EnvironmentVariables ?? Empty; }
        }

        public void SetEnvironmentVariables(IDictionary<string, string> variables)
        {
            _EnvironmentVariables = new Dictionary<string, string>(variables ?? Empty, StringComparer.InvariantCultureIgnoreCase);
        }

        // Abstract Application Services
        public IQuoteProvider QuoteProvider => _QuoteProvider.Value;
        public IProcessManager ProcessManager => _ProcessManager.Value;
        public IManPageProvider ManPageProvider => _ManPageProvider.Value;
        public IVirtualConsoleProvider VirtualConsoleProvider => _VirtualConsoleProvider.Value;
        public IFileSystemProvider FileSystemProvider => _FileSystemProvider.Value;
        public IPagerProvider PagerProvider => _PagerProvider.Value;

        public void SetQuoteProvider(Func<IQuoteProvider> quoteProviderFactory)
        {
            _QuoteProvider = new Lazy<IQuoteProvider>(quoteProviderFactory);
        }

        public void SetVirtualConsoleProvider(Func<IVirtualConsoleProvider> virtualConsoleProviderFactory)
        {
            _VirtualConsoleProvider = new Lazy<IVirtualConsoleProvider>(virtualConsoleProviderFactory);
        }

        [ThreadStatic]
        private static MainApplicationContext CurrentInstance;
        private static readonly IDictionary<string, string> Empty = new Dictionary<string, string>();

        private CommodityPool _CommodityPool;
        private Lazy<IQuoteProvider> _QuoteProvider = new Lazy<IQuoteProvider>(() => new QuoteProvider());
        private Lazy<IProcessManager> _ProcessManager = new Lazy<IProcessManager>(() => new ProcessManager());
        private Lazy<IManPageProvider> _ManPageProvider = new Lazy<IManPageProvider>(() => new ManPageProvider());
        private Lazy<IVirtualConsoleProvider> _VirtualConsoleProvider = new Lazy<IVirtualConsoleProvider>(() => new VirtualConsoleProvider());
        private Lazy<IFileSystemProvider> _FileSystemProvider = new Lazy<IFileSystemProvider>(() => new FileSystemProvider());
        private Lazy<IPagerProvider> _PagerProvider = new Lazy<IPagerProvider>(() => new PagerProvider());
        private IDictionary<string, string> _EnvironmentVariables;
    }
}

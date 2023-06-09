// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts;
using NLedger.Abstracts.Impl;
using NLedger.Commodities;
using NLedger.Extensibility;
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
    public interface IApplicationServiceProvider
    {
        IQuoteProvider QuoteProvider { get; }
        IProcessManager ProcessManager { get; }
        IManPageProvider ManPageProvider { get; }
        IVirtualConsoleProvider VirtualConsoleProvider { get; }
        IFileSystemProvider FileSystemProvider { get; }
        IPagerProvider PagerProvider { get; }
        IExtensionProvider ExtensionProvider { get; }
    }

    public class ApplicationServiceProvider : IApplicationServiceProvider
    {
        public ApplicationServiceProvider(Func<IQuoteProvider> quoteProviderFactory = null, Func<IProcessManager> processManagerFactory = null, 
            Func<IManPageProvider> manPageProviderFactory = null, Func<IVirtualConsoleProvider> virtualConsoleProviderFactory = null,
            Func<IFileSystemProvider> fileSystemProviderFactory = null, Func<IPagerProvider> pagerProviderFactory = null,
            Func<IExtensionProvider> extensionProviderFactory = null)
        {
            _QuoteProvider = new Lazy<IQuoteProvider>(quoteProviderFactory ?? (() => new QuoteProvider()));
            _ProcessManager = new Lazy<IProcessManager>(processManagerFactory ?? (() => new ProcessManager()));
            _ManPageProvider = new Lazy<IManPageProvider>(manPageProviderFactory ?? (() => new ManPageProvider()));
            _VirtualConsoleProvider = new Lazy<IVirtualConsoleProvider>(virtualConsoleProviderFactory ?? (() => new VirtualConsoleProvider()));
            _FileSystemProvider = new Lazy<IFileSystemProvider>(fileSystemProviderFactory ?? (() => new FileSystemProvider()));
            _PagerProvider = new Lazy<IPagerProvider>(pagerProviderFactory ?? (() => new PagerProvider()));
            _ExtensionProvider = new Lazy<IExtensionProvider>(extensionProviderFactory ?? EmptyExtensionProvider.CurrentFactory);
        }

        public IQuoteProvider QuoteProvider => _QuoteProvider.Value;
        public IProcessManager ProcessManager => _ProcessManager.Value;
        public IManPageProvider ManPageProvider => _ManPageProvider.Value;
        public IVirtualConsoleProvider VirtualConsoleProvider => _VirtualConsoleProvider.Value;
        public IFileSystemProvider FileSystemProvider => _FileSystemProvider.Value;
        public IPagerProvider PagerProvider => _PagerProvider.Value;
        public IExtensionProvider ExtensionProvider => _ExtensionProvider.Value;

        private readonly Lazy<IQuoteProvider> _QuoteProvider;
        private readonly Lazy<IProcessManager> _ProcessManager;
        private readonly Lazy<IManPageProvider> _ManPageProvider;
        private readonly Lazy<IVirtualConsoleProvider> _VirtualConsoleProvider;
        private readonly Lazy<IFileSystemProvider> _FileSystemProvider;
        private readonly Lazy<IPagerProvider> _PagerProvider;
        private readonly Lazy<IExtensionProvider> _ExtensionProvider;
    }


    public sealed class MainApplicationContext
    {
        public static MainApplicationContext Current => CurrentInstance;

        public MainApplicationContext(IApplicationServiceProvider applicationServiceProvider = null)
        {
            _ApplicationServiceProvider = applicationServiceProvider ?? new ApplicationServiceProvider();
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

        public Commodity.CommodityDefaults CommodityDefaults
        {
            get { return _CommodityDefaults ?? (_CommodityDefaults = new Commodity.CommodityDefaults()); }
            set { _CommodityDefaults = value; }
        }

        // For FileSystem
        public bool IsAtty { get; set; } = true;

        // For Times
        public TimesCommon TimesCommon { get; set; } = new TimesCommon();

        // For Scope
        public Scope DefaultScope { get; set; }
        public Scope EmptyScope { get; set; }

        // For Item
        public bool UseAuxDate { get; set; }

        // For Logger & Validator
        public ILogger Logger { get; set; } = new Logger();
        public bool IsVerifyEnabled { get; set; }

        // For Format
        public FormatElisionStyleEnum DefaultStyle { get; set; }
        public bool DefaultStyleChanged { get; set; }

        // For datetime extensions
        public TimeZoneInfo TimeZone { get; set; }

        // For Error Context
        public ErrorContext ErrorContext { get; set; } = new ErrorContext();

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
        public IDictionary<string, string> EnvironmentVariables
        {
            get { return _EnvironmentVariables ?? Empty; }
        }

        public void SetEnvironmentVariables(IDictionary<string, string> variables)
        {
            _EnvironmentVariables = new Dictionary<string, string>(variables ?? Empty, StringComparer.InvariantCultureIgnoreCase);
        }

        public ExtendedSession ExtendedSession { get; private set; }

        public ExtendedSession SetExtendedSession(ExtendedSession extendedSession)
        {
            return ExtendedSession = extendedSession;
        }

        // Abstract Application Services
        public IApplicationServiceProvider ApplicationServiceProvider => _ApplicationServiceProvider;

        // Primarily, for testing purposes only
        public void SetApplicationServiceProvider(IApplicationServiceProvider applicationServiceProvider)
        {
            if (applicationServiceProvider == null)
                throw new ArgumentNullException(nameof(applicationServiceProvider));

            _ApplicationServiceProvider = applicationServiceProvider;
        }

        // Request an exclusive access to the current thread
        public ThreadAcquirer AcquireCurrentThread()
        {
            return new ThreadAcquirer(this);
        }

        public MainApplicationContext Clone(IApplicationServiceProvider applicationServiceProvider = null)
        {
            var context = new MainApplicationContext(applicationServiceProvider ?? _ApplicationServiceProvider);
            context.ArgsOnly = ArgsOnly;
            context.InitFile = InitFile;
            context.CommodityPool = CommodityPool;
            context.CommodityDefaults = CommodityDefaults;
            context.IsAtty = IsAtty;
            context.TimesCommon = TimesCommon;
            context.DefaultScope = DefaultScope;
            context.EmptyScope = EmptyScope;
            context.Logger = Logger;
            context.IsVerifyEnabled = IsVerifyEnabled;
            context.DefaultStyle = DefaultStyle;
            context.DefaultStyleChanged = DefaultStyleChanged;
            context.TimeZone = TimeZone;
            context.ErrorContext = ErrorContext;
            context.CancellationSignal = CancellationSignal;
            context.DefaultPager = DefaultPager;
            context._EnvironmentVariables = _EnvironmentVariables;
            context.ExtendedSession = ExtendedSession;
            return context;
        }

        public class ThreadAcquirer : IDisposable
        {
            public ThreadAcquirer(MainApplicationContext context)
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));
                if (CurrentInstance != null)
                    throw new InvalidOperationException("Cannot acquire current thread because it has been already acquired");

                CurrentInstance = context;
            }

            public void Dispose()
            {
                CurrentInstance = null;
            }
        }

        [ThreadStatic]
        private static MainApplicationContext CurrentInstance;
        private static readonly IDictionary<string, string> Empty = new Dictionary<string, string>();

        private CommodityPool _CommodityPool;
        private Commodity.CommodityDefaults _CommodityDefaults;
        private IApplicationServiceProvider _ApplicationServiceProvider;
        private IDictionary<string, string> _EnvironmentVariables;
    }
}

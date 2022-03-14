// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts.Impl;
using NLedger.Utility.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NLedger.Utility.ServiceAPI
{
    /// <summary>
    /// Main Service API manager that creates NLedger API session instances
    /// </summary>
    public class ServiceEngine
    {
        /// <summary>
        /// Create a new instance of API manager.
        /// </summary>
        /// <param name="configureContext">Optional action that allows to configure a session's MainApplicationContext</param>
        /// <param name="createCustomProvider">Optional function that allows to customize service providers for a session</param>
        public ServiceEngine(Action<MainApplicationContext> configureContext = null, Func<MemoryStreamManager, IApplicationServiceProvider> createCustomProvider = null)
        {
            ConfigureContext = configureContext;
            CreateCustomProvider = createCustomProvider;
        }

        public Action<MainApplicationContext> ConfigureContext { get; }
        public Func<MemoryStreamManager, IApplicationServiceProvider> CreateCustomProvider { get; }

        /// <summary>
        /// Creates a session (that basically means reading Ledger journal)
        /// </summary>
        /// <param name="args">Command line arguments that specify session behavior (primarily, source location).</param>
        /// <param name="inputText">Optional source text (if source is specified as '-f /dev/stdin')</param>
        /// <returns></returns>
        public ServiceSession CreateSession(string args, string inputText = null)
        {
            return CreatingSession(args, inputText, CancellationToken.None);
        }

        public Task<ServiceSession> CreateSessionAsync(string args, string inputText = null, CancellationToken token = default(CancellationToken))
        {
            return Task.Run(() => CreatingSession(args, inputText, token));
        }

        public virtual MainApplicationContext CreateContext(MemoryStreamManager memoryStreamManager)
        {
            var context = new MainApplicationContext(CreateApplicationServiceProvider(memoryStreamManager));
            context.IsAtty = false;

            ConfigureContext?.Invoke(context);

            return context;
        }

        public virtual MainApplicationContext CloneContext(MainApplicationContext mainApplicationContext, MemoryStreamManager memoryStreamManager)
        {
            if (mainApplicationContext == null)
                throw new ArgumentNullException(nameof(mainApplicationContext));

            return mainApplicationContext.Clone(CreateApplicationServiceProvider(memoryStreamManager));
        }

        protected virtual IApplicationServiceProvider CreateApplicationServiceProvider(MemoryStreamManager memoryStreamManager)
        {
            if (memoryStreamManager == null)
                throw new ArgumentNullException(nameof(memoryStreamManager));

            var customProvider = CreateCustomProvider?.Invoke(memoryStreamManager);
            if (customProvider != null)
                return customProvider;

            return new ApplicationServiceProvider(virtualConsoleProviderFactory: () => new VirtualConsoleProvider(memoryStreamManager.ConsoleInput, memoryStreamManager.ConsoleOutput, memoryStreamManager.ConsoleError));
        }

        private ServiceSession CreatingSession(string args, string inputText, CancellationToken token)
        {
            return new ServiceSession(this, CommandLine.PreprocessSingleQuotes(args), inputText, token);
        }

    }
}

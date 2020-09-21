// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts.Impl;
using NLedger.Utility.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.ServiceAPI
{
    public class ServiceEngine
    {
        public ServiceSession CreateSession(string args, string inputText)
        {
            return new ServiceSession(this, CommandLine.PreprocessSingleQuotes(args), inputText);
        }

        public Task<ServiceSession> CreateSessionAsync(string args, string inputText)
        {
            return Task.Run(() => CreateSession(args, inputText));
        }

        public virtual MainApplicationContext CreateContext(MemoryStreamManager memoryStreamManager)
        {
            var context = new MainApplicationContext(CreateApplicationServiceProvider(memoryStreamManager));
            context.IsAtty = false;
            // This is the subject for potential configuring: context.TimeZone, context.DefaultPager, context.SetEnvironmentVariables()
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

            return new ApplicationServiceProvider(virtualConsoleProviderFactory: () => new VirtualConsoleProvider(memoryStreamManager.ConsoleInput, memoryStreamManager.ConsoleOutput, memoryStreamManager.ConsoleError));
        }
    }
}

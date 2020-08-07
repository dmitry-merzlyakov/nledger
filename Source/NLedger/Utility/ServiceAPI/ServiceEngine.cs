// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.ServiceAPI
{
    public sealed class ServiceEngine
    {
        public ServiceSession CreateSession(string args, string inputText)
        {
            //var configuration = new NLedgerConfiguration();
            var context = new MainApplicationContext();

            context.IsAtty = false;
            //context.TimeZone = configuration.TimeZoneId.Value;
            //context.DefaultPager = configuration.DefaultPager.Value;
            //context.SetEnvironmentVariables(configuration.SettingsContainer.VarSettings.EnvironmentVariables);

            return new ServiceSession(context, CommandLine.PreprocessSingleQuotes(args), inputText);
        }

        public Task<ServiceSession> CreateSessionAsync(string args, string inputText)
        {
            return Task.Run(() => CreateSession(args, inputText));
        }
    }
}

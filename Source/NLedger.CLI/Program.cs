// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts.Impl;
using NLedger.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            // System.Diagnostics.Debugger.Launch(); // This debugging option might be useful in case of troubleshooting of NLTest issues

            var main = new Main();

            var config = new AppConfiguration();
            MainApplicationContext.Current.IsAtty = config.IsAtty;
            MainApplicationContext.Current.TimeZoneId = config.TimeZoneId;
            Console.OutputEncoding = config.OutputEncoding;
            if (config.IsAnsiTerminalEmulation)
                AnsiTextWriter.Attach();
            MainApplicationContext.Current.SetVirtualConsoleProvider(() => new VirtualConsoleProvider(Console.In, Console.Out, Console.Error));
            MainApplicationContext.Current.DefaultPager = config.DefaultPager;

            var envs = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().ToDictionary(d => d.Key.ToString(), d => d.Value.ToString());
            MainApplicationContext.Current.SetEnvironmentVariables(envs);

            var argString = GetCommandLine(); // This way is prefferable because of double quotas that are missed by using args

            Environment.ExitCode = main.Execute(argString);
        }

        private static string GetCommandLine()
        {
            // returns the original command line arguments w/o execution file name
            var commandLine = Environment.CommandLine;
            int pos = commandLine[0] == '"' ? pos = commandLine.IndexOf('"', 1) : commandLine.IndexOf(' ');
            return pos >= 0 ? commandLine.Substring(pos + 1).TrimStart() : String.Empty;
        }
    }
}

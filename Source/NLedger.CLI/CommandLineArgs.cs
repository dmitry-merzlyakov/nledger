// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NLedger.CLI
{
    public static class CommandLineArgs
    {
        /// <summary>
        /// Returns correct command line arguments for all platforms
        /// </summary>
        public static string GetArguments(string[] args)
        {
#if NETFRAMEWORK
            // For .Net Framework, it is possible to extract the original command line (with single and double quotas) from the Environment class
            return ExtractArguments(Environment.CommandLine);
#else
            // For .Net Core running on Windows, the only way to get the original command line with double quotas is P/Invoke (Win32 GetCommandLine)
            if (NLedger.Utility.PlatformHelper.IsWindows())
            {
                System.IntPtr ptr = GetCommandLine();
                var environmentCommandLine =  Marshal.PtrToStringAuto(ptr);
                return ExtractArguments(environmentCommandLine);
            }

            // For .Net Core running on Linux/OSX, we need to re-compose the original command line by adding quotas to some arguments.
            return Enquote(args);
#endif
        }

        /// <summary>
        /// Returns the original command line arguments w/o execution file name
        /// </summary>
        private static string ExtractArguments(string environmentCommandLine)
        {
            int pos = environmentCommandLine[0] == '"' ? pos = environmentCommandLine.IndexOf('"', 1) : environmentCommandLine.IndexOf(' ');
            return pos >= 0 ? environmentCommandLine.Substring(pos + 1).TrimStart() : String.Empty;
        }

#if !NETFRAMEWORK
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern System.IntPtr GetCommandLine();

        private static string Enquote(IEnumerable<string> args)
        {
            var sb = new StringBuilder();
            foreach(var arg in args)
            {
                if (sb.Length > 0)
                    sb.Append(' ');

                if (arg.StartsWith('\'') && arg.EndsWith('\''))
                    sb.Append(arg);     // Already quoted

                else if (arg.StartsWith('\"') && arg.EndsWith('\"'))
                    sb.Append(arg);     // Already quoted

                else
                {
                    var hasSingleQuote = arg.Contains('\'');
                    var hasDoubleQuote = arg.Contains('\"');
                    var hasSpaces = arg.Contains(' ');

                    var needsQuote = hasSingleQuote || hasDoubleQuote || hasSpaces;
                    var quoteChar = hasSingleQuote ? '\"' : '\'';

                    if (needsQuote)
                        sb.Append(quoteChar);

                    sb.Append(arg);

                    if (needsQuote)
                        sb.Append(quoteChar);
                }
            }
            return sb.ToString();
        }
#endif

    }
}

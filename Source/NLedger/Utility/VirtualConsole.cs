// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public static class VirtualConsole
    {
        public static TextReader Input
        {
            get { return MainApplicationContext.Current.ApplicationServiceProvider.VirtualConsoleProvider.ConsoleInput; }
        }

        public static TextWriter Output
        {
            get { return MainApplicationContext.Current.ApplicationServiceProvider.VirtualConsoleProvider.ConsoleOutput; }
        }

        public static TextWriter Error
        {
            get { return MainApplicationContext.Current.ApplicationServiceProvider.VirtualConsoleProvider.ConsoleError; }
        }

        public static int WindowWidth
        {
            get { return MainApplicationContext.Current.ApplicationServiceProvider.VirtualConsoleProvider.WindowWidth; }
        }

        public static bool IsAtty()
        {
            return MainApplicationContext.Current.IsAtty;
        }

        public static string ReadLine(string prompt)
        {
            if (!String.IsNullOrWhiteSpace(prompt))
                Output.Write(prompt);
            return Input.ReadLine();
        }

        // #readline-library - rl_readline_name
        public static string ReadLineName
        {
            get { return _ReadLineName; }
            set { _ReadLineName = value; }
        }

        // #readline-library - history_expand
        public static int HistoryExpand(string str, ref string output)
        {
            return MainApplicationContext.Current.ApplicationServiceProvider.VirtualConsoleProvider.HistoryExpand(_ReadLineName, str, ref output);
        }

        // #readline-library - add_history
        public static void AddHistory(string str)
        {
            MainApplicationContext.Current.ApplicationServiceProvider.VirtualConsoleProvider.AddHistory(_ReadLineName, str);
        }

        [ThreadStatic]
        private static string _ReadLineName;
    }
}

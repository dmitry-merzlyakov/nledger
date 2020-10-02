// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    public sealed class Main
    {
        public Main(MainApplicationContext mainApplicationContext)
        {
            if (mainApplicationContext == null)
                throw new ArgumentNullException(nameof(mainApplicationContext));

            MainApplicationContext = mainApplicationContext;
        }

        public MainApplicationContext MainApplicationContext { get; }

        public int Execute(string argString)
        {
            using (MainApplicationContext.AcquireCurrentThread())
            {
                var envp = VirtualEnvironment.GetEnvironmentVariables();
                var args = CommandLine.PreprocessSingleQuotes(argString);

                int status = 1;

                // The very first thing we do is handle some very special command-line
                // options, since they affect how the environment is setup:
                //
                //   --verify            ; turns on memory tracing
                //   --verbose           ; turns on logging
                //   --debug CATEGORY    ; turns on debug logging
                //   --trace LEVEL       ; turns on trace logging
                //   --memory            ; turns on memory usage tracing
                //   --init-file         ; directs ledger to use a different init file
                GlobalScope.HandleDebugOptions(args);
                // initialize_memory_tracing - [DM] #memory-tracing
                Logger.Current.Info(() => LedgerStarting);
                // ::textdomain("ledger"); - [DM] #localization
                GlobalScope globalScope = null;
                try
                {
                    // Create the session object, which maintains nearly all state relating to
                    // this invocation of Ledger; and register all known journal parsers.
                    globalScope = new GlobalScope(envp);
                    globalScope.Session.FlushOnNextDataFile = true;

                    // Look for options and a command verb in the command-line arguments
                    BindScope boundScope = new BindScope(globalScope, globalScope.Report);
                    args = globalScope.ReadCommandArguments(boundScope, args);

                    if (globalScope.ScriptHandler.Handled)
                    {
                        // Ledger is being invoked as a script command interpreter
                        globalScope.Session.ReadJournalFiles();

                        status = 0;

                        using (StreamReader sr = FileSystem.GetStreamReader(globalScope.ScriptHandler.Str()))
                        {
                            while (status == 0 && !sr.EndOfStream)
                            {
                                string line = sr.ReadLine().Trim();
                                if (!line.StartsWith("#"))
                                    status = globalScope.ExecuteCommandWrapper(StringExtensions.SplitArguments(line), true);
                            }
                        }
                    }
                    else if (args.Any())
                    {
                        // User has invoke a verb at the interactive command-line
                        status = globalScope.ExecuteCommandWrapper(args, false);
                    }
                    else
                    {
                        // Commence the REPL by displaying the current Ledger version
                        VirtualConsole.Output.WriteLine(globalScope.ShowVersionInfo());

                        globalScope.Session.ReadJournalFiles();

                        bool exitLoop = false;

                        VirtualConsole.ReadLineName = "Ledger";

                        string p;
                        while ((p = VirtualConsole.ReadLine(globalScope.PromptString())) != null)
                        {
                            string expansion = null;
                            int result = VirtualConsole.HistoryExpand(p.Trim(), ref expansion);

                            if (result < 0 || result == 2)
                                throw new LogicError(String.Format(LogicError.ErrorMessageFailedToExpandHistoryReference, p));
                            else if (expansion != null)
                                VirtualConsole.AddHistory(expansion);

                            CancellationManager.CheckForSignal();

                            if (!String.IsNullOrWhiteSpace(p) && p != "#")
                            {
                                if (String.Compare(p, "quit", true) == 0)
                                    exitLoop = true;
                                else
                                    globalScope.ExecuteCommandWrapper(StringExtensions.SplitArguments(p), true);
                            }

                            if (exitLoop)
                                break;
                        }
                        status = 0;    // report success
                    }
                }
                catch (CountError errors)
                {
                    // used for a "quick" exit, and is used only if help text (such as
                    // --help) was displayed
                    status = errors.Count;
                }
                catch (Exception err)
                {
                    if (globalScope != null)
                        globalScope.ReportError(err);
                    else
                        VirtualConsole.Error.WriteLine(String.Format(ExceptionDuringInitialization, err.Message));
                }

                if (globalScope != null)
                {
                    globalScope.QuickClose();
                    globalScope.Dispose();  // {DM] It is the most appropriate place to call Dispose for the global scope.
                }
                Logger.Current.Info(() => LedgerEnded); // let global_scope leak!

                // Return the final status to the operating system, either 1 for error or 0
                // for a successful completion.
                return status;
            }
        }

        private const string LedgerStarting = "Ledger starting";
        private const string LedgerEnded = "Ledger ended";
        private const string ExceptionDuringInitialization = "Exception during initialization: {0}";
    }
}

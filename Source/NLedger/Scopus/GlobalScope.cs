// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Textual;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Scopus
{
    public sealed class GlobalScope : Scope, IDisposable
    {
        public static string GlobalScopeDescription = "global scope";

        public const string OptionArgsOnly = "args_only";
        public const string OptionDebug = "debug_";
        public const string OptionHelp = "help";
        public const string OptionInitFile = "init_file_";
        public const string OptionOptions = "options";
        public const string OptionScript = "script_";
        public const string OptionTrace = "trace_";
        public const string OptionVerbose = "verbose";
        public const string OptionVerify = "verify";
        public const string OptionVerifyMemory = "verify_memory";
        public const string OptionVersion = "version";

        public static bool ArgsOnly
        {
            get { return MainApplicationContext.Current.ArgsOnly; }
            set { MainApplicationContext.Current.ArgsOnly = value; }
        }
        public static string InitFile
        {
            get { return MainApplicationContext.Current.InitFile; }
            set { MainApplicationContext.Current.InitFile = value; }
        }

        public GlobalScope()
        {
            ReportStack = new Stack<Report>();
            CreateOptions();
            CreateLookupItems();
        }

        public GlobalScope(IDictionary<string,string> envp) : this()
        {
            TimesCommon.Current.Epoch = TimesCommon.Current.CurrentDate;

            Session.SetSessionContext(Session = new Session());

            // Create the report object, which maintains state relating to each
            // command invocation.  Because we're running from main(), the
            // distinction between session and report doesn't really matter, but if
            // a GUI were calling into Ledger it would have one session object per
            // open document, with a separate report_t object for each report it
            // generated.
            ReportStack.Push(new Report(Session));
            Scope.DefaultScope = Report;
            Scope.EmptyScope = new EmptyScope();

            // Read the user's options, in the following order:
            //
            //  1. environment variables (LEDGER_<option>)
            //  2. initialization file (~/.ledgerrc)
            //  3. command-line (--option or -o)
            //
            // Before processing command-line options, we must notify the session object
            // that such options are beginning, since options like -f cause a complete
            // override of files found anywhere else.
            if (!ArgsOnly)
            {
                Session.FlushOnNextDataFile = true;
                ReadEnvironmentSettings(envp);
                Session.FlushOnNextDataFile = true;
                ReadInit();
            }
            else
            {
                Session.PriceDbHandler.Off();
            }
        }

        public void Dispose()
        {
            Session.SetSessionContext(null);
        }

        public Session Session { get; private set; }
        public Stack<Report> ReportStack { get; private set; }

        public Option ArgsOnlyHandler { get; private set; }
        public Option DebugHandler { get; private set; }
        public Option HelpHandler { get; private set; }
        public Option InitFileHandler { get; private set; }
        public Option OptionsHandler { get; private set; }
        public Option ScriptHandler { get; private set; }
        public Option TraceHandler { get; private set; }
        public Option VerboseHandler { get; private set; }
        public Option VerifyHandler { get; private set; }
        public Option VerifyMemoryHandler { get; private set; }
        public Option VersionHandler { get; private set; }

        public override string Description
        {
            get { return GlobalScopeDescription; }
        }

        public Report Report
        {
            get { return ReportStack.Peek(); }
        }

        public void PushReport()
        {
            ReportStack.Push(new Report(ReportStack.Peek()));
            Scope.DefaultScope = Report;
        }

        public Value PushCommand(CallScope args)
        {
            // Make a copy at position 2, because the topmost report object has an
            // open output stream at this point.  We want it to get popped off as
            // soon as this command terminate so that the stream is closed cleanly.
            Report temp = ReportStack.Pop();
            ReportStack.Push(new Report(temp));
            ReportStack.Push(temp);
            return Value.Get(true);
        }

        public Value PopCommand(CallScope args)
        {
            PopReport();
            return Value.Get(true);
        }

        public void PopReport()
        {
            if (!ReportStack.Any())
                throw new InvalidOperationException("Stack is empty");

            ReportStack.Peek().QuickClose(); // DM - this code simulates calling Report's destructor at this moment
            ReportStack.Pop();

            // There should always be the "default report" waiting on the stack.
            if (!ReportStack.Any())
                throw new InvalidOperationException("Stack is empty");
            Scope.DefaultScope = Report;
        }

        public Option LookupOption(string s)
        {
            return Options.LookupOption(s, this);
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            return LookupItems.Lookup(kind, name, this);
        }

        public string ShowVersionInfo()
        {
            return String.Format(ShowVersionInfoTemplate, VersionInfo.NLedgerVersion, VersionInfo.Ledger_VERSION_MAJOR, VersionInfo.Ledger_VERSION_MINOR, VersionInfo.Ledger_VERSION_PATCH);
        }

        public static void HandleDebugOptions(IEnumerable<string> args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            using (IEnumerator<string> argsEnumerator = args.GetEnumerator())
            {
                while (argsEnumerator.MoveNext())
                {
                    string arg = argsEnumerator.Current;

                    if (arg.StartsWith("-"))
                    {
                        if (arg == ArgsOnlyKey)
                        {
                            ArgsOnly = true;
                        }
                        else if (arg == VerifyMemoryKey)
                        {
                            Validator.IsVerifyEnabled = true;
                            Logger.Current.LogLevel = LogLevelEnum.LOG_DEBUG;
                            Logger.Current.LogCategory = "memory\\.counts";
                        }
                        else if (arg == VerifyKey)
                        {
                            Validator.IsVerifyEnabled = true;
                        }
                        else if (arg == VerboseKey || arg == ShortVerifyKey)
                        {
                            Logger.Current.LogLevel = LogLevelEnum.LOG_INFO;
                        }
                        else if (arg == InitFileKey && argsEnumerator.MoveNext())
                        {
                            InitFile = argsEnumerator.Current;
                        }
                        else if (arg == DebugKey && argsEnumerator.MoveNext())
                        {
                            Logger.Current.LogLevel = LogLevelEnum.LOG_DEBUG;
                            Logger.Current.LogCategory = argsEnumerator.Current;
                        }
                        else if (arg == TraceKey && argsEnumerator.MoveNext())
                        {
                            Logger.Current.LogLevel = LogLevelEnum.LOG_TRACE;
                            int traceLevel;
                            if (!Int32.TryParse(argsEnumerator.Current, out traceLevel))
                                throw new LogicError(LogicError.ErrorMessageArgumentToTraceMustBeInteger);
                            Logger.Current.TraceLevel = traceLevel;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Ported from global_scope_t::read_environment_settings
        /// </summary>
        public void ReadEnvironmentSettings(IDictionary<string, string> envp)
        {
            var trace = Logger.Current.TraceContext(TimerName.Environment, 1)?.Message("Processed environment variables").Start(); // TRACE_START

            Option.ProcessEnvironment(envp, "LEDGER_", Report);

            // These are here for backwards compatibility, but are deprecated.
            string variable;

            variable = envp.GetEnvironmentVariable("LEDGER");
            if (!String.IsNullOrEmpty(variable) && String.IsNullOrEmpty(envp.GetEnvironmentVariable("LEDGER_FILE")))
                Option.ProcessOption("environ", "file", Report, variable, "LEDGER");

            variable = envp.GetEnvironmentVariable("LEDGER_INIT");
            if (!String.IsNullOrEmpty(variable) && String.IsNullOrEmpty(envp.GetEnvironmentVariable("LEDGER_INIT_FILE")))
                Option.ProcessOption("environ", "init-file", Report, variable, "LEDGER_INIT");

            variable = envp.GetEnvironmentVariable("PRICE_HIST");
            if (!String.IsNullOrEmpty(variable) && String.IsNullOrEmpty(envp.GetEnvironmentVariable("LEDGER_PRICE_DB")))
                Option.ProcessOption("environ", "price-db", Report, variable, "PRICE_HIST");

            variable = envp.GetEnvironmentVariable("PRICE_EXP");
            if (!String.IsNullOrEmpty(variable) && String.IsNullOrEmpty(envp.GetEnvironmentVariable("LEDGER_PRICE_EXP")))
                Option.ProcessOption("environ", "price-exp", Report, variable, "PRICE_EXP");

            trace?.Finish(); // TRACE_FINISH
        }

        public void ReadInit()
        {
            // if specified on the command line init_file_ is filled in
            // global_scope_t::handle_debug_options.  If it was specified on the command line
            // fail if the file doesn't exist. If no init file was specified
            // on the command-line then try the default values, but don't fail if there
            // isn't one.
            string initFile;
            if (InitFileHandler.Handled)
            {
                initFile = InitFileHandler.Str();
                if (!FileSystem.FileExists(initFile))
                    throw new ParseError(String.Format(ParseError.ParseError_CouldNotFindSpecifiedInitFile, initFile));
            }
            else
            {
                initFile = FileSystem.HomePath(".ledgerrc");
                if (!FileSystem.FileExists(initFile))
                    initFile = ".ledgerrc";
            }
            if (FileSystem.FileExists(initFile))
                ParseInit(initFile);
        }

        /// <summary>
        /// Ported from global_scope_t::parse_init
        /// </summary>
        public void ParseInit(string initFile)
        {
            var trace = Logger.Current.TraceContext(TimerName.Init, 1)?.Message("Read initialization file").Start(); // TRACE_START

            ParseContextStack parsingContext = new ParseContextStack();
            parsingContext.Push(initFile);
            parsingContext.GetCurrent().Journal = Session.Journal;
            parsingContext.GetCurrent().Scope = Report;

            if (Session.Journal.Read(parsingContext) > 0 ||
                Session.Journal.AutoXacts.Count > 0 ||
                Session.Journal.PeriodXacts.Count > 0)
                throw new ParseError(String.Format(ParseError.ParseError_TransactionsFoundInInitializationFile, initFile));

            trace?.Finish(); // TRACE_FINISH
        }

        /// <summary>
        /// Ported from global_scope_t::read_command_arguments
        /// </summary>
        public IEnumerable<string> ReadCommandArguments(Scope scope, IEnumerable<string> args)
        {
            var trace = Logger.Current.TraceContext(TimerName.Arguments, 1)?.Message("Processed command-line arguments").Start(); // TRACE_START
            var remaining = Option.ProcessArguments(args, scope);
            trace?.Finish();  // TRACE_FINISH
            return remaining;
        }

        public void VisitManPage()
        {
            if (!MainApplicationContext.Current.ManPageProvider.Show())
                throw new LogicError("Failed to fork child process");
        }

        public string PromptString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ReportStack.Count(); i++)
                sb.Append("]");
            sb.Append(" ");
            return sb.ToString();
        }

        /**
         * @return \c true if a command was actually executed; otherwise, it probably
         *         just resulted in setting some options.
         */
        public int ExecuteCommandWrapper(IEnumerable<string> args, bool atRepl)
        {
            int status = 1;

            try
            {
                if (atRepl)
                    PushReport();
                ExecuteCommand(args, atRepl);
                if (atRepl)
                    PopReport();

                // If we've reached this point, everything succeeded fine.  Ledger uses
                // exceptions to notify of error conditions, so if you're using gdb,
                // just type "catch throw" to find the source point of any error.
                status = 0;
            }
            catch (CountError) // DM - error_count does not inherit std::exception and should be ignored here.
            {
                throw;
            }
            catch(Exception err)
            {
                if (atRepl)
                    PopReport();
                ReportError(err);
            }

            return status;
        }

        public void ExecuteCommand(IEnumerable<string> args, bool atRepl)
        {
            Session.FlushOnNextDataFile = true;

            // Process the command verb, arguments and options
            if (atRepl)
            {
                args = ReadCommandArguments(Report, args);
                if (!args.Any())
                    return;
            }

            string verb = args.First();
            args = args.Skip(1); // DM - skip the first item that is equal to verb = *arg++;

            // Look for a precommand first, which is defined as any defined function
            // whose name starts with "ledger_precmd_".  The difference between a
            // precommand and a regular command is that precommands ignore the journal
            // data file completely, nor is the user's init file read.
            //
            // Here are some examples of pre-commands:
            //
            //   parse STRING       ; show how a value expression is parsed
            //   eval STRING        ; simply evaluate a value expression
            //   format STRING      ; show how a format string is parsed
            //
            // If such a command is found, create the output stream for the result and
            // then invoke the command.

            ExprFunc command;
            bool isPrecommand = false;
            BindScope boundScope = new BindScope(this, Report);

            command = LookForPrecommand(boundScope, verb);
            if (!command.IsNullOrEmpty())
                isPrecommand = true;

            // If it is not a pre-command, then parse the user's ledger data at this
            // time if not done already (i.e., if not at a REPL).  Then patch up the
            // report options based on the command verb.

            if (!isPrecommand)
            {
                if (!atRepl)
                    Session.ReadJournalFiles();

                Report.NormalizeOptions(verb);

                command = LookForCommand(boundScope, verb);
                if (command.IsNullOrEmpty())
                    throw new LogicError(String.Format(LogicError.ErrorMessageUnrecognizedCommand, verb));
            }

            // Create the output stream (it might be a file, the console or a PAGER
            // subprocess) and invoke the report command.  The output stream is closed
            // by the caller of this function.

            Report.OutputStream = FileSystem.OutputStreamInitialize(
                Report.OutputHandler.Handled ? Report.OutputHandler.Str() : String.Empty,
                Report.PagerHandler.Handled ? Report.PagerHandler.Str() : String.Empty);

            // Now that the output stream is initialized, report the options that will
            // participate in this report, if the user specified --options

            if (OptionsHandler.Handled)
                Report.OutputStream.Write(ReportOptions(Report));

            // Create an argument scope containing the report command's arguments, and
            // then invoke the command.  The bound scope causes lookups to happen
            // first in the global scope, and then in the report scope.

            CallScope commandArgs = new CallScope(boundScope);
            foreach (string arg in args)
                commandArgs.PushBack(Value.Get(arg));

            var info = Logger.Current.InfoContext(TimerName.Command)?.Message("Finished executing command").Start(); // INFO_START
            command(commandArgs);
            info?.Finish(); // INFO_FINISH
        }

        public void QuickClose()
        {
            if (ReportStack.Any())
                ReportStack.Peek().QuickClose();
        }

        /// <summary>
        /// Ported from void global_scope_t::report_error(const std::exception& err)
        /// </summary>
        public void ReportError(Exception ex)
        {
            VirtualConsole.Output.Flush();   // first display anything that was pending

            if (!CancellationManager.IsCancellationRequested)
            {
                // Display any pending error context information
                string context = ErrorContext.Current.GetContext();
                if (!String.IsNullOrWhiteSpace(context))
                    VirtualConsole.Error.WriteLine(context);

                VirtualConsole.Error.WriteLine(String.Format("Error: {0}", ex.Message));
            }
            else
            {
                CancellationManager.DiscardCancellationRequest();
            }
        }

        public ExprFunc LookForPrecommand(Scope scope, string verb)
        {
            ExprOp def = scope.Lookup(SymbolKindEnum.PRECOMMAND, verb);
            if (def != null)
                return def.AsFunction;
            else
                return Expr.EmptyFunc;
        }

        public ExprFunc LookForCommand(Scope scope, string verb)
        {
            ExprOp def = scope.Lookup(SymbolKindEnum.COMMAND, verb);
            if (def != null)
                return def.AsFunction;
            else
                return Expr.EmptyFunc;
        }

        public string ReportOptions (Report report)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("===============================================================================");
            sb.AppendLine("[Global scope options]");

            sb.AppendLine(ArgsOnlyHandler.Report());
            sb.AppendLine(DebugHandler.Report());
            sb.AppendLine(InitFileHandler.Report());
            sb.AppendLine(ScriptHandler.Report());
            sb.AppendLine(TraceHandler.Report());
            sb.AppendLine(VerboseHandler.Report());
            sb.AppendLine(VerifyHandler.Report());
            sb.AppendLine(VerifyMemoryHandler.Report());

            sb.AppendLine("[Session scope options]");
            sb.AppendLine(report.Session.ReportOptions());

            sb.AppendLine("[Report scope options]");
            sb.AppendLine(report.ReportOptions());
            sb.AppendLine("===============================================================================");

            return sb.ToString();
        }

        private void CreateOptions()
        {
            ArgsOnlyHandler = Options.Add(new Option(OptionArgsOnly));
            DebugHandler = Options.Add(new Option(OptionDebug));
            HelpHandler = Options.Add(new Option(OptionHelp, (o, w) => VisitManPage()));

            InitFileHandler = Options.Add(new Option(OptionInitFile));
            if (!String.IsNullOrEmpty(GlobalScope.InitFile))
                // _init_file is filled during handle_debug_options
                InitFileHandler.On(null, GlobalScope.InitFile);

            OptionsHandler = Options.Add(new Option(OptionOptions));
            ScriptHandler = Options.Add(new Option(OptionScript));
            TraceHandler = Options.Add(new Option(OptionTrace));
            VerboseHandler = Options.Add(new Option(OptionVerbose));
            VerifyHandler = Options.Add(new Option(OptionVerify));
            VerifyMemoryHandler = Options.Add(new Option(OptionVerifyMemory));
            VersionHandler = Options.Add(new Option(OptionVersion, (o, w) =>
            {
                VirtualConsole.Output.WriteLine(ShowVersionInfo());
                throw new CountError(0, String.Empty); // exit immediately
            }));

            Options.AddLookupOpt(OptionArgsOnly);
            Options.AddLookupOpt(OptionDebug);
            Options.AddLookupOptArgs(OptionHelp, "h");
            Options.AddLookupOptArgs(OptionInitFile, "i");
            Options.AddLookupOpt(OptionOptions);
            Options.AddLookupOpt(OptionScript);
            Options.AddLookupOpt(OptionTrace);
            Options.AddLookupOptArgs(OptionVerbose, "v");
            Options.AddLookupOpt(OptionVerify);
            Options.AddLookupOpt(OptionVerifyMemory);
            Options.AddLookupOpt(OptionVersion);
        }

        private void CreateLookupItems()
        {
            LookupItems.MakeFunctor("push", scope => PushCommand((CallScope)scope), SymbolKindEnum.PRECOMMAND);
            LookupItems.MakeFunctor("pop", scope => PopCommand((CallScope)scope), SymbolKindEnum.PRECOMMAND);

            LookupItems.MakeOptionFunctors(Options);
            LookupItems.MakeOptionHandlers(Options);
        }

        private const string ArgsOnlyKey = "--args-only";
        private const string VerifyMemoryKey = "--verify-memory";
        private const string VerifyKey = "--verify";
        private const string VerboseKey = "--verbose";
        private const string ShortVerifyKey = "-v";
        private const string InitFileKey = "--init-file";
        private const string DebugKey = "--debug";
        private const string TraceKey = "--trace";

        private readonly OptionCollection Options = new OptionCollection();
        private readonly ExprOpCollection LookupItems = new ExprOpCollection();

        public const string ShowVersionInfoTemplate = 
@"NLedger {0}, the command-line accounting tool.
Ported to .Net platform from Ledger {1}.{2}.{3}

Copyright (c) 2003-2017, John Wiegley.  All rights reserved.

This program is made available under the terms of the BSD Public License.
See LICENSE file included with the distribution for details and disclaimer.";

    }
}

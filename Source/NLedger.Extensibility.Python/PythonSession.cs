// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Utility.Settings.CascadeSettings.Sources;
using NLedger.Utils;
using NLedger.Values;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLedger.Extensibility.Python
{
    public class PythonSession : ExtendedSession
    {
        public static new PythonSession Current => ExtendedSession.Current as PythonSession;

        /// <summary>
        /// Helper static method that routes text to Virtual Console standard output. Used by Python Ledger module to redirect output stream.
        /// </summary>
        public static int ConsoleWrite(string s, bool isError = false)
        {
            var consoleProvider = MainApplicationContext.Current?.ApplicationServiceProvider.VirtualConsoleProvider;
            (isError ? consoleProvider.ConsoleError : consoleProvider.ConsoleOutput).Write(s);
            return s?.Length ?? 0;
        }

        /// <summary>
        /// Entry point for Python module initialization
        /// </summary>
        public static void PythonModuleInitialization()
        {
            CreateStandaloneSession(() => new PythonSession(), () =>
            {
                var context = new MainApplicationContext();
                var envs = new EnvironmentVariablesSettingsSource("nledger");
                context.SetEnvironmentVariables(envs.EnvironmentVariables);
                context.IsAtty = String.Equals(envs.GetValue("IsAtty"), bool.TrueString, StringComparison.InvariantCultureIgnoreCase);
                return context;
            });
        }

        /// <summary>
        /// Entry point for Python module shutdown (releasing current session, optional)
        /// </summary>
        public static void PythonModuleShutdown()
        {
            Current?.Dispose();
        }

        public PythonSession()
        {
            PythonValueConverter = new PythonValueConverter(this);
        }

        public bool IsPythonHost => IsStandaloneSession;
        public IDictionary<PyModule, PythonModule> ModulesMap { get; } = new Dictionary<PyModule, PythonModule>();
        public IPythonValueConverter PythonValueConverter { get; }

        public PythonSessionConnectionContext PythonSessionConnectionContext { get; private set; }
        public PythonModule MainModule => PythonSessionConnectionContext?.MainModule;
        public PyModule LedgerModule => PythonSessionConnectionContext?.LedgerModule;

        public IDisposable GIL() => Py.GIL();

        public PythonModule GetOrCreateModule(PyModule pyModule, string name)
        {
            PythonModule pythonModule;
            if (!ModulesMap.TryGetValue(pyModule, out pythonModule))
                ModulesMap.Add(pyModule, pythonModule = new PythonModule(this, name, pyModule));

            return pythonModule;
        }

        public override void DefineGlobal(string name, object value)
        {
            using(GIL())
                MainModule.DefineGlobal(name, PyObject.FromManagedObject(value));
        }

        public override void Eval(string code, ExtensionEvalModeEnum mode)
        {
            if (!IsInitialized())
                Initialize();

            try
            {
                using (GIL())
                    PythonEngine.Exec(code, MainModule.ModuleGlobals);
            }
            catch (Exception ex)
            {
                // [DM] PyErr_Print() is omitted
                ErrorContext.Current.AddErrorContext(ex.ToString());
                throw new RuntimeError("Failed to evaluate Python code");
            }
        }

        public override void ImportOption(string str)
        {
            if (!IsInitialized())
                Initialize();

            var isPyFile = str.EndsWith(".py");
            var name = isPyFile ? AddPath(str) : str;

            try
            {
                if (isPyFile)
                    MainModule.ImportModule(name, true);
                else
                    ImportModule(str);
            }
            catch (PythonException ex)
            {
                // [DM] PyErr_Print() is omitted
                ErrorContext.Current.AddErrorContext(ex.ToString());
                throw new RuntimeError($"Python failed to import: {str}");
            }
        }

        public override void Initialize()
        {
            if (IsInitialized())
                return;

            var trace = Logger.Current.TraceContext("python_init", 1)?.Message("Initialized Python").Start(); // TRACE_START

            try
            {
                Logger.Current.Debug("python.interp", () => "Initializing Python");
                PythonSessionConnectionContext = Platform.PythonConnector.Current.Connect(connector => new PythonSessionConnectionContext(connector, this));
            }
            catch (Exception ex)
            {
                // [DM] PyErr_Print() is omitted
                ErrorContext.Current.AddErrorContext(ex.ToString());
                throw new RuntimeError("Python failed to initialize");
            }

            trace?.Finish(); // TRACE_FINISH
        }

        public override bool IsInitialized()
        {
            return PythonSessionConnectionContext != null;
        }

        public override void Dispose()
        {
            PythonSessionConnectionContext?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Ported from value_t python_interpreter_t::python_command(call_scope_t& args)
        /// </summary>
        public override Value PythonCommand(CallScope args)
        {
            if (!IsInitialized())
                Initialize();

            var argv = new List<string>();

            argv.Add(Environment.GetCommandLineArgs().FirstOrDefault());

            for (int i = 0; i < args.Size; i++)
                argv.Add(args.Get<string>(i));

            int status = 1;

            using (GIL())
            {
                try
                {
                    //[DM] Commented piece of code below (...Runtime.Py_Main...) was initially ported from original Ledger.
                    // However, Py_Main finalizes the context at the end of execution and the current AppDomain becomes unusable.
                    // It is acceptable for a command-line utility but it is not a case for a software library.
                    // The workaround is to execute the target script (w/o parameters) by means of Exec as a script block.
                    // This solution works fine unless you have command-line arguments; they are not supported.
                    //status = Runtime.Py_Main(argv.Count, argv.ToArray());

                    //[DM] Some tests (e.g. regress\B21BF389_py.test) indicate that Python code executed by Ledger's 'python' command
                    // expects to have variable __file___ populated with running Python file name (because Ledger uses Py_Main for this command).
                    // For compatibility purposes, let's populate this variable with proper name.
                    var fileName = System.IO.Path.GetFullPath(argv[1]);
                    MainModule.ModuleObject.SetAttr("__file__", new PyString(fileName));

                    MainModule.ModuleObject.Exec(System.IO.File.ReadAllText(fileName));
                    status = 0;
                }
                catch (Exception ex)
                {
                    // [DM] PyErr_Print() is omitted
                    ErrorContext.Current.AddErrorContext(ex.ToString());
                    throw new RuntimeError("Failed to execute Python module");
                }
            }

            if (status != 0)
                throw new Exception(status.ToString());

            return Value.Empty;
        }

        /// <summary>
        /// Ported from value_t python_interpreter_t::server_command(call_scope_t& args)
        /// </summary>
        public override Value ServerCommand(CallScope args)
        {
            using (GIL())
            {
                PyModule serverModule = null;

                try
                {
                    serverModule = (PyModule)MainModule.ModuleObject.Import("ledger.server");
                }
                catch
                {
                    // [DM] PyErr_Print() is omitted
                    throw new RuntimeError("Could not import ledger.server; please check your PYTHONPATH");
                }

                if (serverModule == null)
                    throw new RuntimeError("Could not import ledger.server; please check your PYTHONPATH");

                using (serverModule)
                {
                    var mainFunction = serverModule.GetAttr("main");
                    if (mainFunction == null)
                        throw new RuntimeError("The ledger.server module is missing its main() function!");

                    var func = new PythonFunctor("main", mainFunction, PythonValueConverter);

                    try
                    {
                        func.ExprFunctor(this);
                        return Value.True;
                    }
                    catch
                    {
                        // [DM] PyErr_Print() is omitted
                        throw new RuntimeError("Error while invoking ledger.server's main() function");
                    }
                }
            }
        }

        protected override ExprOp LookupFunction(string name)
        {
            return MainModule.Lookup(Scopus.SymbolKindEnum.FUNCTION, name);
        }

        private string AddPath(string str)
        {
            using (GIL())
            {
                var sysModule = Py.Import("sys");
                var sysDict = sysModule.GetAttr("__dict__");
                var paths = new PyList(sysDict["path"]);

                var fileSystem = MainApplicationContext.Current.ApplicationServiceProvider.FileSystemProvider;

                var cwd = ParsingContext.GetCurrent().CurrentPath;
                var parent = fileSystem.GetDirectoryName(fileSystem.GetFullPath(fileSystem.PathCombine(cwd, str)));

                Logger.Current.Debug("python.interp", () => $"Adding {parent} to PYTHONPATH");

                var pyParent = parent.ToPython();
                if (!paths.Contains(pyParent))
                    paths.Insert(0, pyParent);
                sysDict["path"] = paths;

                return System.IO.Path.GetFileNameWithoutExtension(fileSystem.GetFileName(str));
            }
        }

        private void ImportModule(string name)
        {
            var module = new PythonModule(this, name);
            MainModule.DefineGlobal(name, module.ModuleObject);
        }
    }
}

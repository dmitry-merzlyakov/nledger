using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Utility;
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
        /// <summary>
        /// Helper static method that routes text to Virtual Console standard output. Used by Python Ledger module to redirect output stream.
        /// </summary>
        public static int ConsoleWrite(string s, bool isError = false)
        {
            var consoleProvider = MainApplicationContext.Current?.ApplicationServiceProvider.VirtualConsoleProvider;
            (isError ? consoleProvider.ConsoleError : consoleProvider.ConsoleOutput).Write(s);
            return s?.Length ?? 0;
        }

        public static void PythonModuleInitialization()
        {
            // MainApplicationContext is empty only if Python module is being initialized in Python session (not in NLedger process).
            // Further code initializes global context for Python session.
            if (MainApplicationContext.Current == null)
            {
                var context = new MainApplicationContext();   // TODO - read settings.
                context.AcquireCurrentThread(); // TODO - add release code.
                var pythonSession = new PythonSession();
                context.SetExtendedSession(pythonSession);
                Session.SetSessionContext(pythonSession);
            }
        }

        public bool IsSessionInitialized { get; private set; }
        public PythonModule MainModule { get; private set; }
        public IDictionary<PyModule, PythonModule> ModulesMap { get; } = new Dictionary<PyModule, PythonModule>();
        public IPythonValueConverter PythonValueConverter { get; } = new PythonValueConverter();
        public PyModule LedgerModule { get; private set; }

        public PythonModule GetOrCreateModule(PyModule pyModule, string name)
        {
            PythonModule pythonModule;
            if (!ModulesMap.TryGetValue(pyModule, out pythonModule))
                ModulesMap.Add(pyModule, pythonModule = new PythonModule(this, name, pyModule));

            return pythonModule;
        }

        public override void DefineGlobal(string name, object value)
        {
            MainModule.DefineGlobal(name, PyObject.FromManagedObject(value));
        }

        public override void Eval(string code, ExtensionEvalModeEnum mode)
        {
            if (!IsInitialized())
                Initialize();

            try
            {
                PythonEngine.Exec(code, MainModule.ModuleGlobals.Handle);
            }
            catch (Exception ex)
            {
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

                if (!PythonEngine.IsInitialized)
                    throw new InvalidOperationException("assert(Py_IsInitialized());");

                MainModule = new PythonModule(this, "__main__", Py.Import("__main__"));
                LedgerModule = Py.Import("ledger");

                // [DM] Redirecting output streams
                LedgerModule.Exec("acquire_output_streams()");

                IsSessionInitialized = true;
            }
            catch (Exception ex)
            {
                // TODO PyErr_Print();
                ErrorContext.Current.AddErrorContext(ex.ToString());
                throw new RuntimeError("Python failed to initialize");
            }

            trace?.Finish(); // TRACE_FINISH
        }

        public override bool IsInitialized()
        {
            return IsSessionInitialized;
        }

        public override void Dispose()
        {
            LedgerModule?.Exec("release_output_streams()");  // TOSO - check PyEngine status
            base.Dispose();
        }

        public override Value PythonCommand(CallScope args)
        {
            if (!IsInitialized())
                Initialize();

            var argv = new List<string>();

            argv.Add(Environment.GetCommandLineArgs().FirstOrDefault());  // TODO

            for (int i = 0; i < args.Size; i++)
                argv.Add(args.Get<string>(i));

            int status = 1;

            try
            {
                status = Runtime.Py_Main(argv.Count, argv.ToArray());
            }
            catch (Exception ex)
            {
                ErrorContext.Current.AddErrorContext(ex.ToString());
                throw new RuntimeError("Failed to execute Python module");
            }

            if (status != 0)
                throw new Exception(status.ToString()); // TODO

            return Value.Empty;
        }

        public override Value ServerCommand(CallScope args)
        {
            throw new NotImplementedException("TODO2");
        }

        protected override ExprOp LookupFunction(string name)
        {
            return MainModule.Lookup(Scopus.SymbolKindEnum.FUNCTION, name);
        }

        private string AddPath(string str)
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

        private void ImportModule(string name)
        {
            var module = new PythonModule(this, name);
            MainModule.DefineGlobal(name, module.ModuleObject);
        }
    }
}

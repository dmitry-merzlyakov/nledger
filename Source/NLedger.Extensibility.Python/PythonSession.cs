using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Values;
using Python.Runtime;
using System;
using System.Collections.Generic;
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
            LedgerModule?.Exec("release_output_streams()");
            base.Dispose();
        }

        public override Value PythonCommand(CallScope scope)
        {
            throw new NotImplementedException("TODO");
        }

        public override Value ServerCommand(CallScope args)
        {
            throw new NotImplementedException("TODO");
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

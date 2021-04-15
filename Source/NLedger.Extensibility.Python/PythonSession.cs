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
        public bool IsSessionInitialized { get; private set; }
        public PythonModule MainModule { get; private set; }
        public IDictionary<PyModule, PythonModule> ModulesMap { get; } = new Dictionary<PyModule, PythonModule>();

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
            throw new NotImplementedException();
        }

        public override void ImportOption(string name)
        {
            throw new NotImplementedException();
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
                Py.Import("ledger");

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
    }
}

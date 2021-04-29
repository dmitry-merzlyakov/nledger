using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Values;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python
{
    public class PythonFunctor
    {
        public PythonFunctor(string name, PyObject obj)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Obj = obj ?? throw new ArgumentNullException(nameof(obj));
        }

        public string Name { get; }
        public PyObject Obj { get; }
        public ExprFunc ExprFunctor => ExprFunc;

        public Value ExprFunc(Scope scope)
        {
            // TDOD
            return Value.Empty;
        }
    }
}

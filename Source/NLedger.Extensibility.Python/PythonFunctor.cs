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
using NLedger.Utils;
using NLedger.Values;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLedger.Extensibility.Python
{
    public class PythonFunctor
    {
        public PythonFunctor(string name, PyObject obj, IPythonValueConverter pythonValueConverter)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Obj = obj ?? throw new ArgumentNullException(nameof(obj));
            PythonValueConverter = pythonValueConverter ?? throw new ArgumentNullException(nameof(pythonValueConverter));
        }

        public string Name { get; }
        public PyObject Obj { get; }
        public IPythonValueConverter PythonValueConverter { get; }

        public ExprFunc ExprFunctor => ExprFunc;

        /// <summary>
        /// Ported from value_t python_interpreter_t::functor_t::operator()(call_scope_t& args)
        /// </summary>
        public Value ExprFunc(Scope args)
        {
            using (Py.GIL())
            {
                if (!Obj.IsCallable())
                {
                    var val = PythonValueConverter.GetValue(Obj);
                    Logger.Current.Debug("python.interp", () => $"Value of Python '{Name}': {val.ToString()}");
                    return val;
                }
                else
                {
                    var arglist = GetParamList((CallScope)args).ToArray();
                    var val = Obj.Invoke(arglist);

                    try
                    {
                        var xval = PythonValueConverter.GetValue(val);
                        Logger.Current.Debug("python.interp", () => $"Return from Python '{Name}': {val.ToString()}");
                        return xval;
                    }
                    catch (Exception ex)
                    {
                        ErrorContext.Current.AddErrorContext(ex.ToString());
                        throw new CalcError($"Failed call to Python function 'Name'");
                    }
                }
            }
        }

        private IEnumerable<PyObject> GetParamList(CallScope callScope)
        {
            if (callScope.Size > 0)
            {
                if (callScope.Value().Type == ValueTypeEnum.Sequence)
                {
                    foreach (var val in callScope.Value().AsSequence)
                        yield return PythonValueConverter.GetObject(val);
                }
                else
                {
                    yield return PythonValueConverter.GetObject(callScope.Value());
                }
            }
        }
    }
}

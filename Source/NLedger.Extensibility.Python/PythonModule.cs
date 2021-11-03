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
using NLedger.Utils;
using NLedger.Values;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python
{
    /// <summary>
    /// Ported from python_module_t
    /// </summary>
    public class PythonModule : Scope
    {
        public PythonModule(PythonSession pythonSession, string name)
        {
            PythonSession = pythonSession;
            ModuleName = name;
            ModuleGlobals = new PyDict();
            ImportModule(name);
        }

        public PythonModule(PythonSession pythonSession, string name, PyModule obj)
        {
            PythonSession = pythonSession;
            ModuleName = name;
            ModuleObject = obj;
            ModuleGlobals = new PyDict(obj.GetAttr("__dict__"));
        }

        public PythonSession PythonSession { get; }
        public string ModuleName { get; }
        public PyModule ModuleObject { get; private set; }
        public PyDict ModuleGlobals { get; private set; }

        public override string Description => ModuleName;

        public void DefineGlobal(string name, PyObject obj)
        {
            using(PythonSession.GIL())
                ModuleGlobals[name] = obj;
        }

        public void ImportModule(string name, bool importDirect = false)
        {
            using (PythonSession.GIL())
            {
                var mod = PythonSession.MainModule.ModuleObject.Import(name) ?? throw new RuntimeError($"Module import failed (couldn't find {name})");
                var globals = mod.GetAttr("__dict__") ?? throw new RuntimeError($"Module import failed (couldn't find {name})");

                if (!importDirect)
                {
                    ModuleObject = (PyModule)mod;
                    ModuleGlobals = new PyDict(globals);
                }
                else
                {
                    ModuleGlobals.Update(mod.GetAttr("__dict__"));
                }
            }
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            if (kind == SymbolKindEnum.FUNCTION)
            {
                using (PythonSession.GIL())
                {
                    Logger.Current.Debug("python.interp", () => $"Python lookup: {name}");
                    if (ModuleGlobals.HasKey(name))
                    {
                        var obj = ModuleGlobals.GetItem(name);
                        if (obj != null)
                        {
                            if (obj.IsModule())
                            {
                                var objModule = (PyModule)PyModule.Import(obj.GetAttr("__name__").ToString());
                                var pythonModule = PythonSession.GetOrCreateModule(objModule, name);
                                return ExprOp.WrapValue(Value.ScopeValue(pythonModule));
                            }
                            else
                            {
                                return ExprOp.WrapFunctor(new PythonFunctor(name, obj, PythonSession.PythonValueConverter).ExprFunctor);
                            }
                        }
                    }
                }
            }

            return null;
        }

    }
}

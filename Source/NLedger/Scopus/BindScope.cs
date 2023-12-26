// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Scopus
{
    /// <summary>
    /// Ported from bind_scope_t
    /// </summary>
    public class BindScope : ChildScope
    {
        public BindScope(Scope parent, Scope grandChild) : base(parent)
        {
            GrandChild = grandChild;
            Logger.Current.Debug("scope.symbols", () => String.Format("Binding scope {0} with {1}", parent, grandChild));
        }

        public Scope GrandChild { get; private set; }

        public override string Description
        {
            get { return GrandChild.Description; }
        }

        public override void Define(SymbolKindEnum kind, string name, ExprOp exprOp)
        {
            Parent.Define(kind, name, exprOp);
            GrandChild.Define(kind, name, exprOp);
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            return GrandChild.Lookup(kind, name) ?? base.Lookup(kind, name);
        }
    }
}

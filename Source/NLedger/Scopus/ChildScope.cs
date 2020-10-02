// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Scopus
{
    public abstract class ChildScope : Scope
    {
        public ChildScope() : this(null)
        {  }

        public ChildScope(Scope parent)
        {
            Parent = parent;
        }

        public Scope Parent { get; private set; }

        public override void Define(SymbolKindEnum kind, string name, ExprOp exprOp)
        {
            if (Parent != null)
                Parent.Define(kind, name, exprOp);
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            if (Parent != null)
                return Parent.Lookup(kind, name);

            return null;
        }
    }
}

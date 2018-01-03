// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
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
    public class SymbolScope : ChildScope
    {
        public SymbolScope() : this(null)
        { }

        public SymbolScope(Scope parent) : base(parent)
        {
            Symbols = new Dictionary<Symbol, ExprOp>();
        }

        public IDictionary<Symbol, ExprOp> Symbols { get; private set; }

        public override string Description
        {
            get { return Parent != null ? Parent.Description : String.Empty; }
        }

        public override void Define(SymbolKindEnum kind, string name, ExprOp exprOp)
        {
            Symbol symbol = new Symbol(kind, name, exprOp);
            Symbols[symbol] = exprOp;
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            ExprOp expr;
            Symbol symbol = new Symbol(kind, name, null);
            if (Symbols.TryGetValue(symbol, out expr))
                return expr;
            else
                return base.Lookup(kind, name);
        }
    }
}

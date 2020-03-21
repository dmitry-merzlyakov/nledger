// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
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
    /// Porrted from symbol_scope_t
    /// </summary>
    public class SymbolScope : ChildScope
    {
        public SymbolScope() : this(null)
        { }

        public SymbolScope(Scope parent) : base(parent)
        { }

        public IDictionary<Symbol, ExprOp> Symbols { get; private set; }

        public override string Description
        {
            get { return Parent != null ? Parent.Description : String.Empty; }
        }

        /// <summary>
        /// Ported from void symbol_scope_t::define
        /// </summary>
        public override void Define(SymbolKindEnum kind, string name, ExprOp exprOp)
        {
            Logger.Current.Debug("scope.symbols", () => String.Format("Defining '{0}' = {1} in {2}", name, exprOp, this));

            if (Symbols == null)
                Symbols = new Dictionary<Symbol, ExprOp>();

            Symbol symbol = new Symbol(kind, name, exprOp);
            Symbols[symbol] = exprOp;
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            if (Symbols != null)
            {
                Logger.Current.Debug("scope.symbols", () => String.Format("Looking for '{0}' in {1}", name, this));
                ExprOp expr;
                Symbol symbol = new Symbol(kind, name, null);
                if (Symbols.TryGetValue(symbol, out expr))
                {
                    Logger.Current.Debug("scope.symbols", () => String.Format("Found '{0}' in {1}", name, this));
                    return expr;
                }
            }
            return base.Lookup(kind, name);
        }
    }
}

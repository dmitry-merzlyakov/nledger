// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
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
    public struct Symbol
    {
        public Symbol(SymbolKindEnum kind, string name, ExprOp definition) : this()
        {
            Kind = kind;
            Name = name;
            Definition = definition;
        }

        public SymbolKindEnum Kind { get; private set; }
        public string Name { get; private set; }
        public ExprOp Definition { get; private set; }

        public override bool Equals(object obj)
        {
            Symbol symbol = (Symbol)obj;
            return Kind == symbol.Kind && Name == symbol.Name;
        }

        public override int GetHashCode()
        {
            return Kind.GetHashCode() ^ Name.GetHashCode();
        }
    }
}

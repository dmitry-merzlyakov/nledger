// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Scopus
{
    public abstract class Scope
    {
        public static Scope DefaultScope
        {
            get { return MainApplicationContext.Current.DefaultScope; }
            set { MainApplicationContext.Current.DefaultScope = value; }
        }

        public static Scope EmptyScope
        {
            get { return MainApplicationContext.Current.EmptyScope; }
            set { MainApplicationContext.Current.EmptyScope = value; }
        }

        public abstract string Description { get; }

        public virtual void Define (SymbolKindEnum kind, string name, ExprOp exprOp)
        { }

        public abstract ExprOp Lookup (SymbolKindEnum kind, string name);

        public virtual ValueTypeEnum TypeContext
        {
            get { return ValueTypeEnum.Void; }
        }

        public virtual bool TypeRequired
        {
            get { return false; }
        }
    }
}

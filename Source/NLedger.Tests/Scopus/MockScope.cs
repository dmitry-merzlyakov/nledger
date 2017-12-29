// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Scopus
{
    public class MockScope : Scope
    {
        public MockScope(string desc = "mock-description")
        {
            Desc = desc;
            LookupCalls = new List<Tuple<SymbolKindEnum, string>>();
            LookupResult = new ExprOp();
            DefineCalls = new List<Tuple<SymbolKindEnum, string, ExprOp>>();
        }

        public IList<Tuple<SymbolKindEnum, string, ExprOp>> DefineCalls { get; private set; }
        public ExprOp LookupResult { get; set; }
        public IList<Tuple<SymbolKindEnum, string>> LookupCalls { get; private set; }

        public override string Description
        {
            get { return Desc; }
        }

        public override void Define(SymbolKindEnum kind, string name, ExprOp exprOp)
        {
            DefineCalls.Add(new Tuple<SymbolKindEnum, string, ExprOp>(kind, name, exprOp));
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            LookupCalls.Add(new Tuple<SymbolKindEnum, string>(kind, name));
            return LookupResult;
        }

        private readonly string Desc;
    }
}

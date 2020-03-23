// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Expressions;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Scopus
{
    [TestClass]
    public class SymbolScopeTests : TestFixture
    {
        [TestMethod]
        public void SymbolScope_Symbols_AreEmptyByDefault()
        {
            MockScope mockScope = new MockScope();
            SymbolScope symbolScope = new SymbolScope(mockScope);
            Assert.IsNull(symbolScope.Symbols);  // By default, Symbols is empty until Define is called
        }

        [TestMethod]
        public void SymbolScope_Description_ReturnsParentDesription()
        {
            MockScope mockScope = new MockScope();
            SymbolScope symbolScope = new SymbolScope(mockScope);
            Assert.AreEqual(mockScope.Description, symbolScope.Description);
        }

        [TestMethod]
        public void SymbolScope_Define_AddsSymbolToSymbols()
        {
            MockScope mockScope = new MockScope();
            SymbolScope symbolScope = new SymbolScope(mockScope);

            SymbolKindEnum kind = SymbolKindEnum.FUNCTION;
            string name = "the-name";
            ExprOp exprOp = new ExprOp(OpKindEnum.CONSTANTS);

            symbolScope.Define(kind, name, exprOp);

            Assert.AreEqual(1, symbolScope.Symbols.Count);
            Assert.AreEqual(kind, symbolScope.Symbols.First().Key.Kind);
            Assert.AreEqual(name, symbolScope.Symbols.First().Key.Name);
            Assert.AreEqual(exprOp, symbolScope.Symbols.First().Key.Definition);
            Assert.AreEqual(exprOp, symbolScope.Symbols[symbolScope.Symbols.First().Key]);
        }

        [TestMethod]
        public void SymbolScope_Lookup_TriesGetAValueFromSymbols()
        {
            MockScope mockScope = new MockScope();
            SymbolScope symbolScope = new SymbolScope(mockScope);

            SymbolKindEnum kind = SymbolKindEnum.FUNCTION;
            string name = "the-name";
            ExprOp exprOp = new ExprOp(OpKindEnum.CONSTANTS);
            symbolScope.Define(kind, name, exprOp);

            ExprOp result = symbolScope.Lookup(kind, name);
            Assert.AreEqual(exprOp, result);

            ExprOp result2 = symbolScope.Lookup(SymbolKindEnum.OPTION, "dummy");
            Assert.AreEqual(mockScope.LookupResult, result2);
        }

    }
}

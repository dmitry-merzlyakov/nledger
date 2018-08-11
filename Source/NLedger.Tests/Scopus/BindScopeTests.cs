// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
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
    public class BindScopeTests : TestFixture
    {
        [TestMethod]
        public void BindScope_Contructor_PopulatesGrandChild()
        {
            MockScope mockScope1 = new MockScope();
            MockScope mockScope2 = new MockScope();

            BindScope bindScope = new BindScope(mockScope1, mockScope2);

            Assert.AreEqual(mockScope1, bindScope.Parent);
            Assert.AreEqual(mockScope2, bindScope.GrandChild);
        }

        [TestMethod]
        public void BindScope_Description_IsGrandChildDescription()
        {
            MockScope mockScope1 = new MockScope();
            MockScope mockScope2 = new MockScope("grand-child");

            BindScope bindScope = new BindScope(mockScope1, mockScope2);

            Assert.AreEqual(mockScope2.Description, bindScope.Description);
        }

        [TestMethod]
        public void BindScope_Lookup_CallsGrandChildLookupFirst()
        {
            MockScope mockScope1 = new MockScope();
            MockScope mockScope2 = new MockScope("grand-child");

            BindScope bindScope = new BindScope(mockScope1, mockScope2);
            ExprOp lookupResult = bindScope.Lookup(SymbolKindEnum.FUNCTION, "dummy");

            Assert.AreEqual(mockScope2.LookupResult, lookupResult);
            Assert.AreEqual(1, mockScope2.LookupCalls.Count);
        }

        [TestMethod]
        public void BindScope_Lookup_CallsParentLookupNext()
        {
            MockScope mockScope1 = new MockScope();
            MockScope mockScope2 = new MockScope("grand-child");
            mockScope2.LookupResult = null;

            BindScope bindScope = new BindScope(mockScope1, mockScope2);
            ExprOp lookupResult = bindScope.Lookup(SymbolKindEnum.FUNCTION, "dummy");

            Assert.AreEqual(mockScope1.LookupResult, lookupResult);
            Assert.AreEqual(1, mockScope1.LookupCalls.Count);
            Assert.AreEqual(1, mockScope2.LookupCalls.Count);
        }

        [TestMethod]
        public void BindScope_Define_CallsParentAndGrandChildDefine()
        {
            MockScope mockScope1 = new MockScope();
            MockScope mockScope2 = new MockScope("grand-child");

            BindScope bindScope = new BindScope(mockScope1, mockScope2);
            bindScope.Define(SymbolKindEnum.FUNCTION, "dummy", new ExprOp());

            Assert.AreEqual(1, mockScope1.DefineCalls.Count);
            Assert.AreEqual(1, mockScope2.DefineCalls.Count);
        }

    }
}

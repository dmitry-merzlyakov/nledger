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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Scopus
{
    public class BindScopeTests : TestFixture
    {
        [Fact]
        public void BindScope_Contructor_PopulatesGrandChild()
        {
            MockScope mockScope1 = new MockScope();
            MockScope mockScope2 = new MockScope();

            BindScope bindScope = new BindScope(mockScope1, mockScope2);

            Assert.Equal(mockScope1, bindScope.Parent);
            Assert.Equal(mockScope2, bindScope.GrandChild);
        }

        [Fact]
        public void BindScope_Description_IsGrandChildDescription()
        {
            MockScope mockScope1 = new MockScope();
            MockScope mockScope2 = new MockScope("grand-child");

            BindScope bindScope = new BindScope(mockScope1, mockScope2);

            Assert.Equal(mockScope2.Description, bindScope.Description);
        }

        [Fact]
        public void BindScope_Lookup_CallsGrandChildLookupFirst()
        {
            MockScope mockScope1 = new MockScope();
            MockScope mockScope2 = new MockScope("grand-child");

            BindScope bindScope = new BindScope(mockScope1, mockScope2);
            ExprOp lookupResult = bindScope.Lookup(SymbolKindEnum.FUNCTION, "dummy");

            Assert.Equal(mockScope2.LookupResult, lookupResult);
            Assert.Equal(1, mockScope2.LookupCalls.Count);
        }

        [Fact]
        public void BindScope_Lookup_CallsParentLookupNext()
        {
            MockScope mockScope1 = new MockScope();
            MockScope mockScope2 = new MockScope("grand-child");
            mockScope2.LookupResult = null;

            BindScope bindScope = new BindScope(mockScope1, mockScope2);
            ExprOp lookupResult = bindScope.Lookup(SymbolKindEnum.FUNCTION, "dummy");

            Assert.Equal(mockScope1.LookupResult, lookupResult);
            Assert.Equal(1, mockScope1.LookupCalls.Count);
            Assert.Equal(1, mockScope2.LookupCalls.Count);
        }

        [Fact]
        public void BindScope_Define_CallsParentAndGrandChildDefine()
        {
            MockScope mockScope1 = new MockScope();
            MockScope mockScope2 = new MockScope("grand-child");

            BindScope bindScope = new BindScope(mockScope1, mockScope2);
            bindScope.Define(SymbolKindEnum.FUNCTION, "dummy", new ExprOp());

            Assert.Equal(1, mockScope1.DefineCalls.Count);
            Assert.Equal(1, mockScope2.DefineCalls.Count);
        }

    }
}

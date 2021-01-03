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
    public class ChildScopeTests : TestFixture
    {
        [Fact]
        public void ChildScope_Constructor_PopulatesParent()
        {
            MockScope parent = new MockScope();
            TestChildScope childScope = new TestChildScope(parent);

            Assert.Equal(parent, childScope.Parent);
        }

        [Fact]
        public void ChildScope_Define_CallsParentDefine()
        {
            MockScope parent = new MockScope();
            TestChildScope childScope = new TestChildScope(parent);

            childScope.Define(SymbolKindEnum.FUNCTION, "dummy", new ExprOp());

            Assert.Equal(1, parent.DefineCalls.Count);
        }

        [Fact]
        public void ChildScope_Lookup_CallsParentLookup()
        {
            MockScope parent = new MockScope() { LookupResult = new ExprOp() };
            TestChildScope childScope = new TestChildScope(parent);

            ExprOp result = childScope.Lookup(SymbolKindEnum.FUNCTION, "dummy");

            Assert.Equal(1, parent.LookupCalls.Count);
            Assert.Equal(result, parent.LookupResult);
        }

        private class TestChildScope : ChildScope
        {
            public TestChildScope(Scope parent) : base(parent)
            { }

            public override string Description
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}

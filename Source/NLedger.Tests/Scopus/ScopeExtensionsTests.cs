// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Scopus
{
    public class ScopeExtensionsTests : TestFixture
    {
        [Fact]
        public void ScopeExtensions_SearchScope_RequiresScopeObject()
        {
            Assert.Throws<ArgumentNullException>(() => ScopeExtensions.SearchScope<Scope>(null));
        }

        [Fact]
        public void ScopeExtensions_SearchScope_ReturnsCurrentScopeIfSearchTypeMatches()
        {
            MockScope scope = new MockScope();
            MockScope result = ScopeExtensions.SearchScope<MockScope>(scope);
            Assert.Equal(scope, result);
        }

        [Fact]
        public void ScopeExtensions_SearchScope_MakesReqursiveCallForBindScope()
        {
            MockScope parent = new MockScope();
            MockScope grandChild = new MockScope();
            BindScope bindScope = new BindScope(parent, grandChild);

            MockScope result = ScopeExtensions.SearchScope<MockScope>(bindScope);
            Assert.Equal(grandChild, result);
        }

        [Fact]
        public void ScopeExtensions_SearchScope_MakesReqursiveCallForChildScope()
        {
            MockScope parent = new MockScope();
            ChildScope childScope = new TestChildScope(parent);

            MockScope result = ScopeExtensions.SearchScope<MockScope>(childScope);
            Assert.Equal(parent, result);
        }

        [Fact]
        public void ScopeExtensions_SearchScope_ReturnsNullIfNoSuccess()
        {
            MockScope scope = new MockScope();
            Scope result = ScopeExtensions.SearchScope<TestChildScope>(scope);
            Assert.Null(result);
        }

        private class TestChildScope : ChildScope
        {
            public TestChildScope(Scope scope) : base(scope)
            {  }

            public override string Description
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}

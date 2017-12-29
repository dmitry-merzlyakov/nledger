// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Scopus
{
    [TestClass]
    public class ScopeExtensionsTests : TestFixture
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScopeExtensions_SearchScope_RequiresScopeObject()
        {
            ScopeExtensions.SearchScope<Scope>(null);
        }

        [TestMethod]
        public void ScopeExtensions_SearchScope_ReturnsCurrentScopeIfSearchTypeMatches()
        {
            MockScope scope = new MockScope();
            MockScope result = ScopeExtensions.SearchScope<MockScope>(scope);
            Assert.AreEqual(scope, result);
        }

        [TestMethod]
        public void ScopeExtensions_SearchScope_MakesReqursiveCallForBindScope()
        {
            MockScope parent = new MockScope();
            MockScope grandChild = new MockScope();
            BindScope bindScope = new BindScope(parent, grandChild);

            MockScope result = ScopeExtensions.SearchScope<MockScope>(bindScope);
            Assert.AreEqual(grandChild, result);
        }

        [TestMethod]
        public void ScopeExtensions_SearchScope_MakesReqursiveCallForChildScope()
        {
            MockScope parent = new MockScope();
            ChildScope childScope = new TestChildScope(parent);

            MockScope result = ScopeExtensions.SearchScope<MockScope>(childScope);
            Assert.AreEqual(parent, result);
        }

        [TestMethod]
        public void ScopeExtensions_SearchScope_ReturnsNullIfNoSuccess()
        {
            MockScope scope = new MockScope();
            Scope result = ScopeExtensions.SearchScope<TestChildScope>(scope);
            Assert.IsNull(result);
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

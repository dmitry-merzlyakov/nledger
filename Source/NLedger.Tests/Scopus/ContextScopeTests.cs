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
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Scopus
{
    [TestClass]
    public class ContextScopeTests : TestFixture
    {
        [TestMethod]
        public void ContextScope_Constructor_PopulatesParentTypeContextAndIsRequired()
        {
            MockScope parent = new MockScope();
            ContextScope contextScope = new ContextScope(parent, ValueTypeEnum.DateTime, true);
            Assert.AreEqual(parent, contextScope.Parent);
            Assert.AreEqual(ValueTypeEnum.DateTime, contextScope.TypeContext);
            Assert.IsTrue(contextScope.IsRequired);
        }

        [TestMethod]
        public void ContextScope_Description_ReturnsParentDescription()
        {
            MockScope parent = new MockScope("special-desc");
            ContextScope contextScope = new ContextScope(parent, ValueTypeEnum.DateTime, true);
            Assert.AreEqual("special-desc", contextScope.Description);
        }

    }
}

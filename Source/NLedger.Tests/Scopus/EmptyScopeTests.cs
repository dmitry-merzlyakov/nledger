// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
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
    public class EmptyScopeTests : TestFixture
    {
        [TestMethod]
        public void EmptyScope_Description_ReturnsEmptyConstant()
        {
            Assert.AreEqual(EmptyScope.EmptyDescription, new EmptyScope().Description);
        }

        [TestMethod]
        public void EmptyScope_Lookup_ReturnsNull()
        {
            Assert.IsNull(new EmptyScope().Lookup(SymbolKindEnum.COMMAND, "none"));
        }

        [TestMethod]
        public void EmptyScope_TypeRequired_IsFalse()
        {
            Assert.IsFalse(new EmptyScope().TypeRequired);
        }

        [TestMethod]
        public void EmptyScope_TypeContext_IsVoid()
        {
            Assert.AreEqual(ValueTypeEnum.Void, new EmptyScope().TypeContext);
        }

        [TestMethod]
        public void EmptyScope_Define_DoesNothing()
        {
            var scope = new EmptyScope();
            scope.Define(SymbolKindEnum.COMMAND, "none", null); // just to check that there is no exceptions
        }
    }
}

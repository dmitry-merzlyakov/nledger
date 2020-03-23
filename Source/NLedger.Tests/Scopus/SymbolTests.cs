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
    public class SymbolTests : TestFixture
    {
        [TestMethod]
        public void Symbol_Equals_ComparesByKind()
        {
            Symbol s1, s2, s3;

            s1 = new Symbol(SymbolKindEnum.FUNCTION, null, null);
            s2 = new Symbol(SymbolKindEnum.FUNCTION, null, null);
            s3 = new Symbol(SymbolKindEnum.OPTION, null, null);

            Assert.AreEqual(s1, s2);
            Assert.AreNotEqual(s1, s3);
        }

        [TestMethod]
        public void Symbol_Equals_ComparesByKindAndName()
        {
            Symbol s1, s2, s3;

            s1 = new Symbol(SymbolKindEnum.FUNCTION, "name-1", null);
            s2 = new Symbol(SymbolKindEnum.FUNCTION, "name-1", null);
            s3 = new Symbol(SymbolKindEnum.FUNCTION, "name-2", null);

            Assert.AreEqual(s1, s2);
            Assert.AreNotEqual(s1, s3);
        }

        [TestMethod]
        public void Symbol_Equals_DoesNotCompareByDefinition()
        {
            Symbol s1, s2, s3;

            s1 = new Symbol(SymbolKindEnum.FUNCTION, "name-1", new ExprOp(OpKindEnum.FUNCTION));
            s2 = new Symbol(SymbolKindEnum.FUNCTION, "name-1", new ExprOp(OpKindEnum.IDENT));
            s3 = new Symbol(SymbolKindEnum.FUNCTION, "name-1", new ExprOp(OpKindEnum.LAST));

            Assert.AreEqual(s1, s2);
            Assert.AreEqual(s1, s3);
        }

    }
}

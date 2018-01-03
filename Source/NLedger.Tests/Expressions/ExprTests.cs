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
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Expressions
{
    [TestClass]
    public class ExprTests : TestFixture
    {
        [TestMethod]
        public void Expr_Constructor_TakesExpressionsToCalculate()
        {
            Expr expr = new Expr("2+2");
            Value result = expr.Calc(new EmptyScope());
            Assert.AreEqual(4, result.AsLong);
        }

        [TestMethod]
        public void Expr_Equals_IgnoresNulls()
        {
            Expr expr = new Expr("2+2");
            Assert.IsFalse(expr.Equals((Expr)null));
        }

        [TestMethod]
        public void Expr_Equals_ComparesStrs()
        {
            Expr expr1 = new Expr("2+2");
            Expr expr2 = new Expr("2+2");
            Expr expr3 = new Expr("3+3");

            Assert.IsTrue(expr1.Equals(expr1));
            Assert.IsTrue(expr1.Equals(expr2));
            Assert.IsFalse(expr1.Equals(expr3));
            Assert.IsFalse(expr2.Equals(expr3));
        }

        [TestMethod]
        public void Expr_GetHashCode_ReturnsEqualValueForEqualExpressions()
        {
            Expr expr1 = new Expr("2+2");
            Expr expr2 = new Expr("2+2");
            Assert.AreEqual(expr1.GetHashCode(), expr2.GetHashCode());
        }

        [TestMethod]
        public void Expr_GetHashCode_ReturnsDifferentValuesForNonEqualExpressions()
        {
            Expr expr1 = new Expr("2+2");
            Expr expr2 = new Expr("2*2");
            Assert.AreNotEqual(expr1.GetHashCode(), expr2.GetHashCode());
        }

    }
}

// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Expressions
{
    public class ExprTests : TestFixture
    {
        [Fact]
        public void Expr_Constructor_TakesExpressionsToCalculate()
        {
            Expr expr = new Expr("2+2");
            Value result = expr.Calc(new EmptyScope());
            Assert.Equal(4, result.AsLong);
        }

        [Fact]
        public void Expr_Equals_IgnoresNulls()
        {
            Expr expr = new Expr("2+2");
            Assert.False(expr.Equals((Expr)null));
        }

        [Fact]
        public void Expr_Equals_ComparesStrs()
        {
            Expr expr1 = new Expr("2+2");
            Expr expr2 = new Expr("2+2");
            Expr expr3 = new Expr("3+3");

            Assert.True(expr1.Equals(expr1));
            Assert.True(expr1.Equals(expr2));
            Assert.False(expr1.Equals(expr3));
            Assert.False(expr2.Equals(expr3));
        }

        [Fact]
        public void Expr_GetHashCode_ReturnsEqualValueForEqualExpressions()
        {
            Expr expr1 = new Expr("2+2");
            Expr expr2 = new Expr("2+2");
            Assert.Equal(expr1.GetHashCode(), expr2.GetHashCode());
        }

        [Fact]
        public void Expr_GetHashCode_ReturnsDifferentValuesForNonEqualExpressions()
        {
            Expr expr1 = new Expr("2+2");
            Expr expr2 = new Expr("2*2");
            Assert.NotEqual(expr1.GetHashCode(), expr2.GetHashCode());
        }

    }
}

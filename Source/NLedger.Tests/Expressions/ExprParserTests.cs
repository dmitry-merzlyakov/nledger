// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Expressions
{
    public class ExprParserTests : TestFixture
    {
        [Fact]
        public void ExprParser_Parse_HandlesAddExpression()
        {
            string expr = "2+3";
            InputTextStream inStream = new InputTextStream(expr);

            ExprParser parser = new ExprParser();
            ExprOp result = parser.Parse(inStream);

            Assert.NotNull(result);
            Assert.Equal(OpKindEnum.O_ADD, result.Kind);
            Assert.Equal(2, result.Left.AsValue.AsLong);
            Assert.Equal(3, result.Right.AsValue.AsLong);

            // Just in case...
            EmptyScope scope = new EmptyScope();
            Value val = result.Calc(scope);
            Assert.Equal(5, val.AsLong);
        }

        [Fact]
        public void ExprParser_Parse_HandlesMultExpression()
        {
            string expr = "2*3";
            InputTextStream inStream = new InputTextStream(expr);

            ExprParser parser = new ExprParser();
            ExprOp result = parser.Parse(inStream);

            Assert.NotNull(result);
            Assert.Equal(OpKindEnum.O_MUL, result.Kind);
            Assert.Equal(2, result.Left.AsValue.AsLong);
            Assert.Equal(3, result.Right.AsValue.AsLong);
        }

        [Fact]
        public void ExprParser_Parse_HandlesSubtractExpression()
        {
            string expr = "4 - 3";  // #fix-expr-parser - note that '4-3' does not work (parse_quantity issue)
            InputTextStream inStream = new InputTextStream(expr);

            ExprParser parser = new ExprParser();
            ExprOp result = parser.Parse(inStream);

            Assert.NotNull(result);
            Assert.Equal(OpKindEnum.O_SUB, result.Kind);
            Assert.Equal(4, result.Left.AsValue.AsLong);
            Assert.Equal(3, result.Right.AsValue.AsLong);
        }

        [Fact]
        public void ExprParser_Parse_HandlesDivExpression()
        {
            string expr = "4/3";
            InputTextStream inStream = new InputTextStream(expr);

            ExprParser parser = new ExprParser();
            ExprOp result = parser.Parse(inStream);

            Assert.NotNull(result);
            Assert.Equal(OpKindEnum.O_DIV, result.Kind);
            Assert.Equal(4, result.Left.AsValue.AsLong);
            Assert.Equal(3, result.Right.AsValue.AsLong);
        }

        [Fact]
        public void ExprParser_Parse_HandlesExpressionsWithParenthesises()
        {
            string expr = "2+(3*(4 - 1)/5)";
            InputTextStream inStream = new InputTextStream(expr);

            ExprParser parser = new ExprParser();
            ExprOp result = parser.Parse(inStream);

            Assert.NotNull(result);
            Assert.Equal(OpKindEnum.O_ADD, result.Kind);
            Assert.Equal(2, result.Left.AsValue.AsLong);

            Assert.Equal(OpKindEnum.O_DIV, result.Right.Kind);
            Assert.Equal(5, result.Right.Right.AsValue.AsLong);

            Assert.Equal(OpKindEnum.O_MUL, result.Right.Left.Kind);
            Assert.Equal(3, result.Right.Left.Left.AsValue.AsLong);

            Assert.Equal(OpKindEnum.O_SUB, result.Right.Left.Right.Kind);
            Assert.Equal(4, result.Right.Left.Right.Left.AsValue.AsLong);
            Assert.Equal(1, result.Right.Left.Right.Right.AsValue.AsLong);
        }

        [Fact]
        public void ExprParser_ParseUnaryExpr_MINUS_Produces_O_NEG()
        {
            var parser = new ExprParser();
            var inStream = new InputTextStream("-SOMETHING");
            var exprOp = parser.ParseUnaryExpr(inStream, AmountParseFlagsEnum.PARSE_DEFAULT);
            Assert.Equal(OpKindEnum.O_NEG, exprOp.Kind);
        }

    }
}

// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Expressions
{
    public class ExprOpTests : TestFixture
    {
        [Fact]
        public void ExprOp_IsIdent_ChecksIdentOpKind()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.FUNCTION);
           
            ExprOp exprOp2 = new ExprOp(OpKindEnum.IDENT);
            exprOp2.AsIdent = "string";

            Assert.False(exprOp1.IsIdent);
            Assert.True(exprOp2.IsIdent);
        }

        [Fact]
        public void ExprOp_IsIdent_GetterRequiresStringData()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.IDENT);
            Assert.Throws<InvalidOperationException>(() => exprOp1.IsIdent);  // Causes exception because of empty (not a string) data
        }

        [Fact]
        public void ExprOp_Right_HoldsAnyExprOps()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_NOT);
            ExprOp exprOp2 = new ExprOp(OpKindEnum.O_NOT);

            Assert.Null(exprOp1.Right);
            exprOp1.Right = exprOp2;
            Assert.Equal(exprOp2, exprOp1.Right);
        }

        [Fact]
        public void ExprOp_Right_FailsIfCurrentExprOpIsLessThanTerminal()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.IDENT);
            Assert.Throws<InvalidOperationException>(() => exprOp1.Right);
        }

        [Fact]
        public void ExprOp_AsOp_ReturnsRightProperty()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_NOT);
            ExprOp exprOp2 = new ExprOp(OpKindEnum.O_NOT);
            exprOp1.Right = exprOp2;
            Assert.Equal(exprOp2, exprOp1.AsOp);
        }

        [Fact]
        public void ExprOp_Calc_O_GTE_ReturnsTrueIfLeftGreaterRight()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_GTE);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(2) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(1) };
            Assert.Equal(2 >= 1, exprOp1.Calc(null).Bool);
        }

        [Fact]
        public void ExprOp_Calc_O_GTE_ReturnsTrueIfLeftEqualsToRight()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_GTE);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(2) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(2) };
            Assert.Equal(2 >= 2, exprOp1.Calc(null).Bool);
        }

        [Fact]
        public void ExprOp_Calc_O_GTE_ReturnsFalseIfLeftLessThanRight()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_GTE);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(1) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(2) };
            Assert.Equal(1 >= 2, exprOp1.Calc(null).Bool);
        }

        [Fact]
        public void ExprOp_Calc_O_NOT_Considers_Int_0_ToBe_False_AndInversesIt()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_NOT);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(0) };
            Assert.True(exprOp1.Calc(null).Bool);
        }

        [Fact]
        public void ExprOp_Calc_O_NOT_Considers_Int_10_ToBe_True_AndInversesIt()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_NOT);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            Assert.False(exprOp1.Calc(null).Bool);
        }

        [Fact]
        public void ExprOp_Calc_O_AND_Considers_Left_Int_0_ToBe_False()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_AND);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(0) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.True };
            Assert.False(exprOp1.Calc(null).Bool);
        }

        [Fact]
        public void ExprOp_Calc_O_AND_Considers_Left_Int_10_ToBe_True()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_AND);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.True };
            Assert.True(exprOp1.Calc(null).Bool);
        }

        [Fact]
        public void ExprOp_Calc_O_OR_Considers_Left_Int_0_ToBe_False()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_OR);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(0) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.False };
            Assert.False(exprOp1.Calc(null).Bool);
        }

        [Fact]
        public void ExprOp_Calc_O_OR_Considers_Left_Int_10_ToBe_True()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_OR);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.False };
            Assert.True(exprOp1.Calc(null).Bool);
        }

        [Fact]
        public void ExprOp_Calc_O_ADD_DoesNotModifyOriginalLeftArgument()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_ADD);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(20) };
            Value result = exprOp1.Calc(null);
            Assert.Equal(10, exprOp1.Left.AsValue.AsLong);
            Assert.Equal(30, result.AsLong);
        }

        [Fact]
        public void ExprOp_Calc_O_SUB_DoesNotModifyOriginalLeftArgument()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_SUB);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(7) };
            Value result = exprOp1.Calc(null);
            Assert.Equal(10, exprOp1.Left.AsValue.AsLong);
            Assert.Equal(3, result.AsLong);
        }

        [Fact]
        public void ExprOp_Calc_O_MUL_DoesNotModifyOriginalLeftArgument()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_MUL);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(7) };
            Value result = exprOp1.Calc(null);
            Assert.Equal(10, exprOp1.Left.AsValue.AsLong);
            Assert.Equal(70, result.AsLong);
        }

        [Fact]
        public void ExprOp_Calc_O_DIV_DoesNotModifyOriginalLeftArgument()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_DIV);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(5) };
            Value result = exprOp1.Calc(null);
            Assert.Equal(10, exprOp1.Left.AsValue.AsLong);
            Assert.Equal(2, result.AsLong);
        }

        [Fact]
        public void ExprOp_Calc_O_OR_ReturnsLeftValueInCaseItIsConsideredAsTrue()
        {
            Value leftValue = Value.StringValue("some-left-string-value");
            Value rightValue = Value.False;

            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_OR);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = leftValue };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = rightValue };
            Value result = exprOp1.Calc(null);

            Assert.Equal("some-left-string-value", result.AsString);
        }

        [Fact]
        public void ExprOp_AsValue_ReturnsObjectReferenceToValue()
        {
            Value val = Value.StringValue("some-left-string-value");
            ExprOp exprOp1 = new ExprOp(OpKindEnum.VALUE) { AsValue = val };
            Value result = exprOp1.AsValue;
            Assert.True(Object.ReferenceEquals(val, result));
        }

        [Fact]
        public void ExprOp_Dump_ReturnsContentOfExprOp()
        {
            Value val = Value.StringValue("some-left-string-value");
            ExprOp exprOp1 = new ExprOp(OpKindEnum.VALUE) { AsValue = val };
            Assert.Equal("VALUE: \"some-left-string-value\" (0)", exprOp1.Dump().TrimEnd());

            ExprOp exprOp2 = new ExprOp(OpKindEnum.O_OR);
            exprOp2.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp2.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.False };
            Assert.Equal("O_OR (0)\n VALUE: 10 (0)\n VALUE: false (0)", exprOp2.Dump().TrimEnd().RemoveCarriageReturns());

        }

    }
}

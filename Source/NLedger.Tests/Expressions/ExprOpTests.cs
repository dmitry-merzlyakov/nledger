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
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Expressions
{
    [TestClass]
    public class ExprOpTests : TestFixture
    {
        [TestMethod]
        public void ExprOp_IsIdent_ChecksIdentOpKind()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.FUNCTION);
           
            ExprOp exprOp2 = new ExprOp(OpKindEnum.IDENT);
            exprOp2.AsIdent = "string";

            Assert.IsFalse(exprOp1.IsIdent);
            Assert.IsTrue(exprOp2.IsIdent);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExprOp_IsIdent_GetterRequiresStringData()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.IDENT);
            Assert.IsTrue(exprOp1.IsIdent);  // Causes exception because of empty (not a string) data
        }

        [TestMethod]
        public void ExprOp_Right_HoldsAnyExprOps()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_NOT);
            ExprOp exprOp2 = new ExprOp(OpKindEnum.O_NOT);

            Assert.IsNull(exprOp1.Right);
            exprOp1.Right = exprOp2;
            Assert.AreEqual(exprOp2, exprOp1.Right);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExprOp_Right_FailsIfCurrentExprOpIsLessThanTerminal()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.IDENT);
            Assert.IsNull(exprOp1.Right);
        }

        [TestMethod]
        public void ExprOp_AsOp_ReturnsRightProperty()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_NOT);
            ExprOp exprOp2 = new ExprOp(OpKindEnum.O_NOT);
            exprOp1.Right = exprOp2;
            Assert.AreEqual(exprOp2, exprOp1.AsOp);
        }

        [TestMethod]
        public void ExprOp_Calc_O_GTE_ReturnsTrueIfLeftGreaterRight()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_GTE);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(2) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(1) };
            Assert.AreEqual(2 >= 1, exprOp1.Calc(null).Bool);
        }

        [TestMethod]
        public void ExprOp_Calc_O_GTE_ReturnsTrueIfLeftEqualsToRight()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_GTE);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(2) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(2) };
            Assert.AreEqual(2 >= 2, exprOp1.Calc(null).Bool);
        }

        [TestMethod]
        public void ExprOp_Calc_O_GTE_ReturnsFalseIfLeftLessThanRight()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_GTE);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(1) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(2) };
            Assert.AreEqual(1 >= 2, exprOp1.Calc(null).Bool);
        }

        [TestMethod]
        public void ExprOp_Calc_O_NOT_Considers_Int_0_ToBe_False_AndInversesIt()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_NOT);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(0) };
            Assert.IsTrue(exprOp1.Calc(null).Bool);
        }

        [TestMethod]
        public void ExprOp_Calc_O_NOT_Considers_Int_10_ToBe_True_AndInversesIt()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_NOT);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            Assert.IsFalse(exprOp1.Calc(null).Bool);
        }

        [TestMethod]
        public void ExprOp_Calc_O_AND_Considers_Left_Int_0_ToBe_False()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_AND);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(0) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.True };
            Assert.IsFalse(exprOp1.Calc(null).Bool);
        }

        [TestMethod]
        public void ExprOp_Calc_O_AND_Considers_Left_Int_10_ToBe_True()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_AND);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.True };
            Assert.IsTrue(exprOp1.Calc(null).Bool);
        }

        [TestMethod]
        public void ExprOp_Calc_O_OR_Considers_Left_Int_0_ToBe_False()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_OR);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(0) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.False };
            Assert.IsFalse(exprOp1.Calc(null).Bool);
        }

        [TestMethod]
        public void ExprOp_Calc_O_OR_Considers_Left_Int_10_ToBe_True()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_OR);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.False };
            Assert.IsTrue(exprOp1.Calc(null).Bool);
        }

        [TestMethod]
        public void ExprOp_Calc_O_ADD_DoesNotModifyOriginalLeftArgument()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_ADD);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(20) };
            Value result = exprOp1.Calc(null);
            Assert.AreEqual(10, exprOp1.Left.AsValue.AsLong);
            Assert.AreEqual(30, result.AsLong);
        }

        [TestMethod]
        public void ExprOp_Calc_O_SUB_DoesNotModifyOriginalLeftArgument()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_SUB);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(7) };
            Value result = exprOp1.Calc(null);
            Assert.AreEqual(10, exprOp1.Left.AsValue.AsLong);
            Assert.AreEqual(3, result.AsLong);
        }

        [TestMethod]
        public void ExprOp_Calc_O_MUL_DoesNotModifyOriginalLeftArgument()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_MUL);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(7) };
            Value result = exprOp1.Calc(null);
            Assert.AreEqual(10, exprOp1.Left.AsValue.AsLong);
            Assert.AreEqual(70, result.AsLong);
        }

        [TestMethod]
        public void ExprOp_Calc_O_DIV_DoesNotModifyOriginalLeftArgument()
        {
            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_DIV);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(5) };
            Value result = exprOp1.Calc(null);
            Assert.AreEqual(10, exprOp1.Left.AsValue.AsLong);
            Assert.AreEqual(2, result.AsLong);
        }

        [TestMethod]
        public void ExprOp_Calc_O_OR_ReturnsLeftValueInCaseItIsConsideredAsTrue()
        {
            Value leftValue = Value.StringValue("some-left-string-value");
            Value rightValue = Value.False;

            ExprOp exprOp1 = new ExprOp(OpKindEnum.O_OR);
            exprOp1.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = leftValue };
            exprOp1.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = rightValue };
            Value result = exprOp1.Calc(null);

            Assert.AreEqual("some-left-string-value", result.AsString);
        }

        [TestMethod]
        public void ExprOp_AsValue_ReturnsObjectReferenceToValue()
        {
            Value val = Value.StringValue("some-left-string-value");
            ExprOp exprOp1 = new ExprOp(OpKindEnum.VALUE) { AsValue = val };
            Value result = exprOp1.AsValue;
            Assert.IsTrue(Object.ReferenceEquals(val, result));
        }

        [TestMethod]
        public void ExprOp_Dump_ReturnsContentOfExprOp()
        {
            Value val = Value.StringValue("some-left-string-value");
            ExprOp exprOp1 = new ExprOp(OpKindEnum.VALUE) { AsValue = val };
            Assert.AreEqual("VALUE: \"some-left-string-value\" (0)", exprOp1.Dump().TrimEnd());

            ExprOp exprOp2 = new ExprOp(OpKindEnum.O_OR);
            exprOp2.Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(10) };
            exprOp2.Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.False };
            Assert.AreEqual("O_OR (0)\r\n VALUE: 10 (0)\r\n VALUE: false (0)", exprOp2.Dump().TrimEnd());

        }

    }
}

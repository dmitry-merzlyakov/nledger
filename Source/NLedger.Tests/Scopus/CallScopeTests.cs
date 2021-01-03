// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Values;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Scopus
{
    public class CallScopeTests : TestFixture
    {
        [Fact]
        public void CallScope_Contructor_PopulatesParentLocusDepth()
        {
            MockScope parentScope = new MockScope();
            ExprOp locus = new ExprOp();
            int depth = 9;

            CallScope callScope = new CallScope(parentScope, locus, depth);

            Assert.Equal(parentScope, callScope.Parent);
            Assert.Equal(locus, callScope.Locus);
            Assert.Equal(depth, callScope.Depth);
        }

        [Fact]
        public void CallScope_Resolve_FailsIfIndexExceedsSize()
        {
            CallScope callScope = new CallScope(new MockScope());
            Assert.Throws<CalcError>(() => callScope.Resolve(0)); // Args is empty, size is 0
        }

        [Fact]
        public void CallScope_Resolve_DoesNotCalculateNonAnyValues()
        {
            CallScope callScope = new CallScope(new MockScope());
            callScope.PushFront(Value.Get(33));

            Value result = callScope.Resolve(0);

            Assert.Equal(33, result.AsLong);
        }

        [Fact]
        public void CallScope_Resolve_CallsAnyValuesAsExpr()
        {
            CallScope callScope = new CallScope(new MockScope());
            ExprOp testExprOp = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(55) };
            callScope.PushFront(Value.Get(testExprOp));

            Value result = callScope.Resolve(0, ValueTypeEnum.Integer, true);

            Assert.Equal(55, result.AsLong);
        }

        [Fact]
        public void CallScope_Resolve_UpdatesArgumentWithExpressionResult_ArgsIsSequence()
        {
            MockScope parent = new MockScope();
            CallScope callScope = new CallScope(parent);

            callScope.Args = Value.Get(new List<Value>());
            callScope.Args.AsSequence.Add(Value.Get(new Expr("22").Op));
            callScope.Args.AsSequence.Add(Value.Get(new Expr("\"A\"").Op));

            Value val1 = callScope.Resolve(0, ValueTypeEnum.Amount, true);
            Value val2 = callScope.Resolve(1, ValueTypeEnum.String, true);

            Assert.Equal(22, val1.AsLong);
            Assert.Equal("A", val2.AsString);

            Assert.Equal(val1, callScope.Args.AsSequence[0]);
            Assert.Equal(val2, callScope.Args.AsSequence[1]);
        }

        [Fact]
        public void CallScope_Resolve_UpdatesArgumentWithExpressionResult_ArgsIsSingleValue()
        {
            MockScope parent = new MockScope();
            CallScope callScope = new CallScope(parent);

            callScope.Args = Value.Get(new Expr("\"ABC\"").Op);
            Value val1 = callScope.Resolve(0, ValueTypeEnum.String, true);

            Assert.Equal("ABC", val1.AsString);
            Assert.Equal(val1, callScope.Args);
            Assert.Equal(val1.AsString, callScope.Args.AsSequence[0].AsString);
        }

        [Fact]
        public void CallScope_Value_MakesSureThatAllArgumentsAreResolved()
        {
            CallScope callScope = new CallScope(new MockScope());

            ExprOp testExprOp = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(55) };
            callScope.PushFront(Value.Get(testExprOp));

            Value result = callScope.Value();

            Assert.Equal(55, callScope.Args.AsSequence[0].AsLong);
        }

        [Fact]
        public void CallScope_Value_ThisIsEqualToResolve()
        {
            CallScope callScope = new CallScope(new MockScope());

            ExprOp testExprOp = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(55) };
            callScope.PushFront(Value.Get(testExprOp));

            Value result = callScope[0];

            Assert.Equal(55, result.AsLong);
        }

        [Fact]
        public void CallScope_PushFront_AddsItemsToFirstPosition()
        {
            CallScope callScope = new CallScope(new MockScope());

            callScope.PushFront(Value.Get(11));
            callScope.PushFront(Value.Get(22));
            callScope.PushFront(Value.Get(33));

            Assert.Equal(33, callScope.Args.AsSequence[0].AsLong);
            Assert.Equal(22, callScope.Args.AsSequence[1].AsLong);
            Assert.Equal(11, callScope.Args.AsSequence[2].AsLong);
        }

        [Fact]
        public void CallScope_PushBack_AddsItemsToLastPosition()
        {
            CallScope callScope = new CallScope(new MockScope());

            callScope.PushBack(Value.Get(11));
            callScope.PushBack(Value.Get(22));
            callScope.PushBack(Value.Get(33));

            Assert.Equal(11, callScope.Args.AsSequence[0].AsLong);
            Assert.Equal(22, callScope.Args.AsSequence[1].AsLong);
            Assert.Equal(33, callScope.Args.AsSequence[2].AsLong);
        }

        [Fact]
        public void CallScope_PopBack_RemovesLastItem()
        {
            CallScope callScope = new CallScope(new MockScope());

            callScope.PushBack(Value.Get(11));
            callScope.PushBack(Value.Get(22));
            callScope.PushBack(Value.Get(33));

            callScope.PopBack();

            Assert.Equal(11, callScope.Args.AsSequence[0].AsLong);
            Assert.Equal(22, callScope.Args.AsSequence[1].AsLong);
            Assert.Equal(2, callScope.Args.AsSequence.Count);
        }

        [Fact]
        public void CallScope_Size_ReflectsNumberOfArguments()
        {
            CallScope callScope = new CallScope(new MockScope());

            Assert.Equal(0, callScope.Size);
            callScope.PushBack(Value.Get(11));
            Assert.Equal(1, callScope.Size);
            callScope.PushBack(Value.Get(22));
            Assert.Equal(2, callScope.Size);
            callScope.PushBack(Value.Get(33));
            Assert.Equal(3, callScope.Size);
            callScope.PopBack();
            Assert.Equal(2, callScope.Size);
        }

        [Fact]
        public void CallScope_IsEmpty_IndicatesWhetherArgsAreEmpty()
        {
            CallScope callScope = new CallScope(new MockScope());

            Assert.True(callScope.IsEmpty);
            callScope.PushBack(Value.Get(11));
            Assert.False(callScope.IsEmpty);
            callScope.PopBack();
            Assert.True(callScope.IsEmpty);
        }

        [Fact]
        public void CallScope_Context_LooksForParentContextByType()
        {
            MockScope parent = new MockScope();
            CallScope callScope = new CallScope(parent);

            MockScope result = callScope.Context<MockScope>();

            Assert.Equal(parent, result);
        }

        [Fact]
        public void CallScope_Get_LooksForParentContextByType()
        {
            MockScope parent = new MockScope();
            CallScope callScope = new CallScope(parent);

            callScope.Args = Value.Get(new List<Value>());
            callScope.Args.AsSequence.Add(Value.Get(new Amount(22)));
            callScope.Args.AsSequence.Add(Value.Get(new Expr("\"A10\"").Op));
            callScope.Args.AsSequence.Add(Value.Get(new Balance()));
            callScope.Args.AsSequence.Add(Value.Get(true));
            callScope.Args.AsSequence.Add(Value.Get(new DateTime(2010, 10, 22)));
            callScope.Args.AsSequence.Add(Value.Get(33));
            callScope.Args.AsSequence.Add(Value.Get(new Mask("\\s")));
            callScope.Args.AsSequence.Add(Value.Get(new Account()));
            callScope.Args.AsSequence.Add(Value.Get(new List<Value>() { Value.Get(34), Value.StringValue("ABC") }));
            callScope.Args.AsSequence.Add(Value.StringValue("DFG"));
            callScope.Args.AsSequence.Add(Value.Empty);

            // Check Amount 22
            Assert.Equal(22, callScope.Get<Amount>(0).Quantity.ToLong());
            Assert.Equal(22, callScope.Get<Balance>(0).SingleAmount.Quantity.ToLong());
            Assert.True(callScope.Get<bool>(0));
            Assert.Equal(22, callScope.Get<int>(0));
            Assert.Equal(22, callScope.Get<long>(0));
            Assert.Equal(22, callScope.Get<IList<Value>>(0)[0].AsLong);
            Assert.Equal("22", callScope.Get<string>(0));

            // Check Expr "A"
            Assert.Equal(10, callScope.Get<Amount>(1, true).Quantity.ToLong());
            Assert.False(callScope.Get<bool>(1));
            Assert.Equal("A10", callScope.Get<IList<Value>>(1)[0].AsString);
            Assert.Equal("A10", callScope.Get<string>(1));

            // Check Balance
            Assert.NotNull(callScope.Get<Balance>(2));

            // Check Boolean
            Assert.True(callScope.Get<Boolean>(3));

            // Check DateTime
            Assert.Equal(new DateTime(2010, 10, 22), callScope.Get<DateTime>(4));

            // Check Integer
            Assert.Equal(33, callScope.Get<Amount>(5).Quantity.ToLong());
            Assert.Equal(33, callScope.Get<Balance>(5).SingleAmount.Quantity.ToLong());
            Assert.True(callScope.Get<bool>(5));
            Assert.Equal(33, callScope.Get<int>(5));
            Assert.Equal(33, callScope.Get<long>(5));
            Assert.Equal(33, callScope.Get<IList<Value>>(5)[0].AsLong);
            Assert.Equal("33", callScope.Get<string>(5));

            // Check Regex
            Assert.Equal(new Mask("\\s").ToString(), callScope.Get<Mask>(6).ToString());
            Assert.Equal("\\s", callScope.Get<string>(6));

            // Check Account
            Assert.NotNull(callScope.Get<Account>(7));

            // Check Sequence
            Assert.Equal(34, callScope.Get<IList<Value>>(8)[0].AsLong);
            Assert.Equal("ABC", callScope.Get<IList<Value>>(8)[1].AsString);

            // Check string
            Assert.Equal("DFG", callScope.Get<string>(9));

            // Check void
            Assert.Equal(default(int), callScope.Get<int>(10));
        }

        [Fact]
        public void CallScope_Size_EmptyArgsListDoesNotCauseSequenceCastException()
        {
            CallScope callScope = new CallScope(new MockScope());
            callScope.Args = Value.Empty;
            Assert.Equal(0, callScope.Size);
        }

        [Fact]
        public void CallScope_Get_ConvertsByDefault()
        {
            MockScope parent = new MockScope();
            CallScope callScope = new CallScope(parent);

            callScope.Args = Value.Get(new List<Value>());
            callScope.Args.AsSequence.Add(Value.Get(new Amount(22)));

            int result = callScope.Get<int>(0); // convert is not specified; "true" by default

            Assert.Equal(22, result); // Amount is automatically converted to int
        }

        [Fact]
        public void CallScope_Get_SupportsDate()
        {
            MockScope parent = new MockScope();
            CallScope callScope = new CallScope(parent);

            callScope.Args = Value.Get(new List<Value>());
            callScope.Args.AsSequence.Add(Value.Get(new Date(2015, 10, 22)));

            Date result = callScope.Get<Date>(0);

            Assert.Equal(new Date(2015, 10, 22), result);
        }

        [Fact]
        public void CallScope_Get_SupportsDateTime()
        {
            MockScope parent = new MockScope();
            CallScope callScope = new CallScope(parent);

            callScope.Args = Value.Get(new List<Value>());
            callScope.Args.AsSequence.Add(Value.Get(new DateTime(2015, 10, 22)));

            DateTime result = callScope.Get<DateTime>(0);

            Assert.Equal(new DateTime(2015, 10, 22), result);
        }

    }
}

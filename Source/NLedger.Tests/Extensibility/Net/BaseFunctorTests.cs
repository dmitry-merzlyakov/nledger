// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Extensibility.Net;
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility.Net
{
    public class BaseFunctorTests
    {
        [Fact]
        public void BaseFunctor_Selector_ReturnsValueFunctorForObjects()
        {
            var valueConverter = new ValueConverter();
            var val = new object();

            var func = BaseFunctor.Selector(val, valueConverter);

            Assert.IsType<ValueFunctor>(func);
            Assert.Equal(val, ((ValueFunctor)func).ObjectValue);
            Assert.Equal(valueConverter, ((ValueFunctor)func).ValueConverter);
        }

        [Fact]
        public void BaseFunctor_Selector_ReturnsMethodFunctorForDelegates()
        {
            var valueConverter = new ValueConverter();
            Func<int> dlg = () => 42;

            var func = BaseFunctor.Selector(dlg, valueConverter);

            Assert.IsType<MethodFunctor>(func);
            Assert.Equal("Invoke", ((MethodFunctor)func).Methods.First().Name);
            Assert.Equal(valueConverter, ((MethodFunctor)func).ValueConverter);
        }

        private class TestFunctor : BaseFunctor
        {
            public TestFunctor(IValueConverter valueConverter) : base(valueConverter)
            { }

            public override Value ExprFunc(Scope scope) => Value.Get(42);

            public new IEnumerable<object> GetParamList(CallScope callScope) => base.GetParamList(callScope);
        }

        [Fact]
        public void BaseFunctor_Constructor_PopulatesProperties()
        {
            var valueConverter = new ValueConverter();
            var func = new TestFunctor(valueConverter);
            Assert.Equal(valueConverter, func.ValueConverter);
        }

        [Fact]
        public void BaseFunctor_ExprFunctorDelegate_CallsExprFunc()
        {
            var valueConverter = new ValueConverter();
            var func = new TestFunctor(valueConverter);

            var result = func.ExprFunctor(new CallScope(new EmptyScope()));
            Assert.Equal(42, result.AsLong);
        }

        [Fact]
        public void BaseFunctor_GetParamList_ReturnsSequence()
        {
            var valueConverter = new ValueConverter();
            var func = new TestFunctor(valueConverter);

            var scope = new CallScope(new EmptyScope());
            scope.PushBack(Value.Get(10));
            scope.PushBack(Value.Get(20));

            var result = func.GetParamList(scope);

            Assert.Equal(new object[] { 10, 20 }, result.ToArray());
        }

        [Fact]
        public void BaseFunctor_GetParamList_ReturnsSequenceForSingleValue()
        {
            var valueConverter = new ValueConverter();
            var func = new TestFunctor(valueConverter);

            var scope = new CallScope(new EmptyScope());
            scope.PushBack(Value.Get(10));

            var result = func.GetParamList(scope);

            Assert.Equal(new object[] { 10 }, result.ToArray());
        }

    }
}

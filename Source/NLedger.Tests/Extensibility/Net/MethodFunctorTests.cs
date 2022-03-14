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
    public class MethodFunctorTests
    {
        [Fact]
        public void MethodFunctor_Constructor_PopulatesProperties()
        {
            var subject = new object();
            var methods = subject.GetType().GetMethods();
            var valueConverter = new ValueConverter();

            var methodFunctor = new MethodFunctor(subject, methods, valueConverter);

            Assert.Equal(subject, methodFunctor.Subject);
            Assert.Equal(methods, methodFunctor.Methods);
            Assert.Equal(valueConverter, methodFunctor.ValueConverter);
        }

        [Fact]
        public void MethodFunctor_Constructor_AcceptsEmptySubject()
        {
            var methodFunctor = new MethodFunctor(null, GetType().GetMethods(), new ValueConverter());
            Assert.Null(methodFunctor.Subject);
        }

        [Fact]
        public void MethodFunctor_Constructor_RequiresMethods()
        {
            Assert.Throws<ArgumentNullException>(() => new MethodFunctor(null, null, new ValueConverter()));
        }

        public class TestClass
        {
            public string Result { get; set; }
            public void ParameterlessMethod() => Result = "ParameterlessMethod";
            public void TwoParameterMethod(string s, int i) => Result = $"TwoParameterMethod('{s}',{i})";
            public void TwoParameterMethod2(string s, long i) => Result = $"TwoParameterMethod2('{s}',{i})";
            public string FunctionWithStringArgument(string s) =>  $"FunctionWithStringArgument('{s}')";
            public string OverloadedFunction1(string s) => $"OverloadedFunction1String('{s}')";
            public string OverloadedFunction1(int i) => $"OverloadedFunction1Int('{i}')";
            public string OverloadedFunction2(int i) => $"OverloadedFunction2('{i}')";
            public string OverloadedFunction2(int i, int j) => $"OverloadedFunction2('{i},{j}')";
            public string OverloadedFunction2(int i, int j, int k) => $"OverloadedFunction2('{i},{j},{k}')";
        }

        [Fact]
        public void MethodFunctor_ExprFunc_RequiresCallScope()
        {
            var methods = typeof(TestClass).GetMethods().Where(m => m.Name == "ParameterlessMethod").ToArray();
            var methodFunctor = new MethodFunctor(new TestClass(), methods, new ValueConverter());
            Assert.Throws<InvalidOperationException>(() => methodFunctor.ExprFunc(new EmptyScope()));
        }

        [Fact]
        public void MethodFunctor_ExprFunc_CallsParameterlessMethod()
        {
            var subject = new TestClass();
            var methods = typeof(TestClass).GetMethods().Where(m => m.Name == "ParameterlessMethod").ToArray();

            var methodFunctor = new MethodFunctor(subject, methods, new ValueConverter());
            var value = methodFunctor.ExprFunc(new CallScope(new EmptyScope()));

            Assert.Equal("ParameterlessMethod", subject.Result);
            Assert.True(Value.IsNullOrEmpty(value));
        }

        [Fact]
        public void MethodFunctor_ExprFunc_CallsMethodWithParameters()
        {
            var subject = new TestClass();
            var methods = typeof(TestClass).GetMethods().Where(m => m.Name == "TwoParameterMethod").ToArray();

            var methodFunctor = new MethodFunctor(subject, methods, new ValueConverter());

            var callScope = new CallScope(new EmptyScope());
            callScope.PushBack(Value.StringValue("text"));
            callScope.PushBack(Value.Get(10));

            var value = methodFunctor.ExprFunc(callScope);

            Assert.Equal("TwoParameterMethod('text',10)", subject.Result);
            Assert.True(Value.IsNullOrEmpty(value));
        }

        [Fact]
        public void MethodFunctor_ExprFunc_CallsMethodWithParametersWithImplicitConversion()
        {
            var subject = new TestClass();
            var methods = typeof(TestClass).GetMethods().Where(m => m.Name == "TwoParameterMethod2").ToArray();

            var methodFunctor = new MethodFunctor(subject, methods, new ValueConverter());

            var callScope = new CallScope(new EmptyScope());
            callScope.PushBack(Value.StringValue("text"));
            callScope.PushBack(Value.Get(10));      // Input parameter is Int32 value whereas the method has Int64 (long) argument

            var value = methodFunctor.ExprFunc(callScope);

            Assert.Equal("TwoParameterMethod2('text',10)", subject.Result);
            Assert.True(Value.IsNullOrEmpty(value));
        }

        [Fact]
        public void MethodFunctor_ExprFunc_ReturnsFunctionResult()
        {
            var subject = new TestClass();
            var methods = typeof(TestClass).GetMethods().Where(m => m.Name == "FunctionWithStringArgument").ToArray();

            var methodFunctor = new MethodFunctor(subject, methods, new ValueConverter());

            var callScope = new CallScope(new EmptyScope());
            callScope.PushBack(Value.StringValue("text"));

            var value = methodFunctor.ExprFunc(callScope);

            Assert.Equal("FunctionWithStringArgument('text')", value.AsString);
        }

        [Fact]
        public void MethodFunctor_ExprFunc_IgnoresAdditionalParameters()
        {
            var subject = new TestClass();
            var methods = typeof(TestClass).GetMethods().Where(m => m.Name == "FunctionWithStringArgument").ToArray();

            var methodFunctor = new MethodFunctor(subject, methods, new ValueConverter());

            var callScope = new CallScope(new EmptyScope());
            callScope.PushBack(Value.StringValue("text"));
            callScope.PushBack(Value.StringValue("text2"));     // This and next parameters are not needed: the function needs for only one argument
            callScope.PushBack(Value.StringValue("text3"));

            var value = methodFunctor.ExprFunc(callScope);

            Assert.Equal("FunctionWithStringArgument('text')", value.AsString);
        }

        [Fact]
        public void MethodFunctor_ExprFunc_SelectsAppropriateMethod()
        {
            var subject = new TestClass();
            var methods = typeof(TestClass).GetMethods().Where(m => m.Name == "OverloadedFunction1").ToArray();

            var methodFunctor = new MethodFunctor(subject, methods, new ValueConverter());

            var callScope = new CallScope(new EmptyScope());
            callScope.PushBack(Value.StringValue("text"));  // There are two functions 'OverloadedFunction1'; the functor should find the function with string argument

            var value = methodFunctor.ExprFunc(callScope);

            Assert.Equal("OverloadedFunction1String('text')", value.AsString);
        }

        [Fact]
        public void MethodFunctor_ExprFunc_SelectsBestMatch()
        {
            var subject = new TestClass();
            var methods = typeof(TestClass).GetMethods().Where(m => m.Name == "OverloadedFunction2").ToArray();

            var methodFunctor = new MethodFunctor(subject, methods, new ValueConverter());

            var callScope = new CallScope(new EmptyScope());
            callScope.PushBack(Value.Get(10));  // There are three functions OverloadedFunction2 (1, 2, 3 arguments
            callScope.PushBack(Value.Get(20));  // The functor should find the function with two arguments according to the param list

            var value = methodFunctor.ExprFunc(callScope);

            Assert.Equal("OverloadedFunction2('10,20')", value.AsString);
        }

    }
}

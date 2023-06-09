// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
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

namespace NLedger.Tests.Scopus
{
    public class OptionTests : TestFixture
    {
        [Fact]
        public void Option_FindOption_ReplacesMinusWithUnderscoreForFirstSearch()
        {
            MockScope scope = new MockScope();
            scope.LookupResult = new ExprOp();

            Tuple<ExprOp, bool> result = Option.FindOption(scope, "search-name");

            Assert.Equal(scope.LookupResult, result.Item1);
            Assert.True(result.Item2);
            Assert.Equal(SymbolKindEnum.OPTION, scope.LookupCalls.First().Item1);
            Assert.Equal("search_name_", scope.LookupCalls.First().Item2);
        }

        [Fact]
        public void Option_FindOption_RepeatsSearchingWithAnotherName()
        {
            MockScope scope = new MockScope();
            scope.LookupResult = null;

            Tuple<ExprOp, bool> result = Option.FindOption(scope, "search-name");

            Assert.Null(result.Item1);
            Assert.False(result.Item2);
            Assert.Equal(SymbolKindEnum.OPTION, scope.LookupCalls.Last().Item1);
            Assert.Equal("search_name", scope.LookupCalls.Last().Item2);
        }

        [Fact]
        public void Option_ProcessOption_PopulatesArgsAndCallsFunc()
        {
            OptFunctScopes.Clear();

            MockScope mockScope = new MockScope();
            string whence = "whence";
            ExprFunc opt = OptFunct;
            string arg = "arg";
            string name = "name";

            Option.ProcessOption(whence, opt, mockScope, arg, name);

            Assert.Equal(1, OptFunctScopes.Count);
            CallScope callScope = OptFunctScopes.First() as CallScope;
            Assert.NotNull(callScope);
            Assert.NotNull(callScope.Args);
            Assert.Equal(2, callScope.Size);
            Assert.Equal(whence, callScope[0].ToString());
            Assert.Equal(arg, callScope[1].ToString());
        }

        [Fact]
        public void Option_ProcessOption_LooksForOpByName()
        {
            OptFunctScopes.Clear();

            MockScope mockScope = new MockScope();
            mockScope.LookupResult = new ExprOp(OpKindEnum.FUNCTION);
            mockScope.LookupResult.AsFunction = OptFunct;

            string whence = "whence";
            string arg = "arg";
            string name = "name";

            Option.ProcessOption(whence, "dummy-name", mockScope, arg, name);

            Assert.Equal(1, OptFunctScopes.Count);
            CallScope callScope = OptFunctScopes.First() as CallScope;
            Assert.NotNull(callScope);
            Assert.NotNull(callScope.Args);
            Assert.Equal(2, callScope.Size);
            Assert.Equal(whence, callScope[0].ToString());
            Assert.Equal(arg, callScope[1].ToString());
        }

        [Fact]
        public void Option_ProcessOption_PassesThroughCountError()
        {
            ExprFunc func = s => { throw new CountError(1, String.Empty); };
            MockScope mockScope = new MockScope();

            Assert.Throws<CountError>(() => Option.ProcessOption("whence", func, mockScope, "arg", "name"));
        }

        [Fact]
        public void Option_ProcessEnvironment_HandlesOptionsByTagName()
        {
            OptFunctScopes.Clear();

            MockScope mockScope = new MockScope();
            mockScope.LookupResult = new ExprOp(OpKindEnum.FUNCTION);
            mockScope.LookupResult.AsFunction = OptFunct;

            IDictionary<string, string> envp = new Dictionary<string, string>();
            envp.Add("opt-1-1", "val1");
            envp.Add("opt-1-2", "val2");
            envp.Add("opt-2-1", "val3");
            envp.Add("opt-2-2", "val4");
            envp.Add("opt-3-1", "val5");
            envp.Add("opt-3-2", "val6");

            Option.ProcessEnvironment(envp, "opt-2", mockScope);

            Assert.Equal(2, OptFunctScopes.Count);
            CallScope callScope = OptFunctScopes.First() as CallScope;
            Assert.Equal(2, callScope.Size);
            Assert.Equal("$-1", callScope[0].ToString());
            Assert.Equal("val3", callScope[1].ToString());

            callScope = OptFunctScopes.Last() as CallScope;
            Assert.Equal(2, callScope.Size);
            Assert.Equal("$-2", callScope[0].ToString());
            Assert.Equal("val4", callScope[1].ToString());
        }

        [Fact]
        public void Option_ProcessEnvironment_ReplacesUndescoreToMinus()
        {
            OptFunctScopes.Clear();

            MockScope mockScope = new MockScope();
            mockScope.LookupResult = new ExprOp(OpKindEnum.FUNCTION);
            mockScope.LookupResult.AsFunction = OptFunct;

            IDictionary<string, string> envp = new Dictionary<string, string>();
            envp.Add("opt-1_1", "val1");

            Option.ProcessEnvironment(envp, "opt-1", mockScope);

            Assert.Equal(1, OptFunctScopes.Count);
            CallScope callScope = OptFunctScopes.First() as CallScope;
            Assert.Equal(2, callScope.Size);
            Assert.Equal("$-1", callScope[0].ToString());
        }

        [Fact]
        public void Option_ProcessEnvironment_IgnoresEmptyValues()
        {
            OptFunctScopes.Clear();

            MockScope mockScope = new MockScope();
            mockScope.LookupResult = new ExprOp(OpKindEnum.FUNCTION);
            mockScope.LookupResult.AsFunction = OptFunct;

            IDictionary<string, string> envp = new Dictionary<string, string>();
            envp.Add("opt-1-1", "val1");
            envp.Add("opt-1-2", "val2");
            envp.Add("opt-2-1", "");
            envp.Add("opt-2-2", "val4");
            envp.Add("opt-3-1", "val5");
            envp.Add("opt-3-2", "val6");

            Option.ProcessEnvironment(envp, "opt-2", mockScope);

            Assert.Equal(1, OptFunctScopes.Count);
            CallScope callScope = OptFunctScopes.First() as CallScope;
            Assert.Equal(2, callScope.Size);
            Assert.Equal("$-2", callScope[0].ToString());
            Assert.Equal("val4", callScope[1].ToString());
        }

        [Fact]
        public void Option_Constructor_PopulatesNameCharAndWantsArgs()
        {
            Option option1 = new Option(null);
            Assert.Null(option1.Name);
            Assert.False(option1.WantsArg);

            Option option2 = new Option("myname");
            Assert.Equal("myname", option2.Name);
            Assert.False(option2.WantsArg);

            Option option3 = new Option("myname_");
            Assert.Equal("myname_", option3.Name);
            Assert.True(option3.WantsArg);            
        }

        [Fact]
        public void Option_Constructor_PopulatesHandlerThunkDelegate()
        {
            HandlerThunkDelegate handlerThunkDelegate = (a, b) => { };
            Option option = new Option("myname_", handlerThunkDelegate);
            Assert.Equal("myname_", option.Name);
            Assert.True(option.WantsArg);
            Assert.Equal(handlerThunkDelegate, option.OnHandlerThunk);
        }

        [Fact]
        public void Option_Constructor_Desc_ReturnsUpdatedNameAndSuffix()
        {
            // WantsArg is false

            Option option1 = new Option("myname");
            Assert.Equal("myname", option1.Desc);

            Option option2 = new Option("my_name");
            Assert.Equal("my-name", option2.Desc);

            Option option3 = new Option("my_name", 'W');
            Assert.Equal("my-name (-W)", option3.Desc);

            // WantsArg is true

            Option option4 = new Option("myname_");
            Assert.Equal("myname", option4.Desc);

            Option option5 = new Option("my_name_");
            Assert.Equal("my-name", option5.Desc);

            Option option6 = new Option("my_name_", 'W');
            Assert.Equal("my-name (-W)", option6.Desc);
        }

        [Fact]
        public void Option_Report_ReturnsEmptyStringIfNotHandled()
        {
            Option option1 = new Option("myname");
            Assert.Equal(String.Empty, option1.Report());
        }

        [Fact]
        public void Option_Report_ReturnsDescAndSourceIfNoWantsArg()
        {
            Option option1 = new Option("myname");
            option1.On("source");  // to handle
            Assert.Equal("                  myname                                             source\n", option1.Report().RemoveCarriageReturns());
        }

        [Fact]
        public void Option_Report_ReturnsDescValueAndSourceIfWantsArg()
        {
            Option option1 = new Option("myname_");
            option1.Value = "val";
            option1.On("source");  // to handle
            Assert.Equal("                  myname = val                                       source\n", option1.Report().RemoveCarriageReturns());
        }

        [Fact]
        public void Option_Str_FailsIfNotHandled()
        {
            Option option1 = new Option("myname");
            Assert.Throws<InvalidOperationException>(() => option1.Str());
        }

        [Fact]
        public void Option_Str_FailsIfEmptyValue()
        {
            Option option1 = new Option("myname");
            option1.On("source");  // to handle
            Assert.Throws<InvalidOperationException>(() => option1.Str());
        }

        [Fact]
        public void Option_Str_ReturnsValue()
        {
            Option option1 = new Option("myname");
            option1.On("source");  // to handle
            option1.Value = "val";
            Assert.Equal("val", option1.Str());
        }

        [Fact]
        public void Option_On_CallsHandlerThunkSetsHandledAndPopulatesSource()
        {
            string calledWhence = null;
            Option option1 = new Option("myname", (a, b, c) => { calledWhence = b; });
            option1.On("source", null);
            Assert.Equal("source", calledWhence);
            Assert.True(option1.Handled);
            Assert.Null(option1.Value);
        }

        [Fact]
        public void Option_On_DoesNotSetValueIfItHasBeenChanged()
        {
            string calledWhence = null;
            Option option1 = new Option("myname", (a, b, c) => { calledWhence = b; a.Value = "val"; });
            option1.On("source", "str");
            Assert.Equal("source", calledWhence);
            Assert.True(option1.Handled);
            Assert.Equal("val", option1.Value);
            //Assert.Equal("source", option1.Source);
        }

        [Fact]
        public void Option_On_SetsValueIfItHasNotBeenChanged()
        {
            string calledWhence = null;
            Option option1 = new Option("myname", (a, b, c) => { calledWhence = b; });
            option1.On("source", "str");
            Assert.Equal("source", calledWhence);
            Assert.True(option1.Handled);
            Assert.Equal("str", option1.Value);
            //Assert.Equal("source", option1.Source);
        }

        [Fact]
        public void Option_Off_SetsHandledToFalse()
        {
            Option option1 = new Option("myname", (a, b, c) => { });
            option1.On("source");
            option1.Value = "val";
            option1.Off();
            Assert.False(option1.Handled);
            Assert.Equal(String.Empty, option1.Value);
            //Assert.Equal("source", option1.Source);
        }

        [Fact]
        public void Option_HandlerThunk_DoesNothingIfHandlerIsNotSpecified()
        {
            Option option1 = new Option("myname");
            option1.HandlerThunk("whence");
            option1.HandlerThunkStr("whence", "str");
        }

        [Fact]
        public void Option_HandlerThunk_CallsHandlerIfHandlerIsSpecified()
        {
            Option paramOpt = null;
            string paramWhence = null;
            string paramStr = null;

            Option option1 = new Option("myname", (a, b, c) => { paramOpt = a; paramWhence = b; paramStr = c; });
            option1.HandlerThunkStr("whence", null);

            Assert.Equal(option1, paramOpt);
            Assert.Equal("whence", paramWhence);
            Assert.Null(paramStr);

            // check the second signature

            Option option2 = new Option("myname", (a, b, c) => { paramOpt = a; paramWhence = b; paramStr = c; });
            option2.HandlerThunkStr("whence2", "str2");

            Assert.Equal(option2, paramOpt);
            Assert.Equal("whence2", paramWhence);
            Assert.Equal("str2", paramStr);
        }

        [Fact]
        public void Option_Handler_ChecksThatTheNumerOfArgumentsNotLessThan2WhenWantsArgIsYes()
        {
            Option option1 = new Option("myname_");
            CallScope scope = new CallScope(new EmptyScope());
            scope.PushFront(Value.Get(true));
            Assert.Equal(1, scope.Size);
            Assert.Throws<InvalidOperationException>(() => option1.Handler(scope));
        }

        [Fact]
        public void Option_Handler_ChecksThatTheNumerOfArgumentsNotMoreThan2WhenWantsArgIsYes()
        {
            Option option1 = new Option("myname_");
            CallScope scope = new CallScope(new EmptyScope());
            scope.PushFront(Value.Get(true));
            scope.PushFront(Value.Get(true));
            scope.PushFront(Value.Get(true));
            Assert.Equal(3, scope.Size);
            Assert.Throws<InvalidOperationException>(() => option1.Handler(scope));
        }

        [Fact]
        public void Option_Handler_ChecksThatTheFirstArgumentIsStringWhenWantsArgIsYes()
        {
            Option option1 = new Option("myname_");
            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushFront(Value.Get("string"));
            scope1.PushBack(Value.Get(true));
            option1.Handler(scope1);
            Assert.Equal("true", option1.Value); // Indicates that the call was successful

            Option option2 = new Option("myname_");
            CallScope scope2 = new CallScope(new EmptyScope());
            scope2.PushBack(Value.Get(true));
            scope2.PushBack(Value.Get(true));
            Assert.Throws<InvalidOperationException>(() => option2.Handler(scope2));
        }

        [Fact]
        public void Option_Handler_CallsOnWithTwoArgumentsIfWantsArgIsYes()
        {
            Option option1 = new Option("myname_");
            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushFront(Value.Get("whence"));
            scope1.PushBack(Value.Get("str"));
            option1.Handler(scope1);
            Assert.Equal("str", option1.Value);
            Assert.True(option1.Handled);
        }

        [Fact]
        public void Option_Handler_ChecksThatTheNumerOfArgumentsNotLessThan1WhenWantsArgIsNo()
        {
            Option option1 = new Option("myname");
            CallScope scope = new CallScope(new EmptyScope());
            Assert.Equal(0, scope.Size);
            Assert.Throws<InvalidOperationException>(() => option1.Handler(scope));
        }

        [Fact]
        public void Option_Handler_ChecksThatTheFirstArgumentIsStringWhenWantsArgIsNo()
        {
            Option option1 = new Option("myname");
            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushFront(Value.Get("string"));
            option1.Handler(scope1);
            Assert.True(option1.Handled); // Indicates that the call was successful

            Option option2 = new Option("myname");
            CallScope scope2 = new CallScope(new EmptyScope());
            scope2.PushBack(Value.Get(true));
            Assert.Throws<InvalidOperationException>(() => option2.Handler(scope2));
        }

        [Fact]
        public void Option_Handler_CallsOnWithOneArgumentIfWantsArgIsNo()
        {
            Option option1 = new Option("myname");
            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushFront(Value.Get("whence"));
            option1.Handler(scope1);
            Assert.True(option1.Handled);
        }

        [Fact]
        public void Option_Handler_CallsReturnsTrueValue()
        {
            Option option1 = new Option("myname");
            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushFront(Value.Get("whence"));
            Value val = option1.Handler(scope1);
            Assert.True(val.AsBoolean);
        }

        [Fact]
        public void Option_Call_CallsHandlerIfArgsNotEmpty()
        {
            Option option1 = new Option("myname_");
            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushFront(Value.Get("str"));
            Value val = option1.Call(scope1);
            Assert.True(val.AsBoolean);
            Assert.True(option1.Handled);
            Assert.Equal("str", option1.Value);
        }

        [Fact]
        public void Option_Call_ReturnsValueIfWantsArgsAndArgsAreEmpty()
        {
            Option option1 = new Option("myname_") { Value = "val" };
            CallScope scope1 = new CallScope(new EmptyScope());
            Value val = option1.Call(scope1);
            Assert.Equal("val", val.AsString);
            Assert.False(option1.Handled);
        }

        [Fact]
        public void Option_Call_ReturnsHandledIfNotWantsArgsAndArgsAreEmpty()
        {
            Option option1 = new Option("myname");
            CallScope scope1 = new CallScope(new EmptyScope());
            Value val = option1.Call(scope1);
            Assert.False(val.AsBoolean);

            option1.On("whence");

            Value val1 = option1.Call(scope1);
            Assert.True(val1.AsBoolean);
        }

        private Value OptFunct(Scope scope)
        {
            OptFunctScopes.Add(scope);
            return null;
        }

        private readonly IList<Scope> OptFunctScopes = new List<Scope>();

    }
}

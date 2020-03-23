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
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Scopus
{
    [TestClass]
    public class OptionTests : TestFixture
    {
        [TestMethod]
        public void Option_FindOption_ReplacesMinusWithUnderscoreForFirstSearch()
        {
            MockScope scope = new MockScope();
            scope.LookupResult = new ExprOp();

            Tuple<ExprOp, bool> result = Option.FindOption(scope, "search-name");

            Assert.AreEqual(scope.LookupResult, result.Item1);
            Assert.IsTrue(result.Item2);
            Assert.AreEqual(SymbolKindEnum.OPTION, scope.LookupCalls.First().Item1);
            Assert.AreEqual("search_name_", scope.LookupCalls.First().Item2);
        }

        [TestMethod]
        public void Option_FindOption_RepeatsSearchingWithAnotherName()
        {
            MockScope scope = new MockScope();
            scope.LookupResult = null;

            Tuple<ExprOp, bool> result = Option.FindOption(scope, "search-name");

            Assert.IsNull(result.Item1);
            Assert.IsFalse(result.Item2);
            Assert.AreEqual(SymbolKindEnum.OPTION, scope.LookupCalls.Last().Item1);
            Assert.AreEqual("search_name", scope.LookupCalls.Last().Item2);
        }

        [TestMethod]
        public void Option_ProcessOption_PopulatesArgsAndCallsFunc()
        {
            OptFunctScopes.Clear();

            MockScope mockScope = new MockScope();
            string whence = "whence";
            ExprFunc opt = OptFunct;
            string arg = "arg";
            string name = "name";

            Option.ProcessOption(whence, opt, mockScope, arg, name);

            Assert.AreEqual(1, OptFunctScopes.Count);
            CallScope callScope = OptFunctScopes.First() as CallScope;
            Assert.IsNotNull(callScope);
            Assert.IsNotNull(callScope.Args);
            Assert.AreEqual(2, callScope.Size);
            Assert.AreEqual(whence, callScope[0].ToString());
            Assert.AreEqual(arg, callScope[1].ToString());
        }

        [TestMethod]
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

            Assert.AreEqual(1, OptFunctScopes.Count);
            CallScope callScope = OptFunctScopes.First() as CallScope;
            Assert.IsNotNull(callScope);
            Assert.IsNotNull(callScope.Args);
            Assert.AreEqual(2, callScope.Size);
            Assert.AreEqual(whence, callScope[0].ToString());
            Assert.AreEqual(arg, callScope[1].ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(CountError))]
        public void Option_ProcessOption_PassesThroughCountError()
        {
            ExprFunc func = s => { throw new CountError(1, String.Empty); };
            MockScope mockScope = new MockScope();

            Option.ProcessOption("whence", func, mockScope, "arg", "name");
        }

        [TestMethod]
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

            Assert.AreEqual(2, OptFunctScopes.Count);
            CallScope callScope = OptFunctScopes.First() as CallScope;
            Assert.AreEqual(2, callScope.Size);
            Assert.AreEqual("$-1", callScope[0].ToString());
            Assert.AreEqual("val3", callScope[1].ToString());

            callScope = OptFunctScopes.Last() as CallScope;
            Assert.AreEqual(2, callScope.Size);
            Assert.AreEqual("$-2", callScope[0].ToString());
            Assert.AreEqual("val4", callScope[1].ToString());
        }

        [TestMethod]
        public void Option_ProcessEnvironment_ReplacesUndescoreToMinus()
        {
            OptFunctScopes.Clear();

            MockScope mockScope = new MockScope();
            mockScope.LookupResult = new ExprOp(OpKindEnum.FUNCTION);
            mockScope.LookupResult.AsFunction = OptFunct;

            IDictionary<string, string> envp = new Dictionary<string, string>();
            envp.Add("opt-1_1", "val1");

            Option.ProcessEnvironment(envp, "opt-1", mockScope);

            Assert.AreEqual(1, OptFunctScopes.Count);
            CallScope callScope = OptFunctScopes.First() as CallScope;
            Assert.AreEqual(2, callScope.Size);
            Assert.AreEqual("$-1", callScope[0].ToString());
        }

        [TestMethod]
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

            Assert.AreEqual(1, OptFunctScopes.Count);
            CallScope callScope = OptFunctScopes.First() as CallScope;
            Assert.AreEqual(2, callScope.Size);
            Assert.AreEqual("$-2", callScope[0].ToString());
            Assert.AreEqual("val4", callScope[1].ToString());
        }

        [TestMethod]
        public void Option_Constructor_PopulatesNameCharAndWantsArgs()
        {
            Option option1 = new Option(null);
            Assert.IsNull(option1.Name);
            Assert.IsFalse(option1.WantsArg);

            Option option2 = new Option("myname");
            Assert.AreEqual("myname", option2.Name);
            Assert.IsFalse(option2.WantsArg);

            Option option3 = new Option("myname_");
            Assert.AreEqual("myname_", option3.Name);
            Assert.IsTrue(option3.WantsArg);            
        }

        [TestMethod]
        public void Option_Constructor_PopulatesHandlerThunkDelegate()
        {
            HandlerThunkDelegate handlerThunkDelegate = (a, b) => { };
            Option option = new Option("myname_", handlerThunkDelegate);
            Assert.AreEqual("myname_", option.Name);
            Assert.IsTrue(option.WantsArg);
            Assert.AreEqual(handlerThunkDelegate, option.OnHandlerThunk);
        }

        [TestMethod]
        public void Option_Constructor_Desc_ReturnsUpdatedNameAndSuffix()
        {
            // WantsArg is false

            Option option1 = new Option("myname");
            Assert.AreEqual("myname", option1.Desc);

            Option option2 = new Option("my_name");
            Assert.AreEqual("my-name", option2.Desc);

            Option option3 = new Option("my_name", 'W');
            Assert.AreEqual("my-name (-W)", option3.Desc);

            // WantsArg is true

            Option option4 = new Option("myname_");
            Assert.AreEqual("myname", option4.Desc);

            Option option5 = new Option("my_name_");
            Assert.AreEqual("my-name", option5.Desc);

            Option option6 = new Option("my_name_", 'W');
            Assert.AreEqual("my-name (-W)", option6.Desc);
        }

        [TestMethod]
        public void Option_Report_ReturnsEmptyStringIfNotHandled()
        {
            Option option1 = new Option("myname");
            Assert.AreEqual(String.Empty, option1.Report());
        }

        [TestMethod]
        public void Option_Report_ReturnsDescAndSourceIfNoWantsArg()
        {
            Option option1 = new Option("myname");
            option1.On("source");  // to handle
            Assert.AreEqual("                  myname                                             source\r\n", option1.Report());
        }

        [TestMethod]
        public void Option_Report_ReturnsDescValueAndSourceIfWantsArg()
        {
            Option option1 = new Option("myname_");
            option1.Value = "val";
            option1.On("source");  // to handle
            Assert.AreEqual("                  myname = val                                       source\r\n", option1.Report());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Option_Str_FailsIfNotHandled()
        {
            Option option1 = new Option("myname");
            option1.Str();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Option_Str_FailsIfEmptyValue()
        {
            Option option1 = new Option("myname");
            option1.On("source");  // to handle
            option1.Str();
        }

        [TestMethod]
        public void Option_Str_ReturnsValue()
        {
            Option option1 = new Option("myname");
            option1.On("source");  // to handle
            option1.Value = "val";
            Assert.AreEqual("val", option1.Str());
        }

        [TestMethod]
        public void Option_On_CallsHandlerThunkSetsHandledAndPopulatesSource()
        {
            string calledWhence = null;
            Option option1 = new Option("myname", (a, b, c) => { calledWhence = b; });
            option1.On("source", null);
            Assert.AreEqual("source", calledWhence);
            Assert.IsTrue(option1.Handled);
            Assert.IsNull(option1.Value);
        }

        [TestMethod]
        public void Option_On_DoesNotSetValueIfItHasBeenChanged()
        {
            string calledWhence = null;
            Option option1 = new Option("myname", (a, b, c) => { calledWhence = b; a.Value = "val"; });
            option1.On("source", "str");
            Assert.AreEqual("source", calledWhence);
            Assert.IsTrue(option1.Handled);
            Assert.AreEqual("val", option1.Value);
            //Assert.AreEqual("source", option1.Source);
        }

        [TestMethod]
        public void Option_On_SetsValueIfItHasNotBeenChanged()
        {
            string calledWhence = null;
            Option option1 = new Option("myname", (a, b, c) => { calledWhence = b; });
            option1.On("source", "str");
            Assert.AreEqual("source", calledWhence);
            Assert.IsTrue(option1.Handled);
            Assert.AreEqual("str", option1.Value);
            //Assert.AreEqual("source", option1.Source);
        }

        [TestMethod]
        public void Option_Off_SetsHandledToFalse()
        {
            Option option1 = new Option("myname", (a, b, c) => { });
            option1.On("source");
            option1.Value = "val";
            option1.Off();
            Assert.IsFalse(option1.Handled);
            Assert.AreEqual(String.Empty, option1.Value);
            //Assert.AreEqual("source", option1.Source);
        }

        [TestMethod]
        public void Option_HandlerThunk_DoesNothingIfHandlerIsNotSpecified()
        {
            Option option1 = new Option("myname");
            option1.HandlerThunk("whence");
            option1.HandlerThunkStr("whence", "str");
        }

        [TestMethod]
        public void Option_HandlerThunk_CallsHandlerIfHandlerIsSpecified()
        {
            Option paramOpt = null;
            string paramWhence = null;
            string paramStr = null;

            Option option1 = new Option("myname", (a, b, c) => { paramOpt = a; paramWhence = b; paramStr = c; });
            option1.HandlerThunkStr("whence", null);

            Assert.AreEqual(option1, paramOpt);
            Assert.AreEqual("whence", paramWhence);
            Assert.AreEqual(null, paramStr);

            // check the second signature

            Option option2 = new Option("myname", (a, b, c) => { paramOpt = a; paramWhence = b; paramStr = c; });
            option2.HandlerThunkStr("whence2", "str2");

            Assert.AreEqual(option2, paramOpt);
            Assert.AreEqual("whence2", paramWhence);
            Assert.AreEqual("str2", paramStr);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Option_Handler_ChecksThatTheNumerOfArgumentsNotLessThan2WhenWantsArgIsYes()
        {
            Option option1 = new Option("myname_");
            CallScope scope = new CallScope(new EmptyScope());
            scope.PushFront(Value.Get(true));
            Assert.AreEqual(1, scope.Size);
            option1.Handler(scope);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Option_Handler_ChecksThatTheNumerOfArgumentsNotMoreThan2WhenWantsArgIsYes()
        {
            Option option1 = new Option("myname_");
            CallScope scope = new CallScope(new EmptyScope());
            scope.PushFront(Value.Get(true));
            scope.PushFront(Value.Get(true));
            scope.PushFront(Value.Get(true));
            Assert.AreEqual(3, scope.Size);
            option1.Handler(scope);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Option_Handler_ChecksThatTheFirstArgumentIsStringWhenWantsArgIsYes()
        {
            Option option1 = new Option("myname_");
            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushFront(Value.Get("string"));
            scope1.PushBack(Value.Get(true));
            option1.Handler(scope1);
            Assert.AreEqual("true", option1.Value); // Indicates that the call was successfull

            Option option2 = new Option("myname_");
            CallScope scope2 = new CallScope(new EmptyScope());
            scope2.PushBack(Value.Get(true));
            scope2.PushBack(Value.Get(true));
            option2.Handler(scope2);
        }

        [TestMethod]
        public void Option_Handler_CallsOnWithTwoArgumentsIfWantsArgIsYes()
        {
            Option option1 = new Option("myname_");
            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushFront(Value.Get("whence"));
            scope1.PushBack(Value.Get("str"));
            option1.Handler(scope1);
            Assert.AreEqual("str", option1.Value);
            Assert.IsTrue(option1.Handled);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Option_Handler_ChecksThatTheNumerOfArgumentsNotLessThan1WhenWantsArgIsNo()
        {
            Option option1 = new Option("myname");
            CallScope scope = new CallScope(new EmptyScope());
            Assert.AreEqual(0, scope.Size);
            option1.Handler(scope);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Option_Handler_ChecksThatTheFirstArgumentIsStringWhenWantsArgIsNo()
        {
            Option option1 = new Option("myname");
            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushFront(Value.Get("string"));
            option1.Handler(scope1);
            Assert.IsTrue(option1.Handled); // Indicates that the call was successfull

            Option option2 = new Option("myname");
            CallScope scope2 = new CallScope(new EmptyScope());
            scope2.PushBack(Value.Get(true));
            option2.Handler(scope2);
        }

        [TestMethod]
        public void Option_Handler_CallsOnWithOneArgumentIfWantsArgIsNo()
        {
            Option option1 = new Option("myname");
            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushFront(Value.Get("whence"));
            option1.Handler(scope1);
            Assert.IsTrue(option1.Handled);
        }

        [TestMethod]
        public void Option_Handler_CallsReturnsTrueValue()
        {
            Option option1 = new Option("myname");
            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushFront(Value.Get("whence"));
            Value val = option1.Handler(scope1);
            Assert.IsTrue(val.AsBoolean);
        }

        [TestMethod]
        public void Option_Call_CallsHandlerIfArgsNotEmpty()
        {
            Option option1 = new Option("myname_");
            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushFront(Value.Get("str"));
            Value val = option1.Call(scope1);
            Assert.IsTrue(val.AsBoolean);
            Assert.IsTrue(option1.Handled);
            Assert.AreEqual("str", option1.Value);
        }

        [TestMethod]
        public void Option_Call_ReturnsValueIfWantsArgsAndArgsAreEmpty()
        {
            Option option1 = new Option("myname_") { Value = "val" };
            CallScope scope1 = new CallScope(new EmptyScope());
            Value val = option1.Call(scope1);
            Assert.AreEqual("val", val.AsString);
            Assert.IsFalse(option1.Handled);
        }

        [TestMethod]
        public void Option_Call_ReturnsHandledIfNotWantsArgsAndArgsAreEmpty()
        {
            Option option1 = new Option("myname");
            CallScope scope1 = new CallScope(new EmptyScope());
            Value val = option1.Call(scope1);
            Assert.IsFalse(val.AsBoolean);

            option1.On("whence");

            Value val1 = option1.Call(scope1);
            Assert.IsTrue(val1.AsBoolean);
        }

        private Value OptFunct(Scope scope)
        {
            OptFunctScopes.Add(scope);
            return null;
        }

        private readonly IList<Scope> OptFunctScopes = new List<Scope>();

    }
}

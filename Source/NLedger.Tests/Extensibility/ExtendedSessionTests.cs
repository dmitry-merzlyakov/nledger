// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Extensibility;
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility
{
    public class ExtendedSessionTests
    {
        public class TestExtendedSessionProvider : IExtensionProvider
        {
            public ExtendedSession CreateExtendedSession() => new TestExtendedSession();
        }

        public class TestExtendedSession : ExtendedSession
        {
            public bool IsInitializedValue { get; set; }
            public string ImportOptionValue { get; private set; }

            public string LookupFunctionRequestedName { get; private set; }
            public bool IsPythonCommandCalled { get; private set; }
            public bool IsServerCommandCalled { get; private set; }

            public override void DefineGlobal(string name, object value)
            { }

            public override void Eval(string code, ExtensionEvalModeEnum mode)
            { }

            public override void ImportOption(string name) => ImportOptionValue = name;

            public override void Initialize()
            { }

            public override bool IsInitialized() => IsInitializedValue;

            protected override ExprOp LookupFunction(string name)
            {
                LookupFunctionRequestedName = name;
                return ExprOp.WrapValue(new Value());
            }

            public override Value PythonCommand(CallScope scope)
            {
                IsPythonCommandCalled = true;
                return new Value();
            }

            public override Value ServerCommand(CallScope scope)
            {
                IsServerCommandCalled = true;
                return new Value();
            }

        }

        [Fact]
        public void ExtendedSession_CreateDisposeLifeCycle()
        {
            var mainApplicationContext = new MainApplicationContext(new ApplicationServiceProvider(extensionProviderFactory: () => new TestExtendedSessionProvider()));
            using (mainApplicationContext.AcquireCurrentThread())
            {
                Assert.Null(ExtendedSession.Current);
                var session = ExtendedSession.CreateExtendedSession();

                Assert.NotNull(ExtendedSession.Current);
                Assert.Equal(session, ExtendedSession.Current);
                Assert.IsType<TestExtendedSession>(session);

                session.Dispose();
                Assert.Null(ExtendedSession.Current);
            }
        }

        [Fact]
        public void ExtendedSession_Lookup_CallsLookupFunction()
        {
            var mainApplicationContext = new MainApplicationContext(new ApplicationServiceProvider(extensionProviderFactory: () => new TestExtendedSessionProvider()));
            using (mainApplicationContext.AcquireCurrentThread())
            {
                using (var session = ExtendedSession.CreateExtendedSession() as TestExtendedSession)
                {
                    session.IsInitializedValue = true;

                    session.Lookup(SymbolKindEnum.FUNCTION, "function-name");
                    Assert.Equal("function-name", session.LookupFunctionRequestedName);

                    session.Lookup(SymbolKindEnum.OPTION, "option-name");
                    Assert.Equal("option_option-name", session.LookupFunctionRequestedName);
                }
            }
        }

        [Fact]
        public void ExtendedSession_Lookup_DoesNotCallLookupFunctionIfNotInitialized()
        {
            var mainApplicationContext = new MainApplicationContext(new ApplicationServiceProvider(extensionProviderFactory: () => new TestExtendedSessionProvider()));
            using (mainApplicationContext.AcquireCurrentThread())
            {
                using (var session = ExtendedSession.CreateExtendedSession() as TestExtendedSession)
                {
                    session.IsInitializedValue = false;

                    session.Lookup(SymbolKindEnum.FUNCTION, "function-name");
                    Assert.Null(session.LookupFunctionRequestedName);

                    session.Lookup(SymbolKindEnum.OPTION, "option-name");
                    Assert.Null(session.LookupFunctionRequestedName);
                }
            }
        }

        [Fact]
        public void ExtendedSession_Lookup_ManagesPythonAndServerPreCommands()
        {
            var mainApplicationContext = new MainApplicationContext(new ApplicationServiceProvider(extensionProviderFactory: () => new TestExtendedSessionProvider()));
            using (mainApplicationContext.AcquireCurrentThread())
            {
                using (var session = ExtendedSession.CreateExtendedSession() as TestExtendedSession)
                {
                    session.Lookup(SymbolKindEnum.PRECOMMAND, "python")?.Call(new Value(), new EmptyScope());
                    Assert.True(session.IsPythonCommandCalled);

                    session.Lookup(SymbolKindEnum.PRECOMMAND, "server")?.Call(new Value(), new EmptyScope());
                    Assert.True(session.IsServerCommandCalled);
                }
            }
        }

        [Fact]
        public void ExtendedSession_Lookup_ManagesImportHandler()
        {
            var mainApplicationContext = new MainApplicationContext(new ApplicationServiceProvider(extensionProviderFactory: () => new TestExtendedSessionProvider()));
            using (mainApplicationContext.AcquireCurrentThread())
            {
                using (var session = ExtendedSession.CreateExtendedSession() as TestExtendedSession)
                {
                    Assert.False(session.ImportHandler.Handled);

                    var args = new Value();
                    args.PushBack(Value.StringValue("importArgument1"));
                    args.PushBack(Value.StringValue("importArgument2"));

                    session.Lookup(SymbolKindEnum.OPTION, "import")?.Call(args, new EmptyScope());
                    Assert.Equal("importArgument2", session.ImportOptionValue);
                    Assert.True(session.ImportHandler.Handled);
                }
            }
        }

    }
}

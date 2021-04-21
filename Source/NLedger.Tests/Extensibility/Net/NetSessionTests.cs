using NLedger.Abstracts.Impl;
using NLedger.Extensibility.Net;
using NLedger.Utility.ServiceAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility.Net
{
    public class NetSessionTests
    {
        [Fact]
        // Net extension example: using .Net function File.Exists in 'check' directive. The function takes PATH tag value as a named parameter ('value').
        public void NetSession_IntegrationTest1()
        {
            var inputText = @"
tag PATH
    check System.IO.File.Exists(value)

2012-02-29 KFC
    ; PATH: test/baseline/feat-import_py.test
    Expenses:Food                $20
    Assets:Cash
";

            var engine = new ServiceEngine(
                createCustomProvider: mem =>
                {
                    return new ApplicationServiceProvider(
                        virtualConsoleProviderFactory: () => new VirtualConsoleProvider(mem.ConsoleInput, mem.ConsoleOutput, mem.ConsoleError),
                        extensionProviderFactory: () => new NetExtensionProvider(
                                () => new NetSession(new NamespaceResolver(true), new ValueConverter())  // enable global scan for all namespaces for this test to check how it works w/o 'import' directive
                            ));
                });

            var session = engine.CreateSession("-f /dev/stdin", inputText);
            Assert.True(session.IsActive, session.ErrorText);
            Assert.Equal("Warning: \"\", line 8: Metadata check failed for (PATH: test/baseline/feat-import_py.test): (((System.IO).File).Exists(value))", session.ErrorText.Trim());

            var response = session.ExecuteCommand("reg");
            Assert.False(response.HasErrors);
        }

        [Fact]
        // Net extension example: using a custom .Net function (customAssert) and a custom value (customValue) in 'check' directive
        public void NetSession_IntegrationTest2()
        {
            var inputText = @"
tag PATH
    check customAssert(value, customValue)

2012-02-29 KFC
    ; PATH: test/baseline/feat-import_py.test
    Expenses:Food                $20
    Assets:Cash
";

            var engine = new ServiceEngine(
                createCustomProvider: mem =>
                {
                    return new ApplicationServiceProvider(
                        virtualConsoleProviderFactory: () => new VirtualConsoleProvider(mem.ConsoleInput, mem.ConsoleOutput, mem.ConsoleError),
                        extensionProviderFactory: () => new NetExtensionProvider(
                                configureAction: extendedSession =>
                                {
                                    extendedSession.DefineGlobal("customValue", "test/baseline/feat-import_py.test");
                                    extendedSession.DefineGlobal("customAssert", (Func<string, string, bool>)((s1, s2) => s1 == s2 ));
                                }
                            ));
                });

            var session = engine.CreateSession("-f /dev/stdin", inputText);
            Assert.True(session.IsActive, session.ErrorText);

            // None warning messages expected if PATH tag value equals to the custom value. Otherwise, you get 'Metadata check failed' warning.
            Assert.True(String.IsNullOrWhiteSpace(session.ErrorText));

            var response = session.ExecuteCommand("reg");
            Assert.False(response.HasErrors);
        }

        [Fact]
        // Net extension example: custom options and using NLedger logger
        public void NetSession_IntegrationTest3()
        {
            var inputText = @"
--myfirst
--mysecond Hey
";
            // TODO 1) underscore 2) context is empty 3) custom object in global variable 4) use logger from context
            Action<object> myFirst = context => { NLedger.Utils.Logger.Current.Info(() => "In myFirst"); };
            Action<object,string> mySecond = (context, val) => { var a1 = context; };

            var engine = new ServiceEngine(
                createCustomProvider: mem =>
                {
                    return new ApplicationServiceProvider(
                        virtualConsoleProviderFactory: () => new VirtualConsoleProvider(mem.ConsoleInput, mem.ConsoleOutput, mem.ConsoleError),
                        extensionProviderFactory: () => new NetExtensionProvider(
                                configureAction: extendedSession =>
                                {
                                    extendedSession.DefineGlobal("option_myfirst_", myFirst);
                                    extendedSession.DefineGlobal("option_mysecond_", mySecond);
                                }
                            ));
                });

            var session = engine.CreateSession("-f /dev/stdin --trace 7", inputText);  // --trace 7 -> Set logging level to LOG_INFO
            Assert.True(session.IsActive, session.ErrorText);

            // TODO
            Assert.Contains("In myFirst", session.ErrorText);

            var response = session.ExecuteCommand("reg");
            Assert.False(response.HasErrors);
        }

        [Fact]
        // Net extension example: importing assemblies and aliases
        public void NetSession_IntegrationTest4()
        {
            var inputText = @"
#import assembly System.IO.FileSystem            # This name is for .Net Core
import assembly mscorlib            # This name is for .Net Core
import alias isfile for System.IO.File.Exists

tag PATH
    check isfile(value)

2012-02-29 KFC
    ; PATH: test/baseline/feat-import_py.test
    Expenses:Food                $20
    Assets:Cash
";

            var engine = new ServiceEngine(
                createCustomProvider: mem =>
                {
                    return new ApplicationServiceProvider(
                        virtualConsoleProviderFactory: () => new VirtualConsoleProvider(mem.ConsoleInput, mem.ConsoleOutput, mem.ConsoleError),
                        extensionProviderFactory: () => new NetExtensionProvider());
                });

            var session = engine.CreateSession("-f /dev/stdin", inputText);
            Assert.True(session.IsActive, session.ErrorText);
            Assert.Equal("Warning: \"\", line 8: Metadata check failed for (PATH: test/baseline/feat-import_py.test): (((System.IO).File).Exists(value))", session.ErrorText.Trim());

            var response = session.ExecuteCommand("reg");
            Assert.False(response.HasErrors);
        }

    }
}

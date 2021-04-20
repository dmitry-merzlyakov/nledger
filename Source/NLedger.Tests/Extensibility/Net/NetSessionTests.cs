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
    }
}

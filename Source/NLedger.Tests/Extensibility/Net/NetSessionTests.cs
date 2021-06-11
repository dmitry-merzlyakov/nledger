using NLedger.Abstracts.Impl;
using NLedger.Amounts;
using NLedger.Extensibility;
using NLedger.Extensibility.Net;
using NLedger.Utility.ServiceAPI;
using NLedger.Xacts;
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
import assembly System.IO.FileSystem            # This name is for .Net Core
import assembly mscorlib                        # This name is for .Net Framework
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
            Assert.Equal("Warning: \"\", line 12: Metadata check failed for (PATH: test/baseline/feat-import_py.test): isfile(value)", session.ErrorText.Trim());

            var response = session.ExecuteCommand("reg");
            Assert.False(response.HasErrors);
        }

        // Helper class for NetSession_IntegrationTest5
        public class TestAverageCalculator
        {
            public Amount Average { get; private set; }
            public int Count { get; private set; }
            public ISet<Xact> ProcessedXacts { get; } = new HashSet<Xact>();

            public bool Process(Xact xact, Amount amount)
            {
                if (Average == null)                                // It makes sense to initialize a common amount here because
                    Average = new Amount(0, amount.Commodity);      // this method is called when commodity context is initialized

                if (!ProcessedXacts.Contains(xact))
                {
                    ProcessedXacts.Add(xact);
                    Count++;
                    var count = new Amount(Count);
                    Average = Average + ((amount - Average) / count);
                }
                return true;                                        // All posts are accepted in this example
            }
        }

        [Fact]
        // Net extension example: average calculation across transactions
        public void NetSession_IntegrationTest5()
        {

            var journal = @"
commodity $
   format $1,000.00
   default

= expr 'averageCalculator(xact,amount)'
  [$account:AVERAGE]  ( calculatedAverage )
  [AVERAGE]  ( -calculatedAverage )
  
2012-02-29 KFC
    Expenses:Food                $20
    Assets:Cash
 
2012-03-01 KFC
    Expenses:Food                $5
    Assets:Cash
";

            var testAverageCalculator = new TestAverageCalculator();

            var engine = new ServiceEngine(
                 createCustomProvider: mem =>
                 {
                     return new ApplicationServiceProvider(
                          virtualConsoleProviderFactory: () => new VirtualConsoleProvider(mem.ConsoleInput, mem.ConsoleOutput, mem.ConsoleError),
                          extensionProviderFactory: () => new NetExtensionProvider(
                                     configureAction: extendedSession =>
                                     {
                                         extendedSession.DefineGlobal("averageCalculator", (Func<Xact, Amount, bool>)((x, a) => testAverageCalculator.Process(x, a)));
                                         extendedSession.DefineGlobal("calculatedAverage", (Func<Amount>)(() => testAverageCalculator.Average));
                                     }
                               ));
                 });

            var session = engine.CreateSession("-f /dev/stdin", journal);
            Assert.True(session.IsActive, session.ErrorText);

            var reg = session.ExecuteCommand("reg");
            Assert.False(reg.HasErrors, reg.ErrorText);
            Assert.Equal(@"
12-Feb-29 KFC                   Expenses:Food                $20.00       $20.00
                                Assets:Cash                 $-20.00            0
                                [Expense:Food:AVERAGE]       $20.00       $20.00
                                [AVERAGE]                   $-20.00            0
                                [Assets:Cash:AVERAGE]        $20.00       $20.00
                                [AVERAGE]                   $-20.00            0
12-Mar-01 KFC                   Expenses:Food                 $5.00        $5.00
                                Assets:Cash                  $-5.00            0
                                [Expense:Food:AVERAGE]       $12.50       $12.50
                                [AVERAGE]                   $-12.50            0
                                [Assets:Cash:AVERAGE]        $12.50       $12.50
                                [AVERAGE]                   $-12.50            0".NormalizeOutput(), reg.OutputText.NormalizeOutput());

            var bal = session.ExecuteCommand("bal");
            Assert.False(bal.HasErrors, bal.ErrorText);
            Assert.Equal(@"
               $7.50  Assets:Cash
              $32.50    AVERAGE
             $-65.00  AVERAGE
              $57.50  Expenses:Food
              $32.50    AVERAGE
--------------------
                   0".NormalizeOutput(), bal.OutputText.NormalizeOutput());

            var balReal = session.ExecuteCommand("bal --real");
            Assert.False(balReal.HasErrors, balReal.ErrorText);
            Assert.Equal(@"
             $-25.00  Assets:Cash
              $25.00  Expenses:Food
--------------------
                   0".NormalizeOutput(), balReal.OutputText.NormalizeOutput());

            var equity = session.ExecuteCommand("equity");
            Assert.False(equity.HasErrors, equity.ErrorText);
            Assert.Equal(@"
2012/03/01 Opening Balances
    Assets:Cash                              $-25.00
    [Assets:Cash:AVERAGE]                     $32.50
    [AVERAGE]                                $-65.00
    Expenses:Food                             $25.00
    [Expenses:Food:AVERAGE]                   $32.50".NormalizeOutput(), equity.OutputText.NormalizeOutput());

        }

    }
}

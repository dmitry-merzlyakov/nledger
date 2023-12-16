// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts.Impl;
using NLedger.Amounts;
using NLedger.Extensibility;
using NLedger.Extensibility.Net;
using NLedger.Utility.ServiceAPI;
using NLedger.Journals;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NLedger.Commodities;

namespace NLedger.Tests.Extensibility.Net
{
    public class NetSessionIntegrationTests
    {
        [Fact]
        // Net extension example (Service API): using .Net function File.Exists in 'check' directive. The function takes PATH tag value as a named parameter ('value').
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
        // Net extension example (Service API): using a custom .Net function (customAssert) and a custom value (customValue) in 'check' directive
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
        // Net extension example (Service API): custom options and using NLedger logger
        public void NetSession_IntegrationTest3()
        {
            var inputText = @"
--myfirst
--mysecond Hey
";
            Action<object> myFirst = whence => { NLedger.Utils.Logger.Current.Info(() => "In myFirst"); };
            Action<object,string> mySecond = (whence, val) => { NLedger.Utils.Logger.Current.Info(() => $"In mysecond: {val}"); };

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

            Assert.Contains("In myFirst", session.ErrorText);
            Assert.Contains("In mysecond: Hey", session.ErrorText);

            var response = session.ExecuteCommand("reg");
            Assert.False(response.HasErrors);
        }

        [Fact]
        // Net extension example (Service API): importing assemblies and aliases
        public void NetSession_IntegrationTest4()
        {
            var inputText = @"
import assembly System.IO.FileSystem            # This name is for .Net Core
import assembly System.Private.CoreLib          # This name is for .Net 6
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
            Assert.Equal("Warning: \"\", line 13: Metadata check failed for (PATH: test/baseline/feat-import_py.test): isfile(value)", session.ErrorText.Trim());

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
        // Net extension example (Service API): average calculation across transactions
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


        public static string NetSession_IntegrationTest6_Input = @"
D 1000.00 EUR
P 2011-01-01 GBP 1.2 EUR

2011-01-01 * Opening balance
    Assets:Bank                    10.00 GBP
    Equity:Opening balance

2012-01-02 * Test
    Assets:Bank                      5.00 GBP
    Income:Whatever

2012-01-03 * Test
    Assets:Bank
    Income:Whatever              -5.00 EUR @ 0.8733 GBP
";

        public static string NetSession_IntegrationTest6_Output = @"
-5.00 GBP
GBP
Total is presently: (0.00 EUR)
Converted to EUR:   (-5.73 EUR)
Total is now:       (-5.73 EUR)

-5.00 EUR {0.8733 GBP} [2012/01/03]
EUR
Total is presently: (-5.73 EUR)
Converted to EUR:   (-5.00 EUR)
Total is now:       (-10.73 EUR)

-10.73 EUR
";

        [Fact]
        // Net extension example (Standalone session): .Net standalone session (ported unit test 78AB4B87_py.test)
        public void NetSession_IntegrationTest6()
        {
            var sb = new StringBuilder();

            using(var session = NetSession.CreateStandaloneSession())
            {
                var eur = CommodityPool.Current.FindOrCreate("EUR");

                var totalEur = new Amount("0.00 EUR");
                var totalGbp = new Amount("0.00 GBP");
                var total = new Amount("0.00 EUR");

                foreach(var post in session.ReadJournalFromString(NetSession_IntegrationTest6_Input).Query("^income:"))
                {
                    sb.AppendLine($"{post.Amount}");
                    sb.AppendLine($"{post.Amount.Commodity.Symbol}");

                    if (post.Amount.Commodity.ToString() == "EUR")
                        totalEur += post.Amount;
                    else if (post.Amount.Commodity.ToString() == "GBP")
                        totalGbp += post.Amount;

                    var a = post.Amount.Value(default(DateTime), eur);
                    if((bool)a)
                    {
                        sb.AppendLine($"Total is presently: ({total})");
                        sb.AppendLine($"Converted to EUR:   ({a})");
                        total += a;
                        sb.AppendLine($"Total is now:       ({total})");
                    }
                    else
                    {
                        sb.AppendLine($"Cannot convert '{post.Amount}'");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine($"{total}");
            }

            Assert.Equal(NetSession_IntegrationTest6_Output.Trim(), sb.ToString().Trim());
        }

    }
}

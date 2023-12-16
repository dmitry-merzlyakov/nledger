// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts.Impl;
using NLedger.Utility.ServiceAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility.ServiceAPI
{
    public class ServiceAPIIntegrationTests
    {
        /// <summary>
        /// Simple Service API example
        /// </summary>
        [Fact]
        public void ServiceAPI_IntegrationTests_1()
        {
            var engine = new ServiceEngine();

            var session = engine.CreateSession("-f /dev/stdin", InputText);
            Assert.True(session.IsActive);

            var response = session.ExecuteCommand("bal checking --account=code");
            Assert.False(response.HasErrors);
            Assert.Equal(BalCheckingOutputText.Replace("\r", "").Trim(), response.OutputText.Trim());

            response = session.ExecuteCommand("bal checking --account=code");
            Assert.False(response.HasErrors);
            Assert.Equal(BalCheckingOutputText.Replace("\r", "").Trim(), response.OutputText.Trim());

            response = session.ExecuteCommand("bal");
            Assert.False(response.HasErrors);
            Assert.Equal(BalOutputText.Replace("\r", "").Trim(), response.OutputText.Trim());

            response = session.ExecuteCommand("reg");
            Assert.False(response.HasErrors);
            Assert.Equal(RegOutputText.Replace("\r", "").Trim(), response.OutputText.Trim());
        }

        /// <summary>
        /// Simple async Service API example
        /// </summary>
        [Fact]
        public void ServiceAPI_IntegrationTests_2()
        {
            var engine = new ServiceEngine();
            var response = ServiceAPI_IntegrationTests_2_Exec(engine).Result;
            Assert.False(response.HasErrors);
            Assert.Equal(BalCheckingOutputText.Replace("\r", "").Trim(), response.OutputText.Trim());
        }

        private async Task<ServiceResponse> ServiceAPI_IntegrationTests_2_Exec(ServiceEngine serviceEngine)
        {
            var session = await serviceEngine.CreateSessionAsync("-f /dev/stdin", InputText);
            var response = await session.ExecuteCommandAsync("bal checking --account=code");
            return response;
        }

        /// <summary>
        /// Simple multi-step async Service API example
        /// </summary>
        [Fact]
        public void ServiceAPI_IntegrationTests_3()
        {
            var engine = new ServiceEngine();
            var session = engine.CreateSessionAsync("-f /dev/stdin", InputText).Result;

            var ex1 = CheckSessionResponseOutput(session.ExecuteCommandAsync("bal checking --account=code").Result, BalCheckingOutputText);
            var ex2 = CheckSessionResponseOutput(session.ExecuteCommandAsync("bal").Result, BalOutputText);
            var ex3 = CheckSessionResponseOutput(session.ExecuteCommandAsync("reg").Result, RegOutputText);

            Assert.Null(CheckSessionResponseOutput(session.ExecuteCommandAsync("bal checking --account=code").Result, BalCheckingOutputText));
            Assert.Null(CheckSessionResponseOutput(session.ExecuteCommandAsync("bal").Result, BalOutputText));
            Assert.Null(CheckSessionResponseOutput(session.ExecuteCommandAsync("reg").Result, RegOutputText));
        }

        /// <summary>
        /// Simple multithreading Service API example
        /// </summary>
        [Fact]
        public void ServiceAPI_IntegrationTests_4()
        {
            var engine = new ServiceEngine();
            var session = engine.CreateSession("-f /dev/stdin", InputText);

            var tasks = new List<Task<Exception>>();
            for(int i=0; i<10; i++)
            {
                tasks.Add(Task.Run(() => CheckSessionResponseOutput(session.ExecuteCommandAsync("bal checking --account=code").Result, BalCheckingOutputText)));
                tasks.Add(Task.Run(() => CheckSessionResponseOutput(session.ExecuteCommandAsync("bal").Result, BalOutputText)));
                tasks.Add(Task.Run(() => CheckSessionResponseOutput(session.ExecuteCommandAsync("reg").Result, RegOutputText)));
            }

            Task.WhenAll(tasks).Wait();
            Assert.DoesNotContain(tasks, t => t.IsFaulted);
            Assert.True(tasks.All(t => t.Result == null));
        }

        /// <summary>
        /// Example of configuring ANSI writer
        /// </summary>
        [Fact]
        public void ServiceAPI_IntegrationTests_5()
        {
            var engine = new ServiceEngine(
                configureContext: context => { context.IsAtty = true; },
                createCustomProvider: mem =>
                {
                    mem.Attach(w => new MemoryAnsiTextWriter(w));
                    return null;
                });

            var session = engine.CreateSession("-f /dev/stdin", InputText);
            var response = session.ExecuteCommand("bal checking --account=code");
            Assert.False(response.HasErrors);
            Assert.Equal(BalCheckingOutputTextWithSpans.Replace("\r", "").Trim(), response.OutputText.Trim());
        }

        /// <summary>
        /// Example of configuring a virtual file system
        /// </summary>
        [Fact]
        public void ServiceAPI_IntegrationTests_6()
        {
            var fs = new MemoryFileSystemProvider();
            fs.CreateFile("input.dat", InputText);

            var engine = new ServiceEngine(
                createCustomProvider: mem =>
                {
                    return new ApplicationServiceProvider(
                        fileSystemProviderFactory: () => fs,
                        virtualConsoleProviderFactory: () => new VirtualConsoleProvider(mem.ConsoleInput, mem.ConsoleOutput, mem.ConsoleError));
                });

            var session = engine.CreateSession("-f input.dat");
            var response = session.ExecuteCommand("bal checking --account=code");
            Assert.False(response.HasErrors);
            Assert.Equal(BalCheckingOutputText.Replace("\r", "").Trim(), response.OutputText.Trim());
        }

        /// <summary>
        /// Example of setting environment variables
        /// </summary>
        [Fact]
        public void ServiceAPI_IntegrationTests_7()
        {
            var engine = new ServiceEngine(
                configureContext: context => 
                    {
                        context.SetEnvironmentVariables(new Dictionary<string, string>() { { "COLUMNS", "120" } });
                    });

            var session = engine.CreateSession("-f /dev/stdin", InputText);
            var response = session.ExecuteCommand("bal checking --account=code");
            Assert.False(response.HasErrors);
            Assert.Equal(BalCheckingOutputText.Replace("\r", "").Trim(), response.OutputText.Trim());
        }

        [Fact]
        public void ServiceAPI_IntegrationTests_8()
        {
            for (int n = 0; n < 2; n++)  // Repeat the same test 2 times. For intensive manual testing, it can be changed to 1000
            {
                var query = "bal checking --account=code";

                Func<string, Task<KeyValuePair<string, string>>> calculation = async ledgerQuery =>
                {
                    var session = await new ServiceEngine().CreateSessionAsync("-f /dev/stdin", InputText).ConfigureAwait(false);
                    var result = await session.ExecuteCommandAsync(ledgerQuery).ConfigureAwait(false);
                    if (result.HasErrors) throw new Exception(result.ErrorText);
                    return new KeyValuePair<string, string>(ledgerQuery, result.OutputText);
                };

                var tasks = Enumerable.Range(0, 100).AsParallel().Select(i => calculation(query)).ToArray();
                Task.WaitAll(tasks);

                Assert.DoesNotContain(tasks.Select(t => t.Result.Value), s => String.IsNullOrEmpty(s));
            }
        }

        /// <summary>
        /// Example of getting query results
        /// </summary>
        [Fact]
        public void ServiceAPI_IntegrationTests_9()
        {
            var inputText = @"
2012-03-17 Payee
    Expenses:Food                $20
    Assets:Cash";

            var engine = new ServiceEngine();
            var session = engine.CreateSession("-f /dev/stdin", inputText);

            var response = session.ExecuteQuery("^expenses:");
            Assert.False(response.HasErrors);

            var posts = response.Posts;
            Assert.Single(posts);

            // Note: once session response is received, current thread is out of NLedger application context.
            // It means that queried objects (posts in this example) are accessible, but provide limited functionality.
            // Some properties or functions are accessible; some might trigger runtime exceptions.
            var post = posts.Single();
            Assert.Equal("Expenses:Food", post.Account.ToString());
            Assert.Equal(20, post.Amount.Quantity.ToLong());

            // If you need to get full access to all properties and functions, you need to set NLedger application context.
            // (at least, for a limited scope like "using" below). In bounds of this scope, all NLedger functionality is available.
            using (response.MainApplicationContext.AcquireCurrentThread())
            {
                Assert.Equal("Payee", post.Payee);
                Assert.Equal("$20", post.Amount.Print());
            }
        }

        [Fact]
        public void ServiceAPI_ServiceSession_Dispose_Verification()
        {
            Task.Run(() => ServiceAPI_ServiceSession_Dispose_Verification_Action()).Wait();
        }

        private async Task ServiceAPI_ServiceSession_Dispose_Verification_Action()
        {
            var DailyLedger = @"
; Income
2011/11/21 Payment for hard work completed
   Bank:Paypal       $350.00
   Income:Hard Work
2012/7/1 Partial payment from Client X
   Bank:Paypal       $100
   Receivable:ClientX";

            var engine = new ServiceEngine();
            for (int i = 0; i < 2; i++)
            {
                var inputText = string.Join(Environment.NewLine, "", "", DailyLedger).Trim();
                using (var session = await engine.CreateSessionAsync("-f /dev/stdin", inputText).ConfigureAwait(false))
                {
                    var result = await session.ExecuteCommandAsync("bal").ConfigureAwait(false);
                    if (result.HasErrors) throw new Exception(result.ErrorText);
                    Assert.True(result.OutputText.Any());
                }
            }
        }


        private Exception CheckSessionResponseOutput(ServiceResponse serviceResponse, string expectedOutput)
        {
            try
            {
                Assert.NotNull(serviceResponse);
                Assert.False(serviceResponse.HasErrors);
                Assert.Equal(expectedOutput.Replace("\r", "").Trim(), serviceResponse.OutputText.Trim());
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private static readonly string InputText = @"
2009/10/29 (XFER) Panera Bread
    Expenses:Food               $4.50
    Assets:Checking

2009/10/30 (DEP) Pay day!
    Assets:Checking            $20.00
    Income

2009/10/30 (XFER) Panera Bread
    Expenses:Food               $4.50
    Assets:Checking

2009/10/31 (559385768438A8D7) Panera Bread
    Expenses:Food               $4.50
    Liabilities:Credit Card
";

        private static readonly string BalCheckingOutputText = @"
              $20.00  DEP:Assets:Checking
              $-9.00  XFER:Assets:Checking
--------------------
              $11.00
";

        private static readonly string BalCheckingOutputTextWithSpans = @"
              $20.00  <span class=""fg1"">DEP:Assets:Checking</span>
              <span class=""fg4"">$-9.00</span>  <span class=""fg1"">XFER:Assets:Checking</span>
--------------------
              $11.00
";

        private static readonly string BalOutputText = @"
              $11.00  Assets:Checking
              $13.50  Expenses:Food
             $-20.00  Income
              $-4.50  Liabilities:Credit Card
--------------------
                   0
";

        private static readonly string RegOutputText = @"
09-Oct-29 Panera Bread          Expenses:Food                 $4.50        $4.50
                                Assets:Checking              $-4.50            0
09-Oct-30 Pay day!              Assets:Checking              $20.00       $20.00
                                Income                      $-20.00            0
09-Oct-30 Panera Bread          Expenses:Food                 $4.50        $4.50
                                Assets:Checking              $-4.50            0
09-Oct-31 Panera Bread          Expenses:Food                 $4.50        $4.50
                                Liabilitie:Credit Card       $-4.50            0
";

    }
}

// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
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

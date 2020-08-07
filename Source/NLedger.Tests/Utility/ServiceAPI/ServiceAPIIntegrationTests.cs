// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Utility.ServiceAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utility.ServiceAPI
{
    [TestClass]
    public class ServiceAPIIntegrationTests
    {
        /// <summary>
        /// Simple Service API example
        /// </summary>
        [TestMethod]
        public void ServiceAPI_IntegrationTests_1()
        {
            var engine = new ServiceEngine();

            var session = engine.CreateSession("-f /dev/stdin", InputText);
            Assert.IsTrue(session.IsActive);

            var response = session.ExecuteCommand("bal checking --account=code");
            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(BalCheckingOutputText.Replace("\r", "").Trim(), response.OutputText.Trim());

            response = session.ExecuteCommand("bal checking --account=code");
            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(BalCheckingOutputText.Replace("\r", "").Trim(), response.OutputText.Trim());

            response = session.ExecuteCommand("bal");
            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(BalOutputText.Replace("\r", "").Trim(), response.OutputText.Trim());

            response = session.ExecuteCommand("reg");
            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(RegOutputText.Replace("\r", "").Trim(), response.OutputText.Trim());
        }

        /// <summary>
        /// Simple async Service API example
        /// </summary>
        [TestMethod]
        public void ServiceAPI_IntegrationTests_2()
        {
            var engine = new ServiceEngine();
            var response = ServiceAPI_IntegrationTests_2_Exec(engine).Result;
            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(BalCheckingOutputText.Replace("\r", "").Trim(), response.OutputText.Trim());
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
        [TestMethod]
        public void ServiceAPI_IntegrationTests_3()
        {
            var engine = new ServiceEngine();
            var session = engine.CreateSessionAsync("-f /dev/stdin", InputText).Result;

            var ex1 = CheckSessionResponseOutput(session.ExecuteCommandAsync("bal checking --account=code").Result, BalCheckingOutputText);
            var ex2 = CheckSessionResponseOutput(session.ExecuteCommandAsync("bal").Result, BalOutputText);
            var ex3 = CheckSessionResponseOutput(session.ExecuteCommandAsync("reg").Result, RegOutputText);

            Assert.IsNull(CheckSessionResponseOutput(session.ExecuteCommandAsync("bal checking --account=code").Result, BalCheckingOutputText));
            Assert.IsNull(CheckSessionResponseOutput(session.ExecuteCommandAsync("bal").Result, BalOutputText));
            Assert.IsNull(CheckSessionResponseOutput(session.ExecuteCommandAsync("reg").Result, RegOutputText));
        }

        /// <summary>
        /// Simple multithreading Service API example
        /// </summary>
        [TestMethod]
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
            Assert.IsFalse(tasks.Any(t => t.IsFaulted));
            Assert.IsTrue(tasks.All(t => t.Result == null));
        }

        private Exception CheckSessionResponseOutput(ServiceResponse serviceResponse, string expectedOutput)
        {
            try
            {
                Assert.IsNotNull(serviceResponse);
                Assert.IsFalse(serviceResponse.HasErrors);
                Assert.AreEqual(expectedOutput.Replace("\r", "").Trim(), serviceResponse.OutputText.Trim());
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

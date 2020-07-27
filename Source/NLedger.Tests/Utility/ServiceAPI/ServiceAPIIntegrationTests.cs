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
        [TestMethod]
        public void ServiceAPI_IntegrationTests_1()
        {
            var engine = new ServiceEngine();

            var session = engine.CreateSession("-f /dev/stdin", InputText);
            Assert.IsTrue(session.IsActive);

            var response = session.ExecuteCommand("bal checking --account=code");
            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(BalOutputText.Replace("\r", "").Trim(), response.OutputText.Trim());
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

        private static readonly string BalOutputText = @"
              $20.00  DEP:Assets:Checking
              $-9.00  XFER:Assets:Checking
--------------------
              $11.00
";
    }
}

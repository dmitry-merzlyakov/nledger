// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class SelectTests : TestFixture
    {
        [TestMethod]
        public void Select_SelectCommand_GetEmptyAccountsListTest()
        {
            MainApplicationContext.Current.SetVirtualConsoleProvider(() => new TestConsoleProvider());

            Report report = new Report(new Session());
            StringWriter output = new StringWriter();
            report.OutputStream = output;

            CallScope scope = new CallScope(report);
            scope.PushBack(Value.StringValue("account from accounts"));

            Value result = Select.SelectCommand(scope);
            string outString = output.ToString();

            Assert.IsTrue(String.IsNullOrEmpty(outString)); // No accounts
            Assert.IsTrue(result.AsBoolean);
        }
    }
}

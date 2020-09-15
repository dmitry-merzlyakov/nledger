// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class SelectTests : TestFixture
    {
        [Fact]
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

            Assert.True(String.IsNullOrEmpty(outString)); // No accounts
            Assert.True(result.AsBoolean);
        }
    }
}

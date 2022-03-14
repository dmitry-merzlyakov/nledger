// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class PreCmdTests : TestFixture
    {
        [Fact]
        public void PreCmd_ParseCommand_OutputsExpressionContent()
        {
            string command = "2+2";

            MemoryStream memoryStream = new MemoryStream();
            Report report = new Report(new Session()) { OutputStream = new StreamWriter(memoryStream) };
            CallScope callScope = new CallScope(report);
            callScope.PushBack(Value.StringValue(command));

            PreCmd.ParseCommand(callScope);

            report.OutputStream.Flush();
            memoryStream.Position = 0;
            string result = new StreamReader(memoryStream).ReadToEnd();

            string expected =
@"--- Context is first posting of the following transaction ---
2004/05/27 Book Store
    ; This note applies to all postings. :SecondTag:
    Expenses:Books                 20 BOOK @ $10
    ; Metadata: Some Value
    ; Typed:: $100 + $200
    ; :ExampleTag:
    ; Here follows a note describing the posting.
    Liabilities:MasterCard        $-200.00

--- Input expression ---
2+2
--- Text as parsed ---
(2 + 2)
--- Expression tree ---
O_ADD (0)
 VALUE: 2 (0)
 VALUE: 2 (0)
--- Compiled tree ---
O_ADD (0)
 VALUE: 2 (0)
 VALUE: 2 (0)
--- Calculated value ---
{4}
";
            Assert.Equal(0, CompareInfo.GetCompareInfo(CultureInfo.CurrentCulture.LCID).Compare(expected, result, CompareOptions.IgnoreSymbols));
        }
    }
}

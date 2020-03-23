// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Times;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Times
{
    [TestClass]
    public class DateParserLexerTests : TestFixture
    {
        [TestMethod]
        public void DateParserLexer_PeekToken_CallsNextTokenIfCacheIsNull()
        {
            var dateParserLexer = new DateParserLexer("100");
            dateParserLexer.TokenCache = null;

            var token = dateParserLexer.PeekToken();

            Assert.AreEqual(LexerTokenKindEnum.TOK_INT, token.Kind);
            Assert.AreEqual(100, token.Value.GetValue<int>());

            Assert.AreEqual(token, dateParserLexer.TokenCache);
        }
    }
}

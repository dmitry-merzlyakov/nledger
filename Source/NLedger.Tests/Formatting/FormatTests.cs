// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Accounts;
using NLedger.Formatting;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Formatting
{
    [TestClass]
    public class FormatTests : TestFixture
    {
        [TestMethod]
        public void Format_IntegrationTest_AccountLineFormat()
        {
            Report report = new Report(new Session());
            Account account = new Account(null, "test-account");

            string formatStr = "%(ansify_if(  justify(scrub(display_total), 20, 20 + int(prepend_width), true, color), bold if should_bold))  %(!options.flat ? depth_spacer : \"\")%-(ansify_if(   ansify_if(partial_account(options.flat), blue if color), bold if should_bold))";
            Format accountLineFormat = new Format(formatStr);

            BindScope boundScope = new BindScope(report, account);
            string result = accountLineFormat.Calc(boundScope);

            Assert.AreEqual("                   0  test-account", result);
        }

        [TestMethod]
        public void Format_Truncate_ReturnsOriginalStringIfWidthIsZero()
        {
            string ustr = "some-string";
            Assert.AreEqual(ustr, Format.Truncate(ustr, width: 0));
        }

        [TestMethod]
        public void Format_Truncate_ReturnsOriginalStringIfItIsLessOrEqualThanWidth()
        {
            string ustr = "some-string";
            Assert.AreEqual(ustr, Format.Truncate(ustr, width: ustr.Length));
            Assert.AreEqual(ustr, Format.Truncate(ustr, width: ustr.Length + 1));
        }

    }
}

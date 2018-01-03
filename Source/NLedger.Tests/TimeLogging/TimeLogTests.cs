// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Accounts;
using NLedger.Items;
using NLedger.Journals;
using NLedger.Textual;
using NLedger.TimeLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.TimeLogging
{
    [TestClass]
    public class TimeLogTests : TestFixture
    {
        [TestMethod]
        public void TimeLog_CreateTimelogXact_ProducesPostWithSpecialCommodity()
        {
            ItemPosition itemPosition = new ItemPosition();

            Journal journal = new Journal();
            Account account = journal.Master;

            TimeXact inEvent  = new TimeXact(itemPosition, new DateTime(2015, 10, 20, 2,  0, 0), account); // 3:30 - 2:00 = 90min
            TimeXact outEvent = new TimeXact(itemPosition, new DateTime(2015, 10, 20, 3, 30, 0), account); // 90 min = 5400s
            ParseContext parseContext = new ParseContext(@"c:\fakepath") { Journal = journal };

            TimeLog.CreateTimelogXact(inEvent, outEvent, parseContext);

            Assert.AreEqual("5400s", account.Posts.First().Amount.ToString());
        }
    }
}

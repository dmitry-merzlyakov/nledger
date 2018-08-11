// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Times
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class TimesCommonTests : TestFixture
    {
        [TestMethod]
        public void TimesCommon_ShowPeriodTokens_ReturnsTokensForGivenTime()
        {
            string result = TimesCommon.Current.ShowPeriodTokens("2012/10/22");
            string expected = "--- Period expression tokens ---\r\nTOK_DATE:  year 2012 month Oct day 22\r\nEND_REACHED: <EOF>\r\n";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TimesCommon_ToIsoExtendedString_MightHaveTimePart()
        {
            string result = TimesCommon.ToIsoExtendedString(new DateTime(2010, 10, 10, 23, 59, 30));
            string expected = "2010-10-10T23:59:30,0000000";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TimesCommon_ToIsoExtendedString_TimePartIsOptional()
        {
            string result = TimesCommon.ToIsoExtendedString(new DateTime(2010, 12, 25));
            string expected = "2010-12-25";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TimesCommon_TimesShutdown_ResetCustomEpoch()
        {
            TimesCommon timesCommon = new TimesCommon();
            timesCommon.Epoch = new DateTime(2017, 10, 10);
            timesCommon.TimesShutdown();
            Assert.IsNotNull(timesCommon.Epoch);
        }

        [TestMethod]
        public void TimesCommon_ParseDateMaskRoutine_ReplacesPointsToSlashes()
        {
            TimesCommon timesCommon = new TimesCommon();
            DateTraits dateTraits;
            var date = timesCommon.ParseDateMaskRoutine("2010.10.20", new DateIO("%Y/%m/%d", true), out dateTraits);
            Assert.AreEqual(new Date(2010, 10, 20), date);
        }

        [TestMethod]
        public void TimesCommon_ParseDateMaskRoutine_ReplacesMinusToSlashes()
        {
            TimesCommon timesCommon = new TimesCommon();
            DateTraits dateTraits;
            var date = timesCommon.ParseDateMaskRoutine("2010-10-20", new DateIO("%Y/%m/%d", true), out dateTraits);
            Assert.AreEqual(new Date(2010, 10, 20), date);
        }

        [TestMethod]
        public void TimesCommon_ParseDateTime_ReplacesMinusToSlashes()
        {
            TimesCommon timesCommon = new TimesCommon();
            timesCommon.TimesInitialize();
            var date = timesCommon.ParseDateTime("2012-03-16 06:47:12");
            Assert.AreEqual(new DateTime(2012, 03, 16, 06, 47, 12), date);
        }

        [TestMethod]
        public void TimesCommon_ParseDate_HandlesDatesWithOrWithoutLeadingZeros()
        {
            TimesCommon timesCommon = new TimesCommon();
            timesCommon.TimesInitialize();
            Assert.AreEqual(new Date(2010, 1, 2), timesCommon.ParseDate("2010/01/02"));
            Assert.AreEqual(new Date(2010, 1, 2), timesCommon.ParseDate("2010/01/2"));
            Assert.AreEqual(new Date(2010, 1, 2), timesCommon.ParseDate("2010/1/02"));
            Assert.AreEqual(new Date(2010, 1, 2), timesCommon.ParseDate("2010/1/2"));
        }

        [TestMethod]
        public void TimesCommon_FormatDate_WrittenFormatGeneratesLeadingZeros()
        {
            TimesCommon timesCommon = new TimesCommon();
            timesCommon.TimesInitialize();
            Assert.AreEqual("2010/01/02", timesCommon.FormatDate(new Date(2010, 1, 2), FormatTypeEnum.FMT_WRITTEN));
            Assert.AreEqual("2010/10/02", timesCommon.FormatDate(new Date(2010, 10, 2), FormatTypeEnum.FMT_WRITTEN));
            Assert.AreEqual("2010/01/20", timesCommon.FormatDate(new Date(2010, 1, 20), FormatTypeEnum.FMT_WRITTEN));
            Assert.AreEqual("2010/10/20", timesCommon.FormatDate(new Date(2010, 10, 20), FormatTypeEnum.FMT_WRITTEN));
        }
    }
}

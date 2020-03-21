// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLedger.Utility;

namespace NLedger.Tests.Utility
{
    [TestClass]
    public class DateExtensionTests : TestFixture
    {
        [TestMethod]
        public void DateExtension_IsNotADateTime_TrueForDefaultDateTime()
        {
            Assert.IsTrue(default(DateTime).IsNotADateTime());
        }

        [TestMethod]
        public void DateExtension_IsNotADateTime_FalseForNonDefaultDateTime()
        {
            Assert.IsFalse(default(DateTime).AddMilliseconds(1).IsNotADateTime());
        }

        [TestMethod]
        public void DateExtension_IsValid_FalseForDefaultDateTime()
        {
            Assert.IsFalse(default(DateTime).IsValid());
        }

        [TestMethod]
        public void DateExtension_IsValid_TrueForNonDefaultDateTime()
        {
            Assert.IsTrue(default(DateTime).AddMilliseconds(1).IsValid());
        }

        [TestMethod]
        public void DateExtension_IsNotADate_TrueForDefaultDate()
        {
            Assert.IsTrue(default(Date).IsNotADate());
        }

        [TestMethod]
        public void DateExtension_IsNotADate_FalseForNonDefaultDate()
        {
            Assert.IsFalse(default(Date).AddDays(1).IsNotADate());
        }

        [TestMethod]
        public void DateExtension_IsValid_FalseForDefaultDate()
        {
            Assert.IsFalse(default(Date).IsValid());
        }

        [TestMethod]
        public void DateExtension_IsValid_TrueForNonDefaultDate()
        {
            Assert.IsTrue(default(Date).AddDays(1).IsValid());
        }

        [TestMethod]
        public void DateExtension_ToPosixTime_ConvertsDateTimeToPosix()
        {
            Assert.AreEqual(0, new DateTime(1970, 1, 1).ToPosixTime());
            Assert.AreEqual(86400, new DateTime(1970, 1, 2).ToPosixTime());
            Assert.AreEqual(1510174618, new DateTime(2017, 11, 08, 20, 56, 58).ToPosixTime());
        }

        [TestMethod]
        public void DateExtension_CurrentTimeZone_ReturnsLocalIfNoneSpecified()
        {
            MainApplicationContext.Current.TimeZone = null;
            Assert.AreEqual(TimeZoneInfo.Local, DateExtension.CurrentTimeZone());
        }

        [TestMethod]
        public void DateExtension_CurrentTimeZone_ReturnsContextTimeZone()
        {
            MainApplicationContext.Current.TimeZone = TimeZoneInfo.Utc;
            Assert.AreEqual(TimeZoneInfo.Utc, DateExtension.CurrentTimeZone());
        }

        [TestMethod]
        public void DateExtension_CurrentTimeZone_ReturnsTimeZoneByCode()
        {
            MainApplicationContext.Current.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            Assert.AreEqual(TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"), DateExtension.CurrentTimeZone());
        }

    }
}

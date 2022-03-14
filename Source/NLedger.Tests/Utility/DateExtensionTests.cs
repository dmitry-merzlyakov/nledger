// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLedger.Utility;
using Xunit;

namespace NLedger.Tests.Utility
{
    public class DateExtensionTests : TestFixture
    {
        [Fact]
        public void DateExtension_IsNotADateTime_TrueForDefaultDateTime()
        {
            Assert.True(default(DateTime).IsNotADateTime());
        }

        [Fact]
        public void DateExtension_IsNotADateTime_FalseForNonDefaultDateTime()
        {
            Assert.False(default(DateTime).AddMilliseconds(1).IsNotADateTime());
        }

        [Fact]
        public void DateExtension_IsValid_FalseForDefaultDateTime()
        {
            Assert.False(default(DateTime).IsValid());
        }

        [Fact]
        public void DateExtension_IsValid_TrueForNonDefaultDateTime()
        {
            Assert.True(default(DateTime).AddMilliseconds(1).IsValid());
        }

        [Fact]
        public void DateExtension_IsNotADate_TrueForDefaultDate()
        {
            Assert.True(default(Date).IsNotADate());
        }

        [Fact]
        public void DateExtension_IsNotADate_FalseForNonDefaultDate()
        {
            Assert.False(default(Date).AddDays(1).IsNotADate());
        }

        [Fact]
        public void DateExtension_IsValid_FalseForDefaultDate()
        {
            Assert.False(default(Date).IsValid());
        }

        [Fact]
        public void DateExtension_IsValid_TrueForNonDefaultDate()
        {
            Assert.True(default(Date).AddDays(1).IsValid());
        }

        [Fact]
        public void DateExtension_ToPosixTime_ConvertsDateTimeToPosix()
        {
            Assert.Equal(0, new DateTime(1970, 1, 1).ToPosixTime());
            Assert.Equal(86400, new DateTime(1970, 1, 2).ToPosixTime());
            Assert.Equal(1510174618, new DateTime(2017, 11, 08, 20, 56, 58).ToPosixTime());
        }

        [Fact]
        public void DateExtension_CurrentTimeZone_ReturnsLocalIfNoneSpecified()
        {
            MainApplicationContext.Current.TimeZone = null;
            Assert.Equal(TimeZoneInfo.Local, DateExtension.CurrentTimeZone());
        }

        [Fact]
        public void DateExtension_CurrentTimeZone_ReturnsContextTimeZone()
        {
            MainApplicationContext.Current.TimeZone = TimeZoneInfo.Utc;
            Assert.Equal(TimeZoneInfo.Utc, DateExtension.CurrentTimeZone());
        }

        [Fact]
        public void DateExtension_CurrentTimeZone_ReturnsTimeZoneByCode()
        {
            var timeZone = TimeZoneInfo.GetSystemTimeZones().First();
            MainApplicationContext.Current.TimeZone = timeZone;
            Assert.Equal(timeZone, DateExtension.CurrentTimeZone());
        }

    }
}

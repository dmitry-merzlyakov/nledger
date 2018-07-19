// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public static class DateExtension
    {
        public static bool IsNotADateTime(this DateTime dateTime)
        {
            return dateTime == default(DateTime);
        }

        public static bool IsValid(this DateTime dateTime)
        {
            return !dateTime.IsNotADateTime();
        }

        public static bool IsNotADate(this Date date)
        {
            return date == default(Date);
        }

        public static bool IsValid(this Date date)
        {
            return !date.IsNotADate();
        }

        public static DateTime FromLocalToUtc(this Date date)
        {
            return ((DateTime)date).FromLocalToUtc();
        }

        public static TimeZoneInfo CurrentTimeZone()
        {
            return MainApplicationContext.Current.TimeZone ?? TimeZoneInfo.Local;
        }

        public static DateTime FromLocalToUtc(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, CurrentTimeZone());
        }

        public static int ToPosixTime(this DateTime dateTime)
        {
            return (int)(dateTime.Subtract(UnixZeroTime)).TotalSeconds;
        }

        private static readonly DateTime UnixZeroTime = new DateTime(1970, 1, 1);
    }
}

// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using NLedger.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Xunit;

namespace NLedger.Tests.Utility
{
    public class DateTests
    {
        [Fact]
        public void Date_HasTimePart_ReturnsTrueIfDateTimeHasTime()
        {
            Assert.True(Date.HasTimePart(default(DateTime).AddHours(2)));
        }

        [Fact]
        public void Date_HasTimePart_ReturnsFalseIfDateTimeHasNoTime()
        {
            Assert.False(Date.HasTimePart(default(DateTime).AddDays(2)));
        }

        [Fact]
        public void Date_ImplicitConversionToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            DateTime dateTime = date;  // Implicit casting Date to DateTime
            Assert.Equal(new DateTime(2015, 10, 22), dateTime);
        }

        [Fact]
        public void Date_ExplicitConversionToDate()
        {
            DateTime dateTime = new DateTime(2015, 10, 22);
            Date date = (Date)dateTime;  // Explicit casting Date to DateTime
            Assert.Equal(new Date(2015, 10, 22), date);
        }

        [Fact]
        public void Date_ExplicitConversionToDate_FailsInCaseOfTimePart()
        {
            DateTime dateTime = new DateTime(2015, 10, 22, 10, 20, 30);
            Assert.Throws<ArgumentException>(() => (Date)dateTime);
        }

        [Fact]
        public void Date_TryParseExact_ProducesDate()
        {
            Date date;
            string s = "2015/10/22";
            Assert.True(Date.TryParseExact(s, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date));
            Assert.Equal(new Date(2015, 10, 22), date);
        }

        [Fact]
        public void Date_TryParseExact_ReturnsFalseInCaseOfTimePart()
        {
            Date date;
            string s = "2015/10/22 11:11:11";
            Assert.False(Date.TryParseExact(s, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date));
        }

        [Fact]
        public void Date_EqualOperator_ComparesTwoDates()
        {
            Assert.True(new Date() == new Date());
            Assert.True(new Date(2015, 10, 22) == new Date(2015, 10, 22));
            Assert.False(new Date(2005, 10, 22) == new Date(2025, 10, 22));
        }

        [Fact]
        public void Date_NotEqualOperator_ComparesTwoDates()
        {
            Assert.False(new Date() != new Date());
            Assert.False(new Date(2015, 10, 22) != new Date(2015, 10, 22));
            Assert.True(new Date(2005, 10, 22) != new Date(2025, 10, 22));
        }

        [Fact]
        public void Date_LessOperator_ComparesTwoDates()
        {
            Assert.False(new Date() < new Date());
            Assert.False(new Date(2015, 10, 22) < new Date(2015, 10, 22));
            Assert.False(new Date(2015, 10, 23) < new Date(2015, 10, 22));
            Assert.True(new Date(2015, 10, 22) < new Date(2015, 10, 23));
        }

        [Fact]
        public void Date_GreaterOperator_ComparesTwoDates()
        {
            Assert.False(new Date() > new Date());
            Assert.False(new Date(2015, 10, 22) > new Date(2015, 10, 22));
            Assert.False(new Date(2015, 10, 22) > new Date(2015, 10, 23));
            Assert.True(new Date(2015, 10, 23) > new Date(2015, 10, 22));
        }

        [Fact]
        public void Date_LessOrEqualOperator_ComparesTwoDates()
        {
            Assert.True(new Date() <= new Date());
            Assert.True(new Date(2015, 10, 22) <= new Date(2015, 10, 22));
            Assert.True(new Date(2015, 10, 22) <= new Date(2015, 10, 23));
            Assert.False(new Date(2015, 10, 23) <= new Date(2015, 10, 22));
        }

        [Fact]
        public void Date_GreaterOrEqualOperator_ComparesTwoDates()
        {
            Assert.True(new Date() >= new Date());
            Assert.True(new Date(2015, 10, 22) >= new Date(2015, 10, 22));
            Assert.False(new Date(2015, 10, 22) >= new Date(2015, 10, 23));
            Assert.True(new Date(2015, 10, 23) >= new Date(2015, 10, 22));
        }

        [Fact]
        public void Date_AddOperator_AddsTimespanToDates()
        {
            Assert.Equal(new Date(2015, 10, 23), new Date(2015, 10, 22) + TimeSpan.FromDays(1));
        }

        [Fact]
        public void Date_SubtractOperator_SubtractsDateFromDate()
        {
            Assert.Equal(TimeSpan.FromDays(1), new Date(2015, 10, 23) - new Date(2015, 10, 22));
        }

        [Fact]
        public void Date_SubtractOperator_SubtractsTimeSpanFromDate()
        {
            Assert.Equal(new Date(2015, 10, 22), new Date(2015, 10, 23) - TimeSpan.FromDays(1));
        }

        [Fact]
        public void Date_Constructor_CreatesDateFromYearMonthDay()
        {
            Date date = new Date(2015, 10, 22);
            Assert.Equal(new DateTime(2015, 10, 22), (DateTime)date);
        }

        [Fact]
        public void Date_Constructor_YearMonthDayPropertiesAreProperlyPopulated()
        {
            Date date = new Date(2015, 10, 22);
            Assert.Equal(10, date.Month);
            Assert.Equal(22, date.Day);
        }

        [Fact]
        public void Date_Constructor_TicksIsProperlyPopulated()
        {
            Date date = new Date(2015, 10, 22);
            Assert.Equal(new DateTime(2015, 10, 22).Ticks, date.Ticks);
        }

        [Fact]
        public void Date_Constructor_DayOfWeekIsProperlyPopulated()
        {
            Date date = new Date(2015, 10, 22);
            Assert.Equal(new DateTime(2015, 10, 22).DayOfWeek, date.DayOfWeek);
        }

        [Fact]
        public void Date_AddDays_AddsDays()
        {
            Date date = new Date(2015, 10, 22).AddDays(2);
            Assert.Equal(new Date(2015, 10, 24), date);
        }

        [Fact]
        public void Date_AddMonths_AddsMonths()
        {
            Date date = new Date(2015, 5, 22).AddMonths(2);
            Assert.Equal(new Date(2015, 7, 22), date);
        }

        [Fact]
        public void Date_AddYears_AddsYears()
        {
            Date date = new Date(2015, 5, 22).AddYears(2);
            Assert.Equal(new Date(2017, 5, 22), date);
        }

        [Fact]
        public void Date_Subtract_SubtractDates()
        {
            TimeSpan timeSpan = new Date(2015, 5, 22).Subtract(new Date(2015, 5, 21));
            Assert.Equal(TimeSpan.FromDays(1), timeSpan);
        }

        [Fact]
        public void Date_ToString_EqualsToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            DateTime dateTime = new DateTime(2015, 10, 22);
            Assert.Equal(date.ToString(), dateTime.ToString());
        }

        [Fact]
        public void Date_ToStringWithFormat_EqualsToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            DateTime dateTime = new DateTime(2015, 10, 22);
            Assert.Equal(date.ToString("yyyy/MM/dd"), dateTime.ToString("yyyy/MM/dd"));
        }

        [Fact]
        public void Date_ToStringWithFormatAndProvider_EqualsToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            DateTime dateTime = new DateTime(2015, 10, 22);
            Assert.Equal(date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture), dateTime.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture));
        }

        [Fact]
        public void Date_ToStringWithProvider_EqualsToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            DateTime dateTime = new DateTime(2015, 10, 22);
            Assert.Equal(date.ToString(CultureInfo.InvariantCulture), dateTime.ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void Date_CompareTo_ComparesDates()
        {
            Assert.Equal(-1, new Date(2015, 10, 22).CompareTo(new Date(2015, 10, 23)) );
            Assert.Equal(0, new Date(2015, 10, 22).CompareTo(new Date(2015, 10, 22)));
            Assert.Equal(1, new Date(2015, 10, 23).CompareTo(new Date(2015, 10, 22)));
        }

        [Fact]
        public void Date_Equals_ComparesDates()
        {
            Assert.True(new Date(2015, 10, 22).Equals(new Date(2015, 10, 22)));
            Assert.False(new Date(2015, 10, 22).Equals(new Date(2015, 10, 23)));
        }

        [Fact]
        public void Date_GetHashCode_EqualsToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            Assert.Equal(new DateTime(2015, 10, 22).GetHashCode(), date.GetHashCode());
        }

        [Fact]
        public void Date_GetTypeCode_EqualsToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            Assert.Equal(TypeCode.DateTime, date.GetTypeCode());
        }


    }
}

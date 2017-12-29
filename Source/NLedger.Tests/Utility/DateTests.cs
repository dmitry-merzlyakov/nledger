// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using NLedger.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace NLedger.Tests.Utility
{
    [TestClass]
    public class DateTests
    {
        [TestMethod]
        public void Date_HasTimePart_ReturnsTrueIfDateTimeHasTime()
        {
            Assert.IsTrue(Date.HasTimePart(default(DateTime).AddHours(2)));
        }

        [TestMethod]
        public void Date_HasTimePart_ReturnsFalseIfDateTimeHasNoTime()
        {
            Assert.IsFalse(Date.HasTimePart(default(DateTime).AddDays(2)));
        }

        [TestMethod]
        public void Date_ImplicitConversionToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            DateTime dateTime = date;  // Implicit casting Date to DateTime
            Assert.AreEqual(new DateTime(2015, 10, 22), dateTime);
        }

        [TestMethod]
        public void Date_ExplicitConversionToDate()
        {
            DateTime dateTime = new DateTime(2015, 10, 22);
            Date date = (Date)dateTime;  // Explicit casting Date to DateTime
            Assert.AreEqual(new Date(2015, 10, 22), date);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Date_ExplicitConversionToDate_FailsInCaseOfTimePart()
        {
            DateTime dateTime = new DateTime(2015, 10, 22, 10, 20, 30);
            Date date = (Date)dateTime;
        }

        [TestMethod]
        public void Date_TryParseExact_ProducesDate()
        {
            Date date;
            string s = "2015/10/22";
            Assert.IsTrue(Date.TryParseExact(s, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date));
            Assert.AreEqual(new Date(2015, 10, 22), date);
        }

        [TestMethod]
        public void Date_TryParseExact_ReturnsFalseInCaseOfTimePart()
        {
            Date date;
            string s = "2015/10/22 11:11:11";
            Assert.IsFalse(Date.TryParseExact(s, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date));
        }

        [TestMethod]
        public void Date_EqualOperator_ComparesTwoDates()
        {
            Assert.IsTrue(new Date() == new Date());
            Assert.IsTrue(new Date(2015, 10, 22) == new Date(2015, 10, 22));
            Assert.IsFalse(new Date(2005, 10, 22) == new Date(2025, 10, 22));
        }

        [TestMethod]
        public void Date_NotEqualOperator_ComparesTwoDates()
        {
            Assert.IsFalse(new Date() != new Date());
            Assert.IsFalse(new Date(2015, 10, 22) != new Date(2015, 10, 22));
            Assert.IsTrue(new Date(2005, 10, 22) != new Date(2025, 10, 22));
        }

        [TestMethod]
        public void Date_LessOperator_ComparesTwoDates()
        {
            Assert.IsFalse(new Date() < new Date());
            Assert.IsFalse(new Date(2015, 10, 22) < new Date(2015, 10, 22));
            Assert.IsFalse(new Date(2015, 10, 23) < new Date(2015, 10, 22));
            Assert.IsTrue(new Date(2015, 10, 22) < new Date(2015, 10, 23));
        }

        [TestMethod]
        public void Date_GreaterOperator_ComparesTwoDates()
        {
            Assert.IsFalse(new Date() > new Date());
            Assert.IsFalse(new Date(2015, 10, 22) > new Date(2015, 10, 22));
            Assert.IsFalse(new Date(2015, 10, 22) > new Date(2015, 10, 23));
            Assert.IsTrue(new Date(2015, 10, 23) > new Date(2015, 10, 22));
        }

        [TestMethod]
        public void Date_LessOrEqualOperator_ComparesTwoDates()
        {
            Assert.IsTrue(new Date() <= new Date());
            Assert.IsTrue(new Date(2015, 10, 22) <= new Date(2015, 10, 22));
            Assert.IsTrue(new Date(2015, 10, 22) <= new Date(2015, 10, 23));
            Assert.IsFalse(new Date(2015, 10, 23) <= new Date(2015, 10, 22));
        }

        [TestMethod]
        public void Date_GreaterOrEqualOperator_ComparesTwoDates()
        {
            Assert.IsTrue(new Date() >= new Date());
            Assert.IsTrue(new Date(2015, 10, 22) >= new Date(2015, 10, 22));
            Assert.IsFalse(new Date(2015, 10, 22) >= new Date(2015, 10, 23));
            Assert.IsTrue(new Date(2015, 10, 23) >= new Date(2015, 10, 22));
        }

        [TestMethod]
        public void Date_AddOperator_AddsTimespanToDates()
        {
            Assert.AreEqual(new Date(2015, 10, 23), new Date(2015, 10, 22) + TimeSpan.FromDays(1));
        }

        [TestMethod]
        public void Date_SubtractOperator_SubtractsDateFromDate()
        {
            Assert.AreEqual(TimeSpan.FromDays(1), new Date(2015, 10, 23) - new Date(2015, 10, 22));
        }

        [TestMethod]
        public void Date_SubtractOperator_SubtractsTimeSpanFromDate()
        {
            Assert.AreEqual(new Date(2015, 10, 22), new Date(2015, 10, 23) - TimeSpan.FromDays(1));
        }

        [TestMethod]
        public void Date_Constructor_CreatesDateFromYearMonthDay()
        {
            Date date = new Date(2015, 10, 22);
            Assert.AreEqual(new DateTime(2015, 10, 22), (DateTime)date);
        }

        [TestMethod]
        public void Date_Constructor_YearMonthDayPropertiesAreProperlyPopulated()
        {
            Date date = new Date(2015, 10, 22);
            Assert.AreEqual(10, date.Month);
            Assert.AreEqual(22, date.Day);
        }

        [TestMethod]
        public void Date_Constructor_TicksIsProperlyPopulated()
        {
            Date date = new Date(2015, 10, 22);
            Assert.AreEqual(new DateTime(2015, 10, 22).Ticks, date.Ticks);
        }

        [TestMethod]
        public void Date_Constructor_DayOfWeekIsProperlyPopulated()
        {
            Date date = new Date(2015, 10, 22);
            Assert.AreEqual(new DateTime(2015, 10, 22).DayOfWeek, date.DayOfWeek);
        }

        [TestMethod]
        public void Date_AddDays_AddsDays()
        {
            Date date = new Date(2015, 10, 22).AddDays(2);
            Assert.AreEqual(new Date(2015, 10, 24), date);
        }

        [TestMethod]
        public void Date_AddMonths_AddsMonths()
        {
            Date date = new Date(2015, 5, 22).AddMonths(2);
            Assert.AreEqual(new Date(2015, 7, 22), date);
        }

        [TestMethod]
        public void Date_AddYears_AddsYears()
        {
            Date date = new Date(2015, 5, 22).AddYears(2);
            Assert.AreEqual(new Date(2017, 5, 22), date);
        }

        [TestMethod]
        public void Date_Subtract_SubtractDates()
        {
            TimeSpan timeSpan = new Date(2015, 5, 22).Subtract(new Date(2015, 5, 21));
            Assert.AreEqual(TimeSpan.FromDays(1), timeSpan);
        }

        [TestMethod]
        public void Date_ToString_EqualsToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            DateTime dateTime = new DateTime(2015, 10, 22);
            Assert.AreEqual(date.ToString(), dateTime.ToString());
        }

        [TestMethod]
        public void Date_ToStringWithFormat_EqualsToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            DateTime dateTime = new DateTime(2015, 10, 22);
            Assert.AreEqual(date.ToString("yyyy/MM/dd"), dateTime.ToString("yyyy/MM/dd"));
        }

        [TestMethod]
        public void Date_ToStringWithFormatAndProvider_EqualsToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            DateTime dateTime = new DateTime(2015, 10, 22);
            Assert.AreEqual(date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture), dateTime.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void Date_ToStringWithProvider_EqualsToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            DateTime dateTime = new DateTime(2015, 10, 22);
            Assert.AreEqual(date.ToString(CultureInfo.InvariantCulture), dateTime.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void Date_CompareTo_ComparesDates()
        {
            Assert.AreEqual(-1, new Date(2015, 10, 22).CompareTo(new Date(2015, 10, 23)) );
            Assert.AreEqual(0, new Date(2015, 10, 22).CompareTo(new Date(2015, 10, 22)));
            Assert.AreEqual(1, new Date(2015, 10, 23).CompareTo(new Date(2015, 10, 22)));
        }

        [TestMethod]
        public void Date_Equals_ComparesDates()
        {
            Assert.IsTrue(new Date(2015, 10, 22).Equals(new Date(2015, 10, 22)));
            Assert.IsFalse(new Date(2015, 10, 22).Equals(new Date(2015, 10, 23)));
        }

        [TestMethod]
        public void Date_GetHashCode_EqualsToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            Assert.AreEqual(new DateTime(2015, 10, 22).GetHashCode(), date.GetHashCode());
        }

        [TestMethod]
        public void Date_GetTypeCode_EqualsToDateTime()
        {
            Date date = new Date(2015, 10, 22);
            Assert.AreEqual(TypeCode.DateTime, date.GetTypeCode());
        }


    }
}

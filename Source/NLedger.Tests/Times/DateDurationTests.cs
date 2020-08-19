// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Times
{
    public class DateDurationTests : TestFixture
    {
        [Fact]
        public void DateDuration_Constructor_PopulatesProperties()
        {
            DateDuration dateDuration1 = new DateDuration();
            Assert.Equal(SkipQuantumEnum.DAYS, dateDuration1.Quantum);
            Assert.Equal(0, dateDuration1.Length);

            DateDuration dateDuration2 = new DateDuration(SkipQuantumEnum.WEEKS, 10);
            Assert.Equal(SkipQuantumEnum.WEEKS, dateDuration2.Quantum);
            Assert.Equal(10, dateDuration2.Length);
        }

        [Fact]
        public void DateDuration_Add_ReturnsDateTimeWithAddedDuration()
        {
            DateDuration dateDuration1 = new DateDuration(SkipQuantumEnum.DAYS, 10);
            DateTime result1 = dateDuration1.Add(new Date(2015, 10, 12));
            Assert.Equal(new DateTime(2015, 10, 22), result1);

            DateDuration dateDuration2 = new DateDuration(SkipQuantumEnum.WEEKS, 2);
            DateTime result2 = dateDuration2.Add(new Date(2015, 10, 10));
            Assert.Equal(new DateTime(2015, 10, 24), result2);

            DateDuration dateDuration3 = new DateDuration(SkipQuantumEnum.MONTHS, 2);
            DateTime result3 = dateDuration3.Add(new Date(2015, 5, 5));
            Assert.Equal(new DateTime(2015, 7, 5), result3);

            DateDuration dateDuration4 = new DateDuration(SkipQuantumEnum.QUARTERS, 2);
            DateTime result4 = dateDuration4.Add(new Date(2015, 5, 5));
            Assert.Equal(new DateTime(2015, 11, 5), result4);

            DateDuration dateDuration5 = new DateDuration(SkipQuantumEnum.YEARS, 2);
            DateTime result5 = dateDuration5.Add(new Date(2015, 5, 5));
            Assert.Equal(new DateTime(2017, 5, 5), result5);
        }

        [Fact]
        public void DateDuration_Subtract_ReturnsDateTimeWithSubtractedDuration()
        {
            DateDuration dateDuration1 = new DateDuration(SkipQuantumEnum.DAYS, 10);
            DateTime result1 = dateDuration1.Subtract(new Date(2015, 10, 12));
            Assert.Equal(new DateTime(2015, 10, 2), result1);

            DateDuration dateDuration2 = new DateDuration(SkipQuantumEnum.WEEKS, 2);
            DateTime result2 = dateDuration2.Subtract(new Date(2015, 10, 24));
            Assert.Equal(new DateTime(2015, 10, 10), result2);

            DateDuration dateDuration3 = new DateDuration(SkipQuantumEnum.MONTHS, 2);
            DateTime result3 = dateDuration3.Subtract(new Date(2015, 5, 5));
            Assert.Equal(new DateTime(2015, 3, 5), result3);

            DateDuration dateDuration4 = new DateDuration(SkipQuantumEnum.QUARTERS, 2);
            DateTime result4 = dateDuration4.Subtract(new Date(2015, 11, 5));
            Assert.Equal(new DateTime(2015, 5, 5), result4);

            DateDuration dateDuration5 = new DateDuration(SkipQuantumEnum.YEARS, 2);
            DateTime result5 = dateDuration5.Subtract(new Date(2015, 5, 5));
            Assert.Equal(new DateTime(2013, 5, 5), result5);
        }

        [Fact]
        public void DateDuration_ToString_ReturnsVisualizedDuration()
        {
            DateDuration dateDuration1 = new DateDuration();
            Assert.Equal("0 day", dateDuration1.ToString());

            DateDuration dateDuration2 = new DateDuration(SkipQuantumEnum.QUARTERS, 3);
            Assert.Equal("3 quarters", dateDuration2.ToString());
        }

        [Fact]
        public void DateDuration_FindNearest_ReturnsGivenDateForDays()
        {
            Date date1 = new Date(2015, 7, 22);
            Date result1 = DateDuration.FindNearest(date1, SkipQuantumEnum.DAYS);
            Assert.Equal<Date>(date1, result1);
        }

        [Fact]
        public void DateDuration_FindNearest_ReturnsFirstPreviousStartOfWeekForWeeks()
        {
            Date date1 = new Date(2015, 7, 22);
            Date result1 = DateDuration.FindNearest(date1, SkipQuantumEnum.WEEKS);
            Assert.Equal(new DateTime(2015, 7, 19), result1);  // Sunday is the first week day by default
        }

        [Fact]
        public void DateDuration_FindNearest_ReturnsFirstMonthDayForMonths()
        {
            Date date1 = new Date(2015, 7, 22);
            Date result1 = DateDuration.FindNearest(date1, SkipQuantumEnum.MONTHS);
            Assert.Equal(new DateTime(2015, 7, 1), result1); 
        }

        [Fact]
        public void DateDuration_FindNearest_ReturnsFirstQuarterDayForQuarters()
        {
            Date date1 = new Date(2015, 6, 22);
            Date result1 = DateDuration.FindNearest(date1, SkipQuantumEnum.QUARTERS);
            Assert.Equal(new DateTime(2015, 4, 1), result1);
        }

        [Fact]
        public void DateDuration_FindNearest_ReturnsFirstYearsDayForYears()
        {
            Date date1 = new Date(2015, 6, 22);
            Date result1 = DateDuration.FindNearest(date1, SkipQuantumEnum.YEARS);
            Assert.Equal(new DateTime(2015, 1, 1), result1);
        }
    }
}

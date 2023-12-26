// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
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
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class DateIntervalTests : TestFixture
    {
        [Fact]
        public void DateInterval_Constructor_PopulatesDefaultValues()
        {
            DateInterval dateInterval = new DateInterval();
            Assert.Null(dateInterval.Range);
            Assert.Null(dateInterval.Start);
            Assert.Null(dateInterval.Finish);
            Assert.False(dateInterval.Aligned);
            Assert.Null(dateInterval.Duration);
            Assert.Null(dateInterval.EndOfDuration);
        }

        [Fact]
        public void DateInterval_Constructor_ParsesStringValue()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/10/17");

            Assert.Equal(new Date(2015, 10, 15), dateInterval.Range.Begin);
            Assert.Equal(new Date(2015, 10, 17), dateInterval.Range.End);
            Assert.Null(dateInterval.Start);
            Assert.Null(dateInterval.Finish);
            Assert.False(dateInterval.Aligned);
            Assert.Null(dateInterval.Next);
            Assert.Null(dateInterval.Duration);
            Assert.Null(dateInterval.EndOfDuration);
        }

        [Fact]
        public void DateInterval_Stabilize_PopulatesStartAndFinishByRangeIfNoDuration()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/10/17");

            Assert.Equal(new Date(2015, 10, 15), dateInterval.Range.Begin);
            Assert.Equal(new Date(2015, 10, 17), dateInterval.Range.End);

            dateInterval.Stabilize((Date)DateTime.UtcNow.Date); // Date value does not matter but we need to send it

            Assert.Equal(new Date(2015, 10, 15), dateInterval.Start);
            Assert.Equal(new Date(2015, 10, 17), dateInterval.Finish);
        }

        [Fact]
        public void DateInterval_Begin_ReturnsStartOrRangeBegin()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/10/17");

            Assert.Null(dateInterval.Start);
            Assert.Equal(new Date(2015, 10, 15), dateInterval.Range.Begin.Value);
            Assert.Equal(new Date(2015, 10, 15), dateInterval.Begin.Value);

            dateInterval.Stabilize((Date)DateTime.UtcNow.Date);
            dateInterval.Range = new DateSpecifierOrRange(new DateRange());

            Assert.Equal(new Date(2015, 10, 15), dateInterval.Start.Value);
            Assert.Null(dateInterval.Range.Begin);
            Assert.Equal(new Date(2015, 10, 15), dateInterval.Begin.Value);
        }

        [Fact]
        public void DateInterval_End_ReturnsFinishOrRangeEnd()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/10/17");

            Assert.Null(dateInterval.Finish);
            Assert.Equal(new Date(2015, 10, 17), dateInterval.Range.End);
            Assert.Equal(new Date(2015, 10, 17), dateInterval.End);

            dateInterval.Stabilize((Date)DateTime.UtcNow.Date);
            dateInterval.Range = new DateSpecifierOrRange(new DateRange());

            Assert.Equal(new Date(2015, 10, 17), dateInterval.Finish);
            Assert.Null(dateInterval.Range.End);
            Assert.Equal(new Date(2015, 10, 17), dateInterval.End);
        }

        [Fact]
        public void DateInterval_IsValid_IndicatesWhetherStartIsPopulated()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/10/17");

            Assert.Null(dateInterval.Start);
            Assert.False(dateInterval.IsValid);

            dateInterval.Stabilize((Date)DateTime.UtcNow.Date);

            Assert.NotNull(dateInterval.Start);
            Assert.True(dateInterval.IsValid);
        }

        [Fact]
        public void DateInterval_Parse_ParsesAndPopulatesProperties()
        {
            DateInterval dateInterval = new DateInterval();
            dateInterval.Parse("from 2015/10/15 to 2015/10/17");

            Assert.Equal(new Date(2015, 10, 15), dateInterval.Range.Begin);
            Assert.Equal(new Date(2015, 10, 17), dateInterval.Range.End);
        }

        [Fact]
        public void DateInterval_ResolveEnd_PopulatesEndOfDurationWithStartValue()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/12/17");
            dateInterval.Stabilize((Date)DateTime.Now.Date); // to populate Start and Finish
            dateInterval.Duration = new DateDuration(SkipQuantumEnum.DAYS, 10);

            dateInterval.ResolveEnd();
            Assert.Equal(new Date(2015, 10, 25), dateInterval.EndOfDuration);
        }

        [Fact]
        public void DateInterval_ResolveEnd_PopulatesEndOfDurationWithFinishIfDurationIsLonger()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/10/17");
            dateInterval.Stabilize((Date)DateTime.Now.Date); // to populate Start and Finish
            dateInterval.Duration = new DateDuration(SkipQuantumEnum.DAYS, 10);

            dateInterval.ResolveEnd();
            Assert.Equal(new Date(2015, 10, 17), dateInterval.EndOfDuration);
        }

        [Fact]
        public void DateInterval_ResolveEnd_PopulatesNextWithStartValue()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/12/17");
            dateInterval.Stabilize((Date)DateTime.Now.Date); // to populate Start and Finish
            dateInterval.Duration = new DateDuration(SkipQuantumEnum.DAYS, 10);

            dateInterval.ResolveEnd();
            Assert.Equal(new Date(2015, 10, 25), dateInterval.Next);
        }

        [Fact]
        public void DateInterval_Stabilize_Example()
        {
            DateInterval dateInterval = new DateInterval();
            dateInterval.Range = new DateSpecifierOrRange(new DateSpecifier(new Date(2015, 5, 5), new DateTraits(true, false, false)));
            dateInterval.Duration = new DateDuration(SkipQuantumEnum.DAYS, 10);

            dateInterval.Stabilize(new Date(2015, 10, 12));

            Assert.Equal(new Date(2015, 1, 1), dateInterval.Range.Begin);
            Assert.Equal(new Date(2016, 1, 1), dateInterval.Range.End);
            Assert.Equal(new Date(2015, 10, 12), dateInterval.Start);
            Assert.Equal(new Date(2016, 1, 1), dateInterval.Finish);
            Assert.True(dateInterval.Aligned);
            Assert.Equal(new Date(2015, 10, 22), dateInterval.Next);
            Assert.Equal(new Date(2015, 10, 22), dateInterval.EndOfDuration);
        }

        [Fact]
        public void DateInterval_EqualOperator_Example()
        {
            DateInterval dateInterval1 = new DateInterval("from 2015/10/15 to 2015/10/17");
            DateInterval dateInterval2 = new DateInterval("from 2015/10/15 to 2015/10/17");
            DateInterval dateInterval3 = new DateInterval("from 2016/10/15 to 2016/10/17");

            dateInterval1.Stabilize((Date)DateTime.UtcNow.Date);
            dateInterval2.Stabilize((Date)DateTime.UtcNow.Date);
            dateInterval3.Stabilize((Date)DateTime.UtcNow.Date);

            Assert.True(dateInterval1 == dateInterval2);
            Assert.False(dateInterval1 != dateInterval2);

            Assert.False(dateInterval1 == dateInterval3);
            Assert.True(dateInterval1 != dateInterval3);

            Assert.False(dateInterval2 == dateInterval3);
            Assert.True(dateInterval2 != dateInterval3);
        }

        [Fact]
        public void DateInterval_Begin_ReturnsRangeBeginIfStartIsEmpty()
        {
            DateSpecifier dateSpecifier = new DateSpecifier((Date)DateTime.Now.Date);
            DateInterval dateInterval = new DateInterval();
            dateInterval.Range = new DateSpecifierOrRange(dateSpecifier);

            Assert.Null(dateInterval.Start);
            Assert.Equal(dateSpecifier.Begin, dateInterval.Begin);
        }

        [Fact]
        public void DateInterval_Begin_ReturnsNullIfStartAndRangeAreEmpty()
        {
            DateInterval dateInterval = new DateInterval();

            Assert.Null(dateInterval.Start);
            Assert.Null(dateInterval.Range);
            Assert.Null(dateInterval.Begin);
        }

        [Fact]
        public void DateInterval_End_ReturnsRangeEndIfFinishIsEmpty()
        {
            DateSpecifier dateSpecifier = new DateSpecifier((Date)DateTime.Now.Date);
            DateInterval dateInterval = new DateInterval();
            dateInterval.Range = new DateSpecifierOrRange(dateSpecifier);

            Assert.Null(dateInterval.Finish);
            Assert.Equal(dateSpecifier.End, dateInterval.End);
        }

        [Fact]
        public void DateInterval_End_ReturnsNullIfStartAndRangeAreEmpty()
        {
            DateInterval dateInterval = new DateInterval();

            Assert.Null(dateInterval.Finish);
            Assert.Null(dateInterval.Range);
            Assert.Null(dateInterval.End);
        }

    }
}

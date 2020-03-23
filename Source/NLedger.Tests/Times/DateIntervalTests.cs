// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
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
    public class DateIntervalTests : TestFixture
    {
        [TestMethod]
        public void DateInterval_Constructor_PopulatesDefaultValues()
        {
            DateInterval dateInterval = new DateInterval();
            Assert.IsNull(dateInterval.Range);
            Assert.IsNull(dateInterval.Start);
            Assert.IsNull(dateInterval.Finish);
            Assert.IsFalse(dateInterval.Aligned);
            Assert.IsNull(dateInterval.Duration);
            Assert.IsNull(dateInterval.EndOfDuration);
        }

        [TestMethod]
        public void DateInterval_Constructor_ParsesStringValue()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/10/17");

            Assert.AreEqual(new Date(2015, 10, 15), dateInterval.Range.Begin);
            Assert.AreEqual(new Date(2015, 10, 17), dateInterval.Range.End);
            Assert.IsNull(dateInterval.Start);
            Assert.IsNull(dateInterval.Finish);
            Assert.IsFalse(dateInterval.Aligned);
            Assert.IsNull(dateInterval.Next);
            Assert.IsNull(dateInterval.Duration);
            Assert.IsNull(dateInterval.EndOfDuration);
        }

        [TestMethod]
        public void DateInterval_Stabilize_PopulatesStartAndFinishByRangeIfNoDuration()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/10/17");

            Assert.AreEqual(new Date(2015, 10, 15), dateInterval.Range.Begin);
            Assert.AreEqual(new Date(2015, 10, 17), dateInterval.Range.End);

            dateInterval.Stabilize((Date)DateTime.UtcNow.Date); // Date value does not matter but we need to send it

            Assert.AreEqual(new Date(2015, 10, 15), dateInterval.Start);
            Assert.AreEqual(new Date(2015, 10, 17), dateInterval.Finish);
        }

        [TestMethod]
        public void DateInterval_Begin_ReturnsStartOrRangeBegin()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/10/17");

            Assert.IsNull(dateInterval.Start);
            Assert.AreEqual(new Date(2015, 10, 15), dateInterval.Range.Begin.Value);
            Assert.AreEqual(new Date(2015, 10, 15), dateInterval.Begin.Value);

            dateInterval.Stabilize((Date)DateTime.UtcNow.Date);
            dateInterval.Range = new DateSpecifierOrRange(new DateRange());

            Assert.AreEqual(new Date(2015, 10, 15), dateInterval.Start.Value);
            Assert.IsNull(dateInterval.Range.Begin);
            Assert.AreEqual(new Date(2015, 10, 15), dateInterval.Begin.Value);
        }

        [TestMethod]
        public void DateInterval_End_ReturnsFinishOrRangeEnd()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/10/17");

            Assert.IsNull(dateInterval.Finish);
            Assert.AreEqual(new Date(2015, 10, 17), dateInterval.Range.End);
            Assert.AreEqual(new Date(2015, 10, 17), dateInterval.End);

            dateInterval.Stabilize((Date)DateTime.UtcNow.Date);
            dateInterval.Range = new DateSpecifierOrRange(new DateRange());

            Assert.AreEqual(new Date(2015, 10, 17), dateInterval.Finish);
            Assert.IsNull(dateInterval.Range.End);
            Assert.AreEqual(new Date(2015, 10, 17), dateInterval.End);
        }

        [TestMethod]
        public void DateInterval_IsValid_IndicatesWhetherStartIsPopulated()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/10/17");

            Assert.IsNull(dateInterval.Start);
            Assert.IsFalse(dateInterval.IsValid);

            dateInterval.Stabilize((Date)DateTime.UtcNow.Date);

            Assert.IsNotNull(dateInterval.Start);
            Assert.IsTrue(dateInterval.IsValid);
        }

        [TestMethod]
        public void DateInterval_Parse_ParsesAndPopulatesProperties()
        {
            DateInterval dateInterval = new DateInterval();
            dateInterval.Parse("from 2015/10/15 to 2015/10/17");

            Assert.AreEqual(new Date(2015, 10, 15), dateInterval.Range.Begin);
            Assert.AreEqual(new Date(2015, 10, 17), dateInterval.Range.End);
        }

        [TestMethod]
        public void DateInterval_ResolveEnd_PopulatesEndOfDurationWithStartValue()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/12/17");
            dateInterval.Stabilize((Date)DateTime.Now.Date); // to populate Start and Finish
            dateInterval.Duration = new DateDuration(SkipQuantumEnum.DAYS, 10);

            dateInterval.ResolveEnd();
            Assert.AreEqual(new Date(2015, 10, 25), dateInterval.EndOfDuration);
        }

        [TestMethod]
        public void DateInterval_ResolveEnd_PopulatesEndOfDurationWithFinishIfDurationIsLonger()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/10/17");
            dateInterval.Stabilize((Date)DateTime.Now.Date); // to populate Start and Finish
            dateInterval.Duration = new DateDuration(SkipQuantumEnum.DAYS, 10);

            dateInterval.ResolveEnd();
            Assert.AreEqual(new Date(2015, 10, 17), dateInterval.EndOfDuration);
        }

        [TestMethod]
        public void DateInterval_ResolveEnd_PopulatesNextWithStartValue()
        {
            DateInterval dateInterval = new DateInterval("from 2015/10/15 to 2015/12/17");
            dateInterval.Stabilize((Date)DateTime.Now.Date); // to populate Start and Finish
            dateInterval.Duration = new DateDuration(SkipQuantumEnum.DAYS, 10);

            dateInterval.ResolveEnd();
            Assert.AreEqual(new Date(2015, 10, 25), dateInterval.Next);
        }

        [TestMethod]
        public void DateInterval_Stabilize_Example()
        {
            DateInterval dateInterval = new DateInterval();
            dateInterval.Range = new DateSpecifierOrRange(new DateSpecifier(new Date(2015, 5, 5), new DateTraits(true, false, false)));
            dateInterval.Duration = new DateDuration(SkipQuantumEnum.DAYS, 10);

            dateInterval.Stabilize(new Date(2015, 10, 12));

            Assert.AreEqual(new Date(2015, 1, 1), dateInterval.Range.Begin);
            Assert.AreEqual(new Date(2016, 1, 1), dateInterval.Range.End);
            Assert.AreEqual(new Date(2015, 10, 12), dateInterval.Start);
            Assert.AreEqual(new Date(2016, 1, 1), dateInterval.Finish);
            Assert.IsTrue(dateInterval.Aligned);
            Assert.AreEqual(new Date(2015, 10, 22), dateInterval.Next);
            Assert.AreEqual(new Date(2015, 10, 22), dateInterval.EndOfDuration);
        }

        [TestMethod]
        public void DateInterval_EqualOperator_Example()
        {
            DateInterval dateInterval1 = new DateInterval("from 2015/10/15 to 2015/10/17");
            DateInterval dateInterval2 = new DateInterval("from 2015/10/15 to 2015/10/17");
            DateInterval dateInterval3 = new DateInterval("from 2016/10/15 to 2016/10/17");

            dateInterval1.Stabilize((Date)DateTime.UtcNow.Date);
            dateInterval2.Stabilize((Date)DateTime.UtcNow.Date);
            dateInterval3.Stabilize((Date)DateTime.UtcNow.Date);

            Assert.IsTrue(dateInterval1 == dateInterval2);
            Assert.IsFalse(dateInterval1 != dateInterval2);

            Assert.IsFalse(dateInterval1 == dateInterval3);
            Assert.IsTrue(dateInterval1 != dateInterval3);

            Assert.IsFalse(dateInterval2 == dateInterval3);
            Assert.IsTrue(dateInterval2 != dateInterval3);
        }

        [TestMethod]
        public void DateInterval_Begin_ReturnsRangeBeginIfStartIsEmpty()
        {
            DateSpecifier dateSpecifier = new DateSpecifier((Date)DateTime.Now.Date);
            DateInterval dateInterval = new DateInterval();
            dateInterval.Range = new DateSpecifierOrRange(dateSpecifier);

            Assert.IsNull(dateInterval.Start);
            Assert.AreEqual(dateSpecifier.Begin, dateInterval.Begin);
        }

        [TestMethod]
        public void DateInterval_Begin_ReturnsNullIfStartAndRangeAreEmpty()
        {
            DateInterval dateInterval = new DateInterval();

            Assert.IsNull(dateInterval.Start);
            Assert.IsNull(dateInterval.Range);
            Assert.IsNull(dateInterval.Begin);
        }

        [TestMethod]
        public void DateInterval_End_ReturnsRangeEndIfFinishIsEmpty()
        {
            DateSpecifier dateSpecifier = new DateSpecifier((Date)DateTime.Now.Date);
            DateInterval dateInterval = new DateInterval();
            dateInterval.Range = new DateSpecifierOrRange(dateSpecifier);

            Assert.IsNull(dateInterval.Finish);
            Assert.AreEqual(dateSpecifier.End, dateInterval.End);
        }

        [TestMethod]
        public void DateInterval_End_ReturnsNullIfStartAndRangeAreEmpty()
        {
            DateInterval dateInterval = new DateInterval();

            Assert.IsNull(dateInterval.Finish);
            Assert.IsNull(dateInterval.Range);
            Assert.IsNull(dateInterval.End);
        }

    }
}

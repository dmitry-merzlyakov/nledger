// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
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

namespace NLedger.IntegrationTests.unit
{
    /// <summary>
    /// Ported from t_times.cc
    /// </summary>
    public class TestTimes : IDisposable
    {
        public TestTimes()
        {
            Initialize();
        }

        public void Dispose()
        {
            Cleanup();
        }

        private void Initialize()
        {
            MainContextAcquirer = new MainApplicationContext().AcquireCurrentThread();
            TimesCommon.Current.TimesInitialize();
        }

        private void Cleanup()
        {
            MainContextAcquirer.Dispose();
        }

        public MainApplicationContext.ThreadAcquirer MainContextAcquirer { get; private set; }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Times_TestConstructors()
        {
            DateTime now = new DateTime(1970, 1, 1);
            DateTime moment = now;
            DateTime localMoment = moment;

            Date d0 = new Date();
            Date d1;
            DateTime d3;
            Date d4;
            Date d5;
            Date d6;
            Date d7;
            Date d8;
            Date d9;

            // #if 0 
            //Date d10;
            //Date d11;
            //Date d12;
            //Date d13;
            //Date d14;
            //DateTime d15;
            // #endif

            d1 = TimesCommon.Current.ParseDate("1990/01/01");
            d3 = localMoment;
            d4 = TimesCommon.Current.ParseDate("2006/12/25");
            d5 = TimesCommon.Current.ParseDate("12/25");
            d6 = TimesCommon.Current.ParseDate("2006.12.25");
            d7 = TimesCommon.Current.ParseDate("12.25");
            d8 = TimesCommon.Current.ParseDate("2006-12-25");
            d9 = TimesCommon.Current.ParseDate("12-25");

            // #if 0
            //d10 = TimesCommon.Current.ParseDate("tue");
            //d11 = TimesCommon.Current.ParseDate("tuesday");
            //d12 = TimesCommon.Current.ParseDate("feb");
            //d13 = TimesCommon.Current.ParseDate("february");
            //d14 = TimesCommon.Current.ParseDate("2006");
            //d15 = d3;
            // #endif

            Assert.True(d0.IsNotADate());
            Assert.True(!d1.IsNotADate());
            Assert.True(!d4.IsNotADate());

            Assert.True(TimesCommon.Current.CurrentDate > d1);
            Assert.True(TimesCommon.Current.CurrentDate > d4);

            // #if 0
            //Assert.AreEqual(d3, d15);
            // #endif

            Assert.Equal(d4, d6);
            Assert.Equal(d4, d8);
            Assert.Equal(d5, d7);
            Assert.Equal(d5, d9);

            // #if 0
            /*
            Assert.AreEqual(d10, d11);
            Assert.AreEqual(d12, d13);

            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("2007/02/29"));
            //BOOST_CHECK_THROW(parse_date("2007/02/29"), boost::gregorian::bad_day_of_month);
            //BOOST_CHECK_THROW(parse_date("2007/13/01"), datetime_error);
            //BOOST_CHECK_THROW(parse_date("2007/00/01"), datetime_error);
            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("2007/01/00"));
            //BOOST_CHECK_THROW(parse_date("2007/01/00"), boost::gregorian::bad_day_of_month);
            //BOOST_CHECK_THROW(parse_date("2007/00/00"), boost::gregorian::bad_day_of_month);
            //BOOST_CHECK_THROW(parse_date("2007/05/32"), boost::gregorian::bad_day_of_month);

            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("2006x/12/25"));
            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("2006/12x/25"));
            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("2006/12/25x"));

            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("feb/12/25"));
            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("2006/mon/25"));
            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("2006/12/web"));

            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("12*25"));

            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("tuf"));
            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("tufsday"));
            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("fec"));
            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("fecruary"));
            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("207x"));
            Boost.CheckThrow<DateError, Date>(() => TimesCommon.Current.ParseDate("hello"));

            d1 = TimesCommon.Current.ParseDate("2002-02-02");
            d1 = TimesCommon.Current.ParseDate("2002/02/02");
            d1 = TimesCommon.Current.ParseDate("2002.02.02");
            d1 = TimesCommon.Current.ParseDate("02-02-2002");
            d1 = TimesCommon.Current.ParseDate("02/02/2002");
            d1 = TimesCommon.Current.ParseDate("02.02.2002");
            d1 = TimesCommon.Current.ParseDate("02-02-02");
            d1 = TimesCommon.Current.ParseDate("02/02/02");
            d1 = TimesCommon.Current.ParseDate("02.02.02");
            d1 = TimesCommon.Current.ParseDate("02-02");
            d1 = TimesCommon.Current.ParseDate("02/02");
            d1 = TimesCommon.Current.ParseDate("02.02");
            d1 = TimesCommon.Current.ParseDate("20020202");
            d1 = TimesCommon.Current.ParseDate("20020202T023318");
            d1 = TimesCommon.Current.ParseDate("20020202T023318-0700");
            d1 = TimesCommon.Current.ParseDate("20020202T023318-0100");
            d1 = TimesCommon.Current.ParseDate("02-Feb-2002");
            d1 = TimesCommon.Current.ParseDate("2002-Feb-02");
            d1 = TimesCommon.Current.ParseDate("02 Feb 2002");
            d1 = TimesCommon.Current.ParseDate("02-Feb-2002");
            d1 = TimesCommon.Current.ParseDate("02 February 2002");
            d1 = TimesCommon.Current.ParseDate("02-February-2002");
            d1 = TimesCommon.Current.ParseDate("2002 Feb 02");
            d1 = TimesCommon.Current.ParseDate("2002-Feb-02");
            d1 = TimesCommon.Current.ParseDate("2002 February 02");
            d1 = TimesCommon.Current.ParseDate("2002-February-02");
            d1 = TimesCommon.Current.ParseDate("02 Feb");
            d1 = TimesCommon.Current.ParseDate("02-Feb");
            d1 = TimesCommon.Current.ParseDate("02 February");
            d1 = TimesCommon.Current.ParseDate("02-February");
            d1 = TimesCommon.Current.ParseDate("Feb 02");
            d1 = TimesCommon.Current.ParseDate("Feb-02");
            d1 = TimesCommon.Current.ParseDate("February 02");
            d1 = TimesCommon.Current.ParseDate("February-02");
            d1 = TimesCommon.Current.ParseDate("Feb 02, 2002");
            d1 = TimesCommon.Current.ParseDate("February 02, 2002");
            d1 = TimesCommon.Current.ParseDate("2002-02-02 12:00:00");
            d1 = TimesCommon.Current.ParseDate("2002-02-02 12:00:00 AM");
            d1 = TimesCommon.Current.ParseDate("2002-02-02 12:00 AM");
            d1 = TimesCommon.Current.ParseDate("2002-02-02 12:00AM");
            d1 = TimesCommon.Current.ParseDate("2002-02-02 12p");
            d1 = TimesCommon.Current.ParseDate("2002-02-02 12a");

            Assert.IsTrue(d1.IsValid());
            */
            // #endif
        }
    }
}

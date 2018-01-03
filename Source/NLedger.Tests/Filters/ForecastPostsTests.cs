// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Filters;
using NLedger.Times;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Filters
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class ForecastPostsTests : TestFixture
    {
        public override void CustomTestInitialize()
        {
            TimesCommon.Current.Epoch = new DateTime(2010, 05, 01);
        }

        [TestMethod]
        public void ForecastPosts_AddPost_DoesNotModifyInputPeriod()
        {
            ForecastPosts forecastPosts = new ForecastPosts(null, null, null, 1);
            DateInterval dateInterval = new DateInterval("from 2010/04/01 to 2010/06/10");
            dateInterval.Duration = new DateDuration(SkipQuantumEnum.DAYS, 5);
            Post post = new Post();

            Assert.IsNull(dateInterval.Start);

            forecastPosts.AddPost(dateInterval, post);

            Assert.IsNull(dateInterval.Start);
            Assert.IsNotNull(forecastPosts.PendingPosts.First().DateInterval.Start);
        }
    }
}

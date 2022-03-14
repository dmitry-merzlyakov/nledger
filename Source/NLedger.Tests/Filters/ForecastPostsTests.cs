// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Filters;
using NLedger.Times;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Filters
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class ForecastPostsTests : TestFixture
    {
        protected override void CustomTestInitialize()
        {
            TimesCommon.Current.Epoch = new DateTime(2010, 05, 01);
        }

        [Fact]
        public void ForecastPosts_AddPost_DoesNotModifyInputPeriod()
        {
            ForecastPosts forecastPosts = new ForecastPosts(null, null, null, 1);
            DateInterval dateInterval = new DateInterval("from 2010/04/01 to 2010/06/10");
            dateInterval.Duration = new DateDuration(SkipQuantumEnum.DAYS, 5);
            Post post = new Post();

            Assert.Null(dateInterval.Start);

            forecastPosts.AddPost(dateInterval, post);

            Assert.Null(dateInterval.Start);
            Assert.NotNull(forecastPosts.PendingPosts.First().DateInterval.Start);
        }
    }
}

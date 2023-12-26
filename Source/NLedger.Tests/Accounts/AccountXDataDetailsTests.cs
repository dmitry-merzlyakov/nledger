// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Items;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Accounts
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class AccountXDataDetailsTests : TestFixture
    {
        [Fact]
        public void AccountXDataDetails_Contructor_ProducesEmptyValues()
        {
            AccountXDataDetails details = new AccountXDataDetails();

            Assert.True(Value.IsNullOrEmpty(details.Total));
            Assert.False(details.Calculated);
            Assert.False(details.Gathered);
            Assert.Equal(0, details.PostsCount);
            Assert.Equal(0, details.PostsVirtualsCount);
            Assert.Equal(0, details.PostsClearedCount);
            Assert.Equal(0, details.PostsLast7Count);
            Assert.Equal(0, details.PostsLast30Count);
            Assert.Equal(0, details.PostsThisMountCount);
            Assert.Equal(default(Date), details.EarliestPost);
            Assert.Equal(default(Date), details.EarliestClearedPost);
            Assert.Equal(default(Date), details.LatestPost);
            Assert.Equal(default(Date), details.LatestClearedPost);
            Assert.Equal(default(DateTime), details.EarliestCheckin);
            Assert.Equal(default(DateTime), details.LatestCheckout);
            Assert.False(details.LatestCheckoutCleared);
            Assert.Empty(details.Filenames);
            Assert.Empty(details.AccountsReferenced);
            Assert.Empty(details.PayeesReferenced);
        }

        [Fact]
        public void AccountXDataDetails_Update_IncreasesCounters()
        {
            DateTime currentDate = new DateTime(2015, 1, 31);
            Date postDate = new Date(2015, 1, 30);
            DateTime checkinDate = new DateTime(2015, 1, 20);
            DateTime checkoutDate = new DateTime(2015, 1, 21);

            TimesCommon.Current.Epoch = currentDate;

            Post post = new Post() 
            { 
                Date = postDate,
                State = ItemStateEnum.Cleared,
                Checkin = checkinDate,
                Checkout = checkoutDate
            };
            post.Flags = post.Flags | SupportsFlagsEnum.POST_COST_VIRTUAL;

            AccountXDataDetails details = new AccountXDataDetails();
            details.Update(post);

            Assert.Equal(1, details.PostsCount);
            Assert.Equal(0, details.PostsVirtualsCount);
            Assert.Equal(1, details.PostsThisMountCount);
            Assert.Equal(1, details.PostsLast30Count);
            Assert.Equal(1, details.PostsLast7Count);
            Assert.Equal(postDate, details.EarliestPost);
            Assert.Equal(postDate, details.LatestPost);
            Assert.Equal(checkinDate, details.EarliestCheckin);
            Assert.Equal(checkoutDate, details.LatestCheckout);
            Assert.Equal(1, details.PostsClearedCount);
            Assert.Equal(postDate, details.EarliestClearedPost);
            Assert.Equal(postDate, details.LatestClearedPost);
        }

        [Fact]
        public void AccountXDataDetails_Update_IncreasesCountersAndGathersPosts()
        {
            DateTime currentDate = new DateTime(2015, 1, 31);
            Date postDate = new Date(2015, 1, 30);

            TimesCommon.Current.Epoch = currentDate;

            Post post = new Post()
            {
                Date = postDate,
                Pos = new ItemPosition() { PathName = "path-name" },
                Account = new Account(null, "account-name")                
            };
            post.SetTag("Payee", Value.Get("payee-name"));

            AccountXDataDetails details = new AccountXDataDetails();
            details.Update(post, true);

            Assert.Equal("path-name", details.Filenames.First());
            Assert.Equal("account-name", details.AccountsReferenced.First());
            Assert.Equal("payee-name", details.PayeesReferenced.First());
        }

        [Fact]
        public void AccountXDataDetails_Add_AddsTwoDetails()
        {
            DateTime currentDate = new DateTime(2015, 1, 31);
            Date postDate = new Date(2015, 1, 30);
            DateTime checkinDate = new DateTime(2015, 1, 20);
            DateTime checkoutDate = new DateTime(2015, 1, 21);

            TimesCommon.Current.Epoch = currentDate;

            // Create and add details
            AccountXDataDetails details1 = new AccountXDataDetails();
            details1.Update(CreatePost(postDate, checkinDate, checkoutDate, "path-name-1", "account-name-1", "payee-name-1"), true);
            details1.Update(CreatePost(postDate, checkinDate, checkoutDate, "path-name-2", "account-name-2", "payee-name-2"), true);

            AccountXDataDetails details2 = new AccountXDataDetails();
            details2.Update(CreatePost(postDate, checkinDate, checkoutDate, "path-name-3", "account-name-3", "payee-name-3"), true);
            details2.Update(CreatePost(postDate, checkinDate, checkoutDate, "path-name-4", "account-name-4", "payee-name-4"), true);
            details2.Update(CreatePost(postDate, checkinDate, checkoutDate, "path-name-5", "account-name-5", "payee-name-5"), true);

            AccountXDataDetails details = details2.Add(details1);

            Assert.Equal(5, details.PostsCount);
            Assert.Equal(0, details.PostsVirtualsCount);
            Assert.Equal(5, details.PostsClearedCount);
            Assert.Equal(5, details.PostsLast7Count);
            Assert.Equal(5, details.PostsLast30Count);
            Assert.Equal(5, details.PostsThisMountCount);
            Assert.Equal(postDate, details.EarliestPost);
            Assert.Equal(postDate, details.EarliestClearedPost);
            Assert.Equal(postDate, details.LatestPost);
            Assert.Equal(postDate, details.LatestClearedPost);

            Assert.Equal(5, details.Filenames.Count());
            Assert.Equal(5, details.AccountsReferenced.Count());
            Assert.Equal(5, details.PayeesReferenced.Count());
        }

        [Fact]
        public void AccountXDataDetails_Update_UsesPostGetDate()
        {
            Date date = (Date)DateTime.Now.Date;
            Post post = new Post();
            post.XData.Date = date;

            AccountXDataDetails details = new AccountXDataDetails();
            details.Update(post);

            Assert.Null(post.Date);
            Assert.Equal(date, post.GetDate());
            Assert.Equal(date, details.LatestPost);
        }

        private Post CreatePost(Date postDate, DateTime checkinDate, DateTime checkoutDate, string pathName, string accountName, string payeeName)
        {
            Post post = new Post()
            {
                Date = postDate,
                State = ItemStateEnum.Cleared,
                Checkin = checkinDate,
                Checkout = checkoutDate,
                Pos = new ItemPosition() { PathName = pathName },
                Account = new Account(null, accountName)
            };
            post.Flags = post.Flags | SupportsFlagsEnum.POST_COST_VIRTUAL;
            post.SetTag("Payee", Value.Get(payeeName));
            return post;
        }
    }
}

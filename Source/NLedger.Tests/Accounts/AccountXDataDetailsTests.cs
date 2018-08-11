// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace NLedger.Tests.Accounts
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class AccountXDataDetailsTests : TestFixture
    {
        [TestMethod]
        public void AccountXDataDetails_Contructor_ProducesEmptyValues()
        {
            AccountXDataDetails details = new AccountXDataDetails();

            Assert.IsTrue(Value.IsNullOrEmpty(details.Total));
            Assert.IsFalse(details.Calculated);
            Assert.IsFalse(details.Gathered);
            Assert.AreEqual(0, details.PostsCount);
            Assert.AreEqual(0, details.PostsVirtualsCount);
            Assert.AreEqual(0, details.PostsClearedCount);
            Assert.AreEqual(0, details.PostsLast7Count);
            Assert.AreEqual(0, details.PostsLast30Count);
            Assert.AreEqual(0, details.PostsThisMountCount);
            Assert.AreEqual(default(Date), details.EarliestPost);
            Assert.AreEqual(default(Date), details.EarliestClearedPost);
            Assert.AreEqual(default(Date), details.LatestPost);
            Assert.AreEqual(default(Date), details.LatestClearedPost);
            Assert.AreEqual(default(DateTime), details.EarliestCheckin);
            Assert.AreEqual(default(DateTime), details.LatestCheckout);
            Assert.IsFalse(details.LatestCheckoutCleared);
            Assert.AreEqual(0, details.Filenames.Count());
            Assert.AreEqual(0, details.AccountsReferenced.Count());
            Assert.AreEqual(0, details.PayeesReferenced.Count());
        }

        [TestMethod]
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

            Assert.AreEqual(1, details.PostsCount);
            Assert.AreEqual(1, details.PostsVirtualsCount);
            Assert.AreEqual(1, details.PostsThisMountCount);
            Assert.AreEqual(1, details.PostsLast30Count);
            Assert.AreEqual(1, details.PostsLast7Count);
            Assert.AreEqual(postDate, details.EarliestPost);
            Assert.AreEqual(postDate, details.LatestPost);
            Assert.AreEqual(checkinDate, details.EarliestCheckin);
            Assert.AreEqual(checkoutDate, details.LatestCheckout);
            Assert.AreEqual(1, details.PostsClearedCount);
            Assert.AreEqual(postDate, details.EarliestClearedPost);
            Assert.AreEqual(postDate, details.LatestClearedPost);
        }

        [TestMethod]
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

            Assert.AreEqual("path-name", details.Filenames.First());
            Assert.AreEqual("account-name", details.AccountsReferenced.First());
            Assert.AreEqual("payee-name", details.PayeesReferenced.First());
        }

        [TestMethod]
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

            Assert.AreEqual(5, details.PostsCount);
            Assert.AreEqual(5, details.PostsVirtualsCount);
            Assert.AreEqual(5, details.PostsClearedCount);
            Assert.AreEqual(5, details.PostsLast7Count);
            Assert.AreEqual(5, details.PostsLast30Count);
            Assert.AreEqual(5, details.PostsThisMountCount);
            Assert.AreEqual(postDate, details.EarliestPost);
            Assert.AreEqual(postDate, details.EarliestClearedPost);
            Assert.AreEqual(postDate, details.LatestPost);
            Assert.AreEqual(postDate, details.LatestClearedPost);

            Assert.AreEqual(5, details.Filenames.Count());
            Assert.AreEqual(5, details.AccountsReferenced.Count());
            Assert.AreEqual(5, details.PayeesReferenced.Count());
        }

        [TestMethod]
        public void AccountXDataDetails_Update_UsesPostGetDate()
        {
            Date date = (Date)DateTime.Now.Date;
            Post post = new Post();
            post.XData.Date = date;

            AccountXDataDetails details = new AccountXDataDetails();
            details.Update(post);

            Assert.IsNull(post.Date);
            Assert.AreEqual(date, post.GetDate());
            Assert.AreEqual(date, details.LatestPost);
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

// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Extensibility.Export;
using NLedger.Journals;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility.Export
{
    public class ListAdapterTests
    {
        [Fact]
        public void ListAdapter_Constructor_TakesOrigin()
        {
            var origin = new List<int>();
            var adapter = new ListAdapter<int>(origin);
            Assert.Equal(origin, adapter.Origin);
        }

        [Fact]
        public void ListAdapter_Constructor_CreatesOrigin()
        {
            var adapter = new ListAdapter<int>();
            Assert.NotNull(adapter.Origin);
            Assert.Empty(adapter.Origin);
        }

        [Fact]
        public void ListAdapter_Index_ForwardsToOrigin()
        {
            var origin = new List<int>() { 10, 20 };
            var adapter = new ListAdapter<int>(origin);

            Assert.Equal(20, adapter[1]);
            adapter[1] = 30;
            Assert.Equal(30, origin[1]);
        }

        [Fact]
        public void ListAdapter_Count_ForwardsToOrigin()
        {
            var origin = new List<int>() { 10, 20 };
            var adapter = new ListAdapter<int>(origin);
            Assert.Equal(origin.Count, adapter.Count);
        }

        [Fact]
        public void ListAdapter_RemoveAt_ForwardsToOrigin()
        {
            var origin = new List<int>() { 10, 20 };
            var adapter = new ListAdapter<int>(origin);
            adapter.RemoveAt(1);
            adapter.RemoveAt(0);
            Assert.Empty(origin);
        }

        [Fact]
        public void ListAdapter_Insert_ForwardsToOrigin()
        {
            var origin = new List<int>() { 10, 20 };
            var adapter = new ListAdapter<int>(origin);
            adapter.Insert(0, 5);
            Assert.Equal(5, origin[0]);
            Assert.Equal(3, origin.Count);
        }

        [Fact]
        public void ListAdapter_Add_ForwardsToOrigin()
        {
            var origin = new List<int>() { 10, 20 };
            var adapter = new ListAdapter<int>(origin);
            adapter.Add(30);
            Assert.Equal(30, origin[2]);
            Assert.Equal(3, origin.Count);
        }

        [Fact]
        public void ListAdapter_ToString_ForwardsToOrigin()
        {
            var origin = new List<int>();
            var adapter = new ListAdapter<int>(origin);
            Assert.Equal(origin.ToString(), adapter.ToString());
        }

        [Fact]
        public void ListAdapter_GetPostXDataSortValues_ReturnsAdapter()
        {
            var data = new PostXData();
            var adapter = ListAdapter.GetPostXDataSortValues(data);
            Assert.Equal(data.SortValues, adapter.Origin);
        }

        [Fact]
        public void ListAdapter_GetAccountXDataSortValues_ReturnsAdapter()
        {
            var data = new AccountXData();
            var adapter = ListAdapter.GetAccountXDataSortValues(data);
            Assert.Equal(data.SortValues, adapter.Origin);
        }

        [Fact]
        public void ListAdapter_GetValueSequence_ReturnsAdapter()
        {
            var data = new Value();
            data.PushBack(Value.Get(10));
            data.PushBack(Value.Get(20));

            var adapter = ListAdapter.GetValueSequence(data);
            Assert.Equal(data.AsSequence, adapter.Origin);
        }

        [Fact]
        public void ListAdapter_SetValueSequence_SetsSequence()
        {
            var data = new Value();

            var list = new List<Value>();
            list.Add(Value.Get(10));
            list.Add(Value.Get(20));
            var adapter = new ListAdapter<Value>(list);

            ListAdapter.SetValueSequence(data, adapter);
            Assert.Equal(2, data.Size);
        }

        [Fact]
        public void ListAdapter_CreateValue_CreatesValueFromSequence()
        {
            var list = new List<Value>();
            list.Add(Value.Get(10));
            list.Add(Value.Get(20));
            var adapter = new ListAdapter<Value>(list);

            var data = ListAdapter.CreateValue(adapter);
            Assert.Equal(2, data.Size);
        }

        [Fact]
        public void ListAdapter_GetPosts_ReturnsAdapterFromXact()
        {
            var data = new Xact();
            var adapter = ListAdapter.GetPosts(data);
            Assert.Equal(data.Posts, adapter.Origin);
        }

        [Fact]
        public void ListAdapter_GetPosts_ReturnsAdapterFromAccountXData()
        {
            var data = new AccountXData();
            var adapter = ListAdapter.GetPosts(data);
            Assert.Equal(data.ReportedPosts, adapter.Origin);
        }

        [Fact]
        public void ListAdapter_GetPosts_ReturnsAdapterFromAccount()
        {
            var data = new Account();
            var adapter = ListAdapter.GetPosts(data);
            Assert.Equal(data.Posts, adapter.Origin);
        }

        [Fact]
        public void ListAdapter_GetAccounts_ReturnsAdapter()
        {
            var data = new Account();
            var adapter = ListAdapter.GetAccounts(data);
            Assert.Equal(data.Accounts.Values.Count, adapter.Origin.Count);
        }

        [Fact]
        public void ListAdapter_GetAmounts_ReturnsAdapter()
        {
            var data = new Balance();
            var adapter = ListAdapter.GetAmounts(data);
            Assert.Equal(data.Amounts.Values.Count, adapter.Origin.Count);
        }

        [Fact]
        public void ListAdapter_GetXacts_ReturnsAdapter()
        {
            var data = new Journal();
            var adapter = ListAdapter.GetXacts(data);
            Assert.Equal(data.Xacts.Count, adapter.Origin.Count);
        }

        [Fact]
        public void ListAdapter_GetAutoXacts_ReturnsAdapter()
        {
            var data = new Journal();
            var adapter = ListAdapter.GetAutoXacts(data);
            Assert.Equal(data.AutoXacts.Count, adapter.Origin.Count);
        }

        [Fact]
        public void ListAdapter_GetPeriodXacts_ReturnsAdapter()
        {
            var data = new Journal();
            var adapter = ListAdapter.GetPeriodXacts(data);
            Assert.Equal(data.PeriodXacts.Count, adapter.Origin.Count);
        }

        [Fact]
        public void ListAdapter_GetFileInfos_ReturnsAdapter()
        {
            var data = new Journal();
            var adapter = ListAdapter.GetPeriodXacts(data);
            Assert.Equal(data.Sources.Count, adapter.Origin.Count);
        }

        [Fact]
        public void ListAdapter_GetQuery_ReturnsAdapter()
        {
            using (var session = NLedger.Extensibility.Net.NetSession.CreateStandaloneSession())
            {
                session.ReadJournalFromString("# empty journal");
                var adapter = ListAdapter.GetQuery(session.Journal, "bal");
                Assert.Equal(session.Journal.Query("bal").Count(), adapter.Origin.Count);
            }
        }

    }
}

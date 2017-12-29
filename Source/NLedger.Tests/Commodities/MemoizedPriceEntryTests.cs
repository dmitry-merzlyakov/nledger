// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Commodities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Commodities
{
    [TestClass]
    public class MemoizedPriceEntryTests : TestFixture
    {
        [TestMethod]
        public void MemoizedPriceEntry_Integration_CanBeUsedAsDictionaryKey()
        {
            DateTime dt1 = new DateTime(2010, 10, 15);
            DateTime dt2 = new DateTime(2015, 10, 25);
            Commodity comm1 = new Commodity(CommodityPool.Current, new CommodityBase("memprice1"));
            Commodity comm2 = new Commodity(CommodityPool.Current, new CommodityBase("memprice2"));

            MemoizedPriceEntry key1 = new MemoizedPriceEntry();
            MemoizedPriceEntry key2 = new MemoizedPriceEntry() { Start = dt1 };
            MemoizedPriceEntry key3 = new MemoizedPriceEntry() { Start = dt1, End = dt2 };
            MemoizedPriceEntry key4 = new MemoizedPriceEntry() { Start = dt2, End = dt1 };
            MemoizedPriceEntry key5 = new MemoizedPriceEntry() { End = dt2 };
            MemoizedPriceEntry key6 = new MemoizedPriceEntry() { Start = dt1, Commodity = comm1 };
            MemoizedPriceEntry key7 = new MemoizedPriceEntry() { Start = dt1, End = dt2, Commodity = comm1 };
            MemoizedPriceEntry key8 = new MemoizedPriceEntry() { Start = dt2, End = dt1, Commodity = comm1 };
            MemoizedPriceEntry key9 = new MemoizedPriceEntry() { End = dt2, Commodity = comm1 };
            MemoizedPriceEntry key10 = new MemoizedPriceEntry() { Commodity = comm1 };
            MemoizedPriceEntry key11 = new MemoizedPriceEntry() { Start = dt1, Commodity = comm2 };
            MemoizedPriceEntry key12 = new MemoizedPriceEntry() { Start = dt1, End = dt2, Commodity = comm2 };
            MemoizedPriceEntry key13 = new MemoizedPriceEntry() { Start = dt2, End = dt1, Commodity = comm2 };
            MemoizedPriceEntry key14 = new MemoizedPriceEntry() { End = dt2, Commodity = comm2 };
            MemoizedPriceEntry key15 = new MemoizedPriceEntry() { Commodity = comm2 };

            IDictionary<MemoizedPriceEntry, string> dictionary = new Dictionary<MemoizedPriceEntry, string>();

            dictionary[key1] = "1";
            dictionary[key2] = "2";
            dictionary[key3] = "3";
            dictionary[key4] = "4";
            dictionary[key5] = "5";
            dictionary[key6] = "6";
            dictionary[key7] = "7";
            dictionary[key8] = "8";
            dictionary[key9] = "9";
            dictionary[key10] = "10";
            dictionary[key11] = "11";
            dictionary[key12] = "12";
            dictionary[key13] = "13";
            dictionary[key14] = "14";
            dictionary[key15] = "15";

            MemoizedPriceEntry check1 = new MemoizedPriceEntry();
            MemoizedPriceEntry check2 = new MemoizedPriceEntry() { Start = dt1 };
            MemoizedPriceEntry check3 = new MemoizedPriceEntry() { Start = dt1, End = dt2 };
            MemoizedPriceEntry check4 = new MemoizedPriceEntry() { Start = dt2, End = dt1 };
            MemoizedPriceEntry check5 = new MemoizedPriceEntry() { End = dt2 };
            MemoizedPriceEntry check6 = new MemoizedPriceEntry() { Start = dt1, Commodity = comm1 };
            MemoizedPriceEntry check7 = new MemoizedPriceEntry() { Start = dt1, End = dt2, Commodity = comm1 };
            MemoizedPriceEntry check8 = new MemoizedPriceEntry() { Start = dt2, End = dt1, Commodity = comm1 };
            MemoizedPriceEntry check9 = new MemoizedPriceEntry() { End = dt2, Commodity = comm1 };
            MemoizedPriceEntry check10 = new MemoizedPriceEntry() { Commodity = comm1 };
            MemoizedPriceEntry check11 = new MemoizedPriceEntry() { Start = dt1, Commodity = comm2 };
            MemoizedPriceEntry check12 = new MemoizedPriceEntry() { Start = dt1, End = dt2, Commodity = comm2 };
            MemoizedPriceEntry check13 = new MemoizedPriceEntry() { Start = dt2, End = dt1, Commodity = comm2 };
            MemoizedPriceEntry check14 = new MemoizedPriceEntry() { End = dt2, Commodity = comm2 };
            MemoizedPriceEntry check15 = new MemoizedPriceEntry() { Commodity = comm2 };

            Assert.AreEqual("1", dictionary[check1]);
            Assert.AreEqual("2", dictionary[check2]);
            Assert.AreEqual("3", dictionary[check3]);
            Assert.AreEqual("4", dictionary[check4]);
            Assert.AreEqual("5", dictionary[check5]);
            Assert.AreEqual("6", dictionary[check6]);
            Assert.AreEqual("7", dictionary[check7]);
            Assert.AreEqual("8", dictionary[check8]);
            Assert.AreEqual("9", dictionary[check9]);
            Assert.AreEqual("10", dictionary[check10]);
            Assert.AreEqual("11", dictionary[check11]);
            Assert.AreEqual("12", dictionary[check12]);
            Assert.AreEqual("13", dictionary[check13]);
            Assert.AreEqual("14", dictionary[check14]);
            Assert.AreEqual("15", dictionary[check15]);
        }
    }
}

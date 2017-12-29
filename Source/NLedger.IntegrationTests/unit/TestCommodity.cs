// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Amounts;
using NLedger.Commodities;
using NLedger.Times;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.IntegrationTests.unit
{
    /// <summary>
    /// Ported from t_commodity.cc
    /// </summary>
    [TestClass]
    public class TestCommodity
    {
        [TestInitialize]
        public void Initialize()
        {
            MainApplicationContext.Initialize();
            TimesCommon.Current.TimesInitialize();
            Amount.Initialize();
            // [DM] not needed
            //amount_t::stream_fullstrings = true; // make reports from UnitTests accurate
        }

        [TestCleanup]
        public void Cleanup()
        {
            MainApplicationContext.Cleanup();
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Commodity_TestPriceHistory()
        {
            DateTime jan17_05;
            DateTime jan17_06;
            DateTime jan17_07;
            DateTime feb27_07;
            DateTime feb28_07;
            DateTime feb28_07sbm;
            DateTime mar01_07;
            DateTime apr15_07;

            jan17_05 = TimesCommon.Current.ParseDateTime("2005/01/17 00:00:00");
            jan17_06 = TimesCommon.Current.ParseDateTime("2006/01/17 00:00:00");
            jan17_07 = TimesCommon.Current.ParseDateTime("2007/01/17 00:00:00");
            feb27_07 = TimesCommon.Current.ParseDateTime("2007/02/27 18:00:00");
            feb28_07 = TimesCommon.Current.ParseDateTime("2007/02/28 06:00:00");
            feb28_07sbm = TimesCommon.Current.ParseDateTime("2007/02/28 11:59:59");
            mar01_07 = TimesCommon.Current.ParseDateTime("2007/03/01 00:00:00");
            apr15_07 = TimesCommon.Current.ParseDateTime("2007/04/15 13:00:00");

            Amount x0 = new Amount();
            Amount x1 = new Amount("100.10 AAPL");

            Boost.CheckThrow<AmountError, Amount>(() => x0.Value());
            Assert.IsTrue(!(bool)x1.Value());

            // Commodities cannot be constructed by themselves, since a great deal
            // of their state depends on how they were seen to be used.
            Commodity aapl = x1.Commodity;

            aapl.AddPrice(jan17_07, new Amount("$10.20"));
            aapl.AddPrice(feb27_07, new Amount("$13.40"));
            aapl.AddPrice(feb28_07, new Amount("$18.33"));
            aapl.AddPrice(feb28_07sbm, new Amount("$18.30"));
            aapl.AddPrice(mar01_07, new Amount("$19.50"));
            aapl.AddPrice(apr15_07, new Amount("$21.22"));
            aapl.AddPrice(jan17_05, new Amount("EUR 23.00"));
            aapl.AddPrice(jan17_06, new Amount("CAD 25.00"));

            Amount one_euro = new Amount("EUR 1.00");
            Commodity euro = one_euro.Commodity;

            euro.AddPrice(feb27_07, new Amount("CAD 1.40"));
            euro.AddPrice(jan17_05, new Amount("$0.78"));

            Amount one_cad = new Amount("CAD 1.00");
            Commodity cad = one_cad.Commodity;

            cad.AddPrice(jan17_06, new Amount("$1.11"));

            Amount amt = x1.Value(feb28_07sbm);
            Assert.IsNotNull(amt);
            Assert.AreEqual(new Amount("$1831.83"), amt);

            amt = x1.Value(TimesCommon.Current.CurrentTime);
            Assert.IsNotNull(amt);
            Assert.AreEqual("$2124.12", amt.ToString());
            Assert.AreEqual("$2124.122", amt.ToFullString());

            amt = x1.Value(TimesCommon.Current.CurrentTime, euro);
            Assert.IsNotNull(amt);
            Assert.AreEqual("EUR 1787.50", amt.Rounded().ToString());

            // Add a newer Euro pricing
            aapl.AddPrice(jan17_07, new Amount("EUR 23.00"));

            amt = x1.Value(TimesCommon.Current.CurrentTime, euro);
            Assert.IsNotNull(amt);
            Assert.AreEqual("EUR 2302.30", amt.ToString());

            amt = x1.Value(TimesCommon.Current.CurrentTime, cad);
            Assert.IsNotNull(amt);
            Assert.AreEqual("CAD 3223.22", amt.ToString());

            Assert.IsTrue(x1.Valid());
        }
    }
}

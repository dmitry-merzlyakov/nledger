// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Scopus;
using NLedger.Times;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.IntegrationTests.unit
{
    /// <summary>
    /// Ported from t_amount.cc
    /// </summary>
    public class TestAmount : IDisposable
    {
        public TestAmount()
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

            // Cause the display precision for dollars to be initialized to 2.
            Amount x1 = new Amount("$1.00");
            Assert.True((bool)x1);

            // [DM] not needed
            //amount_t::stream_fullstrings = true; // make reports from UnitTests accurate
        }

        private void Cleanup()
        {
            MainContextAcquirer.Dispose();
        }

        public MainApplicationContext.ThreadAcquirer MainContextAcquirer { get; private set; }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestParser()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount();
            Amount x2 = new Amount();
            Amount x3 = new Amount();
            Amount x4 = new Amount("123.456");
            Amount x5 = new Amount(x4);
            Amount x6 = new Amount(x4);
            Amount x7 = new Amount(x4);
            Amount x8 = new Amount("$123.45");
            Amount x9 = new Amount(x8);
            Amount x10 = new Amount(x8);
            Amount x11 = new Amount(x8);
            Amount x12 = new Amount("$100");

            Assert.Equal(2, x12.Commodity.Precision);

            //NOT_FOR_PYTHON
            Amount x13 = new Amount();
            x13.Parse("$100...");
            Assert.Equal(x12, x13);

            Amount x14 = new Amount();
            Assert.Throws<AmountError>(() => x14.Parse("DM"));

            Amount x15 = new Amount("$1.000.000,00");   // parsing this switches us to European

            Amount x16 = new Amount("$2000");
            Assert.Equal("$2.000,00", x16.ToString());
            x16.Parse("$2000,00");
            Assert.Equal("$2.000,00", x16.ToString());

            // Since use of a decimal-comma is an additive quality, we must switch back
            // to decimal-period manually.
            x15.Commodity.Flags &= ~Commodities.CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;

            Amount x17 = new Amount("$1,000,000.00");   // parsing this switches back to American

            Amount x18 = new Amount("$2000");
            Assert.Equal("$2,000.00", x18.ToString());
            x18.Parse("$2,000");
            Assert.Equal("$2,000.00", x18.ToString());

            Assert.Equal(x15, x17);

            Amount x19 = new Amount("EUR 1000");
            Amount x20 = new Amount("EUR 1000");

            Assert.Equal("EUR 1000", x19.ToString());
            Assert.Equal("EUR 1000", x20.ToString());

            x1.Parse("$100.0000", AmountParseFlagsEnum.PARSE_NO_MIGRATE);
            Assert.Equal(2, x12.Commodity.Precision);
            Assert.Equal(x1.Commodity, x12.Commodity);
            Assert.Equal(x1, x12);

            x0.Parse("$100.0000");
            Assert.Equal(4, x12.Commodity.Precision);
            Assert.Equal(x0.Commodity, x12.Commodity);
            Assert.Equal(x0, x12);

            x2.Parse("$100.00", AmountParseFlagsEnum.PARSE_NO_REDUCE);
            Assert.Equal(x2, x12);
            x3.Parse("$100.00", AmountParseFlagsEnum.PARSE_NO_MIGRATE | AmountParseFlagsEnum.PARSE_NO_REDUCE);
            Assert.Equal(x3, x12);

            x4.Parse("$100.00");
            Assert.Equal(x4, x12);
            x5.Parse("$100.00", AmountParseFlagsEnum.PARSE_NO_MIGRATE);
            Assert.Equal(x5, x12);
            x6.Parse("$100.00", AmountParseFlagsEnum.PARSE_NO_REDUCE);
            Assert.Equal(x6, x12);
            x7.Parse("$100.00", AmountParseFlagsEnum.PARSE_NO_MIGRATE | AmountParseFlagsEnum.PARSE_NO_REDUCE);
            Assert.Equal(x7, x12);

            x8.Parse("$100.00");
            Assert.Equal(x8, x12);
            x9.Parse("$100.00", AmountParseFlagsEnum.PARSE_NO_MIGRATE);
            Assert.Equal(x9, x12);
            x10.Parse("$100.00", AmountParseFlagsEnum.PARSE_NO_REDUCE);
            Assert.Equal(x10, x12);
            x11.Parse("$100.00", AmountParseFlagsEnum.PARSE_NO_MIGRATE | AmountParseFlagsEnum.PARSE_NO_REDUCE);
            Assert.Equal(x11, x12);

            Assert.True(x0.Valid());
            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
            Assert.True(x7.Valid());
            Assert.True(x8.Valid());
            Assert.True(x9.Valid());
            Assert.True(x10.Valid());
            Assert.True(x11.Valid());
            Assert.True(x12.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestConstructors()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount(123456L);
            Amount x2 = new Amount(123456UL);
            Amount x3 = new Amount("123.456");
            Amount x5 = new Amount("123456");
            Amount x6 = new Amount("123.456");
            Amount x7 = new Amount("123456");
            Amount x8 = new Amount("123.456");
            Amount x9 = new Amount(x3);
            Amount x10 = new Amount(x6);
            Amount x11 = new Amount(x8);

            Assert.Equal(new Amount(), x0);
            Assert.NotEqual(new Amount("0"), x0);
            Assert.NotEqual(new Amount("0.0"), x0);
            Assert.Equal(x2, x1);
            Assert.Equal(x5, x1);
            Assert.Equal(x7, x1);
            Assert.Equal(x6, x3);
            Assert.Equal(x8, x3);
            Assert.Equal(x10, x3);
            Assert.Equal(x10, x9);

            Assert.True(x0.Valid());
            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
            Assert.True(x7.Valid());
            Assert.True(x8.Valid());
            Assert.True(x9.Valid());
            Assert.True(x10.Valid());
            Assert.True(x11.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityConstructors()
        {
            Amount x1 = new Amount("$123.45");
            Amount x2 = new Amount("-$123.45");
            Amount x3 = new Amount("$-123.45");
            Amount x4 = new Amount("DM 123.45");
            Amount x5 = new Amount("-DM 123.45");
            Amount x6 = new Amount("DM -123.45");
            Amount x7 = new Amount("123.45 euro");
            Amount x8 = new Amount("-123.45 euro");
            Amount x9 = new Amount("123.45€");
            Amount x10 = new Amount("-123.45€");

            Assert.Equal(new Amount("$123.45"), x1);
            Assert.Equal(new Amount("-$123.45"), x2);
            Assert.Equal(new Amount("$-123.45"), x3);
            Assert.Equal(new Amount("DM 123.45"), x4);
            Assert.Equal(new Amount("-DM 123.45"), x5);
            Assert.Equal(new Amount("DM -123.45"), x6);
            Assert.Equal(new Amount("123.45 euro"), x7);
            Assert.Equal(new Amount("-123.45 euro"), x8);
            Assert.Equal(new Amount("123.45€"), x9);
            Assert.Equal(new Amount("-123.45€"), x10);

            Assert.Equal("$123.45", x1.ToString());
            Assert.Equal("$-123.45", x2.ToString());
            Assert.Equal("$-123.45", x3.ToString());
            Assert.Equal("DM 123.45", x4.ToString());
            Assert.Equal("DM -123.45", x5.ToString());
            Assert.Equal("DM -123.45", x6.ToString());
            Assert.Equal("123.45 euro", x7.ToString());
            Assert.Equal("-123.45 euro", x8.ToString());
            Assert.Equal("123.45€", x9.ToString());
            Assert.Equal("-123.45€", x10.ToString());

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x4.Valid());
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
            Assert.True(x7.Valid());
            Assert.True(x8.Valid());
            Assert.True(x9.Valid());
            Assert.True(x10.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestAssignment()
        {
            Amount x0 = null;
            Amount x1;
            Amount x2;
            Amount x3;
            Amount x5;
            Amount x6;
            Amount x7;
            Amount x8;
            Amount x9;
            Amount x10;

            x1 = (Amount)123456L;
            x2 = (Amount)123456UL;
            x3 = (Amount)"123.456";
            x5 = (Amount)"123456";
            x6 = (Amount)"123.456";
            x7 = (Amount)"123456";
            x8 = (Amount)"123.456";
            x9 = x3;
            x10 = new Amount(x6);

            Assert.Equal(x2, x1);
            Assert.Equal(x5, x1);
            Assert.Equal(x7, x1);
            Assert.Equal(x6, x3);
            Assert.Equal(x8, x3);
            Assert.Equal(x10, x3);
            Assert.Equal(x10, x9);

            Assert.False(Amount.IsNullOrEmpty(x1));
            x1 = x0;
            Assert.True(Amount.IsNullOrEmpty(x0));
            Assert.True(Amount.IsNullOrEmpty(x1));

            Assert.True(x0 == null || x0.Valid());  // [DM] x0 and x1 are null in .Net; the condition is added.
            Assert.True(x1 == null || x1.Valid());  // Basically, it is possible to create an extension method
            Assert.True(x2.Valid());                // that returns True for Null OR Valid amounts to fit this test,
            Assert.True(x3.Valid());                // but this method is useless in practical .Net development.
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
            Assert.True(x7.Valid());
            Assert.True(x8.Valid());
            Assert.True(x9.Valid());
            Assert.True(x10.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityAssignment()
        {
            Amount x1;
            Amount x2;
            Amount x3;
            Amount x4;
            Amount x5;
            Amount x6;
            Amount x7;
            Amount x8;
            Amount x9;
            Amount x10;

            x1 = (Amount)"$123.45";
            x2 = (Amount)"-$123.45";
            x3 = (Amount)"$-123.45";
            x4 = (Amount)"DM 123.45";
            x5 = (Amount)"-DM 123.45";
            x6 = (Amount)"DM -123.45";
            x7 = (Amount)"123.45 euro";
            x8 = (Amount)"-123.45 euro";
            x9 = (Amount)"123.45€";
            x10 = (Amount)"-123.45€";

            Assert.Equal(new Amount("$123.45"), x1);
            Assert.Equal(new Amount("-$123.45"), x2);
            Assert.Equal(new Amount("$-123.45"), x3);
            Assert.Equal(new Amount("DM 123.45"), x4);
            Assert.Equal(new Amount("-DM 123.45"), x5);
            Assert.Equal(new Amount("DM -123.45"), x6);
            Assert.Equal(new Amount("123.45 euro"), x7);
            Assert.Equal(new Amount("-123.45 euro"), x8);
            Assert.Equal(new Amount("123.45€"), x9);
            Assert.Equal(new Amount("-123.45€"), x10);

            Assert.Equal("$123.45", x1.ToString());
            Assert.Equal("$-123.45", x2.ToString());
            Assert.Equal("$-123.45", x3.ToString());
            Assert.Equal("DM 123.45", x4.ToString());
            Assert.Equal("DM -123.45", x5.ToString());
            Assert.Equal("DM -123.45", x6.ToString());
            Assert.Equal("123.45 euro", x7.ToString());
            Assert.Equal("-123.45 euro", x8.ToString());
            Assert.Equal("123.45€", x9.ToString());
            Assert.Equal("-123.45€", x10.ToString());

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
            Assert.True(x7.Valid());
            Assert.True(x8.Valid());
            Assert.True(x9.Valid());
            Assert.True(x10.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestEquality()
        {
            Amount x1 = new Amount(123456L);
            Amount x2 = new Amount(456789L);
            Amount x3 = new Amount(333333L);
            Amount x4 = new Amount("123456.0");
            Amount x5 = new Amount("123456.0");
            Amount x6 = new Amount("123456.0");

            Assert.True(x1 == (Amount)123456L);
            Assert.True(x1 != x2);
            Assert.True(x1 == (x2 - x3));
            Assert.True(x1 == x4);
            Assert.True(x4 == x5);
            Assert.True(x4 == x6);

            Assert.True(x1 == (Amount)123456L);
            Assert.True((Amount)123456L == x1);
            Assert.True(x1 == (Amount)(long)123456UL);
            Assert.True((Amount)(long)123456UL == x1);
            Assert.True(x1 == new Amount("123456.0"));
            Assert.True(new Amount("123456.0") == x1);

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x4.Valid());
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityEquality()
        {
            Amount x0 = new Amount();   // [DM] Setup empty amount instead of Null to pass specialized tests
            Amount x1 = new Amount("$123.45");
            Amount x2 = new Amount("-$123.45");
            Amount x3 = new Amount("$-123.45");
            Amount x4 = new Amount("DM 123.45");
            Amount x5 = new Amount("-DM 123.45");
            Amount x6 = new Amount("DM -123.45");
            Amount x7 = new Amount("123.45 euro");
            Amount x8 = new Amount("-123.45 euro");
            Amount x9 = new Amount("123.45€");
            Amount x10 = new Amount("-123.45€");

            Assert.True(Amount.IsNullOrEmpty(x0));
            Assert.Throws<AmountError>(() => x0.IsZero);
            Assert.Throws<AmountError>(() => x0.IsRealZero);
            Assert.Throws<AmountError>(() => x0.Sign);
            Assert.Throws<AmountError>(() => x0.Compare(x1));
            Assert.Throws<AmountError>(() => x0.Compare(x2));
            Assert.Throws<AmountError>(() => x0.Compare(x0));

            Assert.True(x1 != x2);
            Assert.True(x1 != x4);
            Assert.True(x1 != x7);
            Assert.True(x1 != x9);
            Assert.True(x2 == x3);
            Assert.True(x4 != x5);
            Assert.True(x5 == x6);
            Assert.True(x7 == -x8);
            Assert.True(x9 == -x10);

            Assert.True(x0.Valid());
            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x4.Valid());
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
            Assert.True(x7.Valid());
            Assert.True(x8.Valid());
            Assert.True(x9.Valid());
            Assert.True(x10.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestComparisons()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount(-123L);
            Amount x2 = new Amount(123L);
            Amount x3 = new Amount("-123.45");
            Amount x4 = new Amount("123.45");
            Amount x5 = new Amount("-123.45");
            Amount x6 = new Amount("123.45");

            Assert.Throws<AmountError>(() => x0 > x1);
            Assert.Throws<AmountError>(() => x0 < x2);
            Assert.Throws<AmountError>(() => x0 > x3);
            Assert.Throws<AmountError>(() => x0 < x4);
            Assert.Throws<AmountError>(() => x0 > x5);
            Assert.Throws<AmountError>(() => x0 < x6);

            Assert.True(x1 > x3);
            Assert.True(x3 <= x5);
            Assert.True(x3 >= x5);
            Assert.True(x3 < x1);
            Assert.True(x3 < x4);

            Assert.True(x1 < (Amount)100L);
            Assert.True((Amount)100L > x1);
            Assert.True(x1 < (Amount)(long)100UL);
            Assert.True((Amount)(long)100UL > x1);
            Assert.True(x1 < (Amount)100.0);
            Assert.True((Amount)100.0 > x1);

            Assert.True(x0.Valid());
            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x4.Valid());
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityComparisons()
        {
            Amount x1 = new Amount("$-123");
            Amount x2 = new Amount("$123.00");
            Amount x3 = Amount.Exact("$-123.4544");
            Amount x4 = Amount.Exact("$123.4544");
            Amount x5 = new Amount("$-123.45");
            Amount x6 = new Amount("$123.45");
            Amount x7 = new Amount("DM 123.45");

            Assert.True(x1 > x3);
            Assert.True(x3 <= x5);
            Assert.True(x3 < x5);
            Assert.True(x3 <= x5);
            Assert.True(!(x3 == x5));
            Assert.True(x3 < x1);
            Assert.True(x3 < x4);
            Assert.True(!(x6 == x7));
            Assert.Throws<AmountError>(() => x6 < x7);

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x4.Valid());
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestIntegerAddition()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount(123L);
            Amount y1 = new Amount(456L);

            Assert.Equal(new Amount(579L), x1 + y1);
            Assert.Equal(new Amount(579L), x1 + (Amount)456L);
            Assert.Equal(new Amount(579L), (Amount)456L + x1);

            x1 += new Amount(456L);
            Assert.Equal(new Amount(579L), x1);
            x1 += (Amount)456L;
            Assert.Equal(new Amount(1035L), x1);

            Amount x4 = new Amount("123456789123456789123456789");

            Assert.Equal(new Amount("246913578246913578246913578"), x4 + x4);

            Assert.True(x0.Valid());
            Assert.True(x1.Valid());
            Assert.True(y1.Valid());
            Assert.True(x4.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestFractionalAddition()
        {
            Amount x1 = new Amount("123.123");
            Amount y1 = new Amount("456.456");

            Assert.Equal(new Amount("579.579"), x1 + y1);
            Assert.Equal(new Amount("579.579"), x1 + new Amount("456.456"));
            Assert.Equal(new Amount("579.579"), new Amount("456.456") + x1);

            x1 += new Amount("456.456");
            Assert.Equal(new Amount("579.579"), x1);
            x1 += new Amount("456.456");
            Assert.Equal(new Amount("1036.035"), x1);
            x1 += (Amount)456L;
            Assert.Equal(new Amount("1492.035"), x1);

            Amount x2=new Amount("123456789123456789.123456789123456789");

            var xx = x2 + x2;
            var xs = xx.ToString();
            Assert.Equal(new Amount("246913578246913578.246913578246913578"), x2 + x2);

            Assert.True(x1.Valid());
            Assert.True(y1.Valid());
            Assert.True(x2.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityAddition()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount("$123.45");
            Amount x2 = Amount.Exact("$123.456789");
            Amount x3 = new Amount("DM 123.45");
            Amount x4 = new Amount("123.45 euro");
            Amount x5 = new Amount("123.45€");
            Amount x6 = new Amount("123.45");

            Assert.Equal(new Amount("$246.90"), x1 + x1);
            Assert.NotEqual(new Amount("$246.91"), x1 + x2);
            Assert.Equal(Amount.Exact("$246.906789"), x1 + x2);

            // Converting to string drops internal precision
            Assert.Equal("$246.90", (x1 + x1).ToString());
            Assert.Equal("$246.91", (x1 + x2).ToString());

            Assert.Throws<AmountError>(() => x1 + x0);
            Assert.Throws<AmountError>(() => x0 + x1);
            Assert.Throws<AmountError>(() => x0 + x0);
            Assert.Throws<AmountError>(() => x1 + x3);
            Assert.Throws<AmountError>(() => x1 + x4);
            Assert.Throws<AmountError>(() => x1 + x5);
            Assert.Equal("$246.90", (x1 + x6).ToString());

            Assert.Equal("$246.90", (x1 + (Amount)123.45).ToString());
            Assert.Equal("$246.45", (x1 + (Amount)123L).ToString());

            Assert.Equal(new Amount("DM 246.90"), x3 + x3);
            Assert.Equal(new Amount("246.90 euro"), x4 + x4);
            Assert.Equal(new Amount("246.90€"), x5 + x5);

            Assert.Equal("DM 246.90", (x3 + x3).ToString());
            Assert.Equal("246.90 euro", (x4 + x4).ToString());
            Assert.Equal("246.90€", (x5 + x5).ToString());

            x1 += new Amount("$456.45");
            Assert.Equal(new Amount("$579.90"), x1);
            x1 += new Amount("$456.45");
            Assert.Equal(new Amount("$1036.35"), x1);
            x1 += new Amount("$456");
            Assert.Equal(new Amount("$1492.35"), x1);

            Amount x7 = Amount.Exact("$123456789123456789.123456789123456789");

            Assert.Equal(Amount.Exact("$246913578246913578.246913578246913578"), x7 + x7);

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x4.Valid());
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
            Assert.True(x7.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestIntegerSubtraction()
        {
            Amount x1 = new Amount(123L);
            Amount y1 = new Amount(456L);

            Assert.Equal(new Amount(333L), y1 - x1);
            Assert.Equal(new Amount(-333L), x1 - y1);
            Assert.Equal(new Amount(23L), x1 - (Amount)100L);
            Assert.Equal(new Amount(-23L), (Amount)100L - x1);

            x1 -= new Amount(456L);
            Assert.Equal(new Amount(-333L), x1);
            x1 -= (Amount)456L;
            Assert.Equal(new Amount(-789L), x1);

            Amount x4 = new Amount("123456789123456789123456789");
            Amount y4 = new Amount("8238725986235986");

            Assert.Equal(new Amount("123456789115218063137220803"), x4 - y4);
            Assert.Equal(new Amount("-123456789115218063137220803"), y4 - x4);

            Assert.True(x1.Valid());
            Assert.True(y1.Valid());
            Assert.True(x4.Valid());
            Assert.True(y4.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestFractionalSubtraction()
        {
            Amount x1 = new Amount("123.123");
            Amount y1 = new Amount("456.456");

            Assert.Equal(new Amount("-333.333"), x1 - y1);
            Assert.Equal(new Amount("333.333"), y1 - x1);

            x1 -= new Amount("456.456");
            Assert.Equal(new Amount("-333.333"), x1);
            x1 -= new Amount("456.456");
            Assert.Equal(new Amount("-789.789"), x1);
            x1 -= (Amount)456L;
            Assert.Equal(new Amount("-1245.789"), x1);

            Amount x2 = new Amount("123456789123456789.123456789123456789");
            Amount y2 = new Amount("9872345982459.248974239578");

            Assert.Equal(new Amount("123446916777474329.874482549545456789"), x2 - y2);
            Assert.Equal(new Amount("-123446916777474329.874482549545456789"), y2 - x2);

            Assert.True(x1.Valid());
            Assert.True(y1.Valid());
            Assert.True(x2.Valid());
            Assert.True(y2.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommoditySubtraction()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount("$123.45");
            Amount x2 = Amount.Exact("$123.456789");
            Amount x3 = new Amount("DM 123.45");
            Amount x4 = new Amount("123.45 euro");
            Amount x5 = new Amount("123.45€");
            Amount x6 = new Amount("123.45");

            Assert.NotEqual(new Amount(), x1 - x1);
            Assert.Equal(new Amount("$0"), x1 - x1);
            Assert.Equal(new Amount("$23.45"), x1 - new Amount("$100.00"));
            Assert.Equal(new Amount("$-23.45"), new Amount("$100.00") - x1);
            Assert.NotEqual(new Amount("$-0.01"), x1 - x2);
            Assert.Equal(Amount.Exact("$-0.006789"), x1 - x2);

            // Converting to string drops internal precision.  If an amount is
            // zero, it drops the commodity as well.
            Assert.Equal("$0.00", (x1 - x1).ToString());
            Assert.Equal("$-0.01", (x1 - x2).ToString());

            Assert.Throws<AmountError>(() => x1 - x0);
            Assert.Throws<AmountError>(() => x0 - x1);
            Assert.Throws<AmountError>(() => x0 - x0);
            Assert.Throws<AmountError>(() => x1 - x3);
            Assert.Throws<AmountError>(() => x1 - x4);
            Assert.Throws<AmountError>(() => x1 - x5);
            Assert.Equal("$0.00", (x1 - x6).ToString());
            // [DM] Not applicable in .Net; exact subtraction produces exact zero with no negative fractional part
            // Assert.Equal("$-0.00", (x1 - (Amount)123.45).ToString());
            Assert.Equal("$0.45", (x1 - (Amount)123L).ToString());

            Assert.Equal(new Amount("DM 0.00"), x3 - x3);
            Assert.Equal(new Amount("DM 23.45"), x3 - new Amount("DM 100.00"));
            Assert.Equal(new Amount("DM -23.45"), new Amount("DM 100.00") - x3);
            Assert.Equal(new Amount("0.00 euro"), x4 - x4);
            Assert.Equal(new Amount("23.45 euro"), x4 - new Amount("100.00 euro"));
            Assert.Equal(new Amount("-23.45 euro"), new Amount("100.00 euro") - x4);
            Assert.Equal(new Amount("0.00€"), x5 - x5);
            Assert.Equal(new Amount("23.45€"), x5 - new Amount("100.00€"));
            Assert.Equal(new Amount("-23.45€"), new Amount("100.00€") - x5);

            Assert.Equal("DM 0.00", (x3 - x3).ToString());
            Assert.Equal("DM 23.45", (x3 - new Amount("DM 100.00")).ToString());
            Assert.Equal("DM -23.45", (new Amount("DM 100.00") - x3).ToString());
            Assert.Equal("0.00 euro", (x4 - x4).ToString());
            Assert.Equal("23.45 euro", (x4 - new Amount("100.00 euro")).ToString());
            Assert.Equal("-23.45 euro", (new Amount("100.00 euro") - x4).ToString());
            Assert.Equal("0.00€", (x5 - x5).ToString());
            Assert.Equal("23.45€", (x5 - new Amount("100.00€")).ToString());
            Assert.Equal("-23.45€", (new Amount("100.00€") - x5).ToString());

            x1 -= new Amount("$456.45");
            Assert.Equal(new Amount("$-333.00"), x1);
            x1 -= new Amount("$456.45");
            Assert.Equal(new Amount("$-789.45"), x1);
            x1 -= new Amount("$456");
            Assert.Equal(new Amount("$-1245.45"), x1);

            Amount x7 = Amount.Exact("$123456789123456789.123456789123456789");
            Amount x8 = Amount.Exact("$2354974984698.98459845984598");

            Assert.Equal(Amount.Exact("$123454434148472090.138858329277476789"), x7 - x8);
            Assert.Equal("$123454434148472090.138858329277476789", (x7 - x8).ToString());
            Assert.Equal("$123454434148472090.14",
                        (new Amount("$1.00") * (x7 - x8)).ToString());
            Assert.Equal(Amount.Exact("$-123454434148472090.138858329277476789"), x8 - x7);
            Assert.Equal("$-123454434148472090.138858329277476789", (x8 - x7).ToString());
            Assert.Equal("$-123454434148472090.14",
                        (new Amount("$1.00") * (x8 - x7)).ToString());

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x4.Valid());
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
            Assert.True(x7.Valid());
            Assert.True(x8.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestIntegerMultiplication()
        {
            Amount x1 = new Amount(123L);
            Amount y1 = new Amount(456L);

            Assert.Equal(new Amount(0L), x1 * (Amount)0L);
            Assert.Equal(new Amount(0L), new Amount(0L) * x1);
            Assert.Equal(new Amount(0L), (Amount)0L * x1);
            Assert.Equal(x1, x1 * (Amount)1L);
            Assert.Equal(x1, new Amount(1L) * x1);
            Assert.Equal(x1, (Amount)1L * x1);
            Assert.Equal(-x1, x1 * (Amount)(-1L));
            Assert.Equal(-x1, new Amount(-1L) * x1);
            Assert.Equal(-x1, (Amount)(-1L) * x1);
            Assert.Equal(new Amount(56088L), x1 * y1);
            Assert.Equal(new Amount(56088L), y1 * x1);
            Assert.Equal(new Amount(56088L), x1 * (Amount)456L);
            Assert.Equal(new Amount(56088L), new Amount(456L) * x1);
            Assert.Equal(new Amount(56088L), (Amount)456L * x1);

            x1 *= new Amount(123L);
            Assert.Equal(new Amount(15129L), x1);
            x1 *= (Amount)123L;
            Assert.Equal(new Amount(1860867L), x1);

            Amount x4 = new Amount("123456789123456789123456789");
            Assert.Equal(new Amount("15241578780673678546105778281054720515622620750190521"), x4 * x4);

            Assert.True(x1.Valid());
            Assert.True(y1.Valid());
            Assert.True(x4.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestFractionalMultiplication()
        {
            Amount x1 = new Amount("123.123");
            Amount y1 = new Amount("456.456");

            Assert.Equal(new Amount(0L), x1 * (Amount)0L);
            Assert.Equal(new Amount(0L), new Amount(0L) * x1);
            Assert.Equal(new Amount(0L), (Amount)0L * x1);
            Assert.Equal(x1, x1 * (Amount)1L);
            Assert.Equal(x1, new Amount(1L) * x1);
            Assert.Equal(x1, (Amount)1L * x1);
            Assert.Equal(-x1, x1 * (Amount)(-1L));
            Assert.Equal(-x1, new Amount(-1L) * x1);
            Assert.Equal(-x1, (Amount)(-1L) * x1);
            Assert.Equal(new Amount("56200.232088"), x1 * y1);
            Assert.Equal(new Amount("56200.232088"), y1 * x1);
            Assert.Equal(new Amount("56200.232088"), x1 * new Amount("456.456"));
            Assert.Equal(new Amount("56200.232088"), new Amount("456.456") * x1);
            Assert.Equal(new Amount("56200.232088"), new Amount("456.456") * x1);

            x1 *= new Amount("123.123");
            Assert.Equal(new Amount("15159.273129"), x1);
            x1 *= new Amount("123.123");
            Assert.Equal(new Amount("1866455.185461867"), x1);
            x1 *= (Amount)123L;
            Assert.Equal(new Amount("229573987.811809641"), x1);

            Amount x2 = new Amount("123456789123456789.123456789123456789");
            Assert.Equal(new Amount("15241578780673678546105778311537878.046486820281054720515622620750190521"), x2 * x2);

            Assert.True(x1.Valid());
            Assert.True(y1.Valid());
            Assert.True(x2.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityMultiplication()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount("$123.12");
            Amount y1 = new Amount("$456.45");
            Amount x2 = Amount.Exact("$123.456789");
            Amount x3 = new Amount("DM 123.45");
            Amount x4 = new Amount("123.45 euro");
            Amount x5 = new Amount("123.45€");

            Assert.Equal(new Amount("$0.00"), x1 * (Amount)0L);
            Assert.Equal(new Amount("$0.00"), (Amount)0L * x1);
            Assert.Equal(x1, x1 * (Amount)1L);
            Assert.Equal(x1, (Amount)1L * x1);
            Assert.Equal(-x1, x1 * (Amount)(-1L));
            Assert.Equal(-x1, (Amount)(-1L) * x1);
            Assert.Equal(Amount.Exact("$56198.124"), x1 * y1);
            Assert.Equal("$56198.12", (x1 * y1).ToString());
            Assert.Equal(Amount.Exact("$56198.124"), y1 * x1);
            Assert.Equal("$56198.12", (y1 * x1).ToString());

            // Internal amounts retain their precision, even when being
            // converted to strings
            Assert.Equal(Amount.Exact("$15199.99986168"), x1 * x2);
            Assert.Equal(Amount.Exact("$15199.99986168"), x2 * x1);
            Assert.Equal("$15200.00", (x1 * x2).ToString());
            Assert.Equal("$15199.99986168", (x2 * x1).ToString());

            Assert.Throws<AmountError>(() => x1 * x0);
            Assert.Throws<AmountError>(() => x0 * x1);
            Assert.Throws<AmountError>(() => x0 * x0);
            //BOOST_CHECK_THROW(x1 * x3, amount_error);
            //BOOST_CHECK_THROW(x1 * x4, amount_error);
            //BOOST_CHECK_THROW(x1 * x5, amount_error);

            x1 *= new Amount("123.12");
            Assert.Equal(Amount.Exact("$15158.5344"), x1);
            Assert.Equal("$15158.53", x1.ToString());
            x1 *= new Amount("123.12");
            Assert.Equal(Amount.Exact("$1866318.755328"), x1);
            Assert.Equal("$1866318.76", x1.ToString());
            x1 *= (Amount)123L;
            Assert.Equal(Amount.Exact("$229557206.905344"), x1);
            Assert.Equal("$229557206.91", x1.ToString());

            Amount x7 = Amount.Exact("$123456789123456789.123456789123456789");
            Assert.Equal(Amount.Exact("$15241578780673678546105778311537878.046486820281054720515622620750190521"), x7 * x7);

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x4.Valid());
            Assert.True(x5.Valid());
            Assert.True(x7.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestIntegerDivision()
        {
            Amount x1 = new Amount(123L);
            Amount y1 = new Amount(456L);

            Assert.Throws<AmountError>(() => x1 / (Amount)0L);  // [DM] "Divide by zero"
            Assert.Equal(new Amount(0L), new Amount(0L) / x1);
            Assert.Equal(new Amount(0L), (Amount)0L / x1);
            Assert.Equal(x1, x1 / (Amount)1L);
            Assert.Equal("0.00813", (new Amount(1L) / x1).ToString());
            Assert.Equal("0.00813", ((Amount)1L / x1).ToString());
            Assert.Equal(-x1, x1 / (Amount)(-1L));
            Assert.Equal("-0.00813", (new Amount(-1L) / x1).ToString());
            Assert.Equal("-0.00813", ((Amount)(-1L) / x1).ToString());
            Assert.Equal("0.269737", (x1 / y1).ToString());
            Assert.Equal("3.707317", (y1 / x1).ToString());
            Assert.Equal("0.269737", (x1 / (Amount)456L).ToString());
            Assert.Equal("3.707317", (new Amount(456L) / x1).ToString());
            Assert.Equal("3.707317", ((Amount)456L / x1).ToString());

            x1 /= new Amount(456L);
            Assert.Equal("0.269737", x1.ToString());
            x1 /= (Amount)456L;
            Assert.Equal("0.000591528163", x1.ToString());

            Amount x4 = new Amount("123456789123456789123456789");
            Amount y4 = new Amount("56");

            Assert.Equal(new Amount(1L), x4 / x4);
            Assert.Equal("2204585520061728377204585.517857", (x4 / y4).ToString());

            Assert.Equal(new Amount("0.000000000000000000000000000001"), new Amount("10") / new Amount("10000000000000000000000000000000"));

            Assert.True(x1.Valid());
            Assert.True(y1.Valid());
            Assert.True(x4.Valid());
            Assert.True(y4.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestFractionalDivision()
        {
            Amount x1 = new Amount("123.123");
            Amount y1 = new Amount("456.456");

            Assert.Throws<AmountError>(() => x1 / (Amount)0L);  // "Divide by zero"
            Assert.Equal("0.0081219593", (new Amount("1.0") / x1).ToString());
            Assert.Equal("0.0081219593", (new Amount("1.0") / x1).ToString());
            Assert.Equal(x1, x1 / new Amount("1.0"));
            Assert.Equal("0.0081219593", (new Amount("1.0") / x1).ToString());
            Assert.Equal("0.0081219593", (new Amount("1.0") / x1).ToString());
            Assert.Equal(-x1, x1 / new Amount("-1.0"));
            Assert.Equal("-0.0081219593", (new Amount("-1.0") / x1).ToString());
            Assert.Equal("-0.0081219593", (new Amount("-1.0") / x1).ToString());
            Assert.Equal("0.269736842105", (x1 / y1).ToString());
            Assert.Equal("3.707317073171", (y1 / x1).ToString());
            Assert.Equal("0.269736842105", (x1 / new Amount("456.456")).ToString());
            Assert.Equal("3.707317073171", (new Amount("456.456") / x1).ToString());
            Assert.Equal("3.707317073171", (new Amount("456.456") / x1).ToString());

            x1 /= new Amount("456.456");
            Assert.Equal("0.269736842105", x1.ToString());
            x1 /= new Amount("456.456");
            Assert.Equal("0.000590937225286255757", x1.ToString());
            x1 /= (Amount)456L;
            Assert.Equal("0.000001295914967733017011337", x1.ToString());

            Amount x4 = new Amount("1234567891234567.89123456789");
            Amount y4 = new Amount("56.789");

            Assert.Equal(new Amount("1.0"), x4 / x4);
            Assert.Equal("21739560323910.75544972737484371973", (x4 / y4).ToString());

            Assert.True(x1.Valid());
            Assert.True(y1.Valid());
            Assert.True(x4.Valid());
            Assert.True(y4.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityDivision()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount("$123.12");
            Amount y1 = new Amount("$456.45");
            Amount x2 = Amount.Exact("$123.456789");
            Amount x3 = new Amount("DM 123.45");
            Amount x4 = new Amount("123.45 euro");
            Amount x5 = new Amount("123.45€");

            Assert.Throws<AmountError>(() => x1 / (Amount)0L);  // "Divide by zero"
            Assert.Equal(new Amount("$0.00"), (Amount)0L / x1);
            Assert.Equal(x1, x1 / (Amount)1L);
            Assert.Equal("$0.00812216", ((Amount)1L / x1).ToFullString());
            Assert.Equal(-x1, x1 / (Amount)(-1L));
            Assert.Equal("$-0.00812216", ((Amount)(-1L) / x1).ToFullString());
            Assert.Equal("$0.26973382", (x1 / y1).ToFullString());
            Assert.Equal("$0.27", (x1 / y1).ToString());
            Assert.Equal("$3.70735867", (y1 / x1).ToFullString());
            Assert.Equal("$3.71", (y1 / x1).ToString());

            // Internal amounts retain their precision, even when being
            // converted to strings
            Assert.Equal("$0.99727201", (x1 / x2).ToFullString());
            Assert.Equal("$1.00273545321637", (x2 / x1).ToFullString());
            Assert.Equal("$1.00", (x1 / x2).ToString());
            Assert.Equal("$1.00273545321637", (x2 / x1).ToString());

            Assert.Throws<AmountError>(() => (x1 / x0));
            Assert.Throws<AmountError>(() => (x0 / x1));
            Assert.Throws<AmountError>(() => (x0 / x0));

            x1 /= new Amount("123.12");
            Assert.Equal("$1.00", x1.ToString());
            x1 /= new Amount("123.12");
            Assert.Equal("$0.00812216", x1.ToFullString());
            Assert.Equal("$0.01", x1.ToString());
            x1 /= (Amount)123L;
            Assert.Equal("$0.00006603", x1.ToFullString());
            Assert.Equal("$0.00", x1.ToString());

            Amount x6 = Amount.Exact("$237235987235987.98723987235978");
            Amount x7 = Amount.Exact("$123456789123456789.123456789123456789");

            Assert.Equal(new Amount("$1"), x7 / x7);
            Assert.Equal("$0.0019216115121765559608381226612019501", (x6 / x7).ToFullString());
            Assert.Equal("$520.39654928343335571379527154924040947272", (x7 / x6).ToFullString());

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x4.Valid());
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
            Assert.True(x7.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestNegation()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount(-123456L);
            Amount x3 = new Amount("-123.456");
            Amount x5 = new Amount("-123456");
            Amount x6 = new Amount("-123.456");
            Amount x7 = new Amount("-123456");
            Amount x8 = new Amount("-123.456");
            Amount x9 = new Amount(-x3);

            Assert.Throws<AmountError>(() => x0.Negated());
            Assert.Equal(x5, x1);
            Assert.Equal(x7, x1);
            Assert.Equal(x6, x3);
            Assert.Equal(x8, x3);
            Assert.Equal(-x6, x9);
            Assert.Equal(x3.Negated(), x9);

            Amount x10 = new Amount(x9.Negated());

            Assert.Equal(x3, x10);

            Assert.True(x1.Valid());
            Assert.True(x3.Valid());
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
            Assert.True(x7.Valid());
            Assert.True(x8.Valid());
            Assert.True(x9.Valid());
            Assert.True(x10.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityNegation()
        {
            Amount x1 = new Amount("$123.45");
            Amount x2 = new Amount("-$123.45");
            Amount x3 = new Amount("$-123.45");
            Amount x4 = new Amount("DM 123.45");
            Amount x5 = new Amount("-DM 123.45");
            Amount x6 = new Amount("DM -123.45");
            Amount x7 = new Amount("123.45 euro");
            Amount x8 = new Amount("-123.45 euro");
            Amount x9 = new Amount("123.45€");
            Amount x10 = new Amount("-123.45€");

            Assert.Equal(new Amount("$-123.45"), -x1);
            Assert.Equal(new Amount("$123.45"), -x2);
            Assert.Equal(new Amount("$123.45"), -x3);
            Assert.Equal(new Amount("DM -123.45"), -x4);
            Assert.Equal(new Amount("DM 123.45"), -x5);
            Assert.Equal(new Amount("DM 123.45"), -x6);
            Assert.Equal(new Amount("-123.45 euro"), -x7);
            Assert.Equal(new Amount("123.45 euro"), -x8);
            Assert.Equal(new Amount("-123.45€"), -x9);
            Assert.Equal(new Amount("123.45€"), -x10);

            Assert.Equal(new Amount("$-123.45"), x1.Negated());
            Assert.Equal(new Amount("$123.45"), x2.Negated());
            Assert.Equal(new Amount("$123.45"), x3.Negated());

            Assert.Equal("$-123.45", (-x1).ToString());
            Assert.Equal("$123.45", (-x2).ToString());
            Assert.Equal("$123.45", (-x3).ToString());
            Assert.Equal("DM -123.45", (-x4).ToString());
            Assert.Equal("DM 123.45", (-x5).ToString());
            Assert.Equal("DM 123.45", (-x6).ToString());
            Assert.Equal("-123.45 euro", (-x7).ToString());
            Assert.Equal("123.45 euro", (-x8).ToString());
            Assert.Equal("-123.45€", (-x9).ToString());
            Assert.Equal("123.45€", (-x10).ToString());

            Assert.Equal(new Amount("$-123.45"), x1.Negated());
            Assert.Equal(new Amount("$123.45"), x2.Negated());
            Assert.Equal(new Amount("$123.45"), x3.Negated());

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x4.Valid());
            Assert.True(x5.Valid());
            Assert.True(x6.Valid());
            Assert.True(x7.Valid());
            Assert.True(x8.Valid());
            Assert.True(x9.Valid());
            Assert.True(x10.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestAbs()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount(-1234L);
            Amount x2 = new Amount(1234L);

            Assert.Throws<AmountError>(() => x0.Abs());
            Assert.Equal(new Amount(1234L), x1.Abs());
            Assert.Equal(new Amount(1234L), x2.Abs());

            Assert.True(x0.Valid());
            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityAbs()
        {
            Amount x1 = new Amount("$-1234.56");
            Amount x2 = new Amount("$1234.56");

            Assert.Equal(new Amount("$1234.56"), x1.Abs());
            Assert.Equal(new Amount("$1234.56"), x2.Abs());

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestFloor()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount("123.123");
            Amount x2 = new Amount("-123.123");

            Assert.Throws<AmountError>(() => x0.Floored());
            Assert.Equal(new Amount(123L), x1.Floored());
            Assert.Equal(new Amount(-124L), x2.Floored());

            Assert.True(x0.Valid());
            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityFloor()
        {
            Amount x1 = new Amount("$1234.56");
            Amount x2 = new Amount("$-1234.56");

            Assert.Equal(new Amount("$1234"), x1.Floored());
            Assert.Equal(new Amount("$-1235"), x2.Floored());

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCeiling()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount("123.123");
            Amount x2 = new Amount("-123.123");

            Assert.Throws<AmountError>(() => x0.Ceilinged());
            Assert.Equal(new Amount(124L), x1.Ceilinged());
            Assert.Equal(new Amount(-123L), x2.Ceilinged());

            Assert.True(x0.Valid());
            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityCeiling()
        {
            Amount x1 = new Amount("$1234.56");
            Amount x2 = new Amount("$-1234.56");

            Assert.Equal(new Amount("$1235"), x1.Ceilinged());
            Assert.Equal(new Amount("$-1234"), x2.Ceilinged());

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestReduction()
        {
            // [DM] The test requires initialized amounts and parse conversions
            Session.SetSessionContext(new Session());

            Amount x0 = new Amount();
            Amount x1 = new Amount("60s");
            Amount x2 = new Amount("600s");
            Amount x3 = new Amount("6000s");
            Amount x4 = new Amount("360000s");
            Amount x5 = new Amount("10m");           // 600s
            Amount x6 = new Amount("100m");          // 6000s
            Amount x7 = new Amount("1000m");         // 60000s
            Amount x8 = new Amount("10000m");        // 600000s
            Amount x9 = new Amount("10h");           // 36000s
            Amount x10 = new Amount("100h");         // 360000s
            Amount x11 = new Amount("1000h");        // 3600000s
            Amount x12 = new Amount("10000h");       // 36000000s

            Assert.Throws<AmountError>(() => x0.Reduced());
            Assert.Throws<AmountError>(() => x0.Unreduced());
            Assert.Equal(x2, x5.Reduced());
            Assert.Equal(x3, x6.Reduced());
            Assert.Equal(x10, x4.Reduced());
            Assert.Equal("100.00h", x4.Unreduced().ToString());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestSign()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount("0.0000000000000000000000000000000000001");
            Amount x2 = new Amount("-0.0000000000000000000000000000000000001");
            Amount x3 = new Amount("1");
            Amount x4 = new Amount("-1");

            Assert.Throws<AmountError>(() => x0.Sign);
            Assert.True(x1.Sign > 0);
            Assert.True(x2.Sign < 0);
            Assert.True(x3.Sign > 0);
            Assert.True(x4.Sign < 0);

            Assert.True(x0.Valid());
            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x4.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommoditySign()
        {
            Amount x1 = Amount.Exact("$0.0000000000000000000000000000000000001");
            Amount x2 = Amount.Exact("$-0.0000000000000000000000000000000000001");
            Amount x3 = new Amount("$1");
            Amount x4 = new Amount("$-1");

            Assert.True(x1.Sign != 0);
            Assert.True(x2.Sign != 0);
            Assert.True(x3.Sign > 0);
            Assert.True(x4.Sign < 0);

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
            Assert.True(x3.Valid());
            Assert.True(x4.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestTruth()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount("1234");
            Amount x2 = new Amount("1234.56");

            Assert.Throws<AmountError>(() => (bool)x0);

            Assert.True((bool)x1);
            Assert.True((bool)x2);

            Assert.True(x0.Valid());
            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityTruth()
        {
            Amount x1 = new Amount("$1234");
            Amount x2 = new Amount("$1234.56");

            if ((bool)x1)
                Assert.True(true);    // [DM] I am not sure whether it makes any sense but the original test had exactly the same code

            if ((bool)x2)
                Assert.True(true);

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestForZero()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount("0.000000000000000000001");

            Assert.True((bool)x1);
            Assert.Throws<AmountError>(() => x0.IsZero);
            Assert.Throws<AmountError>(() => x0.IsRealZero);
            Assert.True(!x1.IsZero);
            Assert.True(!x1.IsRealZero);

            Assert.True(x0.Valid());
            Assert.True(x1.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityForZero()
        {
            Amount x1 = Amount.Exact("$0.000000000000000000001");

            Assert.True((bool)x1);               // an internal amount never betrays its precision
            Assert.True(!x1.IsZero);
            Assert.True(!x1.IsRealZero);

            Assert.True(x1.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestIntegerConversion()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount(123456L);
            Amount x2 = new Amount("12345682348723487324");

            Assert.Throws<AmountError>(() => x0.ToLong());
            Assert.Throws<AmountError>(() => x0.ToDouble());
            Assert.True(!x2.FitsInLong);
            Assert.Equal(123456L, x1.ToLong());
            Assert.Equal(123456.0, x1.ToDouble());
            Assert.Equal("123456", x1.ToString());
            Assert.Equal("123456", x1.QuantityString());

            Assert.True(x1.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestFractionalConversion()
        {
            Amount x1 = new Amount("1234.56");
            Amount x2 = new Amount("1234.5683787634678348734");

            Assert.Equal(1235L, x1.ToLong());
            Assert.Equal(1234.56, x1.ToDouble());
            Assert.Equal("1234.56", x1.ToString());
            Assert.Equal("1234.56", x1.QuantityString());

            Assert.True(x1.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityConversion()
        {
            Amount x1 = new Amount("$1234.56");

            Assert.Equal(1235L, x1.ToLong());
            Assert.Equal(1234.56, x1.ToDouble());
            Assert.Equal("$1234.56", x1.ToString());
            Assert.Equal("1234.56", x1.QuantityString());

            Assert.True(x1.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestPrinting()
        {
            Amount x0 = new Amount();
            Amount x1 = new Amount("982340823.380238098235098235098235098");

            Assert.Equal("<null>", x0.Print());
            Assert.Equal("982340823.380238098235098235098235098", x1.Print());

            Assert.True(x0.Valid());
            Assert.True(x1.Valid());

        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Amount_TestCommodityPrinting()
        {
            Amount x1 = Amount.Exact("$982340823.386238098235098235098235098");
            Amount x2 = new Amount("$982340823.38");

            Assert.Equal("$982340823.386238098235098235098235098", x1.Print());
            Assert.Equal("$964993493285024293.18099172508158508135413499124", (x1 * x2).Print());
            Assert.Equal("$964993493285024293.18", (x2 * x1).Print());

            Assert.True(x1.Valid());
            Assert.True(x2.Valid());
        }

    }
}

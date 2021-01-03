// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
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
    /// Ported from t_balance.cc
    /// </summary>
    public class TestBalance : IDisposable
    {
        public TestBalance()
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
            Amount.Initialize();

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
        public void AutoTestCase_Balance_TestConstructors()
        {
            Balance b0 = new Balance();
            Balance b1 = new Balance(1.00);
            Balance b2 = new Balance(123456UL);
            Balance b3 = new Balance(12345L);
            Balance b4 = new Balance("EUR 123");
            Balance b5 = new Balance("$ 456");
            Balance b6 = new Balance();
            Balance b7 = new Balance(new Amount("$ 1.00"));
            Balance b8 = new Balance(b7);

            Assert.Equal(new Balance(), b0);
            Assert.NotEqual(new Balance("0"), b0);
            Assert.NotEqual(new Balance("0.0"), b0);
            Assert.Equal(b2, (Balance)(long)123456UL);
            Assert.Equal(b3, (Balance)12345L);
            Assert.Equal(b4, (Balance)"EUR 123");
            Assert.Equal(b5, (Balance)"$ 456");
            Assert.Equal(b7, b8);
            Assert.Equal(b8, (Balance)(new Amount("$ 1.00")));

            b5 = (Balance)"euro 2345";
            b6 = (Balance)"DM -34532";
            b7 = (Balance)(new Amount("$ 1.00"));

            b8 = b5;
            Assert.Equal(b5, b8);

            Assert.True(b0.Valid());
            Assert.True(b1.Valid());
            Assert.True(b2.Valid());
            Assert.True(b3.Valid());
            Assert.True(b4.Valid());
            Assert.True(b5.Valid());
            Assert.True(b6.Valid());
            Assert.True(b7.Valid());
            Assert.True(b8.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Balance_TestAddition()
        {
            Amount a0 = new Amount();
            Amount a1 = new Amount("$1");
            Amount a2 = new Amount("2 EUR");
            Amount a3 = new Amount("0.00 CAD");
            Amount a4 = new Amount("$2");

            Balance b0 = new Balance();
            Balance b1 = new Balance(1.00);
            Balance b2 = new Balance(2UL);
            Balance b3 = new Balance(2L);
            Balance b4 = new Balance();
            Balance b5 = new Balance();

            b0 += b1;
            b2 += b3;
            b3 += a1;
            b3 += a2;
            b4 += b3;
            b5 += a1;
            b5 += a4;

            Assert.Equal(new Balance(1.00), b0);
            Assert.Equal(b3 += a3, b4);
            Assert.Equal(new Balance(4L), b2);
            Assert.Equal(new Balance() + new Amount("$3"), b5);

            Assert.Throws<BalanceError>(() => b3 += a0);

            Assert.True(b0.Valid());
            Assert.True(b1.Valid());
            Assert.True(b2.Valid());
            Assert.True(b3.Valid());
            Assert.True(b4.Valid());
            Assert.True(b5.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Balance_TestSubtraction()
        {
            Amount a0 = new Amount();
            Amount a1 = new Amount("$1");
            Amount a2 = new Amount("2 EUR");
            Amount a3 = new Amount("0.00 CAD");
            Amount a4 = new Amount("$2");

            Balance b0 = new Balance();
            Balance b1 = new Balance(1.00);
            Balance b2 = new Balance(2UL);
            Balance b3 = new Balance(2L);
            Balance b4 = new Balance();
            Balance b5 = new Balance();

            b0 -= b1;
            b2 -= b3;
            b3 -= a1;
            b3 -= a2;
            b4 = b3;
            b5 -= a1;
            b5 -= a4;

            Assert.Equal(new Balance(-1.00), b0);
            Assert.Equal(b3 -= a3, b4);
            Assert.Equal(new Balance(), b2);
            Assert.Equal(b3 -= b2, b3);
            Assert.Equal(new Balance() - new Amount("$3"), b5);

            Assert.Throws<BalanceError>(() => b3 -= a0);

            Assert.True(b0.Valid());
            Assert.True(b1.Valid());
            Assert.True(b2.Valid());
            Assert.True(b3.Valid());
            Assert.True(b4.Valid());
            Assert.True(b5.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Balance_TestEqaulity()
        {
            Amount a0 = new Amount();
            Amount a1 = new Amount("$1");
            Amount a2 = new Amount("2 EUR");
            Amount a3 = new Amount("0.00 CAD");

            Balance b0 = new Balance();
            Balance b1 = new Balance(1.00);
            Balance b2 = new Balance(2UL);
            Balance b3 = new Balance(2L);
            Balance b4 = new Balance("EUR 2");
            Balance b5 = new Balance("$-1");
            Balance b6 = new Balance("0.00");
            Balance b7 = new Balance("0.00");

            Assert.True(b2 == b3);
            Assert.True(b4 == a2);
            Assert.True(b1 == (Amount)"1.00");
            Assert.True(b5 == new Amount("-$1"));
            Assert.True(!(b6 == (Amount)"0"));
            Assert.True(!(b6 == a3));
            Assert.True(!(b6 == (Amount)"0.00"));
            Assert.True(b6 == b7);

            b4 += b5;
            b5 += a2;

            Assert.True(b4 == b5);

            Assert.Throws<BalanceError>(() => b0 == (Balance)a0);

            Assert.True(b0.Valid());
            Assert.True(b1.Valid());
            Assert.True(b2.Valid());
            Assert.True(b3.Valid());
            Assert.True(b4.Valid());
            Assert.True(b5.Valid());
            Assert.True(b6.Valid());
            Assert.True(b7.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Balance_TestMultiplication()
        {
            Amount a0 = new Amount();
            Amount a1 = new Amount("0.00");

            Balance b0 = new Balance();
            Balance b1 = new Balance(1.00);
            Balance b2 = new Balance(2UL);
            Balance b3 = new Balance("CAD -3");
            Balance b4 = new Balance("EUR 4.99999");
            Balance b5 = new Balance("$1");
            Balance b6 = new Balance();

            Assert.Equal(b1 *= (Amount)2.00, (Balance)new Amount(2.00));
            Assert.Equal(b2 *= (Amount)2L, (Balance)new Amount(4L));
            Assert.Equal(b2 *= (Amount)2UL, (Balance)new Amount(8UL));
            Assert.Equal(b3 *= new Amount("-8 CAD"), (Balance)new Amount("CAD 24"));
            Assert.Equal(b0 *= (Amount)2UL, b0);
            Assert.Equal(b0 *= a1, (Balance)a1);

            b6 += b3;
            b3 += b4;
            b3 += b5;
            b3 *= (Amount)2L;
            b6 *= (Amount)2L;
            b4 *= (Amount)2L;
            b5 *= (Amount)2L;
            b6 += b4;
            b6 += b5;

            Assert.Equal(b3, b6);

            Assert.Throws<BalanceError>(() => b1 *= a0);
            Assert.Throws<BalanceError>(() => b4 *= new Amount("1 CAD"));
            Assert.Throws<BalanceError>(() => b3 *= new Amount("1 CAD"));

            Assert.True(b0.Valid());
            Assert.True(b1.Valid());
            Assert.True(b2.Valid());
            Assert.True(b3.Valid());
            Assert.True(b4.Valid());
            Assert.True(b5.Valid());
            Assert.True(b6.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Balance_TestDivision()
        {
            Amount a0 = new Amount();
            Amount a1 = new Amount("0.00");

            Balance b0 = new Balance();
            Balance b1 = new Balance(4.00);
            Balance b2 = new Balance(4UL);
            Balance b3 = new Balance("CAD -24");
            Balance b4 = new Balance("EUR 4");
            Balance b5 = new Balance("$2");
            Balance b6 = new Balance();

            Assert.Equal(b1 /= (Amount)2.00, (Balance)new Amount(2.00));
            Assert.Equal(b2 /= (Amount)2L, (Balance)new Amount(2L));
            Assert.Equal(b2 /= (Amount)2UL, (Balance)new Amount(1UL));
            Assert.Equal(b3 /= new Amount("-3 CAD"), (Balance)new Amount("CAD 8"));
            Assert.Equal(b0 /= (Amount)2UL, b0);

            b6 += b3;
            b3 += b4;
            b3 += b5;
            b3 /= (Amount)2L;
            b6 /= (Amount)2L;
            b4 /= (Amount)2L;
            b5 /= (Amount)2L;
            b6 += b4;
            b6 += b5;

            Assert.Equal(b3, b6);

            Assert.Throws<BalanceError>(() => b1 /= a0);
            Assert.Throws<BalanceError>(() => b1 /= a1);
            Assert.Throws<BalanceError>(() => b4 /= new Amount("1 CAD"));
            Assert.Throws<BalanceError>(() => b3 /= new Amount("1 CAD"));

            Assert.True(b0.Valid());
            Assert.True(b1.Valid());
            Assert.True(b2.Valid());
            Assert.True(b3.Valid());
            Assert.True(b4.Valid());
            Assert.True(b5.Valid());
            Assert.True(b6.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Balance_TestNegation()
        {
            Amount a1 = new Amount("0.00");
            Amount a2 = new Amount("$ 123");
            Amount a3 = new Amount("EUR 456");

            Balance b0 = new Balance();
            Balance b1 = new Balance();
            Balance b2 = new Balance();
            Balance b3 = new Balance();

            b1 += a1;
            b1 += a2;
            b1 += a3;
            b2 += -a1;
            b2 += -a2;
            b2 += -a3;
            b3 = -b1;

            Assert.Equal(b0.Negated(), b0);
            Assert.Equal(b2, b3);
            Assert.Equal(b2, -b1);
            Assert.Equal(b2.Negated(), b1);

            b2.InPlaceNegate();

            Assert.Equal(b2, b1);
            Assert.Equal(b1, -b3);

            Assert.True(b0.Valid());
            Assert.True(b1.Valid());
            Assert.True(b2.Valid());
            Assert.True(b3.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Balance_TestAbs()
        {
            Amount a1 = new Amount("0.00");
            Amount a2 = new Amount("$ 123");
            Amount a3 = new Amount("EUR 456");

            Balance b0 = new Balance();
            Balance b1 = new Balance();
            Balance b2 = new Balance();

            b1 += a1;
            b1 += a2;
            b1 += a3;
            b2 += -a1;
            b2 += -a2;
            b2 += -a3;

            Assert.Equal(b0.Abs(), b0);
            Assert.Equal(b2.Abs(), b1.Abs());

            Assert.True(b0.Valid());
            Assert.True(b1.Valid());
            Assert.True(b2.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Balance_TestCeiling()
        {
            Amount a1 = new Amount("0.00");
            Amount a2 = new Amount("$ 123.123");
            Amount a3 = new Amount("EUR 456.56");
            Amount a4 = new Amount(-a1);
            Amount a5 = new Amount(-a2);
            Amount a6 = new Amount(-a3);

            Balance b0 = new Balance();
            Balance b1 = new Balance();
            Balance b2 = new Balance();
            Balance b3 = new Balance();
            Balance b4 = new Balance();

            b1 += a1;
            b1 += a2;
            b1 += a3;
            b2 += -a1;
            b2 += -a2;
            b2 += -a3;

            b3 += a1.Ceilinged();
            b3 += a2.Ceilinged();
            b3 += a3.Ceilinged();
            b4 += a4.Ceilinged();
            b4 += a5.Ceilinged();
            b4 += a6.Ceilinged();

            Assert.Equal(b0.Ceilinged(), b0);
            Assert.Equal(b2.Ceilinged(), b4);
            Assert.Equal(b1.Ceilinged(), b3);

            b1.InPlaceCeiling();
            Assert.Equal(b1, b3);

            Assert.True(b0.Valid());
            Assert.True(b1.Valid());
            Assert.True(b2.Valid());
            Assert.True(b3.Valid());
            Assert.True(b4.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Balance_TestFloor()
        {
            Amount a1 = new Amount("0.00");
            Amount a2 = new Amount("$ 123.123");
            Amount a3 = new Amount("EUR 456.56");
            Amount a4 = new Amount(-a1);
            Amount a5 = new Amount(-a2);
            Amount a6 = new Amount(-a3);

            Balance b0 = new Balance();
            Balance b1 = new Balance();
            Balance b2 = new Balance();
            Balance b3 = new Balance();
            Balance b4 = new Balance();

            b1 += a1;
            b1 += a2;
            b1 += a3;
            b2 += -a1;
            b2 += -a2;
            b2 += -a3;

            b3 += a1.Floored();
            b3 += a2.Floored();
            b3 += a3.Floored();
            b4 += a4.Floored();
            b4 += a5.Floored();
            b4 += a6.Floored();

            Assert.Equal(b0.Floored(), b0);
            Assert.Equal(b2.Floored(), b4);
            Assert.Equal(b1.Floored(), b3);

            b1.InPlaceFloor();
            Assert.Equal(b1, b3);

            Assert.True(b0.Valid());
            Assert.True(b1.Valid());
            Assert.True(b2.Valid());
            Assert.True(b3.Valid());
            Assert.True(b4.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Balance_TestRound()
        {
            Amount a1 = new Amount("0.00");
            Amount a2 = new Amount("$ 123.123");
            Amount a3 = new Amount("EUR 456.567");
            Amount a4 = new Amount("0.00");
            Amount a5 = new Amount("$ 123.12");
            Amount a6 = new Amount("EUR 456.57");

            Balance b0 = new Balance();
            Balance b1 = new Balance();
            Balance b2 = new Balance();
            Balance b3 = new Balance();
            Balance b4 = new Balance();

            b1 += a1;
            b1 += a2;
            b1 += a3;
            b2 += a4;
            b2 += a5;
            b2 += a6;

            a1.InPlaceRoundTo(2);
            a2.InPlaceRoundTo(2);
            a3.InPlaceRoundTo(2);

            // [DM] The code above does not have the same effect in .Net because of another managing of BigInt instances.
            // In opposite to the original code, quantities in NLedger are struct values and they are not shared 
            // among amounts. Therefore, xx.InPlaceRoundTo() does not have effect on the balance instance; 
            // the balance still keeps the original quantity but not rounded one. 
            // It was decided not to rework BigInt implementation in NLedger because it makes the application 
            // performance worse with none functional benefits (the only place that is essential to it is this code).
            // Instead, a corrective code was added below to simulate the original behavior.
            foreach (var amount in b1.Amounts)
                amount.Value.InPlaceRoundTo(2);

            a4.InPlaceRoundTo(2);
            a5.InPlaceRoundTo(2);
            a6.InPlaceRoundTo(2);

            b3 += a1;
            b3 += a2;
            b3 += a3;
            b4 += a4;
            b4 += a5;
            b4 += a6;

            Assert.Equal(b0.Rounded(), b0);
            Assert.Equal(b2.Rounded(), b4);
            Assert.Equal(b1.Rounded(), b4);

            b1.InPlaceRound();
            Assert.Equal(b1, b3);

            Assert.True(b0.Valid());
            Assert.True(b1.Valid());
            Assert.True(b2.Valid());
            Assert.True(b3.Valid());
            Assert.True(b4.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Balance_TestTruth()
        {
            Amount a1 = new Amount("0.00");
            Amount a2 = new Amount("$ 123");
            Amount a3 = new Amount("EUR 456");

            Balance b0 = new Balance();
            Balance b1 = new Balance();

            b1 += a1;
            b1 += a2;
            b1 += a3;

            Assert.True(!(bool)b0);
            Assert.True((bool)b1);

            Assert.True(b0.Valid());
            Assert.True(b1.Valid());
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Balance_TestForZero()
        {
            Amount a1 = new Amount("0.00");
            Amount a2 = new Amount("$ 123");
            Amount a3 = new Amount("EUR 456");

            Balance b0 = new Balance();
            Balance b1 = new Balance();

            b1 += a1;
            b1 += a2;
            b1 += a3;

            Assert.True(b0.IsEmpty);
            Assert.True(b0.IsZero);
            Assert.True(b0.IsRealZero);
            Assert.True(!b0.IsNonZero);
            Assert.True(!b1.IsEmpty);
            Assert.True(!b1.IsZero);
            Assert.True(!b1.IsRealZero);
            Assert.True(b1.IsNonZero);

            Assert.True(b0.Valid());
            Assert.True(b1.Valid());
        }
    }
}

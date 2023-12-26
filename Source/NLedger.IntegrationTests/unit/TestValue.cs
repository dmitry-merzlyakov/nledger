// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.IntegrationTests.unit
{
    /// <summary>
    /// Ported from t_value.cc
    /// </summary>
    public class TestValue : IDisposable
    {
        public TestValue()
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
        public void AutoTestCase_Value_TestConstructors()
        {
            var s1 = new List<Value>();
            Value v1 = new Value();
            Value v2 = new Value(true);
            Value v3 = new Value(new DateTime());
            Value v4 = new Value(TimesCommon.Current.ParseDate("2014/08/14"));
            Value v5 = new Value(2L);
            Value v6 = new Value((long)4UL);
            Value v7 = new Value(1.00);
            Value v8 = new Value(new Amount("4 GBP"));
            Value v9 = new Value(new Balance("3 EUR"));
            Value v10 = new Value(new Mask("regex"));
            Value v11 = new Value(s1);
            Value v12 = new Value("$1");
            Value v13 = new Value("2 CAD");
            Value v14 = new Value("comment", true);
            Value v15 = new Value("tag", true);

            Assert.True(v1.IsValid);
            Assert.True(v2.IsValid);
            Assert.True(v3.IsValid);
            Assert.True(v4.IsValid);
            Assert.True(v5.IsValid);
            Assert.True(v6.IsValid);
            Assert.True(v7.IsValid);
            Assert.True(v8.IsValid);
            Assert.True(v9.IsValid);
            Assert.True(v10.IsValid);
            Assert.True(v11.IsValid);
            Assert.True(v12.IsValid);
            Assert.True(v13.IsValid);
            Assert.True(v14.IsValid);
            Assert.True(v15.IsValid);
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Value_TestAssignment()
        {
            var s1 = new List<Value>();
            Value v1 = new Value();
            Value v2 = (Value)true;
            Value v3 = (Value)new DateTime();
            Value v4 = (Value)TimesCommon.Current.ParseDate("2014/08/14");
            Value v5 = (Value)(-2L);
            Value v6 = (Value)(long)4UL;
            Value v7 = (Value)1.00;
            Value v8 = (Value)new Amount("4 GBP");
            Value v9 = (Value)new Balance("3 EUR");
            Value v10 = (Value)new Mask("regex");
            Value v11 = (Value)s1;
            Value v12 = new Value("$1");
            Value v13 = new Value("2 CAD");
            Value v14 = new Value("comment", true);
            Value v15 = new Value("tag", true);

            Assert.True(v1.IsValid);
            Assert.True(v2.IsValid);
            Assert.True(v3.IsValid);
            Assert.True(v4.IsValid);
            Assert.True(v5.IsValid);
            Assert.True(v6.IsValid);
            Assert.True(v7.IsValid);
            Assert.True(v8.IsValid);
            Assert.True(v9.IsValid);
            Assert.True(v10.IsValid);
            Assert.True(v11.IsValid);
            Assert.True(v12.IsValid);
            Assert.True(v13.IsValid);
            Assert.True(v14.IsValid);
            Assert.True(v15.IsValid);
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Value_TestEquality()
        {
            DateTime localtime = new DateTime(2010, 2, 10);
            var s1 = new List<Value>();

            Value v1 = new Value();
            Value v2 = new Value(true);
            Value v3 = new Value(localtime);
            Value v4 = new Value(TimesCommon.Current.ParseDate("2014/08/14"));
            Value v5 = new Value(2L);
            Value v6 = new Value((long)2UL);
            Value v7 = new Value(1.00);
            Value v8 = new Value(new Amount("4 GBP"));
            Value v9 = new Value(new Balance("4 GBP"));
            Value v10 = new Value(new Mask("regex"));
            Value v11 = new Value(s1);
            Value v12 = new Value("$1");
            Value v13 = new Value("2 CAD");
            Value v14 = new Value("comment", true);
            Value v15 = new Value("comment", true);

            Assert.Equal(v1, new Value());
            Assert.Equal(v2, new Value(true));
            Assert.Equal(v3, new Value(localtime));
            Assert.True(!(v4 == new Value(TimesCommon.Current.ParseDate("2014/08/15"))));

            Value v19 = new Value(new Amount("2"));
            Value v20 = new Value(new Balance("2"));
            Assert.Equal(v5, v6);
            Assert.Equal(v5, v19);
            Assert.Equal(v5, v20);
            Assert.True(v19 == v5);
            Assert.True(v19 == v20);
            Assert.True(v19 == new Value(new Amount("2")));
            Assert.True(v20 == v5);
            Assert.True(v20 == v19);
            Assert.True(v20 == new Value(new Balance(2L)));
            Assert.True(v14 == v15);
            Assert.True(v10 == new Value(new Mask("regex")));
            Assert.True(v11 == new Value(s1));

            Assert.Throws<ValueError>(() => v8 == v10);

            Assert.True(v1.IsValid);
            Assert.True(v2.IsValid);
            Assert.True(v3.IsValid);
            Assert.True(v4.IsValid);
            Assert.True(v5.IsValid);
            Assert.True(v6.IsValid);
            Assert.True(v7.IsValid);
            Assert.True(v8.IsValid);
            Assert.True(v9.IsValid);
            Assert.True(v10.IsValid);
            Assert.True(v11.IsValid);
            Assert.True(v12.IsValid);
            Assert.True(v13.IsValid);
            Assert.True(v14.IsValid);
            Assert.True(v15.IsValid);
            Assert.True(v19.IsValid);
            Assert.True(v20.IsValid);
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Value_TestSequence()
        {
            var s1 = new List<Value>();
            Value v1 = new Value(s1);
            Assert.True(v1.Type == ValueTypeEnum.Sequence);
            v1.PushBack(new Value(2L));
            v1.PushBack(new Value("3 GBP"));

            Value v2 = new Value("3 GBP");
            Value seq = new Value(v1);
            Value v3 = new Value(seq);

            Assert.True(seq.AsSequence.Contains(v2));
            Assert.True(v3.AsSequence.Contains(v2));

            Assert.True(v2 == seq[1]);
            Assert.True(v2 == v3[1]);
            v1.PopBack();
            v1.PopBack();
            v1.PushFront(v2);
            v1.PushFront(new Value(2L));
            Assert.True(v2 == v1[1]);
            // Assert.True(seq == v1); [DM] Not applicable in .Net

            Assert.True(v1.IsValid);
            Assert.True(v2.IsValid);
            Assert.True(v3.IsValid);
            Assert.True(seq.IsValid);
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Value_TestAddition()
        {
            DateTime localtime = new DateTime(2010, 2, 10);
            var s1 = new List<Value>();

            Value v1 = new Value();
            Value v2 = new Value(true);
            Value v3 = new Value(localtime);
            Value v4 = new Value(TimesCommon.Current.ParseDate("2014/08/14"));
            Value v5 = new Value(2L);
            Value v6 = new Value((long)2UL);
            Value v7 = new Value(1.00);
            Value v8 = new Value(new Amount("4 GBP"));
            Value v9 = new Value(new Balance("4 GBP"));
            Value v10 = new Value(new Mask("regex"));
            Value v11 = new Value(s1);
            Value v12 = new Value("$1");
            Value v13 = new Value("2 CAD");
            Value v14 = new Value("comment", true);
            Value v15 = new Value("comment", true);
            Value v16 = new Value(new Amount("2"));

            v14 += v15;
            Assert.Equal(v14, new Value("commentcomment", true));
            v14 += v12;
            Assert.Equal(v14, new Value("commentcomment$1.00", true));

            Assert.Equal(v3, new Value(new DateTime(2010, 2, 10, 0, 0, 0)));
            v3 += new Value(2L);
            Assert.Equal(v3, new Value(new DateTime(2010, 2, 10, 0, 0, 2)));
            v3 += new Value(new Amount("2"));
            Assert.Equal(v3, new Value(new DateTime(2010, 2, 10, 0, 0, 4)));

            v4 += new Value(2L);
            Assert.Equal(v4, new Value(TimesCommon.Current.ParseDate("2014/08/16")));
            v4 += new Value(new Amount("2"));
            Assert.Equal(v4, new Value(TimesCommon.Current.ParseDate("2014/08/18")));

            v5 += new Value(2L);
            Assert.Equal(v5, new Value(4L));
            v5 += new Value(new Amount("2"));
            Assert.Equal(v5, new Value(new Amount("6")));
            v5 += v8;

            v16 += new Value(2L);
            v16 += new Value(new Amount("2"));
            v16 += v8;
            Assert.Equal(v5, v16);

            v8 += new Value("6");
            Assert.Equal(v8, v16);

            Value v17 = new Value(6L);
            v17 += new Value(new Amount("4 GBP"));
            Assert.Equal(v8, v17);

            Value v18 = new Value(6L);
            v18 += v9;
            Value v19 = new Value(new Amount("6"));
            v19 += v9;
            Assert.Equal(v18, v19);

            v9 += new Value(2L);
            v9 += new Value(new Amount("4"));
            v9 += v19;
            v18 += v19;
            Assert.Equal(v9, v18);

            Value v20 = new Value(s1);
            v11 += new Value(2L);
            v11 += new Value("4 GBP");
            Assert.Throws<ValueError>(() => v11 += v20);
            Assert.Throws<ValueError>(() => v10 += v8);

            v20 += new Value(2L);
            v20 += new Value("4 GBP");
            Assert.Equal(v11, v20);
            v11 += v20;
            v20 += v20;
            Assert.Equal(v11, v20);

            Assert.True(v1.IsValid);
            Assert.True(v2.IsValid);
            Assert.True(v3.IsValid);
            Assert.True(v4.IsValid);
            Assert.True(v5.IsValid);
            Assert.True(v6.IsValid);
            Assert.True(v7.IsValid);
            Assert.True(v8.IsValid);
            Assert.True(v9.IsValid);
            Assert.True(v10.IsValid);
            Assert.True(v11.IsValid);
            Assert.True(v12.IsValid);
            Assert.True(v13.IsValid);
            Assert.True(v14.IsValid);
            Assert.True(v15.IsValid);
            Assert.True(v16.IsValid);
            Assert.True(v17.IsValid);
            Assert.True(v18.IsValid);
            Assert.True(v19.IsValid);
            Assert.True(v20.IsValid);
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Value_TestSubtraction()
        {
            DateTime localtime = new DateTime(2010, 2, 10, 0, 0, 4);
            var s1 = new List<Value>();

            Value v1 = new Value();
            Value v2 = new Value(true);
            Value v3 = new Value(localtime);
            Value v4 = new Value(TimesCommon.Current.ParseDate("2014/08/18"));
            Value v5 = new Value(6L);
            Value v6 = new Value((long)6UL);
            Value v7 = new Value(1.00);
            Value v8 = new Value(new Amount("4 GBP"));
            Value v9 = new Value(new Balance("4 GBP"));
            Value v10 = new Value(new Mask("regex"));
            Value v11 = new Value(s1);
            Value v12 = new Value("$1");
            Value v13 = new Value("2 CAD");
            Value v14 = new Value("comment", true);
            Value v15 = new Value("comment", true);
            Value v16 = new Value(new Amount("6"));

            v3 -= new Value(2L);
            Assert.Equal(v3, new Value(new DateTime(2010, 2, 10, 0, 0, 2)));
            v3 -= new Value(new Amount("2"));
            Assert.Equal(v3, new Value(new DateTime(2010, 2, 10, 0, 0, 0)));

            v4 -= new Value(2L);
            Assert.Equal(v4, new Value(TimesCommon.Current.ParseDate("2014/08/16")));
            v4 -= new Value(new Amount("2"));
            Assert.Equal(v4, new Value(TimesCommon.Current.ParseDate("2014/08/14")));

            v5 -= new Value(2L);
            Assert.Equal(v5, new Value(4L));
            v5 -= new Value(new Amount("2"));
            Assert.Equal(v5, new Value(new Amount("2")));
            v5 -= v8;

            v16 -= new Value(2L);
            v16 -= new Value(new Amount("2"));
            v16 -= v8;
            Assert.Equal(v5, v16);

            v8 -= new Value("2");
            Assert.Equal(-v8, v16);

            Value v18 = new Value(6L);
            v18 -= v9;
            Value v19 = new Value(new Amount("6"));
            v19 -= v9;
            Assert.Equal(v18, v19);

            v9 -= new Value(-2L);
            v9 -= new Value(new Amount("-10"));
            v9 -= new Value(new Amount("12 GBP"));
            v9 -= v19;
            Assert.Equal(v9, v18);
            v18 -= v19;
            Assert.Equal(v18, new Value("0"));

            Value v20 = new Value(s1);
            Value v21 = new Value(2L);
            Value v22 = new Value("4 GBP");
            v11.PushBack(v21);
            v11.PushBack(v22);
            Assert.Throws<ValueError>(() => v11 -= v20);
            Assert.Throws<ValueError>(() => v10 -= v8);

            v20.PushBack(v21);
            v20.PushBack(v22);
            v11 -= v20;
            Value v23 = new Value(s1);
            v23.PushBack(new Value(0L));
            v23.PushBack(new Value("0"));
            Assert.Equal(v11, v23);
            v20 -= v21;
            v20 -= v22;
            Assert.Equal(v20, new Value(s1));

            Assert.True(v1.IsValid);
            Assert.True(v2.IsValid);
            Assert.True(v3.IsValid);
            Assert.True(v4.IsValid);
            Assert.True(v5.IsValid);
            Assert.True(v6.IsValid);
            Assert.True(v7.IsValid);
            Assert.True(v8.IsValid);
            Assert.True(v9.IsValid);
            Assert.True(v10.IsValid);
            Assert.True(v11.IsValid);
            Assert.True(v12.IsValid);
            Assert.True(v13.IsValid);
            Assert.True(v14.IsValid);
            Assert.True(v15.IsValid);
            Assert.True(v16.IsValid);
            Assert.True(v18.IsValid);
            Assert.True(v19.IsValid);
            Assert.True(v20.IsValid);
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Value_TestMultiplication()
        {
            DateTime localtime = new DateTime(2010, 2, 10, 0, 0, 4);
            var s1 = new List<Value>();

            Value v1 = new Value();
            Value v2 = new Value(true);
            Value v3 = new Value(localtime);
            Value v4 = new Value(TimesCommon.Current.ParseDate("2014/08/14"));
            Value v5 = new Value(2L);
            Value v6 = new Value((long)2UL);
            Value v7 = new Value(1.00);
            Value v8 = new Value(new Amount("4 GBP"));
            Value v9 = new Value(new Balance("4 GBP"));
            Value v10 = new Value(new Mask("regex"));
            Value v11 = new Value(s1);
            Value v12 = new Value("$1");
            Value v13 = new Value("2 CAD");
            Value v14 = new Value("comment", true);
            Value v15 = new Value("comment", true);
            Value v16 = new Value(new Amount("2"));

            v14 *= new Value(2L);
            Assert.Equal(v14, new Value("commentcomment", true));

            v5 *= new Value(2L);
            Assert.Equal(v5, new Value(4L));
            v5 *= new Value(new Amount("2"));
            Assert.Equal(v5, new Value(new Amount("8")));

            v16 *= new Value(2L);
            v16 *= new Value(new Amount("2"));
            Assert.Equal(v5, v16);

            v8 *= v9;
            Assert.Equal(v8, new Value("16 GBP"));

            Value v17 = new Value(v9);
            v9 *= new Value(2L);
            Assert.Equal(v9, new Value("8 GBP"));
            v17 += new Value(2L);
            v17 *= new Value(2L);
            Value v18 = new Value("8 GBP");
            v18 += new Value(4L);
            Assert.Equal(v17, v18);

            Value v20 = new Value(s1);
            v11.PushBack(new Value(2L));
            v11.PushBack(new Value("2 GBP"));
            v20.PushBack(new Value(4L));
            v20.PushBack(new Value("4 GBP"));
            v11 *= new Value(2L);
            Assert.Equal(v11, v20);

            Assert.Throws<ValueError>(() => v10 *= v8);
            Assert.True(v1.IsValid);
            Assert.True(v2.IsValid);
            Assert.True(v3.IsValid);
            Assert.True(v4.IsValid);
            Assert.True(v5.IsValid);
            Assert.True(v6.IsValid);
            Assert.True(v7.IsValid);
            Assert.True(v8.IsValid);
            Assert.True(v9.IsValid);
            Assert.True(v10.IsValid);
            Assert.True(v11.IsValid);
            Assert.True(v12.IsValid);
            Assert.True(v13.IsValid);
            Assert.True(v14.IsValid);
            Assert.True(v15.IsValid);
            Assert.True(v16.IsValid);
            Assert.True(v17.IsValid);
            Assert.True(v18.IsValid);
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Value_TestDivision()
        {
            DateTime localtime = new DateTime(2010, 2, 10, 0, 0, 4);
            var s1 = new List<Value>();

            Value v1 = new Value();
            Value v2 = new Value(true);
            Value v3 = new Value(localtime);
            Value v4 = new Value(TimesCommon.Current.ParseDate("2014/08/14"));
            Value v5 = new Value(8L);
            Value v6 = new Value((long)2UL);
            Value v7 = new Value(1.00);
            Value v8 = new Value(new Amount("4 GBP"));
            Value v9 = new Value(new Balance("4 GBP"));
            Value v10 = new Value(new Mask("regex"));
            Value v11 = new Value(s1);
            Value v12 = new Value("$1");
            Value v13 = new Value("2 CAD");
            Value v14 = new Value("comment", true);
            Value v15 = new Value("comment", true);
            Value v16 = new Value(new Amount("8"));

            v5 /= new Value(2L);
            Assert.Equal(v5, new Value(4L));
            v5 /= new Value(new Amount("8"));

            // [DM] The original Ledger unit test is corrected here to reflect the fix in INTEGER/AMOUNT division
            // (see comments in ValueStorage.cs; IntegerValueStorage; DivideValueStorage
            // Even from logical standpoint, 4/8=0.5, not 2
            Assert.Equal(v5, new Value(new Amount("0.5")));
            v5 = new Value(2L); // Set to 2 to pass next test steps
            // Original unit test row - Assert.Equal(v5, new Value(new Amount("2")));

            v16 /= new Value(2L);
            v16 /= new Value(new Amount("2"));
            Assert.Equal(v5, v16);

            v8 /= v9;
            v8 /= new Value(new Balance(2L));
            Assert.Equal(v8, new Value("0.5 GBP"));

            Value v17 = new Value(v9);
            v9 /= new Value(2L);
            Assert.Equal(v9, new Value("2 GBP"));
            v17 /= new Value("2 GBP");
            v17 /= new Value("2");
            Assert.Equal(v17, new Value(new Balance("1 GBP")));

            Assert.Throws<ValueError>(() => v10 /= v8);
            Assert.True(v1.IsValid);
            Assert.True(v2.IsValid);
            Assert.True(v3.IsValid);
            Assert.True(v4.IsValid);
            Assert.True(v5.IsValid);
            Assert.True(v6.IsValid);
            Assert.True(v7.IsValid);
            Assert.True(v8.IsValid);
            Assert.True(v9.IsValid);
            Assert.True(v10.IsValid);
            Assert.True(v11.IsValid);
            Assert.True(v12.IsValid);
            Assert.True(v13.IsValid);
            Assert.True(v14.IsValid);
            Assert.True(v15.IsValid);
            Assert.True(v16.IsValid);
            Assert.True(v17.IsValid);
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Value_TestType()
        {
            DateTime localtime = new DateTime(2010, 2, 10, 0, 0, 4);
            var s1 = new List<Value>();
            Value v1 = new Value();
            Value v2 = new Value(true);
            Value v3 = new Value(localtime);
            Value v4 = new Value(TimesCommon.Current.ParseDate("2014/08/14"));
            Value v5 = new Value(2L);
            Value v6 = new Value((double)4UL);
            Value v7 = new Value(1.00);
            Value v8 = new Value(new Amount("4 GBP"));
            Value v9 = new Value(new Balance("3 EUR"));
            Value v10 = new Value(new Mask("regex"));
            Value v11 = new Value(s1);
            Value v12 = new Value("$1");
            Value v13 = new Value("2 CAD");
            Value v14 = new Value("comment", true);
            Value v15 = new Value("tag", true);

            Assert.True(v1.Type == ValueTypeEnum.Void);
            Assert.True(v2.Type == ValueTypeEnum.Boolean);
            Assert.True(v3.Type == ValueTypeEnum.DateTime);
            Assert.True(v4.Type == ValueTypeEnum.Date);
            Assert.True(v5.Type == ValueTypeEnum.Integer);
            Assert.True(v6.Type == ValueTypeEnum.Amount);
            Assert.True(v7.Type == ValueTypeEnum.Amount);
            Assert.True(v8.Type == ValueTypeEnum.Amount);
            Assert.True(v9.Type == ValueTypeEnum.Balance);
            Assert.True(v10.Type == ValueTypeEnum.Mask);
            Assert.True(v11.Type == ValueTypeEnum.Sequence);
            Assert.True(v12.Type == ValueTypeEnum.Amount);
            Assert.True(v13.Type == ValueTypeEnum.Amount);
            Assert.True(v14.Type == ValueTypeEnum.String);
            Assert.True(v15.Type == ValueTypeEnum.String);

            Assert.True(v1.IsValid);
            Assert.True(v2.IsValid);
            Assert.True(v3.IsValid);
            Assert.True(v4.IsValid);
            Assert.True(v5.IsValid);
            Assert.True(v6.IsValid);
            Assert.True(v7.IsValid);
            Assert.True(v8.IsValid);
            Assert.True(v9.IsValid);
            Assert.True(v10.IsValid);
            Assert.True(v11.IsValid);
            Assert.True(v12.IsValid);
            Assert.True(v13.IsValid);
            Assert.True(v14.IsValid);
            Assert.True(v15.IsValid);
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Value_TestForZero()
        {
            var s1 = new List<Value>();
            Value v1 = new Value();
            Value v2 = new Value(true);
            Value v3 = new Value(new DateTime(1970, 1, 1)); // [DM] Here is a difference in behavior between time_t/date_t and .Net
            Value v4 = new Value(new Date(1970, 1, 1));     // Null time is presented as 1/1/1970 there that is non-zero value.
            Value v5 = new Value(2L);
            Value v6 = new Value(0UL);
            Value v7 = new Value(1.00);
            Value v8 = new Value(new Amount("4 GBP"));
            Value v9 = new Value(new Balance("0"));
            Value v10 = new Value(new Mask(""));
            Value v11 = new Value(s1);
            Value v12 = new Value("$1");
            Value v13 = new Value("2 CAD");
            Value v14 = new Value("comment", true);
            Value v15 = new Value("", true);

            Assert.True(Value.IsNullOrEmpty(v1));
            Assert.True(v2.IsNonZero);
            Assert.True(!v3.IsZero);
            Assert.True(v4.IsNonZero);
            Assert.True(v5.IsNonZero);
            Assert.True(v6.IsRealZero);
            Assert.True(v7.IsNonZero);
            Assert.True(v8.IsNonZero);
            Assert.True(v9.IsZero);
            Assert.Throws<ValueError>(() => v10.IsZero);
            Assert.True(v11.IsZero);
            Assert.True(v12.IsNonZero);
            Assert.True(v13.IsNonZero);
            Assert.True(v14.IsNonZero);
            Assert.True(v15.IsZero);

            v11.PushBack(v6);
            Assert.True(v11.IsNonZero);

            Assert.True(v1.IsValid);
            Assert.True(v2.IsValid);
            Assert.True(v3.IsValid);
            Assert.True(v4.IsValid);
            Assert.True(v5.IsValid);
            Assert.True(v6.IsValid);
            Assert.True(v7.IsValid);
            Assert.True(v8.IsValid);
            Assert.True(v9.IsValid);
            Assert.True(v10.IsValid);
            Assert.True(v11.IsValid);
            Assert.True(v12.IsValid);
            Assert.True(v13.IsValid);
            Assert.True(v14.IsValid);
            Assert.True(v15.IsValid);
        }

        [Fact]
        [Trait("Category", "BoostAutoTest")]
        public void AutoTestCase_Value_TestNegation()
        {
            var s1 = new List<Value>();
            Value v1 = new Value();
            Value v2 = new Value(true);
            Value v3 = new Value(new DateTime());
            Value v4 = new Value(TimesCommon.Current.ParseDate("2014/08/09"));
            Value v5 = new Value(2L);
            Value v6 = new Value(0UL);
            Value v7 = new Value(1.00);
            Value v8 = new Value(new Amount("4 GBP"));
            Value v9 = new Value(new Balance("4 GBP"));
            Value v10 = new Value(new Mask(""));
            Value v11 = new Value(s1);
            Value v12 = new Value("$1");
            Value v13 = new Value("$-1");
            Value v14 = new Value("comment", true);
            Value v15 = new Value("comment", true);

            Assert.Throws<ValueError>(() => v1.Negated());
            Assert.Equal(v2.Negated(), new Value(false));
            v5.InPlaceNegate();
            Assert.Equal(v5, new Value(-2L));
            v8.InPlaceNegate();
            v9.InPlaceNegate();
            Assert.Equal(v8, v9);
            Assert.Throws<ValueError>(() => v10.Negated());
            Assert.Equal(-v12, v13);
            Assert.Throws<ValueError>(() => -v14);

            Assert.True(v1.IsValid);
            Assert.True(v2.IsValid);
            Assert.True(v3.IsValid);
            Assert.True(v4.IsValid);
            Assert.True(v5.IsValid);
            Assert.True(v6.IsValid);
            Assert.True(v7.IsValid);
            Assert.True(v8.IsValid);
            Assert.True(v9.IsValid);
            Assert.True(v10.IsValid);
            Assert.True(v11.IsValid);
            Assert.True(v12.IsValid);
            Assert.True(v13.IsValid);
            Assert.True(v14.IsValid);
            Assert.True(v15.IsValid);
        }

    }
}

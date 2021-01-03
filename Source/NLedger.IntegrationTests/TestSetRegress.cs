// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Xunit;
namespace NLedger.IntegrationTests
{
    // [DeploymentItem(@"test\regress", @"test\regress")]
    public class TestSet1_test_regress
    {
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_012ADB60()
        {
            new TestRunner(@"test/regress/012ADB60.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_0161EB1E()
        {
            new TestRunner(@"test/regress/0161EB1E.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_04C5E1CA()
        {
            new TestRunner(@"test/regress/04C5E1CA.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_04D86CD0()
        {
            new TestRunner(@"test/regress/04D86CD0.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_0CA014F9()
        {
            new TestRunner(@"test/regress/0CA014F9.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_0DDDEBC0()
        {
            new TestRunner(@"test/regress/0DDDEBC0.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1036()
        {
            new TestRunner(@"test/regress/1036.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1038_1()
        {
            new TestRunner(@"test/regress/1038_1.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1038_2()
        {
            new TestRunner(@"test/regress/1038_2.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1038_3()
        {
            new TestRunner(@"test/regress/1038_3.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1046()
        {
            new TestRunner(@"test/regress/1046.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1050()
        {
            new TestRunner(@"test/regress/1050.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1055()
        {
            new TestRunner(@"test/regress/1055.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1057()
        {
            new TestRunner(@"test/regress/1057.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1072()
        {
            new TestRunner(@"test/regress/1072.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1074()
        {
            new TestRunner(@"test/regress/1074.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_10D19C11()
        {
            new TestRunner(@"test/regress/10D19C11.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1102()
        {
            new TestRunner(@"test/regress/1102.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1106()
        {
            new TestRunner(@"test/regress/1106.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1127()
        {
            new TestRunner(@"test/regress/1127.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1132()
        {
            new TestRunner(@"test/regress/1132.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1147_a()
        {
            new TestRunner(@"test/regress/1147-a.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1147_b()
        {
            new TestRunner(@"test/regress/1147-b.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1159()
        {
            new TestRunner(@"test/regress/1159.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1182_1()
        {
            new TestRunner(@"test/regress/1182_1.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1182_2()
        {
            new TestRunner(@"test/regress/1182_2.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1187_1()
        {
            new TestRunner(@"test/regress/1187_1.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1187_2()
        {
            new TestRunner(@"test/regress/1187_2.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1187_3()
        {
            new TestRunner(@"test/regress/1187_3.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1187_4()
        {
            new TestRunner(@"test/regress/1187_4.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1187_5()
        {
            new TestRunner(@"test/regress/1187_5.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1222()
        {
            new TestRunner(@"test/regress/1222.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1224()
        {
            new TestRunner(@"test/regress/1224.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1384C1D8()
        {
            new TestRunner(@"test/regress/1384C1D8.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_13965214()
        {
            new TestRunner(@"test/regress/13965214.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_14DB77E7()
        {
            new TestRunner(@"test/regress/14DB77E7.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_15230B79()
        {
            new TestRunner(@"test/regress/15230B79.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_15A80F68()
        {
            new TestRunner(@"test/regress/15A80F68.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1626()
        {
            new TestRunner(@"test/regress/1626.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1702()
        {
            new TestRunner(@"test/regress/1702.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1703()
        {
            new TestRunner(@"test/regress/1703.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1722()
        {
            new TestRunner(@"test/regress/1722.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1723()
        {
            new TestRunner(@"test/regress/1723.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1753()
        {
            new TestRunner(@"test/regress/1753.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1775()
        {
            new TestRunner(@"test/regress/1775.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_178501DC()
        {
            new TestRunner(@"test/regress/178501DC.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1894_1()
        {
            new TestRunner(@"test/regress/1894_1.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1894_2()
        {
            new TestRunner(@"test/regress/1894_2.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1895()
        {
            new TestRunner(@"test/regress/1895.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1A546C4D()
        {
            new TestRunner(@"test/regress/1A546C4D.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1CF1EEC2()
        {
            new TestRunner(@"test/regress/1CF1EEC2.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1D275740()
        {
            new TestRunner(@"test/regress/1D275740.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_1E192DF6()
        {
            new TestRunner(@"test/regress/1E192DF6.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_25A099C9()
        {
            new TestRunner(@"test/regress/25A099C9.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_2CE7DADB()
        {
            new TestRunner(@"test/regress/2CE7DADB.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_2E3496BD()
        {
            new TestRunner(@"test/regress/2E3496BD.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_370_budget_period_days()
        {
            new TestRunner(@"test/regress/370-budget_period_days.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_370_budget_period_weeks()
        {
            new TestRunner(@"test/regress/370-budget_period_weeks.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_370_forecast_period_days()
        {
            new TestRunner(@"test/regress/370-forecast_period_days.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_370_forecast_period_weeks()
        {
            new TestRunner(@"test/regress/370-forecast_period_weeks.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_370_period()
        {
            new TestRunner(@"test/regress/370-period.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_373540CC()
        {
            new TestRunner(@"test/regress/373540CC.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_375()
        {
            new TestRunner(@"test/regress/375.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_383()
        {
            new TestRunner(@"test/regress/383.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_3AAB00ED()
        {
            new TestRunner(@"test/regress/3AAB00ED.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_3AB70168()
        {
            new TestRunner(@"test/regress/3AB70168.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_3FE26304()
        {
            new TestRunner(@"test/regress/3FE26304.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_4509F714()
        {
            new TestRunner(@"test/regress/4509F714.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_461980A1()
        {
            new TestRunner(@"test/regress/461980A1.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_47C579B8()
        {
            new TestRunner(@"test/regress/47C579B8.test").Run();
        }
		
        [Fact(Skip="Requires Python integration")]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_4D9288AE_py()
        {
            new TestRunner(@"test/regress/4D9288AE_py.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_53BCED29()
        {
            new TestRunner(@"test/regress/53BCED29.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_543_a()
        {
            new TestRunner(@"test/regress/543_a.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_543_b()
        {
            new TestRunner(@"test/regress/543_b.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_543_c()
        {
            new TestRunner(@"test/regress/543_c.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_543_d()
        {
            new TestRunner(@"test/regress/543_d.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_550_584()
        {
            new TestRunner(@"test/regress/550-584.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_553()
        {
            new TestRunner(@"test/regress/553.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_55831A79()
        {
            new TestRunner(@"test/regress/55831A79.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_56BBE69B()
        {
            new TestRunner(@"test/regress/56BBE69B.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_5A03CFC3()
        {
            new TestRunner(@"test/regress/5A03CFC3.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_5D92A5EB()
        {
            new TestRunner(@"test/regress/5D92A5EB.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_5F1BAF17()
        {
            new TestRunner(@"test/regress/5F1BAF17.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_5FBF2ED8()
        {
            new TestRunner(@"test/regress/5FBF2ED8.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_605A410D()
        {
            new TestRunner(@"test/regress/605A410D.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_6188B0EC()
        {
            new TestRunner(@"test/regress/6188B0EC.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_620F0674()
        {
            new TestRunner(@"test/regress/620F0674.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_640D3205()
        {
            new TestRunner(@"test/regress/640D3205.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_647D5DB9()
        {
            new TestRunner(@"test/regress/647D5DB9.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_65FECA4D()
        {
            new TestRunner(@"test/regress/65FECA4D.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_686()
        {
            new TestRunner(@"test/regress/686.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_68917252()
        {
            new TestRunner(@"test/regress/68917252.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_6D9066DD()
        {
            new TestRunner(@"test/regress/6D9066DD.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_6DAB9FE3()
        {
            new TestRunner(@"test/regress/6DAB9FE3.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_6E041C52()
        {
            new TestRunner(@"test/regress/6E041C52.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_6E7C2DF9()
        {
            new TestRunner(@"test/regress/6E7C2DF9.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_712_a()
        {
            new TestRunner(@"test/regress/712-a.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_712_b()
        {
            new TestRunner(@"test/regress/712-b.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_713_a()
        {
            new TestRunner(@"test/regress/713-a.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_713_b()
        {
            new TestRunner(@"test/regress/713-b.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_727B2DF8()
        {
            new TestRunner(@"test/regress/727B2DF8.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_730()
        {
            new TestRunner(@"test/regress/730.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_751B2357()
        {
            new TestRunner(@"test/regress/751B2357.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_755()
        {
            new TestRunner(@"test/regress/755.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_785()
        {
            new TestRunner(@"test/regress/785.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_786A3DD0()
        {
            new TestRunner(@"test/regress/786A3DD0.test").Run();
        }
		
        [Fact(Skip="Requires Python integration")]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_78AB4B87_py()
        {
            new TestRunner(@"test/regress/78AB4B87_py.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_793F6BF0()
        {
            new TestRunner(@"test/regress/793F6BF0.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_7C44010B()
        {
            new TestRunner(@"test/regress/7C44010B.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_7F3650FD()
        {
            new TestRunner(@"test/regress/7F3650FD.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_8254755E()
        {
            new TestRunner(@"test/regress/8254755E.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_82763D86()
        {
            new TestRunner(@"test/regress/82763D86.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_83B4A0E5()
        {
            new TestRunner(@"test/regress/83B4A0E5.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_854150DF()
        {
            new TestRunner(@"test/regress/854150DF.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_86D2BDC4()
        {
            new TestRunner(@"test/regress/86D2BDC4.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_889BB167()
        {
            new TestRunner(@"test/regress/889BB167.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_89233B6D()
        {
            new TestRunner(@"test/regress/89233B6D.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_8CE88DB4()
        {
            new TestRunner(@"test/regress/8CE88DB4.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_8EAF77C0()
        {
            new TestRunner(@"test/regress/8EAF77C0.test").Run();
        }
		
        [Fact(Skip="Requires Python integration")]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_9188F587_py()
        {
            new TestRunner(@"test/regress/9188F587_py.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_95350193()
        {
            new TestRunner(@"test/regress/95350193.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_96A8E4A1()
        {
            new TestRunner(@"test/regress/96A8E4A1.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_981()
        {
            new TestRunner(@"test/regress/981.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_999_a()
        {
            new TestRunner(@"test/regress/999-a.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_999_b()
        {
            new TestRunner(@"test/regress/999-b.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_9E0E606D()
        {
            new TestRunner(@"test/regress/9E0E606D.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_9EB10714()
        {
            new TestRunner(@"test/regress/9EB10714.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_A013A73B()
        {
            new TestRunner(@"test/regress/A013A73B.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_A28CF697()
        {
            new TestRunner(@"test/regress/A28CF697.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_A3FA7601()
        {
            new TestRunner(@"test/regress/A3FA7601.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_A560FDAD()
        {
            new TestRunner(@"test/regress/A560FDAD.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_A8FCC765()
        {
            new TestRunner(@"test/regress/A8FCC765.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_AA2FF2B()
        {
            new TestRunner(@"test/regress/AA2FF2B.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_ACE05ECE()
        {
            new TestRunner(@"test/regress/ACE05ECE.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_AEDE9734()
        {
            new TestRunner(@"test/regress/AEDE9734.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_AFAFB804()
        {
            new TestRunner(@"test/regress/AFAFB804.test").Run();
        }
		
        [Fact(Skip="Requires Python integration")]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_B21BF389_py()
        {
            new TestRunner(@"test/regress/B21BF389_py.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_B68FFB0D()
        {
            new TestRunner(@"test/regress/B68FFB0D.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_BBFA1759()
        {
            new TestRunner(@"test/regress/BBFA1759.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_BF3C1F82_2()
        {
            new TestRunner(@"test/regress/BF3C1F82-2.test").Run();
        }
		
        [Fact(Skip=".Net DateTime runtime does not generate such messages as it is expected. Disabled for further decision.")]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_BF3C1F82()
        {
            new TestRunner(@"test/regress/BF3C1F82.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_BFD3FBE1()
        {
            new TestRunner(@"test/regress/BFD3FBE1.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_C0212EAC()
        {
            new TestRunner(@"test/regress/C0212EAC.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_C19E4E9B()
        {
            new TestRunner(@"test/regress/C19E4E9B.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_C523E23F()
        {
            new TestRunner(@"test/regress/C523E23F.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_C927CFFE()
        {
            new TestRunner(@"test/regress/C927CFFE.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_C9D593B3()
        {
            new TestRunner(@"test/regress/C9D593B3.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_CAE63F5C_a()
        {
            new TestRunner(@"test/regress/CAE63F5C-a.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_CAE63F5C_b()
        {
            new TestRunner(@"test/regress/CAE63F5C-b.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_CAE63F5C_c()
        {
            new TestRunner(@"test/regress/CAE63F5C-c.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_CEECC0B0()
        {
            new TestRunner(@"test/regress/CEECC0B0.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_CFE5D8AA()
        {
            new TestRunner(@"test/regress/CFE5D8AA.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_D060256A()
        {
            new TestRunner(@"test/regress/D060256A.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_D2829FC4()
        {
            new TestRunner(@"test/regress/D2829FC4.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_D51BFF74()
        {
            new TestRunner(@"test/regress/D51BFF74.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_D943AE0F()
        {
            new TestRunner(@"test/regress/D943AE0F.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_D9C8EB08()
        {
            new TestRunner(@"test/regress/D9C8EB08.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_DB490507()
        {
            new TestRunner(@"test/regress/DB490507.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_DDB54BB8()
        {
            new TestRunner(@"test/regress/DDB54BB8.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_DE17CCF1()
        {
            new TestRunner(@"test/regress/DE17CCF1.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_E2E479BC()
        {
            new TestRunner(@"test/regress/E2E479BC.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_E4C9A8EA()
        {
            new TestRunner(@"test/regress/E4C9A8EA.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_E627C594()
        {
            new TestRunner(@"test/regress/E627C594.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_E9F130C5()
        {
            new TestRunner(@"test/regress/E9F130C5.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_EA18D948()
        {
            new TestRunner(@"test/regress/EA18D948.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_error_in_include()
        {
            new TestRunner(@"test/regress/error-in-include.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_F06D5554()
        {
            new TestRunner(@"test/regress/F06D5554.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_F524E251()
        {
            new TestRunner(@"test/regress/F524E251.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_F559EC12()
        {
            new TestRunner(@"test/regress/F559EC12.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_FCE11C8D()
        {
            new TestRunner(@"test/regress/FCE11C8D.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_FDFBA165()
        {
            new TestRunner(@"test/regress/FDFBA165.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_fix_missing_trans_in_last_budget_period()
        {
            new TestRunner(@"test/regress/fix-missing-trans-in-last-budget-period.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_GH520()
        {
            new TestRunner(@"test/regress/GH520.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_market_group_by()
        {
            new TestRunner(@"test/regress/market-group-by.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_total_1()
        {
            new TestRunner(@"test/regress/total-1.test").Run();
        }
		
        [Fact]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_total_2()
        {
            new TestRunner(@"test/regress/total-2.test").Run();
        }
		
        [Fact(Skip="Requires Python integration")]
        [Trait("Category", "Integration")]
        public void IntegrationTest_test_regress_xact_code_py()
        {
            new TestRunner(@"test/regress/xact_code_py.test").Run();
        }
		

    }
}

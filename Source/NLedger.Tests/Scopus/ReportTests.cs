// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Scopus
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class ReportTests : TestFixture
    {
        [Fact]
        public void Report_Constuctor_PopulatesSession()
        {
            Session session = new Session();
            Report report = new Report(session);
            Assert.Equal(session, report.Session);
        }

        [Fact]
        public void Report_Description_IndicatesCurrentReport()
        {
            Session session = new Session();
            Report report = new Report(session);
            Assert.Equal(Report.CurrentReportDescription, report.Description);
        }

        [Fact]
        public void Report_MaybeFormat_ReturnsEmptyStringForNullOrUnhandledOptions()
        {
            Session session = new Session();
            Report report = new Report(session);

            // null option
            Assert.Equal(String.Empty, report.MaybeFormat(null));

            // unhandled option
            Option option = new Option("some-name");
            Assert.Equal(String.Empty, report.MaybeFormat(option));
            Assert.False(option.Handled);

            // handled option
            option.On("whence", "str-value");
            Assert.Equal("str-value", report.MaybeFormat(option));
            Assert.True(option.Handled);
        }

        [Fact]
        public void Report_NormalizeOptions_TurnsOnColorHandler()
        {
            // If ForceColorHandler and NoColorHandler are off
            Report report1 = new Report(new Session());
            report1.NormalizeOptions("something");
            Assert.True(report1.ColorHandler.Handled);

            // If ForceColorHandler is on
            Report report2 = new Report(new Session());
            report2.ForceColorHandler.On("test");
            report2.NormalizeOptions("something");
            Assert.True(report2.ColorHandler.Handled);

            // If NoColorHandler is on
            Report report3 = new Report(new Session());
            report3.NoColorHandler.On("test");
            report3.NormalizeOptions("something");
            Assert.False(report3.ColorHandler.Handled);
        }

        [Fact]
        public void Report_ExprOption_InitializesExpr()
        {
            ExprOption exprOption = new ExprOption("some", null);
            Assert.Equal(Expr.Empty, exprOption.Expr);
        }

        [Fact]
        public void Report_FnMarket_UsesThirdArgumentAsTargetCommodity()
        {
            var arguments = new List<Value>() { Value.Get(100), Value.Get(DateTime.Now), Value.Get("target-commodity") };
            var args = new CallScope(new Session());
            args.Args = Value.Get(arguments);

            var report = new Report(new Session());
            var result = report.FnMarket(args);

            Assert.Equal(100, result.AsLong);
        }

        [Fact]
        public void Report_FnGetAt_SecondArgumentIsIndex()
        {
            var list = new List<Value>() { Value.Get(100), Value.Get(200), Value.Get(300) };
            var arguments = new List<Value>() { Value.Get(list), Value.Get(1) };

            var args = new CallScope(new Session());
            args.Args = Value.Get(arguments);

            var report = new Report(new Session());
            var result = report.FnGetAt(args);

            Assert.Equal(200, result.AsLong);
        }

        [Fact]
        public void Report_FnQuoted_HandlesNullArgument()
        {
            var arguments = new List<Value>() { Value.Empty };
            var args = new CallScope(new Session());
            args.Args = Value.Get(arguments);

            var report = new Report(new Session());
            var result = report.FnQuoted(args);

            Assert.Equal("\"\"", result.AsString);
        }

        [Fact]
        public void Report_FnJoin_HandlesNullArgument()
        {
            var arguments = new List<Value>() { Value.Empty };
            var args = new CallScope(new Session());
            args.Args = Value.Get(arguments);

            var report = new Report(new Session());
            var result = report.FnJoin(args);

            Assert.True(String.IsNullOrEmpty(result.AsString));
        }

        [Fact]
        public void Report_FnToday_ReturnsDateValue()
        {
            TimesCommon.Current.Epoch = new DateTime(2010, 10, 22, 23, 55, 59);
            var report = new Report(new Session());
            Value today = report.FnToday(null);
            Assert.Equal(ValueTypeEnum.Date, today.Type);
            Assert.Equal(new Date(2010, 10, 22), today.AsDate);
        }
    }
}

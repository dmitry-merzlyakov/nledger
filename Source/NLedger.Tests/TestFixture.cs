// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NLedger.Tests
{
    [Flags]
    public enum ContextInit
    {
        InitMainApplicationContext = 1,
        InitTimesCommon = 2,
        SaveCultureInfo= 3
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TestFixtureInitAttribute : Attribute
    {
        public TestFixtureInitAttribute(ContextInit contextInit)
        {
            ContextInit = contextInit;
        }

        public ContextInit ContextInit { get; set; }
    }

    public abstract class TestFixture
    {
        [TestInitialize]
        public void TestInitialize()
        {
            ContextInit contextInit = GetContextInit();

            if (contextInit.HasFlag(ContextInit.InitMainApplicationContext))
                MainContextAcquirer = new MainApplicationContext().AcquireCurrentThread();
            if (contextInit.HasFlag(ContextInit.InitTimesCommon))
                TimesCommon.Current.TimesInitialize();
            if (contextInit.HasFlag(ContextInit.SaveCultureInfo))
                CultureInfo = Thread.CurrentThread.CurrentCulture;

            CustomTestInitialize();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ContextInit contextInit = GetContextInit();

            CustomTestCleanup();

            if (contextInit.HasFlag(ContextInit.SaveCultureInfo))
                Thread.CurrentThread.CurrentCulture = CultureInfo;
            if (contextInit.HasFlag(ContextInit.InitMainApplicationContext))
                MainContextAcquirer.Dispose();
        }

        public MainApplicationContext.ThreadAcquirer MainContextAcquirer { get; private set; }

        public virtual void CustomTestInitialize()
        { }

        public virtual void CustomTestCleanup()
        { }

        private ContextInit GetContextInit()
        {
            TestFixtureInitAttribute attribute = (TestFixtureInitAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(TestFixtureInitAttribute));
            return attribute?.ContextInit ?? ContextInit.InitMainApplicationContext;
        }

        private CultureInfo CultureInfo { get; set; }
    }
}

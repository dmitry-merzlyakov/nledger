// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Scopus
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class GlobalScopeTests : TestFixture
    {
        public override void CustomTestInitialize()
        {
            GlobalScopeArgsOnly = GlobalScope.ArgsOnly;
            ValidatorIsVerifyEnabled = Validator.IsVerifyEnabled;
            GlobalScopeInitFile = GlobalScope.InitFile;
        }

        public override void CustomTestCleanup()
        {
            GlobalScope.ArgsOnly = GlobalScopeArgsOnly;
            Validator.IsVerifyEnabled = ValidatorIsVerifyEnabled;
            GlobalScope.InitFile = GlobalScopeInitFile;
        }

        [TestMethod]
        public void GlobalScope_Description_ReturnsGlobalScopeConstant()
        {
            Assert.AreEqual(GlobalScope.GlobalScopeDescription, new GlobalScope().Description);
        }

        [TestMethod]
        public void GlobalScope_HandleDebugOptions_Configures_ArgsOnly_VerifyMemory_InitFile()
        {
            List<string> args = new List<string>();
            args.Add("--args-only");
            args.Add("--verify-memory");
            args.Add("--init-file");
            args.Add("filename.ext");

            GlobalScope.HandleDebugOptions(args);

            Assert.IsTrue(GlobalScope.ArgsOnly);
            Assert.IsTrue(Validator.IsVerifyEnabled);
            Assert.AreEqual(LogLevelEnum.LOG_DEBUG, Logger.Current.LogLevel);
            Assert.AreEqual("memory\\.counts", Logger.Current.LogCategory);
        }

        [TestMethod]
        public void GlobalScope_HandleDebugOptions_Configures_Verify()
        {
            List<string> args = new List<string>();
            args.Add("--verify");

            GlobalScope.HandleDebugOptions(args);

            Assert.IsTrue(Validator.IsVerifyEnabled);
        }

        [TestMethod]
        public void GlobalScope_HandleDebugOptions_Configures_Verbose()
        {
            List<string> args = new List<string>();
            args.Add("--verbose");

            GlobalScope.HandleDebugOptions(args);

            Assert.AreEqual(LogLevelEnum.LOG_INFO, Logger.Current.LogLevel);
        }

        [TestMethod]
        public void GlobalScope_HandleDebugOptions_Configures_Debug()
        {
            List<string> args = new List<string>();
            args.Add("--debug");
            args.Add("debug-category");

            GlobalScope.HandleDebugOptions(args);

            Assert.AreEqual(LogLevelEnum.LOG_DEBUG, Logger.Current.LogLevel);
            Assert.AreEqual("debug-category", Logger.Current.LogCategory);
        }

        [TestMethod]
        public void GlobalScope_HandleDebugOptions_Configures_Trace()
        {
            List<string> args = new List<string>();
            args.Add("--trace");
            args.Add("23");

            GlobalScope.HandleDebugOptions(args);

            Assert.AreEqual(LogLevelEnum.LOG_TRACE, Logger.Current.LogLevel);
            Assert.AreEqual(23, Logger.Current.TraceLevel);
        }

        [TestMethod]
        public void GlobalScope_ShowVersionInfo_PopulatesOriginalLedgerVersion()
        {
            string expected =  String.Format(GlobalScope.ShowVersionInfoTemplate, VersionInfo.NLedgerVersion, VersionInfo.Ledger_VERSION_MAJOR, VersionInfo.Ledger_VERSION_MINOR, VersionInfo.Ledger_VERSION_PATCH);
            GlobalScope globalScope = new GlobalScope();
            Assert.AreEqual(expected, globalScope.ShowVersionInfo());
        }

        private bool GlobalScopeArgsOnly;
        private bool ValidatorIsVerifyEnabled;
        private string GlobalScopeInitFile;
    }
}

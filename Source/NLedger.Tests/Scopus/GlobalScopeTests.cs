// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts.Impl;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Scopus
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class GlobalScopeTests : TestFixture
    {
        protected override void CustomTestInitialize()
        {
            GlobalScopeArgsOnly = GlobalScope.ArgsOnly;
            ValidatorIsVerifyEnabled = Validator.IsVerifyEnabled;
            GlobalScopeInitFile = GlobalScope.InitFile;
        }

        protected override void CustomTestCleanup()
        {
            GlobalScope.ArgsOnly = GlobalScopeArgsOnly;
            Validator.IsVerifyEnabled = ValidatorIsVerifyEnabled;
            GlobalScope.InitFile = GlobalScopeInitFile;
        }

        [Fact]
        public void GlobalScope_Description_ReturnsGlobalScopeConstant()
        {
            Assert.Equal(GlobalScope.GlobalScopeDescription, new GlobalScope().Description);
        }

        [Fact]
        public void GlobalScope_HandleDebugOptions_Configures_ArgsOnly_VerifyMemory_InitFile()
        {
            List<string> args = new List<string>();
            args.Add("--args-only");
            args.Add("--verify-memory");
            args.Add("--init-file");
            args.Add("filename.ext");

            GlobalScope.HandleDebugOptions(args);

            Assert.True(GlobalScope.ArgsOnly);
            Assert.True(Validator.IsVerifyEnabled);
            Assert.Equal(LogLevelEnum.LOG_DEBUG, Logger.Current.LogLevel);
            Assert.Equal("memory\\.counts", Logger.Current.LogCategory);
        }

        [Fact]
        public void GlobalScope_HandleDebugOptions_Configures_Verify()
        {
            List<string> args = new List<string>();
            args.Add("--verify");

            GlobalScope.HandleDebugOptions(args);

            Assert.True(Validator.IsVerifyEnabled);
        }

        [Fact]
        public void GlobalScope_HandleDebugOptions_Configures_Verbose()
        {
            List<string> args = new List<string>();
            args.Add("--verbose");

            GlobalScope.HandleDebugOptions(args);

            Assert.Equal(LogLevelEnum.LOG_INFO, Logger.Current.LogLevel);
        }

        [Fact]
        public void GlobalScope_HandleDebugOptions_Configures_Debug()
        {
            List<string> args = new List<string>();
            args.Add("--debug");
            args.Add("debug-category");

            GlobalScope.HandleDebugOptions(args);

            Assert.Equal(LogLevelEnum.LOG_DEBUG, Logger.Current.LogLevel);
            Assert.Equal("debug-category", Logger.Current.LogCategory);
        }

        [Fact]
        public void GlobalScope_HandleDebugOptions_Configures_Trace()
        {
            List<string> args = new List<string>();
            args.Add("--trace");
            args.Add("23");

            GlobalScope.HandleDebugOptions(args);

            Assert.Equal(LogLevelEnum.LOG_TRACE, Logger.Current.LogLevel);
            Assert.Equal(23, Logger.Current.TraceLevel);
        }

        [Fact]
        public void GlobalScope_ShowVersionInfo_PopulatesOriginalLedgerVersion()
        {
            string expected =  String.Format(GlobalScope.ShowVersionInfoTemplate, VersionInfo.NLedgerVersion, VersionInfo.Ledger_VERSION_MAJOR, VersionInfo.Ledger_VERSION_MINOR, VersionInfo.Ledger_VERSION_PATCH, VersionInfo.Ledger_VERSION_DATE);
            GlobalScope globalScope = new GlobalScope();
            Assert.Equal(expected, globalScope.ShowVersionInfo());
        }

        [Fact]
        public void GlobalScope_ReportError_WritesErrorIfNoCancellationReques()
        {
            var globalScope = new GlobalScope();
            var ex = new Exception("some exception");

            using (var textWriter = new StringWriter())
            {
                MainApplicationContext.Current.SetApplicationServiceProvider(new ApplicationServiceProvider
                    (virtualConsoleProviderFactory: () => new VirtualConsoleProvider(consoleError: textWriter)));

                globalScope.ReportError(ex);

                textWriter.Flush();
                Assert.Equal("Error: some exception", textWriter.ToString().TrimEnd());
            }
        }

        [Fact]
        public void GlobalScope_ReportError_DiscardsCancellationRequestAndDoesNothing()
        {
            var globalScope = new GlobalScope();
            var ex = new Exception("some exception");

            using (var textWriter = new StringWriter())
            {
                MainApplicationContext.Current.SetApplicationServiceProvider(new ApplicationServiceProvider
                    (virtualConsoleProviderFactory: () => new VirtualConsoleProvider(consoleError: textWriter)));

                MainApplicationContext.Current.CancellationSignal = CaughtSignalEnum.INTERRUPTED;
                globalScope.ReportError(ex);

                textWriter.Flush();
                Assert.Equal("", textWriter.ToString().TrimEnd());
                Assert.Equal(CaughtSignalEnum.NONE_CAUGHT, MainApplicationContext.Current.CancellationSignal);
            }
        }

        private class StubTextWriter : TextWriter
        {
            public override Encoding Encoding { get; }
            public bool IsClosed { get; private set; }

            public override void Close()
            {
                IsClosed = true;
            }
        }

        [Fact]
        public void GlobalScope_PopReport_ClosesOutputStream()
        {
            var globalScope = new GlobalScope();
            using (var textWriter = new StubTextWriter())  // Note that this textwriter neither StdOut nor StdErr
            {
                var report = new Report() { OutputStream = textWriter };
                globalScope.ReportStack.Push(report);
                globalScope.ReportStack.Push(report);

                Assert.False(textWriter.IsClosed);
                globalScope.PopReport();
                Assert.True(textWriter.IsClosed);
            }
        }

        private bool GlobalScopeArgsOnly;
        private bool ValidatorIsVerifyEnabled;
        private string GlobalScopeInitFile;
    }
}

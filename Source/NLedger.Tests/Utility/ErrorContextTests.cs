// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts.Impl;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext)]
    public class ErrorContextTests : TestFixture
    {
        [Fact]
        public void ErrorContext_WriteError_AddsToErrorStream()
        {
            using (var outWriter = new StringWriter())
            {
                MainApplicationContext.Current.SetApplicationServiceProvider(new ApplicationServiceProvider
                    (virtualConsoleProviderFactory: () => new VirtualConsoleProvider(consoleError: outWriter)));

                ErrorContext.Current.WriteError("error-text");
                outWriter.Flush();
                var result = outWriter.ToString();
                Assert.Equal("error-text\n", result.RemoveCarriageReturns());
            }
        }

        [Fact]
        public void ErrorContext_WriteError_AddsNothingForEmptyMessage()
        {
            using (var outWriter = new StringWriter())
            {
                MainApplicationContext.Current.SetApplicationServiceProvider(new ApplicationServiceProvider
                    (virtualConsoleProviderFactory: () => new VirtualConsoleProvider(consoleOutput: outWriter)));

                ErrorContext.Current.WriteError("");
                outWriter.Flush();
                var result = outWriter.ToString();
                Assert.Equal("", result);
            }
        }

        [Fact]
        public void ErrorContext_WriteError_AddsErrorWordForException()
        {
            using (var outWriter = new StringWriter())
            {
                MainApplicationContext.Current.SetApplicationServiceProvider(new ApplicationServiceProvider
                    (virtualConsoleProviderFactory: () => new VirtualConsoleProvider(consoleError: outWriter)));

                ErrorContext.Current.WriteError(new Exception("exception-text"));
                outWriter.Flush();
                var result = outWriter.ToString();
                Assert.Equal("Error: exception-text\n", result.RemoveCarriageReturns());
            }
        }

        [Fact]
        public void ErrorContext_WriteWarning_AddsWarningWord()
        {
            using (var outWriter = new StringWriter())
            {
                MainApplicationContext.Current.SetApplicationServiceProvider(new ApplicationServiceProvider
                    (virtualConsoleProviderFactory: () => new VirtualConsoleProvider(consoleError: outWriter)));

                ErrorContext.Current.WriteWarning("warning-text");
                outWriter.Flush();
                var result = outWriter.ToString();
                Assert.Equal("Warning: warning-text\n", result.RemoveCarriageReturns());
            }
        }
    }
}

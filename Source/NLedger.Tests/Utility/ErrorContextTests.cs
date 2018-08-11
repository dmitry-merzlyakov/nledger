// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Abstracts.Impl;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utility
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext)]
    public class ErrorContextTests : TestFixture
    {
        [TestMethod]
        public void ErrorContext_WriteError_AddsToErrorStream()
        {
            using (var outWriter = new StringWriter())
            {
                MainApplicationContext.Current.SetVirtualConsoleProvider(() => new VirtualConsoleProvider(consoleError: outWriter));

                ErrorContext.Current.WriteError("error-text");
                outWriter.Flush();
                var result = outWriter.ToString();
                Assert.AreEqual("error-text\r\n", result);
            }
        }

        [TestMethod]
        public void ErrorContext_WriteError_AddsNothingForEmptyMessage()
        {
            using (var outWriter = new StringWriter())
            {
                MainApplicationContext.Current.SetVirtualConsoleProvider(() => new VirtualConsoleProvider(consoleOutput: outWriter));

                ErrorContext.Current.WriteError("");
                outWriter.Flush();
                var result = outWriter.ToString();
                Assert.AreEqual("", result);
            }
        }

        [TestMethod]
        public void ErrorContext_WriteError_AddsErrorWordForException()
        {
            using (var outWriter = new StringWriter())
            {
                MainApplicationContext.Current.SetVirtualConsoleProvider(() => new VirtualConsoleProvider(consoleError: outWriter));

                ErrorContext.Current.WriteError(new Exception("exception-text"));
                outWriter.Flush();
                var result = outWriter.ToString();
                Assert.AreEqual("Error: exception-text\r\n", result);
            }
        }

        [TestMethod]
        public void ErrorContext_WriteWarning_AddsWarningWord()
        {
            using (var outWriter = new StringWriter())
            {
                MainApplicationContext.Current.SetVirtualConsoleProvider(() => new VirtualConsoleProvider(consoleError: outWriter));

                ErrorContext.Current.WriteWarning("warning-text");
                outWriter.Flush();
                var result = outWriter.ToString();
                Assert.AreEqual("Warning: warning-text\r\n", result);
            }
        }
    }
}

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Abstracts.Impl
{
    [TestClass]
    [DeploymentItem(@"Abstracts\Impl\ProcessManagerBatch.cmd")]
    [DeploymentItem(@"Abstracts\Impl\ProcessManagerErrBatch.cmd")]
    public class ProcessManagerTests
    {
        [TestMethod]
        public void ProcessManager_RunProcess_CanExecuteBatchFile()
        {
            // Note that it expects to find an executable in the current folder (unit test results folder in this case)
            // Note that it does require an extension for executable in case of running "cmd" process
            var result = ProcessManager.RunProcess("cmd", "/c ProcessManagerBatch param1 param2");
            Assert.IsNotNull(result);
            Assert.AreEqual("", result.StandardError);
            Assert.AreEqual("Process Manager Test Batch File\r\nParameter 1 - param1\r\nParameter 2 - param2\r\nParameter 3 - \r\n", result.StandardOutput);
            Assert.AreEqual(0, result.ExitCode);
            Assert.IsFalse(result.IsTimeouted);
        }

        [TestMethod]
        public void ProcessManager_RunProcess_DetectsBatchExistCode()
        {
            var result = ProcessManager.RunProcess("cmd", "/c ProcessManagerErrBatch");
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ExitCode);
        }

    }
}

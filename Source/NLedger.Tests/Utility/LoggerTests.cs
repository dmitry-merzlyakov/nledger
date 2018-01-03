// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class LoggerTests : TestFixture
    {
        public override void CustomTestInitialize()
        {
            SaveOutWriter = Logger.Current.OutWriter;
            SaveLogLevel = Logger.Current.LogLevel;
        }

        public override void CustomTestCleanup()
        {
            Logger.Current.OutWriter = SaveOutWriter;
            Logger.Current.LogLevel = SaveLogLevel;
        }

        [TestMethod]
        public void Logger_Info_SendsMessagesOnlyIfLogLevelIsInfoOrHigher()
        {
            string result = TestSendingMessage(
                () => { Logger.Current.LogLevel = LogLevelEnum.LOG_INFO; },
                () => { Logger.Info("log message"); }
            );
            Assert.IsTrue(result.Trim().EndsWith("ms [INFO]  log message"));

            string result1 = TestSendingMessage(
                () => { Logger.Current.LogLevel = LogLevelEnum.LOG_ERROR; },
                () => { Logger.Info("log message"); }
            );
            Assert.IsTrue(String.IsNullOrWhiteSpace(result1));
        }

        private string TestSendingMessage(Action arrange, Action action)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (TextWriter textWriter = new StreamWriter(stream))
                {
                    Logger.Current.OutWriter = textWriter;

                    arrange();
                    action();

                    textWriter.Flush();
                    stream.Position = 0;
                    using (StreamReader textReader = new StreamReader(stream))
                    {
                        return textReader.ReadToEnd();
                    }
                }
            }
        }

        private TextWriter SaveOutWriter { get; set; }
        private LogLevelEnum SaveLogLevel { get; set; }
    }
}

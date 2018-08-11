// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utils
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class LoggerTests : TestFixture
    {
        [TestMethod]
        public void Logger_ShowTrace_ChecksLogLevelAndTraceLevel()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_TRACE, TraceLevel = 1 };
            Assert.IsTrue(logger.ShowTrace(0));
            Assert.IsTrue(logger.ShowTrace(1));
            Assert.IsFalse(logger.ShowTrace(2));

            logger.LogLevel = LogLevelEnum.LOG_DEBUG;

            Assert.IsFalse(logger.ShowTrace(0));
            Assert.IsFalse(logger.ShowTrace(1));
            Assert.IsFalse(logger.ShowTrace(2));
        }

        [TestMethod]
        public void Logger_ShowDebug_ChecksLogLevel()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_TRACE, LogCategory = "something" };
            Assert.IsTrue(logger.ShowDebug("something"));

            logger.LogLevel = LogLevelEnum.LOG_DEBUG;
            Assert.IsTrue(logger.ShowDebug("something"));

            logger.LogLevel = LogLevelEnum.LOG_INFO;
            Assert.IsFalse(logger.ShowDebug("something"));
        }

        [TestMethod]
        public void Logger_ShowDebug_ChecksCategory()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_TRACE };
            Assert.IsFalse(logger.ShowDebug("something"));

            logger.LogCategory = "something";
            Assert.IsTrue(logger.ShowDebug("something"));

            logger.LogCategory = "some";
            Assert.IsTrue(logger.ShowDebug("something"));
            Assert.IsFalse(logger.ShowDebug("nothing"));
        }

        [TestMethod]
        public void Logger_ShowInfo_ChecksLogLevel()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_TRACE };
            Assert.IsTrue(logger.ShowInfo());

            logger.LogLevel = LogLevelEnum.LOG_DEBUG;
            Assert.IsTrue(logger.ShowInfo());

            logger.LogLevel = LogLevelEnum.LOG_INFO;
            Assert.IsTrue(logger.ShowInfo());

            logger.LogLevel = LogLevelEnum.LOG_WARN;
            Assert.IsFalse(logger.ShowInfo());
        }

        [TestMethod]
        public void Logger_ShowWarn_ChecksLogLevel()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_INFO };
            Assert.IsTrue(logger.ShowWarn());

            logger.LogLevel = LogLevelEnum.LOG_WARN;
            Assert.IsTrue(logger.ShowWarn());

            logger.LogLevel = LogLevelEnum.LOG_ERROR;
            Assert.IsFalse(logger.ShowWarn());
        }

        [TestMethod]
        public void Logger_ShowError_ChecksLogLevel()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_WARN };
            Assert.IsTrue(logger.ShowError());

            logger.LogLevel = LogLevelEnum.LOG_ERROR;
            Assert.IsTrue(logger.ShowError());

            logger.LogLevel = LogLevelEnum.LOG_FATAL;
            Assert.IsFalse(logger.ShowError());
        }

        [TestMethod]
        public void Logger_Trace_GetsMessageOnlyIfEnabled()
        {
            bool isFired = false;

            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_INFO };
            logger.Trace(1, () => { isFired = true; return "text"; });
            Assert.IsFalse(isFired);

            logger.LogLevel = LogLevelEnum.LOG_TRACE;
            logger.TraceLevel = 1;

            logger.Trace(1, () => { isFired = true; return "text"; });
            Assert.IsTrue(isFired);
        }

        [TestMethod]
        public void Logger_Debug_GetsMessageOnlyIfEnabled()
        {
            bool isFired = false;

            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_DEBUG };
            logger.Debug("none", () => { isFired = true; return "text"; });
            Assert.IsFalse(isFired);

            logger.LogCategory = "something";

            logger.Debug("something", () => { isFired = true; return "text"; });
            Assert.IsTrue(isFired);
        }

        [TestMethod]
        public void Logger_Info_GetsMessageOnlyIfEnabled()
        {
            bool isFired = false;

            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_WARN };
            logger.Info(() => { isFired = true; return "text"; });
            Assert.IsFalse(isFired);

            logger.LogLevel = LogLevelEnum.LOG_INFO;

            logger.Info(() => { isFired = true; return "text"; });
            Assert.IsTrue(isFired);
        }

        [TestMethod]
        public void Logger_TraceContext_ReturnsNullIfTracingDisabled()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_WARN };
            Assert.IsNull(logger.TraceContext("some-name", 1));
        }

        [TestMethod]
        public void Logger_InfoContext_ReturnsNullIfTracingDisabled()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_WARN };
            Assert.IsNull(logger.InfoContext("some-name"));
        }

        [TestMethod]
        public void Logger_Info_SendsMessagesOnlyIfLogLevelIsInfoOrHigher()
        {
            string result = TestSendingMessage(
                () => { Logger.Current.LogLevel = LogLevelEnum.LOG_INFO; },
                () => { Logger.Current.Info(() => "log message"); }
            );
            Assert.IsTrue(result.Trim().EndsWith("ms [INFO]  log message"));

            string result1 = TestSendingMessage(
                () => { Logger.Current.LogLevel = LogLevelEnum.LOG_ERROR; },
                () => { Logger.Current.Info(() => "log message"); }
            );
            Assert.IsTrue(String.IsNullOrWhiteSpace(result1));
        }

        private string TestSendingMessage(Action arrange, Action action)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (TextWriter textWriter = new StreamWriter(stream))
                {
                    ((Logger)(Logger.Current)).OutWriter = textWriter;

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

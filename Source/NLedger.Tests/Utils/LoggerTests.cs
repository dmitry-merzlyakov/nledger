// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utils
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class LoggerTests : TestFixture
    {
        [Fact]
        public void Logger_ShowTrace_ChecksLogLevelAndTraceLevel()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_TRACE, TraceLevel = 1 };
            Assert.True(logger.ShowTrace(0));
            Assert.True(logger.ShowTrace(1));
            Assert.False(logger.ShowTrace(2));

            logger.LogLevel = LogLevelEnum.LOG_DEBUG;

            Assert.False(logger.ShowTrace(0));
            Assert.False(logger.ShowTrace(1));
            Assert.False(logger.ShowTrace(2));
        }

        [Fact]
        public void Logger_ShowDebug_ChecksLogLevel()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_TRACE, LogCategory = "something" };
            Assert.True(logger.ShowDebug("something"));

            logger.LogLevel = LogLevelEnum.LOG_DEBUG;
            Assert.True(logger.ShowDebug("something"));

            logger.LogLevel = LogLevelEnum.LOG_INFO;
            Assert.False(logger.ShowDebug("something"));
        }

        [Fact]
        public void Logger_ShowDebug_ChecksCategory()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_TRACE };
            Assert.False(logger.ShowDebug("something"));

            logger.LogCategory = "something";
            Assert.True(logger.ShowDebug("something"));

            logger.LogCategory = "some";
            Assert.True(logger.ShowDebug("something"));
            Assert.False(logger.ShowDebug("nothing"));
        }

        [Fact]
        public void Logger_ShowInfo_ChecksLogLevel()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_TRACE };
            Assert.True(logger.ShowInfo());

            logger.LogLevel = LogLevelEnum.LOG_DEBUG;
            Assert.True(logger.ShowInfo());

            logger.LogLevel = LogLevelEnum.LOG_INFO;
            Assert.True(logger.ShowInfo());

            logger.LogLevel = LogLevelEnum.LOG_WARN;
            Assert.False(logger.ShowInfo());
        }

        [Fact]
        public void Logger_ShowWarn_ChecksLogLevel()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_INFO };
            Assert.True(logger.ShowWarn());

            logger.LogLevel = LogLevelEnum.LOG_WARN;
            Assert.True(logger.ShowWarn());

            logger.LogLevel = LogLevelEnum.LOG_ERROR;
            Assert.False(logger.ShowWarn());
        }

        [Fact]
        public void Logger_ShowError_ChecksLogLevel()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_WARN };
            Assert.True(logger.ShowError());

            logger.LogLevel = LogLevelEnum.LOG_ERROR;
            Assert.True(logger.ShowError());

            logger.LogLevel = LogLevelEnum.LOG_FATAL;
            Assert.False(logger.ShowError());
        }

        [Fact]
        public void Logger_Trace_GetsMessageOnlyIfEnabled()
        {
            bool isFired = false;

            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_INFO };
            logger.Trace(1, () => { isFired = true; return "text"; });
            Assert.False(isFired);

            logger.LogLevel = LogLevelEnum.LOG_TRACE;
            logger.TraceLevel = 1;

            logger.Trace(1, () => { isFired = true; return "text"; });
            Assert.True(isFired);
        }

        [Fact]
        public void Logger_Debug_GetsMessageOnlyIfEnabled()
        {
            bool isFired = false;

            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_DEBUG };
            logger.Debug("none", () => { isFired = true; return "text"; });
            Assert.False(isFired);

            logger.LogCategory = "something";

            logger.Debug("something", () => { isFired = true; return "text"; });
            Assert.True(isFired);
        }

        [Fact]
        public void Logger_Info_GetsMessageOnlyIfEnabled()
        {
            bool isFired = false;

            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_WARN };
            logger.Info(() => { isFired = true; return "text"; });
            Assert.False(isFired);

            logger.LogLevel = LogLevelEnum.LOG_INFO;

            logger.Info(() => { isFired = true; return "text"; });
            Assert.True(isFired);
        }

        [Fact]
        public void Logger_TraceContext_ReturnsNullIfTracingDisabled()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_WARN };
            Assert.Null(logger.TraceContext("some-name", 1));
        }

        [Fact]
        public void Logger_InfoContext_ReturnsNullIfTracingDisabled()
        {
            var logger = new Logger() { LogLevel = LogLevelEnum.LOG_WARN };
            Assert.Null(logger.InfoContext("some-name"));
        }

        [Fact]
        public void Logger_Info_SendsMessagesOnlyIfLogLevelIsInfoOrHigher()
        {
            string result = TestSendingMessage(
                () => { Logger.Current.LogLevel = LogLevelEnum.LOG_INFO; },
                () => { Logger.Current.Info(() => "log message"); }
            );
            Assert.EndsWith("ms [INFO]  log message", result.Trim());

            string result1 = TestSendingMessage(
                () => { Logger.Current.LogLevel = LogLevelEnum.LOG_ERROR; },
                () => { Logger.Current.Info(() => "log message"); }
            );
            Assert.True(String.IsNullOrWhiteSpace(result1));
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

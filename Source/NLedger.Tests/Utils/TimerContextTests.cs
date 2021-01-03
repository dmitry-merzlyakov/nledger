// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utils
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class TimerContextTests : TestFixture
    {
        [Fact]
        public void TimerContext_Constructor_PopulatesProperties()
        {
            Logger logger = new Logger();
            string name = "some-name";
            LogLevelEnum level = LogLevelEnum.LOG_EXCEPT;

            TimerContext context = new TimerContext("some-name", logger, level);

            Assert.Equal(logger, context.Logger);
            Assert.Equal(name, context.TimerName);
            Assert.Equal(level, context.LogLevel);
            Assert.Null(context.TimerMessage);
            Assert.Equal(-1, context.GetElapsedTime());  // Indicates that the timer is not started
        }

        [Fact]
        public void TimerContext_Message_PopulatesTimerMessage()
        {
            TimerContext context = new TimerContext("some-name", new Logger(), LogLevelEnum.LOG_DEBUG);

            var result = context.Message("some-message");

            Assert.Equal(context, result);
            Assert.Equal("some-message", context.TimerMessage);
        }

        [Fact]
        public void TimerContext_Start_RunsTimer()
        {
            TimerContext context = new TimerContext("some-name", new Logger(), LogLevelEnum.LOG_DEBUG);

            var result = context.Start();
            Thread.Sleep(5);

            Assert.Equal(context, result);
            Assert.True(context.GetElapsedTime() > 0);  // Indicates that the timer has been started
        }

        [Fact]
        public void TimerContext_Start_ContinuesTiming()
        {
            TimerContext context = new TimerContext("some-name", new Logger(), LogLevelEnum.LOG_DEBUG);

            context.Start();
            Thread.Sleep(5);
            context.Stop();
            var firstTime = context.GetElapsedTime();

            context.Start();
            Thread.Sleep(5);
            context.Stop();
            var secondTime = context.GetElapsedTime();

            Assert.True(firstTime > 0);
            Assert.True(secondTime > firstTime);
        }

        [Fact]
        public void TimerContext_Stop_StopsTiming()
        {
            TimerContext context = new TimerContext("some-name", new Logger(), LogLevelEnum.LOG_DEBUG);

            context.Start();
            Thread.Sleep(5);
            context.Stop();
            var firstTime = context.GetElapsedTime();
            Thread.Sleep(5);
            var secondTime = context.GetElapsedTime();

            Assert.True(firstTime > 0);
            Assert.True(secondTime == firstTime);
        }

        [Fact]
        public void TimerContext_Finish_DoesNothingIfTimerHasNotBeenStarted()
        {
            var writer = new StringWriter();
            var logger = new Logger() { OutWriter = writer };

            TimerContext context = new TimerContext("some-name", logger, LogLevelEnum.LOG_DEBUG);
            context.Finish();
            writer.Flush();

            Assert.Equal(-1, context.GetElapsedTime());
            Assert.Equal(0, writer.GetStringBuilder().Length);
        }

        [Fact]
        public void TimerContext_Finish_WritesMessageWithNoParenthesis()
        {
            var writer = new StringWriter();
            var logger = new Logger() { OutWriter = writer };
            TimerContext context = new TimerContext("some-name", logger, LogLevelEnum.LOG_DEBUG);

            context.Start();
            context.Message("msg1:");  // Notice the colons at the end
            context.Finish();
            writer.Flush();

            var time = context.GetElapsedTime();
            var expectedMessage = String.Format("[DEBUG] msg1: {0}ms", time);

            Assert.Equal(expectedMessage, writer.GetStringBuilder().ToString().Substring(9).TrimEnd());
        }

        [Fact]
        public void TimerContext_Finish_WritesMessageWithParenthesis()
        {
            var writer = new StringWriter();
            var logger = new Logger() { OutWriter = writer };
            TimerContext context = new TimerContext("some-name", logger, LogLevelEnum.LOG_INFO);

            context.Start();
            context.Message("msg1");  // Notice no colons at the end
            context.Finish();
            writer.Flush();

            var time = context.GetElapsedTime();
            var expectedMessage = String.Format("[INFO]  msg1 ({0}ms)", time);

            Assert.Equal(expectedMessage, writer.GetStringBuilder().ToString().Substring(9).TrimEnd());
        }

        [Fact]
        public void TimerContext_Finish_RemovesTimes()
        {
            Logger logger = new Logger();

            var context1 = logger.GetTimer("some-name", LogLevelEnum.LOG_INFO);
            var context2 = logger.GetTimer("some-name", LogLevelEnum.LOG_INFO);

            context1.Finish();

            var context3 = logger.GetTimer("some-name", LogLevelEnum.LOG_INFO);

            Assert.Equal(context1, context2);
            Assert.NotEqual(context1, context3); // context3 is a new timer instance; previous was removed by Finish
        }

    }
}

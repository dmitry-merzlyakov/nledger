// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLedger.Utils
{
    public class Logger : ILogger
    {
        public static ILogger Current
        {
            get { return MainApplicationContext.Current.Logger; }
        }

        public LogLevelEnum LogLevel { get; set; }
        public string LogCategory
        {
            get { return _LogCategory; }
            set
            {
                _LogCategory = value;
                if (!String.IsNullOrEmpty(value))
                    LogCategoryRe = new Regex(value, RegexOptions.IgnoreCase);
            }
        }
        public int TraceLevel { get; set; }
        public TextWriter OutWriter { get; set; }


        public bool ShowTrace(int lvl)
        {
            return LogLevel >= LogLevelEnum.LOG_TRACE  && lvl <= TraceLevel;
        }

        public bool ShowDebug(string cat)
        {
            return LogLevel >= LogLevelEnum.LOG_DEBUG && CategoryMatches(cat);
        }

        public bool ShowInfo()
        {
            return LogLevel >= LogLevelEnum.LOG_INFO;
        }

        public bool ShowWarn()
        {
            return LogLevel >= LogLevelEnum.LOG_WARN;
        }

        public bool ShowError()
        {
            return LogLevel >= LogLevelEnum.LOG_ERROR;
        }

        public bool ShowFatal()
        {
            return LogLevel >= LogLevelEnum.LOG_FATAL;
        }

        public bool ShowCritical()
        {
            return LogLevel >= LogLevelEnum.LOG_CRIT;
        }

        public bool ShowException()
        {
            return LogLevel >= LogLevelEnum.LOG_EXCEPT;
        }


        public void Trace(int lvl, Func<string> msg)
        {
            if (ShowTrace(lvl))
                WriteLine(LogLevelEnum.LOG_TRACE, msg());
        }

        public void Debug(string cat, Func<string> msg)
        {
            if (ShowDebug(cat))
                WriteLine(LogLevelEnum.LOG_DEBUG, msg());
        }

        public void Info(Func<string> msg)
        {
            if (ShowInfo())
                WriteLine(LogLevelEnum.LOG_INFO, msg());
        }

        public void Warn(Func<string> msg)
        {
            if (ShowWarn())
                WriteLine(LogLevelEnum.LOG_WARN, msg());
        }

        public void Error(Func<string> msg)
        {
            if (ShowError())
                WriteLine(LogLevelEnum.LOG_ERROR, msg());
        }

        public void Fatal(Func<string> msg)
        {
            if (ShowError())
                WriteLine(LogLevelEnum.LOG_FATAL, msg());
        }

        public void Critical(Func<string> msg)
        {
            if (ShowCritical())
                WriteLine(LogLevelEnum.LOG_CRIT, msg());
        }

        public void Exception(Func<string> msg)
        {
            if (ShowException())
                WriteLine(LogLevelEnum.LOG_EXCEPT, msg());
        }


        public ITimerContext TraceContext(string name, int lvl)
        {
            if (!ShowTrace(lvl))
                return null;

            return GetTimer(name, LogLevelEnum.LOG_TRACE);
        }

        public ITimerContext DebugContext(string name, string cat)
        {
            if (!ShowDebug(cat))
                return null;

            return GetTimer(name, LogLevelEnum.LOG_DEBUG);
        }

        public ITimerContext InfoContext(string name)
        {
            if (!ShowInfo())
                return null;

            return GetTimer(name, LogLevelEnum.LOG_INFO);
        }

        /// <summary>
        /// Ported from bool category_matches(const char * cat)
        /// </summary>
        public bool CategoryMatches(string cat)
        {
            return !String.IsNullOrEmpty(LogCategory) && LogCategoryRe.IsMatch(cat);
        }

        public void WriteLine(LogLevelEnum logLevel, string message)
        {
            if (LoggerStart == null)
                LoggerStart = TimesCommon.Current.TrueCurrentTime;

            long totalMiliseconds = (long)(TimesCommon.Current.TrueCurrentTime - LoggerStart.Value).TotalMilliseconds;
            string level = LogLevelStrings[logLevel];
            Writer.WriteLine(String.Format("{0,6}ms {1,-7} {2}", totalMiliseconds, level, message));
        }

        public ITimerContext GetTimer(string timerName, LogLevelEnum logLevel)
        {
            if (String.IsNullOrEmpty(timerName))
                throw new ArgumentNullException("timerName");

            TimerContext timer = Timers.Value.GetOrAdd(timerName, n => new TimerContext(n, this, logLevel));
            if (timer.LogLevel != logLevel)
            {
                timer = new TimerContext(timerName, this, logLevel);
                timer = Timers.Value.AddOrUpdate(timerName, n => timer, (n,t) => timer);
            }

            return timer;
        }

        public void RemoveTimer(string timerName)
        {
            if (String.IsNullOrEmpty(timerName))
                throw new ArgumentNullException("timerName");

            TimerContext timer;
            Timers.Value.TryRemove(timerName, out timer);
        }

        private TextWriter Writer
        {
            get { return OutWriter ?? VirtualConsole.Error; }
        }

        private DateTime? LoggerStart = null;
        private string _LogCategory = null;
        private Regex LogCategoryRe = null;
        private Lazy<ConcurrentDictionary<string, TimerContext>> Timers = new Lazy<ConcurrentDictionary<string, TimerContext>>(true);

        private static IDictionary<LogLevelEnum, string> LogLevelStrings = new Dictionary<LogLevelEnum, string>()
        {
            { LogLevelEnum.LOG_OFF, String.Empty},
            { LogLevelEnum.LOG_CRIT, "[CRIT]"},
            { LogLevelEnum.LOG_FATAL, "[FATAL]"},
            { LogLevelEnum.LOG_ASSERT, "[ASSRT]"},
            { LogLevelEnum.LOG_ERROR, "[ERROR]"},
            { LogLevelEnum.LOG_VERIFY, "[VERFY]"},
            { LogLevelEnum.LOG_WARN, "[WARN]"},
            { LogLevelEnum.LOG_INFO, "[INFO]"},
            { LogLevelEnum.LOG_EXCEPT, "[EXCPT]"},
            { LogLevelEnum.LOG_DEBUG, "[DEBUG]"},
            { LogLevelEnum.LOG_TRACE, "[TRACE]"},
            { LogLevelEnum.LOG_ALL, String.Empty}
        };
    }
}

// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Times;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public enum LogLevelEnum
    {
        LOG_OFF = 0,
        LOG_CRIT,
        LOG_FATAL,
        LOG_ASSERT,
        LOG_ERROR,
        LOG_VERIFY,
        LOG_WARN,
        LOG_INFO,
        LOG_EXCEPT,
        LOG_DEBUG,
        LOG_TRACE,
        LOG_ALL
    }

    public class Logger
    {
        public static Logger Current
        {
            get
            { return MainApplicationContext.Current.Logger ??
                    (MainApplicationContext.Current.Logger = new Logger());
            }
        }

        public LogLevelEnum LogLevel { get; set; }
        public string LogCategory { get; set; }
        public int TraceLevel { get; set; }
        public TextWriter OutWriter { get; set; }


        public static void Info(string message)
        {
            Logger.Current.LogMacro(LogLevelEnum.LOG_INFO, message);
        }

        public static void Debug(string message)
        {
            Logger.Current.LogMacro(LogLevelEnum.LOG_DEBUG, message);
        }

        public static bool IsDebugOn
        {
            get { return Logger.Current.LogLevel >= LogLevelEnum.LOG_DEBUG; }
        }

        public static void Debug(string message, Func<string> values)
        {
            if (IsDebugOn)
                Logger.Current.WriteLine(message, values());
        }

        public static void Info(string message, params object[] values)
        {
            Logger.Current.LogMacro(LogLevelEnum.LOG_INFO, message, values);
        }

        private void LogMacro(LogLevelEnum logLevel, string message, params object[] values)
        {
            if (LogLevel >= logLevel)
                WriteLine(message, values);
        }

        private void WriteLine(string message, params object[] values)
        {
            if (values != null && values.Length > 0)
            {
                var nMessage = String.Format(message, values);
                if (message == nMessage)
                {
                    var sb = new StringBuilder(message);
                    sb.Append(" ");
                    foreach (var val in values)
                    {
                        sb.Append(val);
                        sb.Append(" ");
                    }
                    message = sb.ToString();
                }
                else
                {
                    message = nMessage;
                }
            }

            if (LoggerStart == null)
                LoggerStart = TimesCommon.Current.TrueCurrentTime;

            long totalMiliseconds = (long)(TimesCommon.Current.TrueCurrentTime - LoggerStart.Value).TotalMilliseconds;
            string level = LogLevelStrings[LogLevel];
            Writer.WriteLine(String.Format("{0,6}ms {1,-7} {2}", totalMiliseconds, level, message));
        }

        private TextWriter Writer
        {
            get { return OutWriter ?? FileSystem.ConsoleOutput; }
        }

        private DateTime? LoggerStart = null;

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

// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utils
{
    public sealed class TimerContext : ITimerContext
    {
        public TimerContext(string timerName, Logger logger, LogLevelEnum logLevel)
        {
            if (String.IsNullOrWhiteSpace(timerName))
                throw new ArgumentNullException("timerName");
            if (logger == null)
                throw new ArgumentNullException("logger");

            TimerName = timerName;
            Logger = logger;
            LogLevel = logLevel;
        }

        public string TimerName { get; private set; }
        public LogLevelEnum LogLevel { get; private set; }
        public Logger Logger { get; private set; }

        public string TimerMessage { get; private set; }

        public void Finish()
        {
            if (Stopwatch.IsValueCreated)
            {
                Stop();

                bool needParen = TimerMessage != null && !TimerMessage.EndsWith(":");

                if (needParen)
                    Logger.WriteLine(LogLevel, String.Format("{0} ({1}ms)", TimerMessage, Stopwatch.Value.ElapsedMilliseconds));
                else
                    Logger.WriteLine(LogLevel, String.Format("{0} {1}ms", TimerMessage, Stopwatch.Value.ElapsedMilliseconds));
            }

            Logger.RemoveTimer(TimerName);
        }

        public ITimerContext Message(string msg)
        {
            TimerMessage = msg;
            return this;
        }

        public ITimerContext Start()
        {
            Stopwatch.Value.Start();
            return this;
        }

        public void Stop()
        {
            if (Stopwatch.IsValueCreated)
                Stopwatch.Value.Stop();
        }

        public long GetElapsedTime()
        {
            return Stopwatch.IsValueCreated ? Stopwatch.Value.ElapsedMilliseconds : -1;
        }

        public Lazy<Stopwatch> Stopwatch = new Lazy<Stopwatch>();
    }
}

// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts.Impl;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NLedger.Utility.ServiceAPI
{
    /// <summary>
    /// Service API command response (contains the result of an executed command)
    /// </summary>
    public class ServiceResponse
    {
        public ServiceResponse(ServiceSession serviceSession, string command, CancellationToken token)
        {
            if (serviceSession == null)
                throw new ArgumentNullException(nameof(serviceSession));
            if (String.IsNullOrWhiteSpace(command))
                throw new ArgumentNullException(nameof(command));

            ServiceSession = serviceSession;
            using (new ScopeTimeTracker(time => ExecutionTime = time))
                MainApplicationContext = InitializeResponse(command, token);
        }

        public ServiceSession ServiceSession { get; }
        public MainApplicationContext MainApplicationContext { get; }
        public int Status { get; private set; } = 1;
        public TimeSpan ExecutionTime { get; private set; }
        public bool HasErrors => Status > 0;

        public string OutputText { get; private set; }
        public string ErrorText { get; private set; }

        private MainApplicationContext InitializeResponse(string command, CancellationToken token)
        {
            // [DM] This is a quick workaround to fix multithreading issues caused by original non-thread-safe legder code.
            // When ExecuteCommandWrapper is executing, it changes the state of GlobalScope object, it can also change the state of 
            // accounts and xacts (depending on the command). However, it properly restores the original state when it finishes.
            // Therefore, the quick solution is to limit parallel requests with only one running executor at the moment.
            // The right solution would be cloning GlobalScope object for every thread (or, fixing thread-unsafe code).
            lock (ServiceSession)
            {
                using (var memoryStreamManager = new MemoryStreamManager())
                {
                    var context = ServiceSession.ServiceEngine.CloneContext(ServiceSession.MainApplicationContext, memoryStreamManager);
                    token.Register(() => context.CancellationSignal = CaughtSignalEnum.INTERRUPTED);
                    context.Logger = new Logger();
                    context.ErrorContext = new ErrorContext();

                    using (context.AcquireCurrentThread())
                    {

                        try
                        {
                            Status = ServiceSession.GlobalScope.ExecuteCommandWrapper(StringExtensions.SplitArguments(command), true);
                        }
                        catch (CountError errors)
                        {
                            Status = errors.Count;
                        }
                        catch (Exception err)
                        {
                            ServiceSession.GlobalScope.ReportError(err);
                        }

                        OutputText = memoryStreamManager.GetOutputText();
                        ErrorText = memoryStreamManager.GetErrorText();
                        return context;
                    }
                }
            }
        }
    }
}

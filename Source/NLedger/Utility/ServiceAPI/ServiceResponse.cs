﻿using NLedger.Abstracts.Impl;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.ServiceAPI
{
    public class ServiceResponse
    {
        public ServiceResponse(ServiceSession serviceSession, string command)
        {
            if (serviceSession == null)
                throw new ArgumentNullException(nameof(serviceSession));
            if (String.IsNullOrWhiteSpace(command))
                throw new ArgumentNullException(nameof(command));

            ServiceSession = serviceSession;
            
            MainApplicationContext = ServiceSession.MainApplicationContext.Clone();
            MainApplicationContext.Logger = new Logger();
            MainApplicationContext.ErrorContext = new ErrorContext();

            InitializeResponse(command);
        }

        public ServiceSession ServiceSession { get; }
        public MainApplicationContext MainApplicationContext { get; }
        public int Status { get; private set; } = 1;
        public bool HasErrors => Status > 0;

        public string OutputText { get; private set; }
        public string ErrorText { get; private set; }

        private void InitializeResponse(string command)
        {
            using (MainApplicationContext.AcquireCurrentThread())
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
                        MainApplicationContext.SetVirtualConsoleProvider(() => new VirtualConsoleProvider(memoryStreamManager.ConsoleInput, memoryStreamManager.ConsoleOutput, memoryStreamManager.ConsoleError));

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
                    }
                }
            }
        }
    }
}

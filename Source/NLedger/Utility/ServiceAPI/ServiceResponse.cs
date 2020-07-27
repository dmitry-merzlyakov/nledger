using NLedger.Abstracts.Impl;
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

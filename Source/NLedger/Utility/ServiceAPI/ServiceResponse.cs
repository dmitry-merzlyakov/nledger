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
            
            //MainApplicationContext = ServiceSession.MainApplicationContext.Clone();
            InitializeResponse(command);
        }

        public ServiceSession ServiceSession { get; }
        public MainApplicationContext MainApplicationContext { get; }
        public int Status { get; private set; } = 1;

        private void InitializeResponse(string command)
        {
            using (MainApplicationContext.AcquireCurrentThread())
            {
                Status = ServiceSession.GlobalScope.ExecuteCommandWrapper(StringExtensions.SplitArguments(command), true);
            }
        }
    }
}

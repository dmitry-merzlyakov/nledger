using NLedger.Utility.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.ServiceAPI
{
    public sealed class ServiceEngine
    {
        public ServiceSession CreateSession(string args)
        {
            var context = new MainApplicationContext();
            new NLedgerConfiguration().ConfigureConsole(context);

            return new ServiceSession(context, CommandLine.PreprocessSingleQuotes(args));
        }
    }
}

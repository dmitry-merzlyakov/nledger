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
        public ServiceSession CreateSession(string args, string inputText)
        {
            //var configuration = new NLedgerConfiguration();
            var context = new MainApplicationContext();

            context.IsAtty = false;
            //context.TimeZone = configuration.TimeZoneId.Value;
            //context.DefaultPager = configuration.DefaultPager.Value;
            //context.SetEnvironmentVariables(configuration.SettingsContainer.VarSettings.EnvironmentVariables);

            return new ServiceSession(context, CommandLine.PreprocessSingleQuotes(args), inputText);
        }

        public Task<ServiceSession> CreateSessionAsync(string args, string inputText)
        {
            return Task.Run(() => CreateSession(args, inputText));
        }
    }
}

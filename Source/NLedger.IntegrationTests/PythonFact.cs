using NLedger.Extensibility.Python;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.IntegrationTests
{
    /// <summary>
    /// The fact requires NLedger Python connection configured. Otherwise, the test is skipped.
    /// </summary>
    public sealed class PythonFact : FactAttribute
    {
        public PythonFact()
        {
            if (!PythonHostConnector.HasConfiguration)
            {
                Skip = $"NLedger Python Extension is not configured (no file {PythonHostConnector.PythonHostSettingsName}). Test is skipped.";
            }

            var isFramework = AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName.Contains(".NETFramework");
            if (isFramework)
            {
                Skip = $"PythonNet has an initialization problem when it is run on .Net Framework in xUnit context. Python tests are ignored.";
            }
        }
    }
}

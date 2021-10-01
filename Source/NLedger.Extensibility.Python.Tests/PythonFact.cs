using NLedger.Extensibility.Python.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Extensibility.Python.Tests
{
    /// <summary>
    /// The fact requires NLedger Python connection configured. Otherwise, the test is skipped.
    /// </summary>
    public sealed class PythonFact : FactAttribute
    {
        public PythonFact()
        {
            if (!PythonConnector.Current.IsAvailable)
            {
                Skip = $"NLedger Python Extension is not configured (no file {XmlFilePythonConfigurationReader.DefaultFileName}). Test is skipped.";
            }
        }
    }
}

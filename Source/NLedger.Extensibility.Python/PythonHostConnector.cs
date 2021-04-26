using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python
{
    public class PythonHostConnector
    {
        public static PythonHostConnector Current => _Current.Value;

        public PythonHostConnector()
        {
            try
            {
                //PythonHostSettings = PythonHostSettings.Read(PythonHostSettingsName);
                //PythonHost = new PythonHost(PythonHostSettings);
                IsInitialized = true;
            }
            catch(Exception ex)
            {
                //Logger.Current.Debug("python", () => $"NLedger Python Extension is not configured (file '{PythonHostSettingsName}' not found or incorrect)");
            }
        }

        public bool IsInitialized { get; }
        public PythonHostSettings PythonHostSettings { get; }
        public PythonHost PythonHost { get; }

        private static readonly Lazy<PythonHostConnector> _Current = new Lazy<PythonHostConnector>(true);
    }
}

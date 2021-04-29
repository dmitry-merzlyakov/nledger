using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NLedger.Extensibility.Python
{
    /// <summary>
    /// PythonHostConnector serves connection to a current PythonHost instance that is expected to be single in AppDomain scope.
    /// </summary>
    public class PythonHostConnector
    {
        public static PythonHostConnector Current => _Current.Value;

        public static void Reconnect()
        {
            _Current = new Lazy<PythonHostConnector>(true);
        }

        public PythonHostConnector()
        {
            try
            {
                var pythonHostSettings = PythonHostSettings.Read(PythonHostSettingsName);
                PythonHost = new PythonHost(pythonHostSettings.PyHome, pythonHostSettings.PyPath, pythonHostSettings.PyDll);
            }
            catch(Exception ex)
            {
                InitializationError = ex;
                Logger.Current.Debug("python", () => $"NLedger Python Extension is not configured (file '{PythonHostSettingsName}' not found or incorrect)");
            }
        }

        public string PythonHostSettingsName => Path.GetFullPath($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/NLedger/NLedger.Extensibility.Python.settings.xml");
        public bool IsInitialized => PythonHost != null;

        public PythonHost PythonHost { get; }
        public Exception InitializationError { get; }

        private static Lazy<PythonHostConnector> _Current = new Lazy<PythonHostConnector>(true);
    }
}

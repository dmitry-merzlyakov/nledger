using NLedger.Extensibility.Python.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python
{
    public class PythonExtensionProvider : IExtensionProvider
    {
        public ExtendedSession CreateExtendedSession()
        {
            if (!PythonConnector.Current.IsAvailable)
                return null;

            return new PythonSession();
        }
    }
}

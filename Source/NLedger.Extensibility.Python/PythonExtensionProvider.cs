using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python
{
    public class PythonExtensionProvider : IExtensionProvider
    {
        public ExtendedSession CreateExtendedSession()
        {
            if (!PythonHostConnector.Current.IsInitialized)
                return null;

            return new PythonSession();
        }
    }
}

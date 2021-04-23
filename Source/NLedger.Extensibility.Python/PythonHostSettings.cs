using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python
{
    public class PythonHostSettings
    {
        public string PyHome { get; }
        public string[] PyPath { get; }
        public string PyNetRuntimeDll { get; }
        public string PyDll { get; }
        public string PyExecutable { get; }
    }
}

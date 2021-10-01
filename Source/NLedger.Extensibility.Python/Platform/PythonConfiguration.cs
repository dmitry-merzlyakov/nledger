using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python.Platform
{
    /// <summary>
    /// PythonNet configuration settings
    /// </summary>
    public class PythonConfiguration
    {
        /// <summary>
        /// Path to Python Home folder (the root folder for Python binaries that contain files like python38.dll etc)
        /// </summary>
        public string PyHome { get; set; }

        /// <summary>
        /// Collection of python search paths (the same that python's sys.path returns)
        /// </summary>
        public string[] PyPath { get; set; }

        /// <summary>
        /// Name of python core binary file that is located in python home, e.g. python38 (without extension)
        /// </summary>
        public string PyDll { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python.Platform
{
    /// <summary>
    /// PythonNet configuration reader representation
    /// </summary>
    public interface IPythonConfigurationReader
    {
        /// <summary>
        /// Indicates whether Python configuration is available
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Reads Python configuration and returns a configuration object
        /// </summary>
        PythonConfiguration Read();
    }
}

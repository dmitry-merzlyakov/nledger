using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml.Linq;

namespace NLedger.Extensibility.Python.Platform
{
    /// <summary>
    /// Python configuration reader that gets settings from an XML file
    /// </summary>
    public class XmlFilePythonConfigurationReader : IPythonConfigurationReader
    {
        /// <summary>
        /// Full path to the default Python settings XML file
        /// </summary>
        public static string DefaultFileName => Path.GetFullPath($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/NLedger/NLedger.Extensibility.Python.settings.xml");

        public XmlFilePythonConfigurationReader(string fileName = null)
        {
            FileName = String.IsNullOrWhiteSpace(fileName) ? DefaultFileName : fileName;
        }

        public string FileName { get; }

        public bool IsAvailable => File.Exists(FileName);

        public PythonConfiguration Read()
        {
            var xdoc = XDocument.Load(FileName);

            return new PythonConfiguration()
            {
                PyHome = xdoc.Descendants(XName.Get("py-home")).FirstOrDefault()?.Value,
                PyPath = xdoc.Descendants(XName.Get("py-path")).FirstOrDefault()?.Value.Split(';') ?? new string[0],
                PyDll = xdoc.Descendants(XName.Get("py-dll")).FirstOrDefault()?.Value
            };
        }
    }
}

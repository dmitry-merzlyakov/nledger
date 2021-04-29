using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml.Linq;

namespace NLedger.Extensibility.Python
{
    public class PythonHostSettings
    {
        public static PythonHostSettings Read(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (!File.Exists(fileName))
                throw new ArgumentException($"File {fileName} not found");

            var xdoc = XDocument.Load(fileName);
            var pyExecutable = xdoc.Descendants(XName.Get("py-executable")).FirstOrDefault();
            var pyHome = xdoc.Descendants(XName.Get("py-home")).FirstOrDefault();
            var pyPath = xdoc.Descendants(XName.Get("py-path")).FirstOrDefault();
            var pyDll = xdoc.Descendants(XName.Get("py-dll")).FirstOrDefault();
            var pyNetRuntimeDll = xdoc.Descendants(XName.Get("py-net-runtime-dll")).FirstOrDefault();

            return new PythonHostSettings(pyHome?.Value, pyPath?.Value?.Split(';') ?? new string[0], pyNetRuntimeDll?.Value, pyDll?.Value, pyExecutable?.Value);
        }

        public PythonHostSettings(string pyHome, string[] pyPath, string pyNetRuntimeDll, string pyDll, string pyExecutable)
        {
            if (String.IsNullOrWhiteSpace(pyHome))
                throw new ArgumentNullException(nameof(pyHome));
            if (String.IsNullOrWhiteSpace(pyNetRuntimeDll))
                throw new ArgumentNullException(nameof(pyNetRuntimeDll));
            if (String.IsNullOrWhiteSpace(pyDll))
                throw new ArgumentNullException(nameof(pyDll));
            if (String.IsNullOrWhiteSpace(pyExecutable))
                throw new ArgumentNullException(nameof(pyExecutable));

            PyHome = pyHome;
            PyPath = pyPath ?? throw new ArgumentNullException(nameof(pyPath));
            PyNetRuntimeDll = pyNetRuntimeDll;
            PyDll = pyDll;
            PyExecutable = pyExecutable;
        }

        public string PyHome { get; }
        public string[] PyPath { get; }
        public string PyNetRuntimeDll { get; }
        public string PyDll { get; }
        public string PyExecutable { get; }
    }
}

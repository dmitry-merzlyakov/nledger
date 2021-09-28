﻿using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NLedger.Extensibility.Python
{
    /// <summary>
    /// Manages PythonNet engine life cycle (initializing, functioning, disposing)
    /// </summary>
    public class PythonHost : IDisposable
    {
        /// <summary>
        /// Creates an instance of the host class and initializes Python Engine.
        /// </summary>
        /// <param name="pyHome">Path to Python Home folder (the root folder for Python binaries that contain files like python38.dll etc).</param>
        /// <param name="pyPath">Collection of python search paths (the same that python's sys.path returns).</param>
        /// <param name="pyDll">Name of python core binary file that is located in python home, e.g. python38 (without extension).</param>
        public PythonHost(string pyHome, IEnumerable<string> pyPath, string pyDll)
        {
            if (String.IsNullOrWhiteSpace(pyHome))
                throw new ArgumentNullException(nameof(pyHome));
            if (pyPath == null)
                throw new ArgumentNullException(nameof(pyPath));
            if (String.IsNullOrWhiteSpace(pyDll))
                throw new ArgumentNullException(nameof(pyDll));

            if (!Directory.Exists(pyHome))
                throw new ArgumentException($"Folder {pyHome} not found");

            var wrongPathItems = pyPath.Where(p => !Directory.Exists(p) && !File.Exists(p)).ToList();
            if (wrongPathItems.Any())
                throw new ArgumentException($"The following Python Path items were not found (neither a folder not a file exists): {String.Join(";", wrongPathItems)}");

            // Specifying Python core (always first step)

            Runtime.PythonDLL = pyDll;

            // Add Python Home to system path (if needed)

            var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            if (!path.StartsWith(pyHome))
                Environment.SetEnvironmentVariable("PATH", pyHome + ";" + path, EnvironmentVariableTarget.Process);

            // Add path to the current assembly to pyPath. It allows to load local application modules located in assembly folder

            var pyPathList = pyPath.ToList();
            pyPathList.Insert(0, Path.Combine(GetCurrentAssemblyPath(), "pymodules"));

            // Populating Python Home and Python Path settings

            var pyPathLine = String.Join(";", pyPathList);
            Environment.SetEnvironmentVariable("PYTHONHOME", pyHome, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH", pyPathLine, EnvironmentVariableTarget.Process);
            PythonEngine.PythonHome = pyHome;
            PythonEngine.PythonPath = pyPathLine;

            // Initialize Python

            PythonEngine.Initialize();
        }

        public void Dispose()
        {
            PythonEngine.Shutdown();
        }

        public static string GetCurrentAssemblyPath()
        {
            var uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Python.Runtime;
using System.Reflection;

namespace NLedger.Extensibility.Python.Platform
{
    /// <summary>
    /// PythonHost is Responsible for initialization and disposing PythonNet engine in the current application domain
    /// </summary>
    public class PythonHost : IDisposable
    {
        public PythonHost(PythonConfiguration pythonConfiguration, bool addPathToAssembly = true)
        {
            if (pythonConfiguration == null)
                throw new ArgumentNullException(nameof(pythonConfiguration));

            // Validate settings

            if (!Directory.Exists(pythonConfiguration.PyHome))
                throw new ArgumentException($"Folder {pythonConfiguration.PyHome} not found");

            var wrongPathItems = pythonConfiguration.PyPath?.Where(p => !Directory.Exists(p) && !File.Exists(p)).ToList() ?? Enumerable.Empty<string>();
            if (wrongPathItems.Any())
                throw new ArgumentException($"The following Python Path items were not found (neither a folder not a file exists): {String.Join(";", wrongPathItems)}");

            // Specifying Python core (always first step)

            Runtime.PythonDLL = pythonConfiguration.PyDll;

            // Add Python Home to system path (if needed)

            var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            if (!path.StartsWith(pythonConfiguration.PyHome))
                Environment.SetEnvironmentVariable("PATH", pythonConfiguration.PyHome + ";" + path, EnvironmentVariableTarget.Process);

            // Add path to the current assembly to pyPath. It allows to load local application modules located in assembly folder

            var pyPathList = (pythonConfiguration.PyPath ?? Enumerable.Empty<string>()).ToList();

            if (addPathToAssembly)
                pyPathList.Insert(0, Path.Combine(GetCurrentAssemblyPath(), "pymodules"));

            // Populating Python Home and Python Path settings

            var pyPathLine = String.Join(";", pyPathList);
            Environment.SetEnvironmentVariable("PYTHONHOME", pythonConfiguration.PyHome, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH", pyPathLine, EnvironmentVariableTarget.Process);
            PythonEngine.PythonHome = pythonConfiguration.PyHome;
            PythonEngine.PythonPath = pyPathLine;

            // Initialize Python

            PythonEngine.Initialize();
            ThreadState = PythonEngine.BeginAllowThreads();
        }

        public IntPtr ThreadState { get; }

        public void Dispose()
        {
            PythonEngine.EndAllowThreads(ThreadState);
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

// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
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
    /// PythonHost is responsible for initialization and disposing PythonNet engine in the current application domain
    /// </summary>
    public class PythonHost : IDisposable
    {
        public PythonHost(PythonConfiguration pythonConfiguration)
        {
            if (pythonConfiguration == null)
                throw new ArgumentNullException(nameof(pythonConfiguration));

            // Validate settings

            if (!File.Exists(pythonConfiguration.PyDll))
                throw new ArgumentException($"PyDll {pythonConfiguration.PyDll} not found");

            // Specifying path to LibPython

            Runtime.PythonDLL = pythonConfiguration.PyDll;

            // Initialize Python

            PythonEngine.Initialize();            

            // Add path to app module

            var appModulesPath = pythonConfiguration.AppModulesPath;
            if (!String.IsNullOrEmpty(appModulesPath))
            {
                using (Py.GIL())
                    PythonEngine.Exec($"import sys;sys.path.insert(0,'{appModulesPath.Replace(@"\", @"\\")}')");
            }

            // Enable thread management

            ThreadState = PythonEngine.BeginAllowThreads();
        }

        public IntPtr ThreadState { get; }

        public void Dispose()
        {
            PythonEngine.EndAllowThreads(ThreadState);
            PythonEngine.Shutdown();
        }
    }
}

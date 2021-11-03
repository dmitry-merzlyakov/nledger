// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
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

        /// <summary>
        /// Optional path to a folder containing application module(s).
        /// </summary>
        public string AppModulesPath { get; set; }
    }
}

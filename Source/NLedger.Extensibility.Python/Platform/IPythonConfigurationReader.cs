// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python.Platform
{
    /// <summary>
    /// PythonNet configuration reader provides a PythonConfiguration object
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

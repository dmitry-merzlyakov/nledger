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
    public enum EnvPythonConfigurationStatus
    {
        NotSpecified,
        Active,
        Disabled
    }

    /// <summary>
    /// Python configuration reader that tries to get data from environment variables.
    /// If variables are not specified, it forwards the request to the underlying reader.
    /// It basically allows to override existing XML settings in environment variables.
    /// </summary>
    public class EnvPythonConfigurationReader : IPythonConfigurationReader
    {
        public EnvPythonConfigurationReader(IPythonConfigurationReader basePythonConfigurationReader)
        {
            BasePythonConfigurationReader = basePythonConfigurationReader;

            EnvPythonConfigurationStatus status;
            if (Enum.TryParse(Environment.GetEnvironmentVariable("NLedgerPythonConnectionStatus"), out status))
            {
                Status = status;

                if (Status == EnvPythonConfigurationStatus.Active)
                {
                    PyHome = Environment.GetEnvironmentVariable("NLedgerPythonConnectionPyHome");
                    PyDll = Environment.GetEnvironmentVariable("NLedgerPythonConnectionPyDll");
                    PyPath = Environment.GetEnvironmentVariable("NLedgerPythonConnectionPyPath")?.Split(',');
                }
            }
            else
            {
                Status = EnvPythonConfigurationStatus.NotSpecified;
            }
        }

        public IPythonConfigurationReader BasePythonConfigurationReader { get; }

        public EnvPythonConfigurationStatus Status { get; }
        public string PyHome { get; }
        public string[] PyPath { get; }
        public string PyDll { get; }

        public bool IsAvailable
        {
            get
            {
                if (Status == EnvPythonConfigurationStatus.Disabled)
                    return false;

                if (Status == EnvPythonConfigurationStatus.Active)
                    return true;

                return BasePythonConfigurationReader?.IsAvailable ?? false;
            }
        }

        public PythonConfiguration Read()
        {
            if (Status == EnvPythonConfigurationStatus.Disabled)
                return null;

            if (Status == EnvPythonConfigurationStatus.Active)
                return new PythonConfiguration()
                {
                    PyHome = PyHome,
                    PyPath = PyPath,
                    PyDll = PyDll
                };

            return BasePythonConfigurationReader?.Read();
        }
    }
}

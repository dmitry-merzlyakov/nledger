// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Extensibility.Python.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Extensibility.Python.Tests.Platform
{
    public class EnvPythonConfigurationReaderTests : IDisposable
    {
        public void Dispose()
        {
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionStatus", String.Empty);
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionPyHome", String.Empty);
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionPyDll", String.Empty);
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionPyPath", String.Empty);
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionAppModulesPath", String.Empty);
        }

        public class TestPythonConfigurationReader : IPythonConfigurationReader
        {
            public bool IsAvailable { get; set; } = true;

            public string PyDll { get; set; } = "py-dll";
            public string AppModulesPath { get; set; } = "app-modules-path";

            public PythonConfiguration Read()
            {
                return new PythonConfiguration()
                {
                    PyDll = PyDll,
                    AppModulesPath = AppModulesPath
                };
            }
        }

        [Fact]
        public void EnvPythonConfigurationReader_Constructor_PopulatesProperties()
        {
            var baseReader = new TestPythonConfigurationReader();
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionStatus", "Active");
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionPyHome", "env-py-home");
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionPyDll", "env-py-dll");
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionPyPath", "path-1;path-2");
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionAppModulesPath", "app-modules-path");

            var envReader = new EnvPythonConfigurationReader(baseReader);

            Assert.Equal(baseReader, envReader.BasePythonConfigurationReader);
            Assert.Equal(EnvPythonConfigurationStatus.Active, envReader.Status);
            Assert.Equal("env-py-dll", envReader.PyDll);
            Assert.Equal("app-modules-path", envReader.AppModulesPath);
        }

        [Fact]
        public void EnvPythonConfigurationReader_IsAvailable_ReturnsFalseIfDisabled()
        {
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionStatus", "Disabled");
            var envReader = new EnvPythonConfigurationReader(null);
            Assert.Equal(EnvPythonConfigurationStatus.Disabled, envReader.Status);
            Assert.False(envReader.IsAvailable);
        }

        [Fact]
        public void EnvPythonConfigurationReader_IsAvailable_ReturnsTrueIfActive()
        {
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionStatus", "Active");
            var envReader = new EnvPythonConfigurationReader(null);
            Assert.Equal(EnvPythonConfigurationStatus.Active, envReader.Status);
            Assert.True(envReader.IsAvailable);
        }

        [Fact]
        public void EnvPythonConfigurationReader_IsAvailable_ReturnsBaseIfNotSpecified()
        {
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionStatus", "");
            var testReader = new TestPythonConfigurationReader();
            var envReader = new EnvPythonConfigurationReader(testReader);
            Assert.Equal(EnvPythonConfigurationStatus.NotSpecified, envReader.Status);

            testReader.IsAvailable = true;
            Assert.True(envReader.IsAvailable);

            testReader.IsAvailable = false;
            Assert.False(envReader.IsAvailable);

            envReader = new EnvPythonConfigurationReader(null);
            Assert.False(envReader.IsAvailable);
        }

        [Fact]
        public void EnvPythonConfigurationReader_Read_ReturnsNullIfDisabled()
        {
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionStatus", "Disabled");
            var envReader = new EnvPythonConfigurationReader(null);
            Assert.Null(envReader.Read());
        }

        [Fact]
        public void EnvPythonConfigurationReader_Read_ReturnsOwnConfigIfActive()
        {
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionStatus", "Active");
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionPyDll", "env-py-dll");
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionAppModulesPath", "env-app-modules-path");

            var envReader = new EnvPythonConfigurationReader(null);
            var config = envReader.Read();

            Assert.Equal("env-py-dll", config.PyDll);
            Assert.Equal("env-app-modules-path", config.AppModulesPath);
        }

        [Fact]
        public void EnvPythonConfigurationReader_Read_ReturnsBaseIfNotSpecified()
        {
            Environment.SetEnvironmentVariable("NLedgerPythonConnectionStatus", "");

            var envReader = new EnvPythonConfigurationReader(new TestPythonConfigurationReader());
            var config = envReader.Read();

            Assert.Equal("py-dll", config.PyDll);
            Assert.Equal("app-modules-path", config.AppModulesPath);

            envReader = new EnvPythonConfigurationReader(null);
            config = envReader.Read();
            Assert.Null(config);
        }

    }
}

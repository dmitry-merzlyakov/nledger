// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Extensibility.Python.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Extensibility.Python.Tests.Platform
{
    public class XmlFilePythonConfigurationReaderTests : IDisposable
    {

        public static readonly string XmlFilePythonConfigurationReaderTestFileContent = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<nledger-python-settings>
  <py-executable>py-executable-value</py-executable>
  <py-dll>py-dll-value</py-dll>
</nledger-python-settings>
".Trim();

        public XmlFilePythonConfigurationReaderTests()
        {
            FileName = Path.GetFullPath($"{Path.GetTempPath()}/XmlFilePythonConfigurationReaderTestFile.xml");
            File.WriteAllText(FileName, XmlFilePythonConfigurationReaderTestFileContent);
        }

        public void Dispose()
        {
            if (File.Exists(FileName))
                File.Delete(FileName);
        }

        public string FileName { get; }

        public class TestAppModuleResolver : IAppModuleResolver
        {
            public string AppModulePath { get; set; } = "some-path";
            public string GetAppModulePath() => AppModulePath;
        }

        [Fact]
        public void XmlFilePythonConfigurationReader_Constructore_PopulatesProperties()
        {
            var resolver = new TestAppModuleResolver();
            var reader = new XmlFilePythonConfigurationReader("some-file", resolver);
            Assert.Equal("some-file", reader.FileName);
            Assert.Equal(resolver, reader.AppModuleResolver);
        }

        [Fact]
        public void XmlFilePythonConfigurationReader_Constructore_BuildsProperties()
        {
            var reader = new XmlFilePythonConfigurationReader();
            Assert.Equal(XmlFilePythonConfigurationReader.DefaultFileName, reader.FileName);
            Assert.Null(reader.AppModuleResolver);
        }

        [Fact]
        public void XmlFilePythonConfigurationReader_IsAvailable_ReturnsFalseIfFileDoesNotExist()
        {
            var reader = new XmlFilePythonConfigurationReader("some-file-that-does-not-exist");
            Assert.False(reader.IsAvailable);
        }

        [Fact]
        public void XmlFilePythonConfigurationReader_IsAvailable_ReturnsTrueIfFileExists()
        {
            var reader = new XmlFilePythonConfigurationReader(FileName);
            Assert.True(reader.IsAvailable);
        }

        [Fact]
        public void XmlFilePythonConfigurationReader_Read_ReturnsConfig()
        {
            var reader = new XmlFilePythonConfigurationReader(FileName, new TestAppModuleResolver());
            var config = reader.Read();
            Assert.Equal("py-dll-value", config.PyDll);
            Assert.Equal("some-path", config.AppModulesPath);
        }

    }
}

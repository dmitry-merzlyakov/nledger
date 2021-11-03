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
  <py-home>py-home-value</py-home>
  <py-path>py-path1;py-path2</py-path>
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
            Assert.Equal("py-home-value", config.PyHome);
            Assert.Equal(new string[] { "py-path1", "py-path2" }, config.PyPath);
            Assert.Equal("py-dll-value", config.PyDll);
            Assert.Equal("some-path", config.AppModulesPath);
        }

    }
}

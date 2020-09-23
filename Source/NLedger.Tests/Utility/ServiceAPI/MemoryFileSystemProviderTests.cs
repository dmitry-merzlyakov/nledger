using NLedger.Utility.ServiceAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility.ServiceAPI
{
    public class MemoryFileSystemProviderTests
    {
        [Fact]
        public void MemoryFileSystemProvider_AppendAllText_CreatesANewFile()
        {
            var provider = new MemoryFileSystemProvider();

            provider.AppendAllText("input.dat", "test-content");

            Assert.True(provider.FileExists("input.dat"));
            Assert.Equal("test-content", provider.OpenText("input.dat").ReadToEnd());
        }

        [Fact]
        public void MemoryFileSystemProvider_AppendAllText_AddsNewContent()
        {
            var provider = new MemoryFileSystemProvider();
            provider.CreateFile("input.dat", "test-content");

            provider.AppendAllText("input.dat", "+test-content");

            Assert.True(provider.FileExists("input.dat"));
            Assert.Equal("test-content+test-content", provider.OpenText("input.dat").ReadToEnd());
        }

        [Fact]
        public void MemoryFileSystemProvider_CreateText_CreatesNewFileAndTextWriter()
        {
            var provider = new MemoryFileSystemProvider();

            using (var textWriter = provider.CreateText("input.dat"))
            {
                textWriter.Write("test-content");
                textWriter.Close();
            }

            Assert.True(provider.FileExists("input.dat"));
            Assert.Equal("test-content", provider.OpenText("input.dat").ReadToEnd());
        }

        [Fact]
        public void MemoryFileSystemProvider_DirectoryExists_ChecksWhetherDirectoryExists()
        {
            var provider = new MemoryFileSystemProvider();

            Assert.True(provider.DirectoryExists("/"));

            Assert.False(provider.DirectoryExists("home"));
            Assert.False(provider.DirectoryExists("/home"));
            provider.CreateFolder("home");
            Assert.True(provider.DirectoryExists("home"));
            Assert.True(provider.DirectoryExists("/home"));
        }

        [Fact]
        public void MemoryFileSystemProvider_FileExists_ChecksWhetherFileExists()
        {
            var provider = new MemoryFileSystemProvider();

            Assert.False(provider.FileExists("input.dat"));
            Assert.False(provider.FileExists("/input.dat"));
            provider.CreateFile("input.dat");
            Assert.True(provider.FileExists("input.dat"));
            Assert.True(provider.FileExists("/input.dat"));
        }

        [Fact]
        public void MemoryFileSystemProvider_GetCurrentDirectory_ReturnsCurrentFolder()
        {
            var provider = new MemoryFileSystemProvider();
            
            Assert.Equal("/", provider.GetCurrentDirectory());

            provider.CreateFolder("home");
            provider.SetCurrentDirectory("home");

            Assert.Equal("/home", provider.GetCurrentDirectory());
        }

        [Fact]
        public void MemoryFileSystemProvider_GetFileName_ReturnsNamePartOfPath()
        {
            var provider = new MemoryFileSystemProvider();
            Assert.Equal("input.dat", provider.GetFileName("input.dat"));
            Assert.Equal("input.dat", provider.GetFileName("/input.dat"));
            Assert.Equal("input.dat", provider.GetFileName("./input.dat"));
            Assert.Equal("input.dat", provider.GetFileName("/home/input.dat"));
            Assert.Equal("input.dat", provider.GetFileName("home/input.dat"));
        }

    }
}

// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility.ServiceAPI;
using System;
using System.Collections.Generic;
using System.IO;
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

        [Fact]
        public void MemoryFileSystemProvider_GetFiles_ReturnsCollectionOfFiles()
        {
            var provider = new MemoryFileSystemProvider();

            Assert.False(provider.GetFiles("/").Any());

            provider.CreateFile("input.dat");
            Assert.Single(provider.GetFiles("/"));
            Assert.Equal("/input.dat", provider.GetFiles("/").First());

            provider.CreateFolder("home");
            provider.CreateFile("/home/input1.dat");
            provider.CreateFile("/home/input2.dat");
            Assert.Equal(2, provider.GetFiles("/home").Count());
            Assert.Equal("/home/input1.dat", provider.GetFiles("/home").First());
            Assert.Equal("/home/input2.dat", provider.GetFiles("/home").Last());
        }

        [Fact]
        public void MemoryFileSystemProvider_GetFileSize_ReturnsSizeOfFile()
        {
            var provider = new MemoryFileSystemProvider();
            provider.CreateFile("input.dat", "1234567890");
            Assert.Equal(10, provider.GetFileSize("input.dat"));
        }

        [Fact]
        public void MemoryFileSystemProvider_GetFullPath_ReturnsAbolutePath()
        {
            var provider = new MemoryFileSystemProvider();

            Assert.Equal("/input.dat", provider.GetFullPath("input.dat"));
            Assert.Equal("/input.dat", provider.GetFullPath("./input.dat"));
            Assert.Equal("/input.dat", provider.GetFullPath("/input.dat"));

            provider.CreateFolder("home");
            provider.SetCurrentDirectory("home");

            Assert.Equal("/home/input.dat", provider.GetFullPath("input.dat"));
            Assert.Equal("/home/input.dat", provider.GetFullPath("./input.dat"));
            Assert.Equal("/home/input.dat", provider.GetFullPath("../home/input.dat"));
            Assert.Equal("/home/input.dat", provider.GetFullPath("/home/input.dat"));
        }

        [Fact]
        public void MemoryFileSystemProvider_GetLastWriteTimeUtc_ReturnsTimeOfLastModification()
        {
            var provider = new MemoryFileSystemProvider();
            provider.CreateFile("input.dat", "12345");
            Assert.True((provider.GetLastWriteTimeUtc("input.dat") - DateTime.UtcNow) < TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void MemoryFileSystemProvider_OpenRead_ReturnsStream()
        {
            var provider = new MemoryFileSystemProvider();
            provider.CreateFile("input.dat", "12345");

            using(var stream = provider.OpenRead("input.dat"))
            {
                var reader = new StreamReader(stream);
                string text = reader.ReadToEnd();
                Assert.Equal("12345", text);
            }
        }

        [Fact]
        public void MemoryFileSystemProvider_OpenText_ReturnsStreamReader()
        {
            var provider = new MemoryFileSystemProvider();
            provider.CreateFile("input.dat", "12345");

            using (var reader = provider.OpenText("input.dat"))
            {
                string text = reader.ReadToEnd();
                Assert.Equal("12345", text);
            }
        }

        [Fact]
        public void MemoryFileSystemProvider_PathCombine_CombinesTwoPaths()
        {
            var provider = new MemoryFileSystemProvider();

            Assert.Equal("/absolutepath", provider.PathCombine("something-that-will-be-ignored", "/absolutepath"));
            Assert.Equal("path1/path2", provider.PathCombine("path1", "path2"));
            Assert.Equal("path1/path2", provider.PathCombine("./path1", "./path2"));
            Assert.Equal("/path1/path2", provider.PathCombine("/path1", "path2"));
            Assert.Equal("/path1/path2", provider.PathCombine("/path1", "./path2"));
            Assert.Equal("path1/path2", provider.PathCombine("path1/subpath", "../path2"));
            Assert.Equal("/path1/path2", provider.PathCombine("/path1/subpath/..", "./path2"));
        }

        [Fact]
        public void MemoryFileSystemProvider_GetFolderPath_AlwaysReturnsRoot()
        {
            var provider = new MemoryFileSystemProvider();

            Assert.Equal("/", provider.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            Assert.Equal("/", provider.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            Assert.Equal("/", provider.GetFolderPath(Environment.SpecialFolder.Personal));
        }

        [Fact]
        public void MemoryFileSystemProvider_SetCurrentDirectory_ChangesCurrentFolder()
        {
            var provider = new MemoryFileSystemProvider();

            Assert.Equal("/", provider.GetCurrentDirectory());

            provider.CreateFolder("home");
            provider.SetCurrentDirectory("home");

            Assert.Equal("/home", provider.GetCurrentDirectory());
        }

        [Fact]
        public void MemoryFileSystemProvider_CreateFile_CreatesNewFile()
        {
            var provider = new MemoryFileSystemProvider();

            provider.CreateFile("input1.dat");
            provider.CreateFile("/input2.dat");

            provider.CreateFolder("home");
            provider.CreateFile("/home/input3.dat");

            provider.SetCurrentDirectory("home");
            provider.CreateFile("input4.dat");
            provider.CreateFile("/home/input5.dat");

            Assert.True(provider.FileExists("/input1.dat"));
            Assert.True(provider.FileExists("/input2.dat"));
            Assert.True(provider.FileExists("/home/input3.dat"));
            Assert.True(provider.FileExists("/home/input4.dat"));
            Assert.True(provider.FileExists("/home/input5.dat"));
        }

        [Fact]
        public void MemoryFileSystemProvider_CreateFile_CreatesNewFolder()
        {
            var provider = new MemoryFileSystemProvider();

            provider.CreateFolder("home");
            provider.CreateFolder("home/user1");
            provider.CreateFolder("/home/user2");

            provider.SetCurrentDirectory("home");
            provider.CreateFolder("user3");
            provider.CreateFolder("user3/data");

            Assert.True(provider.DirectoryExists("/home"));
            Assert.True(provider.DirectoryExists("/home/user1"));
            Assert.True(provider.DirectoryExists("/home/user2"));
            Assert.True(provider.DirectoryExists("/home/user3"));
            Assert.True(provider.DirectoryExists("/home/user3/data"));
        }

    }
}

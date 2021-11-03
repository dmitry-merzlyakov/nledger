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
    public class LocalResourceAppModuleResolverTests : IDisposable
    {
        public LocalResourceAppModuleResolverTests()
        {
            AppModulePath = Path.GetTempPath();
            AppModuleFile = Path.GetFullPath($"{AppModulePath}/ledger/__init__.py");

            if (File.Exists(AppModuleFile))
                File.Delete(AppModuleFile);
        }

        public void Dispose()
        {
            if (File.Exists(AppModuleFile))
                File.Delete(AppModuleFile);
        }

        public string AppModulePath { get; }
        public string AppModuleFile { get; }

        [Fact]
        public void LocalResourceAppModuleResolver_Constructor_PopulatesProperties()
        {
            var resolver = new LocalResourceAppModuleResolver("some-path");
            Assert.Equal("some-path", resolver.AppModulePath);
        }

        [Fact]
        public void LocalResourceAppModuleResolver_Constructor_BuildsDefaultPath()
        {
            var version = typeof(LocalResourceAppModuleResolver).Assembly.GetName().Version.ToString();
            var path = Path.GetFullPath($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/NLedger/PyModules/{version}");

            var resolver = new LocalResourceAppModuleResolver();
            Assert.Equal(path, resolver.AppModulePath);
        }

        [Fact]
        public void LocalResourceAppModuleResolver_GetAppModulePath_ReturnsPath()
        {
            var resolver = new LocalResourceAppModuleResolver(AppModulePath);
            Assert.Equal(AppModulePath, resolver.GetAppModulePath());
        }

        [Fact]
        public void LocalResourceAppModuleResolver_GetAppModulePath_CreatesFile()
        {
            Assert.False(File.Exists(AppModuleFile));
            new LocalResourceAppModuleResolver(AppModulePath).GetAppModulePath();
            Assert.True(File.Exists(AppModuleFile));

            var content = File.ReadAllText(AppModuleFile);
            using (var resource = typeof(LocalResourceAppModuleResolver).Assembly.GetManifestResourceStream("NLedger.Extensibility.Python.__init__.py"))
            {
                using (var reader = new StreamReader(resource))
                {
                    string result = reader.ReadToEnd();
                    Assert.Equal(result, content);
                }
            }
        }

        [Fact]
        public void LocalResourceAppModuleResolver_GetAppModulePath_CreatesFileOnce()
        {
            Assert.False(File.Exists(AppModuleFile));
            var resolver1 = new LocalResourceAppModuleResolver(AppModulePath);
            
            bool created = false;
            resolver1.OnFileCreated += () => created = true;

            // First attempt - creates file
            resolver1.GetAppModulePath();
            Assert.True(created);

            // Second attempt - file already exists
            created = false;
            resolver1.GetAppModulePath();
            Assert.False(created);

            // Create new resolver
            var resolver2 = new LocalResourceAppModuleResolver(AppModulePath);
            resolver2.OnFileCreated += () => created = true;

            // Second attempt - but file already exists
            created = false;
            resolver2.GetAppModulePath();
            Assert.False(created);
        }

        [Fact]
        public void LocalResourceAppModuleResolver_GetAppModulePath_RestoresChangedFile()
        {
            Assert.False(File.Exists(AppModuleFile));
            var resolver1 = new LocalResourceAppModuleResolver(AppModulePath);

            bool created = false;
            resolver1.OnFileCreated += () => created = true;

            resolver1.GetAppModulePath();
            Assert.True(created);

            File.WriteAllText(AppModuleFile, "some-content");

            // Create new resolver
            var resolver2 = new LocalResourceAppModuleResolver(AppModulePath);
            resolver2.OnFileCreated += () => created = true;

            // Second attempt - file already exists but changed
            created = false;
            resolver2.GetAppModulePath();
            Assert.True(created);

            // Second attempt - file already exists
            created = false;
            resolver2.GetAppModulePath();
            Assert.False(created);

        }
    }
}

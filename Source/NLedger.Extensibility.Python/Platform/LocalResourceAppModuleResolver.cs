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
using System.IO;
using System.Reflection;
using System.Text;

namespace NLedger.Extensibility.Python.Platform
{
    /// <summary>
    /// This AppModuleResolver is responsible to extract Ledger module from current assembly's embedded resources and put it to application data folder.
    /// </summary>
    public class LocalResourceAppModuleResolver : IAppModuleResolver
    {
        public LocalResourceAppModuleResolver(string appModulePath = null)
        {
            AppModulePath = appModulePath ?? BuildAppModulePath();
        }

        public string AppModulePath { get; }
        public event Action OnFileCreated;

        public string GetAppModulePath()
        {
            if (!IsPathValidated)
            {
                var targetFileName = Path.GetFullPath($"{AppModulePath}/ledger/__init__.py");
                var assemblyDate = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);

                if (!File.Exists(targetFileName) || assemblyDate != File.GetLastWriteTime(targetFileName))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));

                    using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("NLedger.Extensibility.Python.__init__.py"))
                    {
                        using (var file = new FileStream(targetFileName, FileMode.Create, FileAccess.Write))
                            resource.CopyTo(file);
                    }

                    File.SetLastWriteTime(targetFileName, assemblyDate);
                    OnFileCreated?.Invoke();
                }

                IsPathValidated = true;
            }

            return AppModulePath;
        }

        protected virtual string BuildAppModulePath()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return Path.GetFullPath($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/NLedger/PyModules/{version}");
        }

        private bool IsPathValidated { get; set; }
    }
}

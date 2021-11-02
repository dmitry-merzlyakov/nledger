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
using System.IO;
using System.Reflection;
using System.Text;

namespace NLedger.Extensibility.Python.Platform
{
    /// <summary>
    /// Application Modules Resolver is responsible for extracting Ledger module from embedded resources to a specific location (Local App Data folder),
    /// managing modules per assembly versions, checking module code consistency and restoring it if it detects issues.
    /// </summary>
    public class LocalResourceAppModuleResolver : IAppModuleResolver
    {
        public string GetAppModulePath()
        {
            if (String.IsNullOrEmpty(AppModulePath))
            {
                var targetFileName = ModuleInitFilePath();
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
                }

                AppModulePath = Path.GetDirectoryName(Path.GetDirectoryName(targetFileName));
            }

            return AppModulePath;
        }

        public string ModuleInitFilePath()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return Path.GetFullPath($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/NLedger/PyModules/{version}/ledger/__init__.py");
        }

        private string AppModulePath { get; set; }
    }
}

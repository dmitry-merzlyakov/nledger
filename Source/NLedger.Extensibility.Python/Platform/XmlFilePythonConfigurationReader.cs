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
using System.Text;
using System.Linq;
using System.Xml.Linq;

namespace NLedger.Extensibility.Python.Platform
{
    /// <summary>
    /// Python configuration reader that gets settings from an XML file
    /// </summary>
    public class XmlFilePythonConfigurationReader : IPythonConfigurationReader
    {
        /// <summary>
        /// Full path to the default Python settings XML file
        /// </summary>
        public static string DefaultFileName => Path.GetFullPath($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/NLedger/NLedger.Extensibility.Python.settings.xml");

        public XmlFilePythonConfigurationReader(string fileName = null, IAppModuleResolver appModuleResolver = null)
        {
            FileName = String.IsNullOrWhiteSpace(fileName) ? DefaultFileName : fileName;
            AppModuleResolver = appModuleResolver;
        }

        public string FileName { get; }

        public bool IsAvailable => File.Exists(FileName);

        public IAppModuleResolver AppModuleResolver { get; }

        public PythonConfiguration Read()
        {
            var xdoc = XDocument.Load(FileName);

            return new PythonConfiguration()
            {
                PyDll = xdoc.Descendants(XName.Get("py-dll")).FirstOrDefault()?.Value,
                AppModulesPath = AppModuleResolver?.GetAppModulePath()
            };
        }
    }
}

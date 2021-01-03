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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NLedger.Utility.Settings.CascadeSettings.Sources
{
    /// <summary>
    /// Configuration source based on an XML file
    /// </summary>
    /// <remarks>
    /// Read an XML file with any name on any location with a structure that is similar to a regular app.config.
    /// Expected elements: configuration, appSettings, add(with key and value attributes).
    /// </remarks>
    public class ConfigurationFileSettingsSource : ISettingsSource
    {
        /// <summary>
        /// Creates an instance of the source
        /// </summary>
        /// <param name="filePath">Absolute or relative path to the file. File can not exist.</param>
        public ConfigurationFileSettingsSource(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            FilePath = filePath;
        }

        public string FilePath { get; private set; }

        public bool FileExists
        {
            get { return File.Exists(FilePath); }
        }

        public SettingScopeEnum Scope
        {
            get { return SettingScopeEnum.User; }
        }

        public string GetValue(string key)
        {
            EnsureSettings();

            if (Settings != null)
            {
                string value;
                if (Settings.TryGetValue(key, out value))
                    return value;
            }

            return null;
        }

        private void EnsureSettings()
        {
            if (IsInitialized)
                return;

            IsInitialized = true;

            if (!FileExists)
                return;

            var xml = XDocument.Load(FilePath);
            var adds = xml.Root.Element("appSettings")?.Elements("add");
            if (adds != null)
                Settings = adds.ToDictionary(xe => xe.Attribute("key").Value, xe => xe.Attribute("value").Value);
        }

        private bool IsInitialized = false;
        private IDictionary<string, string> Settings { get; set; }
    }
}

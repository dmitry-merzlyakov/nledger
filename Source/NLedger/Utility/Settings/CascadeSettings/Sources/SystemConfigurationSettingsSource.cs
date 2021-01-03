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
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Settings.CascadeSettings.Sources
{
    /// <summary>
    /// Returns app setting values specifies in app.config
    /// </summary>
    public sealed class SystemConfigurationSettingsSource : ISettingsSource
    {
        public static string AppConfigFileName
        {
            get { return _AppConfigFileName ?? GetAppConfigFileName(); }
            set { _AppConfigFileName = value; }
        }

        public SettingScopeEnum Scope
        {
            get { return SettingScopeEnum.Application; }
        }

        public string GetValue(string key)
        {
            return Source.Value.GetValue(key);
        }

        private readonly Lazy<ConfigurationFileSettingsSource> Source = new Lazy<ConfigurationFileSettingsSource>(() => new ConfigurationFileSettingsSource(AppConfigFileName));

        private static string GetAppConfigFileName()
        {
            return System.Reflection.Assembly.GetEntryAssembly().Location + ".config";
        }

        private static string _AppConfigFileName = null;
    }
}

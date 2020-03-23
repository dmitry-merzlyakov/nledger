// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Settings.CascadeSettings.Sources
{
    /// <summary>
    /// The source that manages a collection of default values provided by setting definitions
    /// </summary>
    public sealed class DefaultSettingsSource : ISettingsSource
    {
        /// <summary>
        /// Creates a source instance
        /// </summary>
        /// <param name="definitions">Collection of setting definitions</param>
        public DefaultSettingsSource(IEnumerable<ISettingDefinition> definitions)
        {
            if (definitions == null)
                throw new ArgumentNullException("definitions");

            SettingDefinitions = definitions.ToDictionary(d => d.Name, d => d);
        }

        public IDictionary<string,ISettingDefinition> SettingDefinitions { get; private set; }

        public SettingScopeEnum Scope
        {
            get { return SettingScopeEnum.Application; }
        }

        public string GetValue(string key)
        {
            ISettingDefinition settingDefinition;
            if (SettingDefinitions.TryGetValue(key, out settingDefinition))
                return settingDefinition.DefaultValue;

            return null;
        }
    }
}

// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Settings.CascadeSettings.Sources
{
    /// <summary>
    /// Source that returns app settings from environment variables
    /// </summary>
    public sealed class EnvironmentVariablesSettingsSource : ISettingsSource
    {
        /// <summary>
        /// Creates an instance of the source.
        /// </summary>
        /// <param name="namePrefix">Optinal filtering prefix for setting names</param>
        public EnvironmentVariablesSettingsSource(string namePrefix = null)
        {
            NamePrefix = namePrefix;

            var variables = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().
                Select(de => new KeyValuePair<string,string>(de.Key.ToString(), de.Value.ToString()));

            EnvironmentVariables = variables.ToDictionary(d => d.Key, d => d.Value);

            if (String.IsNullOrEmpty(NamePrefix))
                EffectiveVariables = EnvironmentVariables;
            else
                EffectiveVariables = variables.Where(d => d.Key.StartsWith(NamePrefix)).
                    ToDictionary(d => d.Key.Substring(NamePrefix.Length), d => d.Value);
        }

        public IDictionary<string, string> EnvironmentVariables { get; private set; }
        public IDictionary<string, string> EffectiveVariables { get; private set; }
        public string NamePrefix { get; private set; }

        public SettingScopeEnum Scope
        {
            get { return SettingScopeEnum.Application; }
        }

        public string GetValue(string key)
        {
            string value;
            if (EffectiveVariables.TryGetValue(key, out value))
                return value;

            return null;
        }
    }
}

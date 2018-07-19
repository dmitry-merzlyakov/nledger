// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Settings.CascadeSettings
{
    /// <summary>
    /// Container for all types of setting sources that are managed in proper order
    /// </summary>
    public class CascadeSettingsContainer
    {
        /// <summary>
        /// Collection of setting sources. If more than one source has the same key, the latter has the highest priority.
        /// </summary>
        public IList<ISettingsSource> Sources { get; private set; } = new List<ISettingsSource>();

        /// <summary>
        /// Specifies the level of setting scope. "User" indicates that all sources are observed for settings;
        /// "Application" indicates that only the sources with "Application" scope are observed. 
        /// </summary>
        public SettingScopeEnum EffectiveScope { get; set; } = SettingScopeEnum.User;

        /// <summary>
        /// Getting an effective (the most relevant) setting value.
        /// </summary>
        /// <remarks>
        /// This method observes all sources starting from the last one (which has highest priority)
        /// till a source returns a not-null value for given key.
        /// </remarks>
        /// <param name="key">The setting key</param>
        /// <param name="scope">The scope for searching a setting value. "User" indicates that 
        /// all sources are observed; "Application" means that only "Application" sources are observed.
        /// Default value is "User".
        /// </param>
        /// <returns>The effective setting value or null if none of sources contains given key.</returns>
        public string GetEffectiveValue(string key, SettingScopeEnum scope = SettingScopeEnum.User)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (EffectiveScope < scope)
                scope = EffectiveScope;

            for (int i=Sources.Count-1; i>=0; i--)
            {
                var source = Sources[i];
                if (scope < source.Scope)
                    continue;

                var value = source.GetValue(key);
                if (value != null)
                    return value;
            }

            return null;
        }

        /// <summary>
        /// Adds a source to the collection and returns the instance
        /// </summary>
        /// <typeparam name="T">Type of the source</typeparam>
        /// <param name="source">Instance of the source</param>
        /// <returns>The same instance of the source.</returns>
        public T AddSource<T>(T source) where T : ISettingsSource
        {
            if (source == null)
                throw new ArgumentNullException("source");

            Sources.Add(source);
            return source;
        }
    }
}

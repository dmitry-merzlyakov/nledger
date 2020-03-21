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

namespace NLedger.Utility.Settings.CascadeSettings
{
    /// <summary>
    /// Represents any source for application settings
    /// </summary>
    public interface ISettingsSource
    {
        /// <summary>
        /// Returns a setting value for given key
        /// </summary>
        /// <param name="key">The setting key</param>
        /// <returns>Returns a setting or an empty value. If setting is not presented, returns Null.</returns>
        string GetValue(string key);

        /// <summary>
        /// Specifies a scope for given collection of settings
        /// </summary>
        /// <remarks>
        /// "User" means that the source contains user-level settings. If the container is set to "Application",
        /// this source is ignored. 
        /// "Application" means that the source contains application-level settings that are never ignored.
        /// </remarks>
        SettingScopeEnum Scope { get; }
    }
}

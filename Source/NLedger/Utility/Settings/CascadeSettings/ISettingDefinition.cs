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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Settings.CascadeSettings
{
    /// <summary>
    /// Defines an application setting
    /// </summary>
    public interface ISettingDefinition
    {
        /// <summary>
        /// The setting name as it appears in a configuration file
        /// </summary>
        string Name { get; }

        /// <summary>
        /// User-friendly description for the setting
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Default value
        /// </summary>
        string DefaultValue { get; }

        /// <summary>
        /// Returns a collection of possible values (if applicable)
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetValues();

        /// <summary>
        /// Specifies a scope for the setting.
        /// Application-level scope indicates that the setting cannot be overriden in user configuration files
        /// User-level scope implies that the setting can be changed anywhere
        /// </summary>
        SettingScopeEnum Scope { get; }

        /// <summary>
        /// Returns the type of the setting values
        /// </summary>
        Type SettingType { get; }
    }
}

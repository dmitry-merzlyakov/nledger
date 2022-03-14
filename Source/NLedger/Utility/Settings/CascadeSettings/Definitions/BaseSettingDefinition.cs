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

namespace NLedger.Utility.Settings.CascadeSettings.Definitions
{
    /// <summary>
    /// Base class for a setting definition
    /// </summary>
    /// <typeparam name="T">Setting type</typeparam>
    public abstract class BaseSettingDefinition<T> : ISettingDefinition
    {
        public BaseSettingDefinition(string name, string description, T defaultValue, SettingScopeEnum scope)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            Name = name;
            Description = description;
            DefaultValue = ConvertToString(defaultValue);
            Scope = scope;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public string DefaultValue { get; private set; }
        public SettingScopeEnum Scope { get; private set; }

        public Type SettingType
        {
            get { return typeof(T); }
        }

        public virtual IEnumerable<string> GetValues()
        {
            return Enumerable.Empty<string>();
        }

        public virtual string ConvertToString(T value)
        {
            return value.ToString();
        }

        public abstract T ConvertFromString(string stringValue);
    }
}

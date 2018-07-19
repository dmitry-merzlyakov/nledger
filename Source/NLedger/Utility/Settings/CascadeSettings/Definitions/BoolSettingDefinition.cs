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

namespace NLedger.Utility.Settings.CascadeSettings.Definitions
{
    /// <summary>
    /// Defines a boolean setting
    /// </summary>
    public sealed class BoolSettingDefinition : BaseSettingDefinition<bool>
    {
        public BoolSettingDefinition(string name, string description, bool value, SettingScopeEnum scope = SettingScopeEnum.User)
            : base (name, description, value, scope)
        { }

        public override IEnumerable<string> GetValues()
        {
            return new string[] { bool.FalseString, bool.TrueString };
        }

        public override bool ConvertFromString(string stringValue)
        {
            return bool.Parse(stringValue);
        }
    }
}

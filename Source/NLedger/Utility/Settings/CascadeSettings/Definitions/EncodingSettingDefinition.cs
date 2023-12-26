// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
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
    /// Defines an Encoding setting
    /// </summary>
    public sealed class EncodingSettingDefinition : BaseSettingDefinition<Encoding>
    {
        public EncodingSettingDefinition(string name, string description, Encoding value, SettingScopeEnum scope = SettingScopeEnum.User)
            : base (name, description, value, scope)
        { }

        public override IEnumerable<string> GetValues()
        {
            return Encoding.GetEncodings().Select(encodingInfo => encodingInfo.Name);
        }

        public override string ConvertToString(Encoding value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return value.HeaderName;
        }

        public override Encoding ConvertFromString(string stringValue)
        {
            return Encoding.GetEncoding(stringValue);
        }

    }
}

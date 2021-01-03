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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Settings.CascadeSettings.Definitions
{
    public sealed class TimeZoneSettingDefinition : BaseSettingDefinition<TimeZoneInfo>
    {
        public TimeZoneSettingDefinition(string name, string description, TimeZoneInfo value, SettingScopeEnum scope = SettingScopeEnum.User)
            : base (name, description, value, scope)
        { }

        public override IEnumerable<string> GetValues()
        {
            return TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id);
        }

        public override string ConvertToString(TimeZoneInfo value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return value.Id;
        }

        public override TimeZoneInfo ConvertFromString(string stringValue)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(stringValue);
        }

    }
}

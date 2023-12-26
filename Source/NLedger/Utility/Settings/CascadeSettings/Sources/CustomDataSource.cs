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

namespace NLedger.Utility.Settings.CascadeSettings.Sources
{
    /// <summary>
    /// Simple setting source that only return data that are in its dictionary.
    /// </summary>
    /// <remarks>
    /// It does not retrieve data on its own but expect a developer to put it in advance.
    /// </remarks>
    public class CustomDataSource : ISettingsSource
    {
        public IDictionary<string, string> Data { get; private set; } = new Dictionary<string, string>();
        public SettingScopeEnum Scope { get; set; } = SettingScopeEnum.User;

        public string GetValue(string key)
        {
            string value;
            if (Data.TryGetValue(key, out value))
                return value;

            return null;
        }
    }
}

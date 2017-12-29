// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.CLI
{
    public sealed class AppConfiguration
    {
        public AppConfiguration()
        {
            IsAtty = GetBool("IsAtty", true);
            TimeZoneId = GetString("TimeZoneId", null);
            OutputEncoding = GetEncoding("OutputEncoding", Encoding.Default);
            IsAnsiTerminalEmulation = GetBool("AnsiTerminalEmulation", true);
        }

        public bool IsAtty { get; private set; }
        public string TimeZoneId { get; private set; }
        public Encoding OutputEncoding { get; private set; }
        public bool IsAnsiTerminalEmulation { get; private set; }

        private bool GetBool(string key, bool defaultValue)
        {
            string value = GetString(key, defaultValue.ToString());

            bool boolValue;
            if (!bool.TryParse(value, out boolValue))
                throw new InvalidOperationException(String.Format("Key '{0}' has a value that cannot be converted to Boolean ({1}) ", key, value));

            return boolValue;
        }

        private string GetString(string key, string defaultValue)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");

            return Environment.GetEnvironmentVariable("nledger" + key) ?? 
                ConfigurationManager.AppSettings[key] ?? defaultValue;
        }

        private Encoding GetEncoding(string key, Encoding defaultValue)
        {
            string value = GetString(key, null);
            if (String.IsNullOrWhiteSpace(value))
                return defaultValue;

            return Encoding.GetEncoding(value);
        }
    }
}

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
    /// Specifies a scope for a setting or a collection of settings
    /// </summary>
    public enum SettingScopeEnum
    {
        /// <summary>
        /// Application-level scope
        /// </summary>
        Application = 0,

        /// <summary>
        /// User-level scope; can be changes in common/user configuration files
        /// </summary>
        User = 1
    }
}

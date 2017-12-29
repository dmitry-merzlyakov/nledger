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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public class Validator
    {
        public static bool IsVerifyEnabled { get; set; }

        public static void Verify(bool result)
        {
            if (IsVerifyEnabled && !result)
                throw new InvalidOperationException("Validation failed");
        }

        public static void Verify(Func<bool> resultFunc)
        {
            if (resultFunc == null)
                throw new ArgumentNullException("resultFunc");

            if (IsVerifyEnabled && !resultFunc())
                throw new InvalidOperationException("Validation failed");
        }

    }
}

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

namespace NLedger.Utility
{
    public sealed class FileNameComparer : IEqualityComparer<string>
    {
        public static readonly FileNameComparer Instance = new FileNameComparer();

        public bool Equals(string x, string y)
        {
            if (String.IsNullOrWhiteSpace(x))
                return String.IsNullOrWhiteSpace(y);

            if (String.IsNullOrWhiteSpace(y))
                return String.IsNullOrWhiteSpace(x);

            var fullX = FileSystem.ResolvePath(x);
            var fullY = FileSystem.ResolvePath(y);

            return String.Equals(fullX, fullY, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            if (obj == null)
                return 0;

            return obj.GetHashCode();
        }
    }
}

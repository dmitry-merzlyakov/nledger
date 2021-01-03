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

namespace NLedger.Utility
{
    public class UniString
    {
        public static string Justify(string str, int width, bool right = false, bool redden = false)
        {
            StringBuilder sb = new StringBuilder();

            if (!right)
            {
                if (redden)
                    sb.Append(AnsiTextWriter.ForegroundColorRed);
                sb.Append(str);
                if (redden)
                    sb.Append(AnsiTextWriter.NormalColor);
            }

            int spacing = width - str.Length;
            while (spacing-- > 0)
                sb.Append(" ");

            if (right)
            {
                if (redden)
                    sb.Append(AnsiTextWriter.ForegroundColorRed);
                sb.Append(str);
                if (redden)
                    sb.Append(AnsiTextWriter.NormalColor);
            }

            return sb.ToString();
        }
    }
}

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

namespace NLedger.Utility
{
    public sealed class CommandLine
    {
        public static IEnumerable<string> PreprocessSingleQuotes(IEnumerable<string> args)
        {
            return PreprocessSingleQuotes(String.Join(" ", args ?? Enumerable.Empty<string>()).Trim());
        }

        public static IEnumerable<string> PreprocessSingleQuotes(string args)
        {
            args = args ?? String.Empty;
            if (!String.IsNullOrWhiteSpace(args))
            {
                var sb = new StringBuilder();
                int pos = 0;
                char quote = Char.MinValue;

                while (pos < args.Length)
                {
                    var current = args[pos++];

                    if (current == quote)
                    {
                        quote = Char.MinValue;
                    }
                    else if ((current == '\"' || current == '\'') && quote == Char.MinValue)
                    {
                        quote = current;
                    }
                    else if (current == ' ' && quote == Char.MinValue)
                    {
                        if (sb.Length > 0)
                        {
                            yield return sb.ToString();
                            sb.Clear();
                        }
                    }
                    else
                    {
                        sb.Append(current);
                    }
                }

                if (sb.Length > 0)
                    yield return sb.ToString();
            }
        }

    }
}

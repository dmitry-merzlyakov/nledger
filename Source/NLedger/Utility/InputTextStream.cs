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
    public class InputTextStream
    {
        public const char EndOfFileChar = char.MaxValue;

        public InputTextStream(string source)
        {
            Source = source ?? String.Empty;
        }

        public string Source { get; private set; }
        public int Pos { get; set; }

        public char Peek
        {
            get { return !Eof ? Source[Pos] : EndOfFileChar; }
        }

        public bool Eof
        {
            get { return Pos < 0 || Pos >= Source.Length; }
        }

        public string RemainSource
        {
            get { return Eof ? string.Empty : Source.Substring(Pos); }
        }

        public char Get()
        {
            if (!Eof)
                return Source[Pos++];
            else
                return EndOfFileChar;
        }

        public void Unget()
        {
            if (Pos > 0)
                Pos--;
        }

        public char PeekNextNonWS()
        {
            char c = Peek;
            while (!Eof && Char.IsWhiteSpace(c))
            {
                Get();
                c = Peek;
            }
            return c;
        }

        public int ReadInto(out string str, out char c, Func<char,bool> condition = null)
        {
            int length = 0;
            c = Peek;
            StringBuilder sb = new StringBuilder();
            while (!Eof && c != '\n' && (condition == null || condition(c)))
            {
                c = Get();
                /// if (Eof)
                ///   break;
                length++;
                if (c == '\\'  && !Eof)
                {
                    c = Get();
                    ///if (Eof)
                    ///    break;
                    switch(c)
                    {
                        case 'b': c = '\b'; break;
                        case 'f': c = '\f'; break;
                        case 'n': c = '\n'; break;
                        case 'r': c = '\r'; break;
                        case 't': c = '\t'; break;
                        case 'v': c = '\v'; break;
                        default: break;
                    }
                    length++;
                }
                sb.Append(c);
                c = Peek;
            }
            str = sb.ToString();
            return length;
        }

        public int ReadInt(int defaultValue, out char nextChar)
        {
            if (!Eof && Char.IsDigit(Peek))
            {
                StringBuilder sb = new StringBuilder(Get().ToString());
                while (!Eof && Char.IsDigit(Peek))
                    sb.Append(Get());
                nextChar = Peek;
                return Int32.Parse(sb.ToString());
            }
            else
            {
                nextChar = Peek;
                return defaultValue;
            }
        }
    }
}

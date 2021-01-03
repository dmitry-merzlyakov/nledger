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

namespace NLedger.Times
{
    public enum NetDateTimeFormat
    {
        ParseFormat,
        PrintFormat
    }

    // Converts ctime format to .Net format
    // Note: std::strftime - http://en.cppreference.com/w/cpp/chrono/c/strftime
    // %d - writes day of the month as a decimal number (range [01,31]) 
    // %m - writes month as a decimal number (range [01,12]) 
    // %b - writes abbreviated month name, e.g. Oct (locale dependent) 
    // %y - writes year as a 4 digit decimal number
    // Corresponded .Net formats - https://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx
    public class CTimeToNetFormatConverter
    {
        public static string ConvertCTimeToNet(string ctime, NetDateTimeFormat netDateTimeFormat)
        {
            return new CTimeToNetFormatConverter().Convert(ctime, netDateTimeFormat);
        }

        public bool IsMarker { get; private set; }
        public bool IsQuote { get; private set; }

        public string Convert(string cTimeFormat, NetDateTimeFormat netDateTimeFormat)
        {
            if (String.IsNullOrWhiteSpace(cTimeFormat))
                return cTimeFormat;

            foreach(char c in cTimeFormat)
            {
                if (c == MarkerChar)
                {
                    if (IsMarker)
                    {
                        OpenQuotes();
                        Builder.Append(MarkerChar);
                        IsMarker = false;
                    }
                    else
                    {
                        CloseQuotes();
                        IsMarker = true;
                    }
                }
                else
                {
                    if (IsMarker)
                    {
                        Tuple<string,string> netFormat;
                        if (NetFormats.TryGetValue(c, out netFormat))
                        {
                            CloseQuotes();
                            Builder.Append(
                                netDateTimeFormat == NetDateTimeFormat.ParseFormat 
                                    ? netFormat.Item1 
                                    : netFormat.Item2);
                        }
                        else
                        {
                            Builder.Append(MarkerChar);
                            Builder.Append(c);
                        }

                        IsMarker = false;
                    }
                    else
                    {
                        OpenQuotes();
                        Builder.Append(c);
                    }
                }
            }

            CloseQuotes();

            return Builder.ToString();
        }

        public void OpenQuotes()
        {
            if (!IsQuote)
            {
                Builder.Append(QuoteChar);
                IsQuote = true;
            }
        }

        public void CloseQuotes()
        {
            if (IsQuote)
            {
                Builder.Append(QuoteChar);
                IsQuote = false;
            }
        }

        public override string ToString()
        {
            return Builder.ToString();
        }

        // Dictionary key is ctime format symbol; value is a pair of .Net format symbols (parse and print)
        private static IDictionary<char, Tuple<string,string>> NetFormats = new Dictionary<char, Tuple<string, string>>()
        {
            { 'Y', new Tuple<string, string> ("yyyy", "yyyy") },
            { 'y', new Tuple<string, string> ("yy",   "yy") },
            { 'm', new Tuple<string, string> ("M",    "MM") },  // Parsing should handle possible leading zeros; printing always produces two digits
            { 'b', new Tuple<string, string> ("MMM",  "MMM") },
            { 'd', new Tuple<string, string> ("d",    "dd") },  // Parsing should handle possible leading zeros; printing always produces two digits
            { 'H', new Tuple<string, string> ("HH",   "HH") },
            { 'M', new Tuple<string, string> ("mm",   "mm") },
            { 'S', new Tuple<string, string> ("ss",   "ss") },
            { 'I', new Tuple<string, string> ("hh",   "hh") },
            { 'p', new Tuple<string, string> ("tt",   "tt") },
            { 'A', new Tuple<string, string> ("dddd", "dddd") },
            { 'F', new Tuple<string, string> ("yyyy-M-d", "yyyy-MM-dd") },  // equivalent to "%Y-%m-%d" (the ISO 8601 date format) 
        };

        private const char MarkerChar = '%';
        private const char QuoteChar = '\'';

        private readonly StringBuilder Builder = new StringBuilder();
    }
}

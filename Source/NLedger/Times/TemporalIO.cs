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

namespace NLedger.Times
{
    public abstract class TemporalIO<T> where T : struct
    {
        public TemporalIO(string fmtStr, bool input)
        {
            SetFormat(fmtStr);
            Input = input;
        }

        public DateTraits Traits { get; private set; }
        public bool Input { get; private set; }

        public void SetFormat(string fmt)
        {
            FmtStr = fmt;
            Traits = new DateTraits(fmt.Contains("%y") || fmt.Contains("%Y"), fmt.Contains("%m") || fmt.Contains("%b"), fmt.Contains("%d"));
            ParseDotNetFmtStr = CTimeToNetFormatConverter.ConvertCTimeToNet(FmtStr, NetDateTimeFormat.ParseFormat);
            PrintDotNetFmtStr = CTimeToNetFormatConverter.ConvertCTimeToNet(FmtStr, NetDateTimeFormat.PrintFormat);
        }

        public abstract T Parse(string str);

        public abstract string Format(T when);

        protected string ParseDotNetFmtStr { get; set; }
        protected string PrintDotNetFmtStr { get; set; }

        private string FmtStr { get; set; }
    }
}

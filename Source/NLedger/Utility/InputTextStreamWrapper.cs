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
    public sealed class InputTextStreamWrapper : IDisposable
    {
        public InputTextStreamWrapper(InputTextStream inputTextStream)
        {
            if (inputTextStream == null)
                throw new ArgumentNullException("inputTextStream");

            InputTextStream = inputTextStream;
            Source = inputTextStream.RemainSource;
        }

        public string Source;

        public void Dispose()
        {
            if (String.IsNullOrEmpty(Source))
            {
                InputTextStream.Pos = InputTextStream.Source.Length;
                return;
            }

            if (!InputTextStream.Source.EndsWith(Source))
                throw new InvalidOperationException("Parsing logic violation");

            InputTextStream.Pos = InputTextStream.Source.Length - Source.Length;
        }

        private InputTextStream InputTextStream { get; set; }
    }
}

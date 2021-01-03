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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    /// <summary>
    /// Helper class to write XML in proper encoding.
    /// See http://stackoverflow.com/questions/14743310/how-to-change-encoding-in-textwriter-object
    /// </summary>
    public class EncodingStringWriter : StringWriter
    {
        public EncodingStringWriter(Encoding encoding)
            : base()
        {
            _encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return _encoding; }
        }

        private readonly Encoding _encoding;
    }
}

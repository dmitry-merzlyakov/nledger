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
using System.IO;
using NLedger.Utility;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    public class TextualReader : ITextualReader
    {
        public TextualReader(StreamReader streamReader)
        {
            if (streamReader == null)
                throw new ArgumentNullException("streamReader");

            StreamReader = streamReader;
        }

        public StreamReader StreamReader { get; private set; }

        public bool IsEof()
        {
            return StreamReader.EndOfStream;
        }

        public long Position
        {
            get { return StreamReader.GetPosition(); }
        }

        public string ReadLine()
        {
            return (StreamReader.ReadLine() ?? String.Empty).TrimEnd();
        }

        public bool PeekWhitespaceLine()
        {
            var ch = (char)StreamReader.Peek();
            return !IsEof() && (ch == ' ' || ch == '\t');
        }

        public void Dispose()
        {
            if (StreamReader != null)
                StreamReader.Dispose();
        }
    }
}

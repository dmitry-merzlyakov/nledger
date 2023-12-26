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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests
{
    public class TextualReaderTests : TestFixture
    {
        [Fact]
        public void TextualReader_PeekWhitespaceLine_TrueIfIsNotEndOfFile()
        {
            using (var sr = new StreamReader(new MemoryStream()))
            {
                var reader = new TextualReader(sr);
                Assert.False(reader.PeekWhitespaceLine());
            }
        }

        [Fact]
        public void TextualReader_PeekWhitespaceLine_TrueIfIsSpaceOrTabAndFalseOtherwise()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new StreamWriter(ms);
                writer.WriteLine("ABC");
                writer.WriteLine(" DE");
                writer.WriteLine("\tF");
                writer.WriteLine("");
                writer.WriteLine("XYZ");
                writer.Flush();
                ms.Position = 0;

                var reader = new TextualReader(new StreamReader(ms));
                Assert.False(reader.PeekWhitespaceLine()); reader.ReadLine();
                Assert.True(reader.PeekWhitespaceLine()); reader.ReadLine();
                Assert.True(reader.PeekWhitespaceLine()); reader.ReadLine();
                Assert.False(reader.PeekWhitespaceLine()); reader.ReadLine();
                Assert.False(reader.PeekWhitespaceLine()); reader.ReadLine();
            }
        }

    }
}

// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests
{
    [TestClass]
    public class TextualReaderTests : TestFixture
    {
        [TestMethod]
        public void TextualReader_PeekWhitespaceLine_TrueIfIsNotEndOfFile()
        {
            using (var sr = new StreamReader(new MemoryStream()))
            {
                var reader = new TextualReader(sr);
                Assert.IsFalse(reader.PeekWhitespaceLine());
            }
        }

        [TestMethod]
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
                Assert.IsFalse(reader.PeekWhitespaceLine()); reader.ReadLine();
                Assert.IsTrue(reader.PeekWhitespaceLine()); reader.ReadLine();
                Assert.IsTrue(reader.PeekWhitespaceLine()); reader.ReadLine();
                Assert.IsFalse(reader.PeekWhitespaceLine()); reader.ReadLine();
                Assert.IsFalse(reader.PeekWhitespaceLine()); reader.ReadLine();
            }
        }

    }
}

// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Times;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Times
{
    [TestClass]
    public class CTimeToNetFormatConverterTests : TestFixture
    {
        [TestMethod]
        public void CTimeToNetFormatConverter_Constructor_SetsDefaultValues()
        {
            CTimeToNetFormatConverter converter = new CTimeToNetFormatConverter();
            Assert.IsFalse(converter.IsMarker);
            Assert.IsFalse(converter.IsQuote);
        }

        [TestMethod]
        public void CTimeToNetFormatConverter_OpenQuotes_AddsQuoteIfNotInQuotes()
        {
            CTimeToNetFormatConverter converter = new CTimeToNetFormatConverter();
            Assert.IsFalse(converter.IsQuote);
            converter.OpenQuotes();
            Assert.IsTrue(converter.IsQuote);
            Assert.AreEqual("'", converter.ToString());
        }

        [TestMethod]
        public void CTimeToNetFormatConverter_OpenQuotes_AddsNothingIfInQuotes()
        {
            CTimeToNetFormatConverter converter = new CTimeToNetFormatConverter();
            converter.OpenQuotes();

            converter.OpenQuotes();
            Assert.IsTrue(converter.IsQuote);
            Assert.AreEqual("'", converter.ToString());
        }

        [TestMethod]
        public void CTimeToNetFormatConverter_CloseQuotes_AddsQuoteIfInQuotes()
        {
            CTimeToNetFormatConverter converter = new CTimeToNetFormatConverter();
            converter.OpenQuotes();

            converter.CloseQuotes();
            Assert.IsFalse(converter.IsQuote);
            Assert.AreEqual("''", converter.ToString());
        }

        [TestMethod]
        public void CTimeToNetFormatConverter_CloseQuotes_AddsNothingIfNotInQuotes()
        {
            CTimeToNetFormatConverter converter = new CTimeToNetFormatConverter();
            converter.OpenQuotes();
            converter.CloseQuotes();

            converter.CloseQuotes();
            Assert.IsFalse(converter.IsQuote);
            Assert.AreEqual("''", converter.ToString());
        }

        [TestMethod]
        public void CTimeToNetFormatConverter_ConvertCTimeToNet_VariousFormats()
        {
            Assert.AreEqual("",  CTimeToNetFormatConverter.ConvertCTimeToNet("", NetDateTimeFormat.ParseFormat));
            Assert.AreEqual(" ", CTimeToNetFormatConverter.ConvertCTimeToNet(" ", NetDateTimeFormat.ParseFormat));
            Assert.AreEqual("'no marker string'", CTimeToNetFormatConverter.ConvertCTimeToNet("no marker string", NetDateTimeFormat.ParseFormat));
            Assert.AreEqual("yyyyyyMMMMdHHmmsshhttdddd", CTimeToNetFormatConverter.ConvertCTimeToNet("%Y%y%m%b%d%H%M%S%I%p%A", NetDateTimeFormat.ParseFormat));
            Assert.AreEqual("yyyyyyMMMMMddHHmmsshhttdddd", CTimeToNetFormatConverter.ConvertCTimeToNet("%Y%y%m%b%d%H%M%S%I%p%A", NetDateTimeFormat.PrintFormat));
            Assert.AreEqual("yyyy-M-d", CTimeToNetFormatConverter.ConvertCTimeToNet("%F", NetDateTimeFormat.ParseFormat));
            Assert.AreEqual("yyyy-MM-dd", CTimeToNetFormatConverter.ConvertCTimeToNet("%F", NetDateTimeFormat.PrintFormat));
            Assert.AreEqual("'Today is 'yyyy' 'dd' - 'MM", CTimeToNetFormatConverter.ConvertCTimeToNet("Today is %Y %d - %m", NetDateTimeFormat.PrintFormat));
        }

        [TestMethod]
        public void CTimeToNetFormatConverter_ConvertCTimeToNet_HandlesDoubleMarkerAsSingleChar()
        {
            Assert.AreEqual("'%'", CTimeToNetFormatConverter.ConvertCTimeToNet("%%", NetDateTimeFormat.ParseFormat));
            Assert.AreEqual("'%'", CTimeToNetFormatConverter.ConvertCTimeToNet("%%", NetDateTimeFormat.PrintFormat));
        }

    }
}

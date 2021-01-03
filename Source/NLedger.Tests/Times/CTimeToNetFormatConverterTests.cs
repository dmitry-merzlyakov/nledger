// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Times;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Times
{
    public class CTimeToNetFormatConverterTests : TestFixture
    {
        [Fact]
        public void CTimeToNetFormatConverter_Constructor_SetsDefaultValues()
        {
            CTimeToNetFormatConverter converter = new CTimeToNetFormatConverter();
            Assert.False(converter.IsMarker);
            Assert.False(converter.IsQuote);
        }

        [Fact]
        public void CTimeToNetFormatConverter_OpenQuotes_AddsQuoteIfNotInQuotes()
        {
            CTimeToNetFormatConverter converter = new CTimeToNetFormatConverter();
            Assert.False(converter.IsQuote);
            converter.OpenQuotes();
            Assert.True(converter.IsQuote);
            Assert.Equal("'", converter.ToString());
        }

        [Fact]
        public void CTimeToNetFormatConverter_OpenQuotes_AddsNothingIfInQuotes()
        {
            CTimeToNetFormatConverter converter = new CTimeToNetFormatConverter();
            converter.OpenQuotes();

            converter.OpenQuotes();
            Assert.True(converter.IsQuote);
            Assert.Equal("'", converter.ToString());
        }

        [Fact]
        public void CTimeToNetFormatConverter_CloseQuotes_AddsQuoteIfInQuotes()
        {
            CTimeToNetFormatConverter converter = new CTimeToNetFormatConverter();
            converter.OpenQuotes();

            converter.CloseQuotes();
            Assert.False(converter.IsQuote);
            Assert.Equal("''", converter.ToString());
        }

        [Fact]
        public void CTimeToNetFormatConverter_CloseQuotes_AddsNothingIfNotInQuotes()
        {
            CTimeToNetFormatConverter converter = new CTimeToNetFormatConverter();
            converter.OpenQuotes();
            converter.CloseQuotes();

            converter.CloseQuotes();
            Assert.False(converter.IsQuote);
            Assert.Equal("''", converter.ToString());
        }

        [Fact]
        public void CTimeToNetFormatConverter_ConvertCTimeToNet_VariousFormats()
        {
            Assert.Equal("",  CTimeToNetFormatConverter.ConvertCTimeToNet("", NetDateTimeFormat.ParseFormat));
            Assert.Equal(" ", CTimeToNetFormatConverter.ConvertCTimeToNet(" ", NetDateTimeFormat.ParseFormat));
            Assert.Equal("'no marker string'", CTimeToNetFormatConverter.ConvertCTimeToNet("no marker string", NetDateTimeFormat.ParseFormat));
            Assert.Equal("yyyyyyMMMMdHHmmsshhttdddd", CTimeToNetFormatConverter.ConvertCTimeToNet("%Y%y%m%b%d%H%M%S%I%p%A", NetDateTimeFormat.ParseFormat));
            Assert.Equal("yyyyyyMMMMMddHHmmsshhttdddd", CTimeToNetFormatConverter.ConvertCTimeToNet("%Y%y%m%b%d%H%M%S%I%p%A", NetDateTimeFormat.PrintFormat));
            Assert.Equal("yyyy-M-d", CTimeToNetFormatConverter.ConvertCTimeToNet("%F", NetDateTimeFormat.ParseFormat));
            Assert.Equal("yyyy-MM-dd", CTimeToNetFormatConverter.ConvertCTimeToNet("%F", NetDateTimeFormat.PrintFormat));
            Assert.Equal("'Today is 'yyyy' 'dd' - 'MM", CTimeToNetFormatConverter.ConvertCTimeToNet("Today is %Y %d - %m", NetDateTimeFormat.PrintFormat));
        }

        [Fact]
        public void CTimeToNetFormatConverter_ConvertCTimeToNet_HandlesDoubleMarkerAsSingleChar()
        {
            Assert.Equal("'%'", CTimeToNetFormatConverter.ConvertCTimeToNet("%%", NetDateTimeFormat.ParseFormat));
            Assert.Equal("'%'", CTimeToNetFormatConverter.ConvertCTimeToNet("%%", NetDateTimeFormat.PrintFormat));
        }

    }
}

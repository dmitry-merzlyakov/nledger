// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility
{
    public class StringExtensionsTests
    {
        [Fact]
        public void StringExtensions_NextElement_Returns_Empty_For_Empty()
        {
            string s = null;
            Assert.Equal(String.Empty, StringExtensions.NextElement(ref s));

            s = String.Empty;
            Assert.Equal(String.Empty, StringExtensions.NextElement(ref s));
        }

        [Fact]
        public void StringExtensions_NextElement_Returns_Empty_If_No_Spaces_Found()
        {
            string s = "abcdefghk";
            Assert.Equal(String.Empty, StringExtensions.NextElement(ref s));
            Assert.Equal("abcdefghk", s);
        }

        [Fact]
        public void StringExtensions_NextElement_Returns_Empty_For_Empty_String()
        {
            string s = "        ";
            Assert.Equal(String.Empty, StringExtensions.NextElement(ref s));
            Assert.Equal(String.Empty, s);
        }

        [Fact]
        public void StringExtensions_NextElement_Returns_Text_If_String_Begins_With_Spaces()
        {
            string s = " string";
            Assert.Equal("string", StringExtensions.NextElement(ref s));
            Assert.Equal(String.Empty, s);

            s = "      string";
            Assert.Equal("string", StringExtensions.NextElement(ref s));
            Assert.Equal(String.Empty, s);
        }

        [Fact]
        public void StringExtensions_NextElement_Returns_Text_After_First_Space()
        {
            string s = "my string";
            Assert.Equal("string", StringExtensions.NextElement(ref s));
            Assert.Equal("my", s);

            s = "my" + '\t' + "string";
            Assert.Equal("string", StringExtensions.NextElement(ref s));
            Assert.Equal("my", s);

            s = "my    string";
            Assert.Equal("string", StringExtensions.NextElement(ref s));
            Assert.Equal("my", s);
        }

        [Fact]
        public void StringExtensions_NextElement_Returns_Text_After_Double_Spaces()
        {
            string s = "my string";
            Assert.Equal(String.Empty, StringExtensions.NextElement(ref s, true));
            Assert.Equal("my string", s);

            s = "my  string";
            Assert.Equal("string", StringExtensions.NextElement(ref s, true));
            Assert.Equal("my", s);

            s = "my long  string";
            Assert.Equal("string", StringExtensions.NextElement(ref s, true));
            Assert.Equal("my long", s);
        }

        [Fact]
        public void StringExtensions_SplitBySeparator_Returns_Empty_For_Empty()
        {
            string s = null;
            Assert.Equal(String.Empty, StringExtensions.SplitBySeparator(ref s, ':'));

            s = String.Empty;
            Assert.Equal(String.Empty, StringExtensions.SplitBySeparator(ref s, ':'));
        }

        [Fact]
        public void StringExtensions_SplitBySeparator_Returns_Empty_If_Separator_Is_Not_Found()
        {
            string s = "abcdefgh";
            Assert.Equal(String.Empty, StringExtensions.SplitBySeparator(ref s, ':'));
            Assert.Equal("abcdefgh", s);
        }

        [Fact]
        public void StringExtensions_SplitBySeparator_Returns_Text_After_Separator()
        {
            string s = "abcd:efgh";
            Assert.Equal("efgh", StringExtensions.SplitBySeparator(ref s, ':'));
            Assert.Equal("abcd", s);
        }

        [Fact]
        public void StringExtensions_SplitBySeparator_Returns_Text_After_Separator_If_Preceeded_By_Spaces()
        {
            string s = "abcd:efgh";
            Assert.Equal(String.Empty, StringExtensions.SplitBySeparator(ref s, ':', true));
            Assert.Equal("abcd:efgh", s);

            s = "abcd :efgh";
            Assert.Equal(String.Empty, StringExtensions.SplitBySeparator(ref s, ':', true));
            Assert.Equal("abcd :efgh", s);

            s = "abcd  :efgh";
            Assert.Equal("efgh", StringExtensions.SplitBySeparator(ref s, ':', true));
            Assert.Equal("abcd", s);

            s = "abcd\t  :efgh";
            Assert.Equal("efgh", StringExtensions.SplitBySeparator(ref s, ':', true));
            Assert.Equal("abcd", s);

            s = "abcd\t:efgh";
            Assert.Equal("efgh", StringExtensions.SplitBySeparator(ref s, ':', true));
            Assert.Equal("abcd", s);
        }

        [Fact]
        public void StringExtensions_ReadInto_ReturnsEmptyStringForEmptyString()
        {
            string s = null;
            Assert.Equal(String.Empty, StringExtensions.ReadInto(ref s, TestIsDigit));

            s = String.Empty;
            Assert.Equal(String.Empty, StringExtensions.ReadInto(ref s, TestIsDigit));
        }

        [Fact]
        public void StringExtensions_ReadInto_ReturnsOriginalStringIfAllSymbolsMatched()
        {
            string initialString = "52637182735265";
            string s = initialString;
            string result = StringExtensions.ReadInto(ref s, TestIsDigit);
            Assert.Equal(String.Empty, s);
            Assert.Equal(initialString, result);
        }

        [Fact]
        public void StringExtensions_ReadInto_ReturnsEmptyStringIfBeginningSymbolsNotMatched()
        {
            string initialString = "abcdefgh12345";
            string s = initialString;
            string result = StringExtensions.ReadInto(ref s, TestIsDigit);
            Assert.Equal(initialString, s);
            Assert.Equal(String.Empty, result);
        }

        [Fact]
        public void StringExtensions_ReadInto_ReturnsStringWithMatchedSymbols()
        {
            string initialString = "12345abcdefgh";
            string s = initialString;
            string result = StringExtensions.ReadInto(ref s, TestIsDigit);
            Assert.Equal("abcdefgh", s);
            Assert.Equal("12345", result);
        }

        [Fact]
        public void StringExtensions_EncodeEscapeSequenecs_ReturnsEmptyStringForEmptytString()
        {
            string s = null;
            Assert.Equal(String.Empty, s.EncodeEscapeSequenecs());

            s = String.Empty;
            Assert.Equal(String.Empty, s.EncodeEscapeSequenecs());
        }

        [Fact]
        public void StringExtensions_EncodeEscapeSequenecs_ReturnsOriginalStringIfNoEscapeSequences()
        {
            string s = "absdef";
            Assert.Equal(s, s.EncodeEscapeSequenecs());
        }

        [Fact]
        public void StringExtensions_EncodeEscapeSequenecs_ReplacesEscapeSequencesWithChars()
        {
            string origin = @"1\b2\f3\n4\r5\t6\v";
            string expectedResult = "1\b2\f3\n4\r5\t6\v";
            string result = origin.EncodeEscapeSequenecs();
            Assert.Equal(expectedResult, result);
            Assert.Equal(12, result.Length);
            Assert.Equal(12, expectedResult.Length);
            Assert.Equal(18, origin.Length);
        }

        [Fact]
        public void StringExtensions_SplitArguments_AcceptsNullsOrEmptyStrings()
        {
            var result1 = StringExtensions.SplitArguments(null);
            Assert.Empty(result1);

            var result2 = StringExtensions.SplitArguments(String.Empty);
            Assert.Empty(result2);
        }

        [Fact]
        public void StringExtensions_SplitArguments_AcceptsWhiteSpaceStrings()
        {
            var result1 = StringExtensions.SplitArguments(" ");
            Assert.Empty(result1);

            var result2 = StringExtensions.SplitArguments("      ");
            Assert.Empty(result2);
        }

        [Fact]
        public void StringExtensions_SplitArguments_AcceptsSingleParameters()
        {
            var result1 = StringExtensions.SplitArguments("param1 ");
            Assert.Single(result1);
            Assert.Equal("param1", result1.ElementAt(0));
        }

        [Fact]
        public void StringExtensions_SplitArguments_AcceptsSeveralParameters()
        {
            var result1 = StringExtensions.SplitArguments("param1 param2");
            Assert.Equal(2, result1.Count());
            Assert.Equal("param1", result1.ElementAt(0));
            Assert.Equal("param2", result1.ElementAt(1));
        }

        [Fact]
        public void StringExtensions_SplitArguments_AcceptsQuotedParameters()
        {
            var result1 = StringExtensions.SplitArguments("param1 'param2 param3' param4");
            Assert.Equal(3, result1.Count());
            Assert.Equal("param1", result1.ElementAt(0));
            Assert.Equal("param2 param3", result1.ElementAt(1));
            Assert.Equal("param4", result1.ElementAt(2));
        }

        [Fact]
        public void StringExtensions_SafeSubstring_ReturnsSubstingForIndexAndLength()
        {
            Assert.Equal("234", "12345".SafeSubstring(1, 3));
        }

        [Fact]
        public void StringExtensions_SafeSubstring_ReturnsEmptyStringForWrongIndexAndLength()
        {
            Assert.Equal(String.Empty, "12345".SafeSubstring(21, 3));
        }

        [Fact]
        public void StringExtensions_SafeSubstring_ReturnsSubstingForIndex()
        {
            Assert.Equal("2345", "12345".SafeSubstring(1));
        }

        [Fact]
        public void StringExtensions_SafeSubstring_ReturnsEmptyStringForWrongIndex()
        {
            Assert.Equal(String.Empty, "12345".SafeSubstring(21));
        }

        [Fact]
        public void StringExtensions_GetFirstLine_ReturnsOriginalEmptyString()
        {
            Assert.Null(StringExtensions.GetFirstLine(null));
            Assert.Equal(String.Empty, String.Empty.GetFirstLine());
        }

        [Fact]
        public void StringExtensions_GetFirstLine_ReturnsEntireStringIfThereIsNotCrLf()
        {
            Assert.Equal("abcdef", "abcdef".GetFirstLine());
        }

        [Fact]
        public void StringExtensions_GetFirstLine_HonoursBothCrLf()
        {
            Assert.Equal("abc", "abc\rdef".GetFirstLine());
            Assert.Equal("abc", "abc\ndef".GetFirstLine());
            Assert.Equal("abc", "abc\r\ndef".GetFirstLine());
            Assert.Equal("abc", "abc\ndef\rasd\nczxczx\r\nssfdsd".GetFirstLine());
            Assert.Equal("", "\ndef".GetFirstLine());
            Assert.Equal("", "\rdef".GetFirstLine());
        }

        [Fact]
        public void StringExtensions_GetWord_ManagesEmptyString()
        {
            string inp = null;
            Assert.Equal(String.Empty, StringExtensions.GetWord(ref inp));
            Assert.Null(inp);

            inp = String.Empty;
            Assert.Equal(String.Empty, StringExtensions.GetWord(ref inp));
            Assert.Equal(String.Empty, inp);
        }

        [Fact]
        public void StringExtensions_GetWord_IgnoresInitialWhiteSpaces()
        {
            string inp = " text";
            Assert.Equal("text", StringExtensions.GetWord(ref inp));
            Assert.Equal(String.Empty, inp);

            inp = "\ttext";
            Assert.Equal("text", StringExtensions.GetWord(ref inp));
            Assert.Equal(String.Empty, inp);
        }

        [Fact]
        public void StringExtensions_GetWord_IgnoresWhiteSpacesBetweenWords()
        {
            string inp = "text1 text2";
            Assert.Equal("text1", StringExtensions.GetWord(ref inp));
            Assert.Equal("text2", inp);

            inp = "text1\ttext2";
            Assert.Equal("text1", StringExtensions.GetWord(ref inp));
            Assert.Equal("text2", inp);

            inp = "text1 \t text2";
            Assert.Equal("text1", StringExtensions.GetWord(ref inp));
            Assert.Equal("text2", inp);
        }

        [Fact]
        public void StringExtensions_GetWord_ReturnsFirstWord()
        {
            string inp = " text1  text2   text3";
            Assert.Equal("text1", StringExtensions.GetWord(ref inp));
            Assert.Equal("text2   text3", inp);
        }

        private Func<char, bool> TestIsDigit = (c) => Char.IsDigit(c);
    }
}

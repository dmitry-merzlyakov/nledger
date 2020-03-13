// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utility
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void StringExtensions_NextElement_Returns_Empty_For_Empty()
        {
            string s = null;
            Assert.AreEqual(String.Empty, StringExtensions.NextElement(ref s));

            s = String.Empty;
            Assert.AreEqual(String.Empty, StringExtensions.NextElement(ref s));
        }

        [TestMethod]
        public void StringExtensions_NextElement_Returns_Empty_If_No_Spaces_Found()
        {
            string s = "abcdefghk";
            Assert.AreEqual(String.Empty, StringExtensions.NextElement(ref s));
            Assert.AreEqual("abcdefghk", s);
        }

        [TestMethod]
        public void StringExtensions_NextElement_Returns_Empty_For_Empty_String()
        {
            string s = "        ";
            Assert.AreEqual(String.Empty, StringExtensions.NextElement(ref s));
            Assert.AreEqual(String.Empty, s);
        }

        [TestMethod]
        public void StringExtensions_NextElement_Returns_Text_If_String_Begins_With_Spaces()
        {
            string s = " string";
            Assert.AreEqual("string", StringExtensions.NextElement(ref s));
            Assert.AreEqual(String.Empty, s);

            s = "      string";
            Assert.AreEqual("string", StringExtensions.NextElement(ref s));
            Assert.AreEqual(String.Empty, s);
        }

        [TestMethod]
        public void StringExtensions_NextElement_Returns_Text_After_First_Space()
        {
            string s = "my string";
            Assert.AreEqual("string", StringExtensions.NextElement(ref s));
            Assert.AreEqual("my", s);

            s = "my" + '\t' + "string";
            Assert.AreEqual("string", StringExtensions.NextElement(ref s));
            Assert.AreEqual("my", s);

            s = "my    string";
            Assert.AreEqual("string", StringExtensions.NextElement(ref s));
            Assert.AreEqual("my", s);
        }

        [TestMethod]
        public void StringExtensions_NextElement_Returns_Text_After_Double_Spaces()
        {
            string s = "my string";
            Assert.AreEqual(String.Empty, StringExtensions.NextElement(ref s, true));
            Assert.AreEqual("my string", s);

            s = "my  string";
            Assert.AreEqual("string", StringExtensions.NextElement(ref s, true));
            Assert.AreEqual("my", s);

            s = "my long  string";
            Assert.AreEqual("string", StringExtensions.NextElement(ref s, true));
            Assert.AreEqual("my long", s);
        }

        [TestMethod]
        public void StringExtensions_SplitBySeparator_Returns_Empty_For_Empty()
        {
            string s = null;
            Assert.AreEqual(String.Empty, StringExtensions.SplitBySeparator(ref s, ':'));

            s = String.Empty;
            Assert.AreEqual(String.Empty, StringExtensions.SplitBySeparator(ref s, ':'));
        }

        [TestMethod]
        public void StringExtensions_SplitBySeparator_Returns_Empty_If_Separator_Is_Not_Found()
        {
            string s = "abcdefgh";
            Assert.AreEqual(String.Empty, StringExtensions.SplitBySeparator(ref s, ':'));
            Assert.AreEqual("abcdefgh", s);
        }

        [TestMethod]
        public void StringExtensions_SplitBySeparator_Returns_Text_After_Separator()
        {
            string s = "abcd:efgh";
            Assert.AreEqual("efgh", StringExtensions.SplitBySeparator(ref s, ':'));
            Assert.AreEqual("abcd", s);
        }

        [TestMethod]
        public void StringExtensions_SplitBySeparator_Returns_Text_After_Separator_If_Preceeded_By_Spaces()
        {
            string s = "abcd:efgh";
            Assert.AreEqual(String.Empty, StringExtensions.SplitBySeparator(ref s, ':', true));
            Assert.AreEqual("abcd:efgh", s);

            s = "abcd :efgh";
            Assert.AreEqual(String.Empty, StringExtensions.SplitBySeparator(ref s, ':', true));
            Assert.AreEqual("abcd :efgh", s);

            s = "abcd  :efgh";
            Assert.AreEqual("efgh", StringExtensions.SplitBySeparator(ref s, ':', true));
            Assert.AreEqual("abcd", s);

            s = "abcd\t  :efgh";
            Assert.AreEqual("efgh", StringExtensions.SplitBySeparator(ref s, ':', true));
            Assert.AreEqual("abcd", s);

            s = "abcd\t:efgh";
            Assert.AreEqual("efgh", StringExtensions.SplitBySeparator(ref s, ':', true));
            Assert.AreEqual("abcd", s);
        }

        [TestMethod]
        public void StringExtensions_ReadInto_ReturnsEmptyStringForEmptyString()
        {
            string s = null;
            Assert.AreEqual(String.Empty, StringExtensions.ReadInto(ref s, TestIsDigit));

            s = String.Empty;
            Assert.AreEqual(String.Empty, StringExtensions.ReadInto(ref s, TestIsDigit));
        }

        [TestMethod]
        public void StringExtensions_ReadInto_ReturnsOriginalStringIfAllSymbolsMatched()
        {
            string initialString = "52637182735265";
            string s = initialString;
            string result = StringExtensions.ReadInto(ref s, TestIsDigit);
            Assert.AreEqual(String.Empty, s);
            Assert.AreEqual(initialString, result);
        }

        [TestMethod]
        public void StringExtensions_ReadInto_ReturnsEmptyStringIfBeginningSymbolsNotMatched()
        {
            string initialString = "abcdefgh12345";
            string s = initialString;
            string result = StringExtensions.ReadInto(ref s, TestIsDigit);
            Assert.AreEqual(initialString, s);
            Assert.AreEqual(String.Empty, result);
        }

        [TestMethod]
        public void StringExtensions_ReadInto_ReturnsStringWithMatchedSymbols()
        {
            string initialString = "12345abcdefgh";
            string s = initialString;
            string result = StringExtensions.ReadInto(ref s, TestIsDigit);
            Assert.AreEqual("abcdefgh", s);
            Assert.AreEqual("12345", result);
        }

        [TestMethod]
        public void StringExtensions_EncodeEscapeSequenecs_ReturnsEmptyStringForEmptytString()
        {
            string s = null;
            Assert.AreEqual(String.Empty, s.EncodeEscapeSequenecs());

            s = String.Empty;
            Assert.AreEqual(String.Empty, s.EncodeEscapeSequenecs());
        }

        [TestMethod]
        public void StringExtensions_EncodeEscapeSequenecs_ReturnsOriginalStringIfNoEscapeSequences()
        {
            string s = "absdef";
            Assert.AreEqual(s, s.EncodeEscapeSequenecs());
        }

        [TestMethod]
        public void StringExtensions_EncodeEscapeSequenecs_ReplacesEscapeSequencesWithChars()
        {
            string origin = @"1\b2\f3\n4\r5\t6\v";
            string expectedResult = "1\b2\f3\n4\r5\t6\v";
            string result = origin.EncodeEscapeSequenecs();
            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(12, result.Length);
            Assert.AreEqual(12, expectedResult.Length);
            Assert.AreEqual(18, origin.Length);
        }

        [TestMethod]
        public void StringExtensions_SplitArguments_AcceptsNullsOrEmptyStrings()
        {
            var result1 = StringExtensions.SplitArguments(null);
            Assert.AreEqual(0, result1.Count());

            var result2 = StringExtensions.SplitArguments(String.Empty);
            Assert.AreEqual(0, result2.Count());
        }

        [TestMethod]
        public void StringExtensions_SplitArguments_AcceptsWhiteSpaceStrings()
        {
            var result1 = StringExtensions.SplitArguments(" ");
            Assert.AreEqual(0, result1.Count());

            var result2 = StringExtensions.SplitArguments("      ");
            Assert.AreEqual(0, result2.Count());
        }

        [TestMethod]
        public void StringExtensions_SplitArguments_AcceptsSingleParameters()
        {
            var result1 = StringExtensions.SplitArguments("param1 ");
            Assert.AreEqual(1, result1.Count());
            Assert.AreEqual("param1", result1.ElementAt(0));
        }

        [TestMethod]
        public void StringExtensions_SplitArguments_AcceptsSeveralParameters()
        {
            var result1 = StringExtensions.SplitArguments("param1 param2");
            Assert.AreEqual(2, result1.Count());
            Assert.AreEqual("param1", result1.ElementAt(0));
            Assert.AreEqual("param2", result1.ElementAt(1));
        }

        [TestMethod]
        public void StringExtensions_SplitArguments_AcceptsQuotedParameters()
        {
            var result1 = StringExtensions.SplitArguments("param1 'param2 param3' param4");
            Assert.AreEqual(3, result1.Count());
            Assert.AreEqual("param1", result1.ElementAt(0));
            Assert.AreEqual("param2 param3", result1.ElementAt(1));
            Assert.AreEqual("param4", result1.ElementAt(2));
        }

        [TestMethod]
        public void StringExtensions_SafeSubstring_ReturnsSubstingForIndexAndLength()
        {
            Assert.AreEqual("234", "12345".SafeSubstring(1, 3));
        }

        [TestMethod]
        public void StringExtensions_SafeSubstring_ReturnsEmptyStringForWrongIndexAndLength()
        {
            Assert.AreEqual(String.Empty, "12345".SafeSubstring(21, 3));
        }

        [TestMethod]
        public void StringExtensions_SafeSubstring_ReturnsSubstingForIndex()
        {
            Assert.AreEqual("2345", "12345".SafeSubstring(1));
        }

        [TestMethod]
        public void StringExtensions_SafeSubstring_ReturnsEmptyStringForWrongIndex()
        {
            Assert.AreEqual(String.Empty, "12345".SafeSubstring(21));
        }

        [TestMethod]
        public void StringExtensions_GetFirstLine_ReturnsOriginalEmptyString()
        {
            Assert.AreEqual(null, StringExtensions.GetFirstLine(null));
            Assert.AreEqual(String.Empty, String.Empty.GetFirstLine());
        }

        [TestMethod]
        public void StringExtensions_GetFirstLine_ReturnsEntireStringIfThereIsNotCrLf()
        {
            Assert.AreEqual("abcdef", "abcdef".GetFirstLine());
        }

        [TestMethod]
        public void StringExtensions_GetFirstLine_HonoursBothCrLf()
        {
            Assert.AreEqual("abc", "abc\rdef".GetFirstLine());
            Assert.AreEqual("abc", "abc\ndef".GetFirstLine());
            Assert.AreEqual("abc", "abc\r\ndef".GetFirstLine());
            Assert.AreEqual("abc", "abc\ndef\rasd\nczxczx\r\nssfdsd".GetFirstLine());
            Assert.AreEqual("", "\ndef".GetFirstLine());
            Assert.AreEqual("", "\rdef".GetFirstLine());
        }

        [TestMethod]
        public void StringExtensions_GetWord_ManagesEmptyString()
        {
            string inp = null;
            Assert.AreEqual(String.Empty, StringExtensions.GetWord(ref inp));
            Assert.IsNull(inp);

            inp = String.Empty;
            Assert.AreEqual(String.Empty, StringExtensions.GetWord(ref inp));
            Assert.AreEqual(String.Empty, inp);
        }

        [TestMethod]
        public void StringExtensions_GetWord_IgnoresInitialWhiteSpaces()
        {
            string inp = " text";
            Assert.AreEqual("text", StringExtensions.GetWord(ref inp));
            Assert.AreEqual(String.Empty, inp);

            inp = "\ttext";
            Assert.AreEqual("text", StringExtensions.GetWord(ref inp));
            Assert.AreEqual(String.Empty, inp);
        }

        [TestMethod]
        public void StringExtensions_GetWord_IgnoresWhiteSpacesBetweenWords()
        {
            string inp = "text1 text2";
            Assert.AreEqual("text1", StringExtensions.GetWord(ref inp));
            Assert.AreEqual("text2", inp);

            inp = "text1\ttext2";
            Assert.AreEqual("text1", StringExtensions.GetWord(ref inp));
            Assert.AreEqual("text2", inp);

            inp = "text1 \t text2";
            Assert.AreEqual("text1", StringExtensions.GetWord(ref inp));
            Assert.AreEqual("text2", inp);
        }

        [TestMethod]
        public void StringExtensions_GetWord_ReturnsFirstWord()
        {
            string inp = " text1  text2   text3";
            Assert.AreEqual("text1", StringExtensions.GetWord(ref inp));
            Assert.AreEqual("text2   text3", inp);
        }

        private Func<char, bool> TestIsDigit = (c) => Char.IsDigit(c);
    }
}

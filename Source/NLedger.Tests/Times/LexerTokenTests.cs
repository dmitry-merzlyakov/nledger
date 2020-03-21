// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Times
{
    [TestClass]
    public class LexerTokenTests : TestFixture
    {
        [TestMethod]
        [ExpectedException(typeof(DateError), DateError.ErrorMessageUnexpectedEnd)]
        public void LexerToken_Expected_ReturnsUnexpectedEndForZeroChar()
        {
            LexerToken.Expected(default(char));
        }

        [TestMethod]
        [ExpectedException(typeof(DateError), DateError.ErrorMessageMissing)]
        public void LexerToken_Expected_ReturnsMissingForNonZeroChar()
        {
            LexerToken.Expected('A');
        }

        [TestMethod]
        [ExpectedException(typeof(DateError), DateError.ErrorMessageInvalidChar)]
        public void LexerToken_Expected_ReturnsInvalidCharForZeroChar()
        {
            LexerToken.Expected(default(char), 'B');
        }

        [TestMethod]
        [ExpectedException(typeof(DateError), DateError.ErrorMessageInvalidCharWanted)]
        public void LexerToken_Expected_ReturnsInvalidCharWantedForNonZeroChar()
        {
            LexerToken.Expected('A', 'B');
        }

        [TestMethod]
        public void LexerToken_Constructor_CreatesEmptyToken()
        {
            LexerToken lexerToken = new LexerToken();
            Assert.AreEqual(LexerTokenKindEnum.UNKNOWN, lexerToken.Kind);
            Assert.IsTrue(lexerToken.Value.IsEmpty);
        }

        [TestMethod]
        public void LexerToken_Constructor_SetsKind()
        {
            LexerToken lexerToken = new LexerToken(LexerTokenKindEnum.TOK_YEAR);
            Assert.AreEqual(LexerTokenKindEnum.TOK_YEAR, lexerToken.Kind);
            Assert.IsTrue(lexerToken.Value.IsEmpty);
        }

        [TestMethod]
        public void LexerToken_Constructor_SetsKindAndValue()
        {
            LexerToken lexerToken = new LexerToken(LexerTokenKindEnum.TOK_YEAR, new BoostVariant(33));
            Assert.AreEqual(LexerTokenKindEnum.TOK_YEAR, lexerToken.Kind);
            Assert.AreEqual(33, lexerToken.Value.GetValue<int>());
        }

        [TestMethod]
        public void LexerToken_IsNotEnd_IndicatesThatTheEndIsNotReached()
        {
            LexerToken lexerToken = new LexerToken();
            Assert.IsTrue(lexerToken.IsNotEnd);

            lexerToken = new LexerToken(LexerTokenKindEnum.END_REACHED);
            Assert.IsFalse(lexerToken.IsNotEnd);
        }

        [TestMethod]
        public void LexerToken_Comparison_EqualsForEqualKinds()
        {
            LexerToken lexerToken1 = new LexerToken(LexerTokenKindEnum.TOK_DAILY);
            LexerToken lexerToken2 = new LexerToken(LexerTokenKindEnum.TOK_DAILY);
            Assert.IsTrue(lexerToken1.Equals(lexerToken2));
            Assert.IsTrue(lexerToken1 == lexerToken2);
        }

        [TestMethod]
        public void LexerToken_Comparison_EqualsForEqualKindsAndValues()
        {
            LexerToken lexerToken1 = new LexerToken(LexerTokenKindEnum.TOK_DAILY, new BoostVariant(3));
            LexerToken lexerToken2 = new LexerToken(LexerTokenKindEnum.TOK_DAILY, new BoostVariant(3));
            Assert.IsTrue(lexerToken1.Equals(lexerToken2));
            Assert.IsTrue(lexerToken1 == lexerToken2);
        }

    }
}

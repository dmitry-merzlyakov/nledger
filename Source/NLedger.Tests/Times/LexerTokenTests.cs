// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Times
{
    public class LexerTokenTests : TestFixture
    {
        [Fact]
        public void LexerToken_Expected_ReturnsUnexpectedEndForZeroChar()
        {
            var exception = Assert.Throws<DateError>(() => LexerToken.Expected(default(char)));
            Assert.Equal(DateError.ErrorMessageUnexpectedEnd, exception.Message);
        }

        [Fact]
        public void LexerToken_Expected_ReturnsMissingForNonZeroChar()
        {
            var exception = Assert.Throws<DateError>(() => LexerToken.Expected('A'));
            Assert.Equal(String.Format(DateError.ErrorMessageMissing, 'A'), exception.Message);
        }

        [Fact]
        public void LexerToken_Expected_ReturnsInvalidCharForZeroChar()
        {
            var exception = Assert.Throws<DateError>(() => LexerToken.Expected(default(char), 'B'));
            Assert.Equal(String.Format(DateError.ErrorMessageInvalidChar, 'B'), exception.Message);
        }

        [Fact]
        public void LexerToken_Expected_ReturnsInvalidCharWantedForNonZeroChar()
        {
            var exception = Assert.Throws<DateError>(() => LexerToken.Expected('A', 'B'));
            Assert.Equal(String.Format(DateError.ErrorMessageInvalidCharWanted, 'B', 'A'), exception.Message);
        }

        [Fact]
        public void LexerToken_Constructor_CreatesEmptyToken()
        {
            LexerToken lexerToken = new LexerToken();
            Assert.Equal(LexerTokenKindEnum.UNKNOWN, lexerToken.Kind);
            Assert.True(lexerToken.Value.IsEmpty);
        }

        [Fact]
        public void LexerToken_Constructor_SetsKind()
        {
            LexerToken lexerToken = new LexerToken(LexerTokenKindEnum.TOK_YEAR);
            Assert.Equal(LexerTokenKindEnum.TOK_YEAR, lexerToken.Kind);
            Assert.True(lexerToken.Value.IsEmpty);
        }

        [Fact]
        public void LexerToken_Constructor_SetsKindAndValue()
        {
            LexerToken lexerToken = new LexerToken(LexerTokenKindEnum.TOK_YEAR, new BoostVariant(33));
            Assert.Equal(LexerTokenKindEnum.TOK_YEAR, lexerToken.Kind);
            Assert.Equal(33, lexerToken.Value.GetValue<int>());
        }

        [Fact]
        public void LexerToken_IsNotEnd_IndicatesThatTheEndIsNotReached()
        {
            LexerToken lexerToken = new LexerToken();
            Assert.True(lexerToken.IsNotEnd);

            lexerToken = new LexerToken(LexerTokenKindEnum.END_REACHED);
            Assert.False(lexerToken.IsNotEnd);
        }

        [Fact]
        public void LexerToken_Comparison_EqualsForEqualKinds()
        {
            LexerToken lexerToken1 = new LexerToken(LexerTokenKindEnum.TOK_DAILY);
            LexerToken lexerToken2 = new LexerToken(LexerTokenKindEnum.TOK_DAILY);
            Assert.True(lexerToken1.Equals(lexerToken2));
            Assert.True(lexerToken1 == lexerToken2);
        }

        [Fact]
        public void LexerToken_Comparison_EqualsForEqualKindsAndValues()
        {
            LexerToken lexerToken1 = new LexerToken(LexerTokenKindEnum.TOK_DAILY, new BoostVariant(3));
            LexerToken lexerToken2 = new LexerToken(LexerTokenKindEnum.TOK_DAILY, new BoostVariant(3));
            Assert.True(lexerToken1.Equals(lexerToken2));
            Assert.True(lexerToken1 == lexerToken2);
        }

    }
}

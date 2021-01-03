// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
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
    public class CommandLineTests
    {
        [Fact]
        public void CommandLine_PreprocessSingleQuotes_AcceptsEmptyInput()
        {
            Assert.False(CommandLine.PreprocessSingleQuotes((string)null).Any());
            Assert.False(CommandLine.PreprocessSingleQuotes(new List<string>()).Any());
            Assert.False(CommandLine.PreprocessSingleQuotes(new string[] { "   " }).Any());
        }

        [Fact]
        public void CommandLine_PreprocessSingleQuotes_HandlesSingleWord()
        {
            var args = new string[] { "single-wpord" };
            Assert.Equal(args.Single(), CommandLine.PreprocessSingleQuotes(args).Single());
        }

        [Fact]
        public void CommandLine_PreprocessSingleQuotes_HandlesCollectionWithNoQuotes()
        {
            var args = new string[] { "bal", "reg", "tag", "and", "food" };
            var result = CommandLine.PreprocessSingleQuotes(args);
            Assert.Equal(args.Count(), result.Count());
            Assert.Equal(args.ElementAt(0), result.ElementAt(0));
            Assert.Equal(args.ElementAt(1), result.ElementAt(1));
            Assert.Equal(args.ElementAt(2), result.ElementAt(2));
            Assert.Equal(args.ElementAt(3), result.ElementAt(3));
            Assert.Equal(args.ElementAt(4), result.ElementAt(4));
        }

        [Fact]
        public void CommandLine_PreprocessSingleQuotes_HandlesQuotesInTheMiddleOfCommandLine()
        {
            var args = new string[] { "bal", "reg", "ta'g", "an'd", "food" };
            var result = CommandLine.PreprocessSingleQuotes(args);
            Assert.Equal(4, result.Count());
            Assert.Equal("bal", result.ElementAt(0));
            Assert.Equal("reg", result.ElementAt(1));
            Assert.Equal("tag and", result.ElementAt(2));
            Assert.Equal("food", result.ElementAt(3));
        }

        [Fact]
        public void CommandLine_PreprocessSingleQuotes_HandlesStartingQuotes()
        {
            var args = new string[] { "'bal", "r'eg", "tag", "and", "food" };
            var result = CommandLine.PreprocessSingleQuotes(args);
            Assert.Equal(4, result.Count());
            Assert.Equal("bal reg", result.ElementAt(0));
            Assert.Equal("tag", result.ElementAt(1));
            Assert.Equal("and", result.ElementAt(2));
            Assert.Equal("food", result.ElementAt(3));
        }

        [Fact]
        public void CommandLine_PreprocessSingleQuotes_HandlesFinalizingQuotes()
        {
            var args = new string[] { "bal", "reg", "tag", "an'd", "food'" };
            var result = CommandLine.PreprocessSingleQuotes(args);
            Assert.Equal(4, result.Count());
            Assert.Equal("bal", result.ElementAt(0));
            Assert.Equal("reg", result.ElementAt(1));
            Assert.Equal("tag", result.ElementAt(2));
            Assert.Equal("and food", result.ElementAt(3));
        }

        [Fact]
        public void CommandLine_PreprocessSingleQuotes_HandlesUnclosedQuotes()
        {
            var args = new string[] { "bal", "reg", "tag", "an'd", "food" };
            var result = CommandLine.PreprocessSingleQuotes(args);
            Assert.Equal(4, result.Count());
            Assert.Equal("bal", result.ElementAt(0));
            Assert.Equal("reg", result.ElementAt(1));
            Assert.Equal("tag", result.ElementAt(2));
            Assert.Equal("and food", result.ElementAt(3));
        }

        [Fact]
        public void CommandLine_PreprocessSingleQuotes_HandlesMultipleUnclosedQuotes()
        {
            var args = new string[] { "'bal", "re'g", "tag", "an'd", "food" };
            var result = CommandLine.PreprocessSingleQuotes(args);
            Assert.Equal(3, result.Count());
            Assert.Equal("bal reg", result.ElementAt(0));
            Assert.Equal("tag", result.ElementAt(1));
            Assert.Equal("and food", result.ElementAt(2));
        }

    }
}

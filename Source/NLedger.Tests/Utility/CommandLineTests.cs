// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
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
    public class CommandLineTests
    {
        [TestMethod]
        public void CommandLine_PreprocessSingleQuotes_AcceptsEmptyInput()
        {
            Assert.IsFalse(CommandLine.PreprocessSingleQuotes((string)null).Any());
            Assert.IsFalse(CommandLine.PreprocessSingleQuotes(new List<string>()).Any());
            Assert.IsFalse(CommandLine.PreprocessSingleQuotes(new string[] { "   " }).Any());
        }

        [TestMethod]
        public void CommandLine_PreprocessSingleQuotes_HandlesSingleWord()
        {
            var args = new string[] { "single-wpord" };
            Assert.AreEqual(args.Single(), CommandLine.PreprocessSingleQuotes(args).Single());
        }

        [TestMethod]
        public void CommandLine_PreprocessSingleQuotes_HandlesCollectionWithNoQuotes()
        {
            var args = new string[] { "bal", "reg", "tag", "and", "food" };
            var result = CommandLine.PreprocessSingleQuotes(args);
            Assert.AreEqual(args.Count(), result.Count());
            Assert.AreEqual(args.ElementAt(0), result.ElementAt(0));
            Assert.AreEqual(args.ElementAt(1), result.ElementAt(1));
            Assert.AreEqual(args.ElementAt(2), result.ElementAt(2));
            Assert.AreEqual(args.ElementAt(3), result.ElementAt(3));
            Assert.AreEqual(args.ElementAt(4), result.ElementAt(4));
        }

        [TestMethod]
        public void CommandLine_PreprocessSingleQuotes_HandlesQuotesInTheMiddleOfCommandLine()
        {
            var args = new string[] { "bal", "reg", "ta'g", "an'd", "food" };
            var result = CommandLine.PreprocessSingleQuotes(args);
            Assert.AreEqual(4, result.Count());
            Assert.AreEqual("bal", result.ElementAt(0));
            Assert.AreEqual("reg", result.ElementAt(1));
            Assert.AreEqual("tag and", result.ElementAt(2));
            Assert.AreEqual("food", result.ElementAt(3));
        }

        [TestMethod]
        public void CommandLine_PreprocessSingleQuotes_HandlesStartingQuotes()
        {
            var args = new string[] { "'bal", "r'eg", "tag", "and", "food" };
            var result = CommandLine.PreprocessSingleQuotes(args);
            Assert.AreEqual(4, result.Count());
            Assert.AreEqual("bal reg", result.ElementAt(0));
            Assert.AreEqual("tag", result.ElementAt(1));
            Assert.AreEqual("and", result.ElementAt(2));
            Assert.AreEqual("food", result.ElementAt(3));
        }

        [TestMethod]
        public void CommandLine_PreprocessSingleQuotes_HandlesFinalizingQuotes()
        {
            var args = new string[] { "bal", "reg", "tag", "an'd", "food'" };
            var result = CommandLine.PreprocessSingleQuotes(args);
            Assert.AreEqual(4, result.Count());
            Assert.AreEqual("bal", result.ElementAt(0));
            Assert.AreEqual("reg", result.ElementAt(1));
            Assert.AreEqual("tag", result.ElementAt(2));
            Assert.AreEqual("and food", result.ElementAt(3));
        }

        [TestMethod]
        public void CommandLine_PreprocessSingleQuotes_HandlesUnclosedQuotes()
        {
            var args = new string[] { "bal", "reg", "tag", "an'd", "food" };
            var result = CommandLine.PreprocessSingleQuotes(args);
            Assert.AreEqual(4, result.Count());
            Assert.AreEqual("bal", result.ElementAt(0));
            Assert.AreEqual("reg", result.ElementAt(1));
            Assert.AreEqual("tag", result.ElementAt(2));
            Assert.AreEqual("and food", result.ElementAt(3));
        }

        [TestMethod]
        public void CommandLine_PreprocessSingleQuotes_HandlesMultipleUnclosedQuotes()
        {
            var args = new string[] { "'bal", "re'g", "tag", "an'd", "food" };
            var result = CommandLine.PreprocessSingleQuotes(args);
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual("bal reg", result.ElementAt(0));
            Assert.AreEqual("tag", result.ElementAt(1));
            Assert.AreEqual("and food", result.ElementAt(2));
        }

    }
}

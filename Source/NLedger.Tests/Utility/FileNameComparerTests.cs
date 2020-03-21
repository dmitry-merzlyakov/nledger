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
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class FileNameComparerTests : TestFixture
    {
        [TestMethod]
        public void FileNameComparer_Instance_IsPopuplated()
        {
            Assert.IsNotNull(FileNameComparer.Instance as IEqualityComparer<string>);
        }

        [TestMethod]
        public void FileNameComparer_GetHashCode_ReturnsStringHashcode()
        {
            var comparer = new FileNameComparer();
            Assert.AreEqual("10".GetHashCode(), comparer.GetHashCode("10"));
            Assert.AreEqual("".GetHashCode(), comparer.GetHashCode(""));
            Assert.AreEqual(0, comparer.GetHashCode(null));
        }

        [TestMethod]
        public void FileNameComparer_Equals_ReturnsTrueIfBothEmpty()
        {
            var comparer = new FileNameComparer();
            Assert.IsTrue(comparer.Equals("", ""));
            Assert.IsTrue(comparer.Equals(" ", ""));
            Assert.IsTrue(comparer.Equals("", " "));
            Assert.IsTrue(comparer.Equals(null, ""));
            Assert.IsTrue(comparer.Equals("", null));
            Assert.IsFalse(comparer.Equals("", "something"));
            Assert.IsFalse(comparer.Equals("something", ""));
        }

        [TestMethod]
        public void FileNameComparer_Equals_ReturnsTrueForEqualFiles()
        {
            var comparer = new FileNameComparer();
            Assert.IsTrue(comparer.Equals(@"aa.txt", @"aa.txt"));
            Assert.IsTrue(comparer.Equals(@".\aa.txt", @".\aa\..\aa.txt"));
        }

        [TestMethod]
        public void FileNameComparer_Equals_IsCaseInsensitive()
        {
            var comparer = new FileNameComparer();
            Assert.IsTrue(comparer.Equals(@"aa.txt", @"AA.txt"));
        }

    }
}

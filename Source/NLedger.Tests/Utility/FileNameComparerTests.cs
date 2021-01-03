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
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class FileNameComparerTests : TestFixture
    {
        [Fact]
        public void FileNameComparer_Instance_IsPopuplated()
        {
            Assert.NotNull(FileNameComparer.Instance as IEqualityComparer<string>);
        }

        [Fact]
        public void FileNameComparer_GetHashCode_ReturnsStringHashcode()
        {
            var comparer = new FileNameComparer();
            Assert.Equal("10".GetHashCode(), comparer.GetHashCode("10"));
            Assert.Equal("".GetHashCode(), comparer.GetHashCode(""));
            Assert.Equal(0, comparer.GetHashCode(null));
        }

        [Fact]
        public void FileNameComparer_Equals_ReturnsTrueIfBothEmpty()
        {
            var comparer = new FileNameComparer();
            Assert.True(comparer.Equals("", ""));
            Assert.True(comparer.Equals(" ", ""));
            Assert.True(comparer.Equals("", " "));
            Assert.True(comparer.Equals(null, ""));
            Assert.True(comparer.Equals("", null));
            Assert.False(comparer.Equals("", "something"));
            Assert.False(comparer.Equals("something", ""));
        }

        [Fact]
        public void FileNameComparer_Equals_ReturnsTrueForEqualFiles()
        {
            var comparer = new FileNameComparer();
            Assert.True(comparer.Equals(@"aa.txt", @"aa.txt"));
            Assert.True(comparer.Equals(@"./aa.txt", @"./aa/../aa.txt"));  // Path.GetFullPath can properly normalize path on linux only if it contains forward slashes. On Windows, any slashes work fine.
        }

        [Fact]
        public void FileNameComparer_Equals_IsCaseInsensitive()
        {
            var comparer = new FileNameComparer();
            Assert.True(comparer.Equals(@"aa.txt", @"AA.txt"));
        }

    }
}

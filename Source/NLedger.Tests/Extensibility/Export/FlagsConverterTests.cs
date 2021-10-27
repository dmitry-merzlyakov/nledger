// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Extensibility.Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility.Export
{
    public class FlagsConverterTests
    {
        public class TestFlags
        {
            public const int FLAG_A = 1;
            public const int FLAG_B = 2;
            public const int FLAG_C = 4;

            public TestFlags(bool propertyA, bool propertyB, bool propertyC)
            {
                PropertyA = propertyA;
                PropertyB = propertyB;
                PropertyC = propertyC;
            }

            public bool PropertyA { get; set; }
            public bool PropertyB { get; set; }
            public bool PropertyC { get; set; }

            public static FlagsConverter<TestFlags> GetAdapter() => new FlagsConverter<TestFlags>().
                AddMapping(FLAG_A, a => a.PropertyA, (a, v) => a.PropertyA = v).
                AddMapping(FLAG_B, a => a.PropertyB, (a, v) => a.PropertyB = v).
                AddMapping(FLAG_C, a => a.PropertyC, (a, v) => a.PropertyC = v);
        }

        [Fact]
        public void FlagsConverter_GetFlags_RequiresObject()
        {
            var adapter = TestFlags.GetAdapter();
            Assert.Throws<ArgumentNullException>(() => adapter.GetFlags(null));
        }

        [Theory]
        [MemberData(nameof(FlagsConverter_GetFlags_Data))]
        public void FlagsConverter_GetFlags_Tests(TestFlags obj, uint expected)
        {
            var adapter = TestFlags.GetAdapter();
            Assert.Equal(expected, adapter.GetFlags(obj));
        }

        public static IEnumerable<object[]> FlagsConverter_GetFlags_Data => new object[][]
        {
            new object[] { new TestFlags(false,false,false), 0 },
            new object[] { new TestFlags(false,false,true), TestFlags.FLAG_C },
            new object[] { new TestFlags(false,true,false), TestFlags.FLAG_B },
            new object[] { new TestFlags(false,true,true), TestFlags.FLAG_B | TestFlags.FLAG_C },
            new object[] { new TestFlags(true,false,false), TestFlags.FLAG_A },
            new object[] { new TestFlags(true,false,true), TestFlags.FLAG_A | TestFlags.FLAG_C },
            new object[] { new TestFlags(true,true,false), TestFlags.FLAG_A | TestFlags.FLAG_B },
            new object[] { new TestFlags(true,true,true), TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C }
        };

        [Fact]
        public void FlagsConverter_SetFlags_RequiresObject()
        {
            var adapter = TestFlags.GetAdapter();
            Assert.Throws<ArgumentNullException>(() => adapter.SetFlags(null, 0));
        }

        [Theory]
        [MemberData(nameof(FlagsConverter_SetFlags_Data))]
        public void FlagsConverter_SetFlags_Tests(TestFlags obj, uint setFlags, uint expected)
        {
            var adapter = TestFlags.GetAdapter();
            adapter.SetFlags(obj, setFlags);
            Assert.Equal(expected, adapter.GetFlags(obj));
        }

        public static IEnumerable<object[]> FlagsConverter_SetFlags_Data => new object[][]
        {
            new object[] { new TestFlags(false, false, false), 0, 0 },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_C, TestFlags.FLAG_C },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_B, TestFlags.FLAG_B },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_B | TestFlags.FLAG_C, TestFlags.FLAG_B | TestFlags.FLAG_C },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A, TestFlags.FLAG_A },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A | TestFlags.FLAG_C, TestFlags.FLAG_A | TestFlags.FLAG_C },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A | TestFlags.FLAG_B, TestFlags.FLAG_A | TestFlags.FLAG_B },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C, TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C },

            new object[] { new TestFlags(true, true, true), 0, 0 },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_C, TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_B, TestFlags.FLAG_B },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_B | TestFlags.FLAG_C, TestFlags.FLAG_B | TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A, TestFlags.FLAG_A },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A | TestFlags.FLAG_C, TestFlags.FLAG_A | TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A | TestFlags.FLAG_B, TestFlags.FLAG_A | TestFlags.FLAG_B },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C, TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C }
        };

        [Fact]
        public void FlagsConverter_HasFlags_RequiresObject()
        {
            var adapter = TestFlags.GetAdapter();
            Assert.Throws<ArgumentNullException>(() => adapter.HasFlags(null, 0));
        }

        [Theory]
        [MemberData(nameof(FlagsConverter_HasFlags_Data))]
        public void FlagsConverter_HasFlags_Tests(TestFlags obj, uint testFlags, bool expected)
        {
            var adapter = TestFlags.GetAdapter();
            Assert.Equal(expected, adapter.HasFlags(obj, testFlags));
        }

        public static IEnumerable<object[]> FlagsConverter_HasFlags_Data => new object[][]
        {
            new object[] { new TestFlags(false, false, false), 0, true },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_C, false },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_B, false },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_B | TestFlags.FLAG_C, false },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A, false },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A | TestFlags.FLAG_C, false },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A | TestFlags.FLAG_B, false },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C, false },

            new object[] { new TestFlags(true, true, true), 0, true },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_C, true },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_B, true },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_B | TestFlags.FLAG_C, true },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A, true },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A | TestFlags.FLAG_C, true },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A | TestFlags.FLAG_B, true },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C, true }
        };

        [Fact]
        public void FlagsConverter_ClearFlags_RequiresObject()
        {
            var adapter = TestFlags.GetAdapter();
            Assert.Throws<ArgumentNullException>(() => adapter.ClearFlags(null));
        }

        [Fact]
        public void FlagsConverter_ClearFlags_ResetsFlags()
        {
            var adapter = TestFlags.GetAdapter();
            var obj = new TestFlags(true, true, true);
            adapter.ClearFlags(obj);
            Assert.False(obj.PropertyA);
            Assert.False(obj.PropertyB);
            Assert.False(obj.PropertyC);
        }

        [Fact]
        public void FlagsConverter_AddFlags_RequiresObject()
        {
            var adapter = TestFlags.GetAdapter();
            Assert.Throws<ArgumentNullException>(() => adapter.AddFlags(null, 0));
        }

        [Theory]
        [MemberData(nameof(FlagsConverter_AddFlags_Data))]
        public void FlagsConverter_AddFlags_Tests(TestFlags obj, uint setFlags, uint expected)
        {
            var adapter = TestFlags.GetAdapter();
            adapter.AddFlags(obj, setFlags);
            Assert.Equal(expected, adapter.GetFlags(obj));
        }

        public static IEnumerable<object[]> FlagsConverter_AddFlags_Data => new object[][]
        {
            new object[] { new TestFlags(false, false, false), 0, 0 },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_C, TestFlags.FLAG_C },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_B, TestFlags.FLAG_B },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_B | TestFlags.FLAG_C, TestFlags.FLAG_B | TestFlags.FLAG_C },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A, TestFlags.FLAG_A },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A | TestFlags.FLAG_C, TestFlags.FLAG_A | TestFlags.FLAG_C },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A | TestFlags.FLAG_B, TestFlags.FLAG_A | TestFlags.FLAG_B },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C, TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C },

            new object[] { new TestFlags(true, true, true), 0, TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_C, TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_B, TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_B | TestFlags.FLAG_C, TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A, TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A | TestFlags.FLAG_C, TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A | TestFlags.FLAG_B, TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C, TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C }
        };

        [Fact]
        public void FlagsConverter_DropFlags_RequiresObject()
        {
            var adapter = TestFlags.GetAdapter();
            Assert.Throws<ArgumentNullException>(() => adapter.DropFlags(null, 0));
        }

        [Theory]
        [MemberData(nameof(FlagsConverter_DropFlags_Data))]
        public void FlagsConverter_DropFlags_Tests(TestFlags obj, uint dropFlags, uint expected)
        {
            var adapter = TestFlags.GetAdapter();
            adapter.DropFlags(obj, dropFlags);
            Assert.Equal(expected, adapter.GetFlags(obj));
        }

        public static IEnumerable<object[]> FlagsConverter_DropFlags_Data => new object[][]
        {
            new object[] { new TestFlags(false, false, false), 0, 0 },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_C, 0 },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_B, 0 },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_B | TestFlags.FLAG_C, 0 },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A, 0 },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A | TestFlags.FLAG_C, 0 },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A | TestFlags.FLAG_B, 0 },
            new object[] { new TestFlags(false, false, false), TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C, 0 },

            new object[] { new TestFlags(true, true, true), 0, TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_C, TestFlags.FLAG_A | TestFlags.FLAG_B },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_B, TestFlags.FLAG_A | TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_B | TestFlags.FLAG_C, TestFlags.FLAG_A },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A, TestFlags.FLAG_B | TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A | TestFlags.FLAG_C, TestFlags.FLAG_B },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A | TestFlags.FLAG_B, TestFlags.FLAG_C },
            new object[] { new TestFlags(true, true, true), TestFlags.FLAG_A | TestFlags.FLAG_B | TestFlags.FLAG_C, 0 }
        };

        [Theory]
        [MemberData(nameof(FlagsConverter_IsFlag_ChecksWhetherNumberIsFlag_Data))]
        public void FlagsConverter_IsFlag_ChecksWhetherNumberIsFlag(uint testValue, bool expected)
        {
            Assert.Equal(expected, FlagsConverter<TestFlags>.IsFlag(testValue));
        }

        public static IEnumerable<object[]> FlagsConverter_IsFlag_ChecksWhetherNumberIsFlag_Data => new object[][]
        {
            new object[] { 0x000, false },
            new object[] { 0x001, true },
            new object[] { 0x002, true },
            new object[] { 0x003, false },
            new object[] { 0x004, true },
            new object[] { 0x005, false },
            new object[] { 0x006, false },
            new object[] { 0x007, false },
            new object[] { 0x008, true },
            new object[] { 0x009, false },
            new object[] { 0x010, true },
        };

    }
}

// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
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
    public class FileSystemTests : TestFixture
    {
        [Fact]
        public void FileSystem_IsAtty_ReflectsContextState()
        {
            Assert.Equal(MainApplicationContext.Current.IsAtty, VirtualConsole.IsAtty());
        }
    }
}

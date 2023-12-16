// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility
{
    public class EmptyExtensionProviderTests
    {
        [Fact]
        public void EmptyExtensionProvider_Current_KeepsInstance()
        {
            Assert.NotNull(EmptyExtensionProvider.Current);
            Assert.IsType<EmptyExtensionProvider>(EmptyExtensionProvider.Current);
        }

        [Fact]
        public void EmptyExtensionProvider_CurrentFactory_ReturnsCurrent()
        {
            Assert.Equal(EmptyExtensionProvider.Current, EmptyExtensionProvider.CurrentFactory());
        }

        [Fact]
        public void EmptyExtensionProvider_CreateExtendedSession_ReturnsNull()
        {
            Assert.Null(EmptyExtensionProvider.Current.CreateExtendedSession());
        }

    }
}

// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Scopus
{
    public class EmptyScopeTests : TestFixture
    {
        [Fact]
        public void EmptyScope_Description_ReturnsEmptyConstant()
        {
            Assert.Equal(EmptyScope.EmptyDescription, new EmptyScope().Description);
        }

        [Fact]
        public void EmptyScope_Lookup_ReturnsNull()
        {
            Assert.Null(new EmptyScope().Lookup(SymbolKindEnum.COMMAND, "none"));
        }

        [Fact]
        public void EmptyScope_TypeRequired_IsFalse()
        {
            Assert.False(new EmptyScope().TypeRequired);
        }

        [Fact]
        public void EmptyScope_TypeContext_IsVoid()
        {
            Assert.Equal(ValueTypeEnum.Void, new EmptyScope().TypeContext);
        }

        [Fact]
        public void EmptyScope_Define_DoesNothing()
        {
            var scope = new EmptyScope();
            scope.Define(SymbolKindEnum.COMMAND, "none", null); // just to check that there is no exceptions
        }
    }
}

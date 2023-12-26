// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
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
    public class ContextScopeTests : TestFixture
    {
        [Fact]
        public void ContextScope_Constructor_PopulatesParentTypeContextAndIsRequired()
        {
            MockScope parent = new MockScope();
            ContextScope contextScope = new ContextScope(parent, ValueTypeEnum.DateTime, true);
            Assert.Equal(parent, contextScope.Parent);
            Assert.Equal(ValueTypeEnum.DateTime, contextScope.TypeContext);
            Assert.True(contextScope.IsRequired);
        }

        [Fact]
        public void ContextScope_Description_ReturnsParentDescription()
        {
            MockScope parent = new MockScope("special-desc");
            ContextScope contextScope = new ContextScope(parent, ValueTypeEnum.DateTime, true);
            Assert.Equal("special-desc", contextScope.Description);
        }

    }
}

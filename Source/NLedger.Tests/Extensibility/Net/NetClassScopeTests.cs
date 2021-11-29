// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Extensibility.Net;
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility.Net
{
    public class NetClassScopeTests : TestFixture
    {
        [Fact]
        public void NetClassScope_Constructor_PopulatesProperties()
        {
            var valueConverter = new ValueConverter();
            var type = typeof(ASCIIEncoding);

            var netClassScope = new NetClassScope(type, valueConverter);

            Assert.Equal(type, netClassScope.ClassType);
            Assert.Equal(valueConverter, netClassScope.ValueConverter);
        }

        [Fact]
        public void NetClassScope_Constructor_GetsClassByName()
        {
            var valueConverter = new ValueConverter();
            var type = typeof(ASCIIEncoding);

            var netClassScope = new NetClassScope(type.FullName, valueConverter);

            Assert.Equal(type, netClassScope.ClassType);
            Assert.Equal(valueConverter, netClassScope.ValueConverter);
        }

        [Fact]
        public void NetClassScope_Description_ReturnsClassName()
        {
            var type = typeof(ASCIIEncoding);
            var netClassScope = new NetClassScope(type, new ValueConverter());
            Assert.Equal(type.FullName, netClassScope.Description);
        }

        [Fact]
        public void NetClassScope_Lookup_ReturnsClassMethodByName()
        {
            var netClassScope = new NetClassScope(typeof(Encoding), new ValueConverter());
            var exprOp = netClassScope.Lookup(SymbolKindEnum.FUNCTION, "GetEncoding");

            var result = exprOp.Call(Value.StringValue(ASCIIEncoding.ASCII.EncodingName), new EmptyScope());
            Assert.Equal(Encoding.ASCII.ToString(), result.AsString);
        }

        [Fact]
        public void NetClassScope_Lookup_ReturnsPropertyByName()
        {
            var netClassScope = new NetClassScope(typeof(Encoding), new ValueConverter());
            var exprOp = netClassScope.Lookup(SymbolKindEnum.FUNCTION, "Default");

            var result = exprOp.Call(new Value(), new EmptyScope());
            Assert.Equal(Encoding.Default.ToString(), result.AsString);
        }

    }
}

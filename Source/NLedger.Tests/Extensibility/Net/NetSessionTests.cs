// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Extensibility.Net;
using NLedger.Scopus;
using NLedger.Textual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility.Net
{
    public class NetSessionTests : TestFixture
    {
        [Fact]
        public void NetSession_Constructor_PopulatesProperties()
        {
            var namespaceResolver = new NamespaceResolver();
            var valueConverter = new ValueConverter();

            var netSession = new NetSession(namespaceResolver, valueConverter);

            Assert.Equal(namespaceResolver, netSession.NamespaceResolver);
            Assert.Equal(valueConverter, netSession.ValueConverter);
            Assert.True(netSession.RootNamespace.IsRoot);
        }

        [Fact]
        public void NetSession_DefineGlobal_AddsGlobalFunctorsForObjects()
        {
            var obj = new Object();
            var netSession = new NetSession(new NamespaceResolver(), new ValueConverter());

            netSession.DefineGlobal("globalObj", obj);

            var func = netSession.Lookup(SymbolKindEnum.FUNCTION, "globalObj");
            var val = func.AsFunction(new EmptyScope());

            Assert.Equal(obj, val.AsAny());
        }

        [Fact]
        public void NetSession_ImportOption_AssembliesLoadAll()
        {
            var netSession = new NetSession(new NamespaceResolver(), new ValueConverter());

            Assert.False(netSession.RootNamespace.NamespaceResolver.IsNamespace("System"));
            netSession.ImportOption("assemblies");
            Assert.True(netSession.RootNamespace.NamespaceResolver.IsNamespace("System"));
        }

        [Fact]
        public void NetSession_ImportOption_AssembliesDoesNotRequireArguments()
        {
            var netSession = new NetSession(new NamespaceResolver(), new ValueConverter());
            Assert.Throws<ParseError>(() => netSession.ImportOption("assemblies something"));
        }

        [Fact]
        public void NetSession_ImportOption_AssemblyLoadsOne()
        {
            var netSession = new NetSession(new NamespaceResolver(), new ValueConverter());

            Assert.False(netSession.RootNamespace.NamespaceResolver.IsNamespace("NLedger"));
            netSession.ImportOption($"assembly NLedger");
            Assert.True(netSession.RootNamespace.NamespaceResolver.IsNamespace("NLedger"));
        }

        [Fact]
        public void NetSession_ImportOption_AssembliesRequireSingleArgument()
        {
            var netSession = new NetSession(new NamespaceResolver(), new ValueConverter());
            Assert.Throws<ParseError>(() => netSession.ImportOption("assembly"));
            Assert.Throws<ParseError>(() => netSession.ImportOption("assembly two arguments"));
        }

        [Fact]
        public void NetSession_ImportOption_FileLoadsOne()
        {
            var netSession = new NetSession(new NamespaceResolver(), new ValueConverter());

            Assert.False(netSession.RootNamespace.NamespaceResolver.IsNamespace("NLedger"));
            netSession.ImportOption($"file 'NLedger.dll'");
            Assert.True(netSession.RootNamespace.NamespaceResolver.IsNamespace("NLedger"));
        }

        [Fact]
        public void NetSession_ImportOption_FileRequireSingleArgument()
        {
            var netSession = new NetSession(new NamespaceResolver(), new ValueConverter());
            Assert.Throws<ParseError>(() => netSession.ImportOption("file"));
            Assert.Throws<ParseError>(() => netSession.ImportOption("file two arguments"));
        }

        [Fact]
        public void NetSession_ImportOption_AliasCreatesGlobalFunctor()
        {
            var namespaceResolver = new NamespaceResolver();
            namespaceResolver.AddAssembly(typeof(System.Text.ASCIIEncoding).Assembly);

            var netSession = new NetSession(namespaceResolver, new ValueConverter());

            Assert.Null(netSession.Lookup(SymbolKindEnum.FUNCTION, "ASCII"));
            netSession.ImportOption($"alias ASCII for System.Text.ASCIIEncoding.Default.EncodingName");
            Assert.NotNull(netSession.Lookup(SymbolKindEnum.FUNCTION, "ASCII"));
        }

    }
}

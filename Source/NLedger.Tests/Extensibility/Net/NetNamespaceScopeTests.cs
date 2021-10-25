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
    public class NetNamespaceScopeTests : TestFixture
    {
        [Fact]
        public void NetNamespaceScope_Constructor_PopulatesPropertiesForRoot()
        {
            var namespaceResolver = new NamespaceResolver();
            var valueConverter = new ValueConverter();

            var netNamespaceScope = new NetNamespaceScope(namespaceResolver, valueConverter);

            Assert.Equal(namespaceResolver, netNamespaceScope.NamespaceResolver);
            Assert.Equal(valueConverter, netNamespaceScope.ValueConverter);
            Assert.Null(netNamespaceScope.Name);
            Assert.Null(netNamespaceScope.ParentNamespace);
            Assert.True(netNamespaceScope.IsRoot);
            Assert.Equal("{Root}", netNamespaceScope.Description);
        }

        [Fact]
        public void NetNamespaceScope_Lookup_ReturnsChildNamespaceScope()
        {
            var namespaceResolver = new NamespaceResolver();
            namespaceResolver.AddAssembly(typeof(Encoding).Assembly);
            var valueConverter = new ValueConverter();

            var netNamespaceScope = new NetNamespaceScope(namespaceResolver, valueConverter);
            var exprOp = netNamespaceScope.Lookup(SymbolKindEnum.FUNCTION, "System");
            var childScope = exprOp.AsValue.AsScope as NetNamespaceScope;

            Assert.Equal(namespaceResolver, childScope.NamespaceResolver);
            Assert.Equal(valueConverter, childScope.ValueConverter);
            Assert.Equal("System", childScope.Name);
            Assert.Equal(netNamespaceScope, childScope.ParentNamespace);
            Assert.False(childScope.IsRoot);
            Assert.Equal("System", childScope.Description);
        }

        [Fact]
        public void NetNamespaceScope_Lookup_ReturnsChildClassScope()
        {
            var namespaceResolver = new NamespaceResolver();
            namespaceResolver.AddAssembly(typeof(Encoding).Assembly);
            var valueConverter = new ValueConverter();

            var netNamespaceScope = new NetNamespaceScope(namespaceResolver, valueConverter);
            var exprOp = netNamespaceScope.Lookup(SymbolKindEnum.FUNCTION, "System");
            var childScopeSystem = exprOp.AsValue.AsScope as NetNamespaceScope;

            var exprOp1 = childScopeSystem.Lookup(SymbolKindEnum.FUNCTION, "Text");
            var childScopeText = exprOp1.AsValue.AsScope as NetNamespaceScope;

            var exprOp2 = childScopeText.Lookup(SymbolKindEnum.FUNCTION, "Encoding");
            var classScope = exprOp2.AsValue.AsScope as NetClassScope;

            Assert.Equal(typeof(Encoding), classScope.ClassType);
            Assert.Equal(valueConverter, classScope.ValueConverter);
            Assert.Equal("System.Text.Encoding", classScope.Description);
        }

    }
}

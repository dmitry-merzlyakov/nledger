using NLedger.Extensibility.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility.Net
{
    public class NetNamespaceScopeTests
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
    }
}

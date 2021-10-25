using NLedger.Extensibility.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility.Net
{
    public class NamespaceResolverTests
    {
        [Fact]
        public void NamespaceResolver_Constructor_PopulatesProperties()
        {
            Assert.False(new NamespaceResolver().GlobalScan);
            Assert.False(new NamespaceResolver(false).GlobalScan);
            Assert.True(new NamespaceResolver(true).GlobalScan);
        }

        [Fact]
        public void NamespaceResolver_IsClass_ChecksClassName()
        {
            var resolver = new NamespaceResolver();
            resolver.AddAssembly(Assembly.GetExecutingAssembly());

            Assert.True(resolver.IsClass("NLedger.Tests.Extensibility.Net.NamespaceResolverTests"));
            Assert.False(resolver.IsClass("NLedger.Tests.Extensibility.Net.IncorrectClassName"));
        }

        [Fact]
        public void NamespaceResolver_IsClass_ChecksNamespace()
        {
            var resolver = new NamespaceResolver();
            resolver.AddAssembly(Assembly.GetExecutingAssembly());

            Assert.True(resolver.IsNamespace("NLedger.Tests"));
            Assert.True(resolver.IsNamespace("NLedger.Tests.Extensibility.Net"));
            Assert.False(resolver.IsNamespace("NLedger.Tests.Extensibility.Net.NamespaceResolverTests")); // Class name is not a namespace
            Assert.False(resolver.IsNamespace("IncorrectNamespace")); 
        }

        [Fact]
        public void NamespaceResolver_GetClassType_ReturnsType()
        {
            var resolver = new NamespaceResolver();
            resolver.AddAssembly(Assembly.GetExecutingAssembly());

            var type = resolver.GetClassType("NLedger.Tests.Extensibility.Net.NamespaceResolverTests");
            Assert.Equal(typeof(NamespaceResolverTests), type);
        }

        [Fact]
        public void NamespaceResolver_IsNamespace_ChecksWhetherGivenStringIsNamespace()
        {
            var resolver = new NamespaceResolver();
            resolver.AddAssembly(Assembly.GetExecutingAssembly());

            Assert.True(resolver.IsNamespace("NLedger.Tests.Extensibility.Net"));
            Assert.False(resolver.IsNamespace("Unknown.namespace"));
        }

        [Fact]
        public void NamespaceResolver_ContainsAssembly_ChecksWhetherAssemblyWasScanned()
        {
            var resolver = new NamespaceResolver();
            var assm = Assembly.GetExecutingAssembly();
            resolver.AddAssembly(assm);

            Assert.True(resolver.ContainsAssembly(assm));
            Assert.False(resolver.ContainsAssembly(typeof(NLedger.Balance).Assembly));
        }

        [Fact]
        public void NamespaceResolver_AddAllAssemblies_ScansAllAppDomainAssemblies()
        {
            var resolver = new NamespaceResolver();
            var assm = Assembly.GetExecutingAssembly();

            resolver.AddAllAssemblies();

            Assert.True(resolver.ContainsAssembly(assm));
            Assert.True(resolver.ContainsAssembly(typeof(NLedger.Balance).Assembly));
        }

        [Fact]
        public void NamespaceResolver_AddAssembly_ScansSingleAssembly()
        {
            var resolver = new NamespaceResolver();
            var assm = Assembly.GetExecutingAssembly();

            Assert.False(resolver.ContainsAssembly(assm));
            resolver.AddAssembly(assm);
            Assert.True(resolver.ContainsAssembly(assm));
        }

        [Fact]
        public void NamespaceResolver_AddAssembly_ScansAssemblyTypes()
        {
            var resolver = new NamespaceResolver();

            Assert.False(resolver.IsNamespace("System"));
            Assert.False(resolver.IsNamespace("System.IO"));
            Assert.False(resolver.IsClass("System.IO.File"));

            resolver.AddAssembly(typeof(System.IO.File).Assembly);

            Assert.True(resolver.IsNamespace("System"));
            Assert.True(resolver.IsNamespace("System.IO"));
            Assert.True(resolver.IsClass("System.IO.File"));
        }

    }
}

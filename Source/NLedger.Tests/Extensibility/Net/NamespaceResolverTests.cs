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

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

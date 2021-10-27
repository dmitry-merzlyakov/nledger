using NLedger.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility
{
    public class ExtensionProviderSelectorTests
    {
        [Fact]
        public void ExtensionProviderSelector_AddProvider_RequiresNameAndFactory()
        {
            var selector = new ExtensionProviderSelector();
            Assert.Throws<ArgumentNullException>(() => selector.AddProvider(null, () => new EmptyExtensionProvider()));
            Assert.Throws<ArgumentNullException>(() => selector.AddProvider("name", null));
        }

        [Fact]
        public void ExtensionProviderSelector_AddProvider_AddsFactory()
        {
            Func<EmptyExtensionProvider> factory = () => new EmptyExtensionProvider();
            var selector = new ExtensionProviderSelector();
            selector.AddProvider("empty", factory);
            Assert.Equal(factory, selector.Providers["empty"]);
        }

        [Fact]
        public void ExtensionProviderSelector_AddProvider_DeclinesDuplicatedNames()
        {
            var selector = new ExtensionProviderSelector();
            selector.AddProvider("empty", () => new EmptyExtensionProvider());
            Assert.Throws<InvalidOperationException>(() => selector.AddProvider("empty", () => new EmptyExtensionProvider()));
        }

        [Fact]
        public void ExtensionProviderSelector_GetProvider_ReturnsNullIfNoName()
        {
            var selector = new ExtensionProviderSelector();
            Assert.Null(selector.GetProvider(null));
        }

        [Fact]
        public void ExtensionProviderSelector_GetProvider_ReturnsFactory()
        {
            Func<EmptyExtensionProvider> factory = () => new EmptyExtensionProvider();
            var selector = new ExtensionProviderSelector();
            selector.AddProvider("empty", factory);
            Assert.Equal(factory, selector.GetProvider("empty"));
        }

        [Fact]
        public void ExtensionProviderSelector_GetProvider_FailsIfProviderNotFound()
        {
            Func<EmptyExtensionProvider> factory = () => new EmptyExtensionProvider();
            var selector = new ExtensionProviderSelector();
            selector.AddProvider("empty", factory);
            Assert.Throws<InvalidOperationException>(() => selector.GetProvider("unknown-provider"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility
{
    public class EmptyExtensionProvider : IExtensionProvider
    {
        public static readonly EmptyExtensionProvider Current = new EmptyExtensionProvider();
        public static readonly Func<IExtensionProvider> CurrentFactory = () => Current;

        public ExtendedSession CreateExtendedSession() => null;
    }
}

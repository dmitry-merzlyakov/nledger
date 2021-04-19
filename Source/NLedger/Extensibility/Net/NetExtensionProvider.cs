using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public class NetExtensionProvider : IExtensionProvider
    {
        public NetExtensionProvider(Func<NetSession> netSessionFactory = null)
        {
            NetSessionFactory = netSessionFactory ?? (() => new NetSession(new NamespaceResolver(), new ValueConverter()));
        }

        public Func<NetSession> NetSessionFactory { get; }

        public ExtendedSession CreateExtendedSession()
        {
            return NetSessionFactory();
        }
    }
}

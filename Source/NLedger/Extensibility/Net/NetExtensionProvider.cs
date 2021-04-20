using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public class NetExtensionProvider : IExtensionProvider
    {
        public NetExtensionProvider(Func<NetSession> netSessionFactory = null, Action<NetSession> configureAction = null)
        {
            NetSessionFactory = netSessionFactory ?? (() => new NetSession(new NamespaceResolver(), new ValueConverter()));
            ConfigureAction = configureAction;
        }

        public Func<NetSession> NetSessionFactory { get; }
        public Action<NetSession> ConfigureAction { get; }

        public ExtendedSession CreateExtendedSession()
        {
            var extendedSession = NetSessionFactory();
            ConfigureAction?.Invoke(extendedSession);
            return extendedSession;
        }
    }
}

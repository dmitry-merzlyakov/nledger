using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public class NetExtensionProvider : IExtensionProvider
    {
        public ExtendedSession CreateExtendedSession()
        {
            return new NetSession();
        }
    }
}

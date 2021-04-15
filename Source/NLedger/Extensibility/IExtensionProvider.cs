using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility
{
    public interface IExtensionProvider
    {
        ExtendedSession CreateExtendedSession();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public interface INamespaceResolver
    {
        bool IsNamespace(string name);
        bool IsClass(string name);
        void AddAssembly(Assembly assembly);
    }
}

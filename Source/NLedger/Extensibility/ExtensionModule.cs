using NLedger.Expressions;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility
{
    /// <summary>
    /// Reflects Ledger Python bridge component: python_module_t
    /// </summary>
    public class ExtensionModule : Scope
    {
        public ExtensionModule(string moduleName, IExtensionProvider extensionProvider)
        {
            if (String.IsNullOrWhiteSpace(moduleName))
                throw new ArgumentNullException(nameof(moduleName));

            ExtensionProvider = extensionProvider ?? throw new ArgumentNullException(nameof(extensionProvider));
            ModuleName = moduleName;
            ExtensionProvider.ImportModule(this);
        }

        public IExtensionProvider ExtensionProvider { get; }
        public string ModuleName { get; }
        public IDictionary<string, object> ModuleGlobals { get; } = new Dictionary<string, object>();

        public override string Description => ModuleName;

        public void DefineGlobal(string name, object obj)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            ModuleGlobals[name] = obj;
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            throw new NotImplementedException();
        }
    }
}

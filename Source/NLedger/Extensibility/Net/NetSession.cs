using NLedger.Expressions;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public class NetSession : ExtendedSession
    {
        public NetSession(INamespaceResolver namespaceResolver, IValueConverter valueConverter)
        {
            NamespaceResolver = namespaceResolver ?? throw new ArgumentNullException(nameof(namespaceResolver));
            ValueConverter = valueConverter ?? throw new ArgumentNullException(nameof(valueConverter));
            RootNamespace = new NetNamespaceScope(NamespaceResolver, ValueConverter);
        }

        public INamespaceResolver NamespaceResolver { get; }
        public IValueConverter ValueConverter { get; }
        public NetNamespaceScope RootNamespace { get; }

        public override void DefineGlobal(string name, object value)
        {
            //throw new NotImplementedException();
        }

        public override void Eval(string code, ExtensionEvalModeEnum mode)
        {
            //throw new NotImplementedException();
        }

        public override void ImportOption(string name)
        {
            //throw new NotImplementedException();
        }

        public override void Initialize()
        {
            //throw new NotImplementedException();
        }

        public override bool IsInitialized()
        {
            return true;
        }

        protected override ExprOp LookupFunction(string name)
        {
            //TODO check globals
            return RootNamespace.Lookup(Scopus.SymbolKindEnum.FUNCTION, name);
            //return ExprOp.WrapValue(Value.ScopeValue(new NetModule()));
        }
    }
}

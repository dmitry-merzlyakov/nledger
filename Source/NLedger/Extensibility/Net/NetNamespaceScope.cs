using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public class NetNamespaceScope : Scope
    {
        public NetNamespaceScope(INamespaceResolver namespaceResolver)
        {
            _NamespaceResolver = namespaceResolver ?? throw new ArgumentNullException(nameof(namespaceResolver));
        }

        protected NetNamespaceScope(NetNamespaceScope parentNamespace, string name)
        {
            if(String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            ParentNamespace = ParentNamespace ?? throw new ArgumentNullException(nameof(parentNamespace));
        }

        public string Name { get; }

        public NetNamespaceScope ParentNamespace { get; }

        public bool IsRoot => ParentNamespace == null;

        public INamespaceResolver NamespaceResolver => _NamespaceResolver ?? ParentNamespace.NamespaceResolver;

        public override string Description => IsRoot ? "." : Name;

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            if(kind == SymbolKindEnum.FUNCTION)
            {
                Scope scope;
                if (!Children.TryGetValue(name, out scope))
                {
                    var fullName = FullName + "." + name;

                    if (NamespaceResolver.IsNamespace(fullName))
                        scope = new NetNamespaceScope(this, name);
                    else
                    {
                        if (NamespaceResolver.IsClass(fullName))
                            scope = new NetClassScope(fullName);
                        else
                            return null;
                    }

                    Children.Add(name, scope);
                }
                return ExprOp.WrapValue(Value.ScopeValue(scope));
            }

            return null;
        }

        private string FullName => IsRoot ? String.Empty : ParentNamespace?.FullName + "." + Name;

        private readonly INamespaceResolver _NamespaceResolver;
        private readonly IDictionary<string, Scope> Children = new Dictionary<string, Scope>();
    }
}

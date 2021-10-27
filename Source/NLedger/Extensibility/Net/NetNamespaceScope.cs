// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
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
        public NetNamespaceScope(INamespaceResolver namespaceResolver, IValueConverter valueConverter)
        {
            _NamespaceResolver = namespaceResolver ?? throw new ArgumentNullException(nameof(namespaceResolver));
            _ValueConverter = valueConverter ?? throw new ArgumentNullException(nameof(valueConverter));
        }

        protected NetNamespaceScope(NetNamespaceScope parentNamespace, string name)
        {
            if(String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            ParentNamespace = parentNamespace ?? throw new ArgumentNullException(nameof(parentNamespace));
        }

        public string Name { get; }

        public NetNamespaceScope ParentNamespace { get; }

        public bool IsRoot => ParentNamespace == null;

        public INamespaceResolver NamespaceResolver => _NamespaceResolver ?? ParentNamespace.NamespaceResolver;
        public IValueConverter ValueConverter => _ValueConverter ?? ParentNamespace.ValueConverter;

        public override string Description => IsRoot ? "{Root}" : Name;

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            if(kind == SymbolKindEnum.FUNCTION)
            {
                Scope scope;
                if (!Children.TryGetValue(name, out scope))
                {
                    var fullName = GetFullName(name);

                    if (NamespaceResolver.IsNamespace(fullName))
                        scope = new NetNamespaceScope(this, name);
                    else
                    {
                        if (NamespaceResolver.IsClass(fullName))
                            scope = new NetClassScope(NamespaceResolver.GetClassType(fullName), ValueConverter);
                        else
                            return null;
                    }

                    Children.Add(name, scope);
                }
                return ExprOp.WrapValue(Value.ScopeValue(scope));
            }

            return null;
        }

        public string GetFullName()
        {
            if (IsRoot)
                return String.Empty;

            var parent = ParentNamespace.GetFullName();
            return String.IsNullOrEmpty(parent) ? Name : parent + "." + Name;
        }

        public string GetFullName(string name)
        {
            var parent = GetFullName();
            return String.IsNullOrEmpty(parent) ? name : parent + "." + name;
        }

        private readonly INamespaceResolver _NamespaceResolver;
        private readonly IValueConverter _ValueConverter;
        private readonly IDictionary<string, Scope> Children = new Dictionary<string, Scope>();
    }
}

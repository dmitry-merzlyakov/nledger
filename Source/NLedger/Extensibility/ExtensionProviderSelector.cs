// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility
{
    public sealed class ExtensionProviderSelector
    {
        public IDictionary<string, Func<IExtensionProvider>> Providers { get; set; } = new Dictionary<string, Func<IExtensionProvider>>(StringComparer.InvariantCultureIgnoreCase);

        public ExtensionProviderSelector AddProvider(string name, Func<IExtensionProvider> factory)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (Providers.ContainsKey(name))
                throw new InvalidOperationException($"Provider '{name}' already exists");

            Providers.Add(name, factory);
            return this;
        }

        public Func<IExtensionProvider> GetProvider(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return null;

            Func<IExtensionProvider> factory;
            if (!Providers.TryGetValue(name, out factory))
                throw new InvalidOperationException($"No extension provider with name '{name}'");

            return factory;
        }
    }
}

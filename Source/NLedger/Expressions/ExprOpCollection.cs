// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Expressions
{
    public sealed class ExprOpCollection
    {
        public void MakeFunctor(string name, ExprFunc exprFunc, SymbolKindEnum symbol = SymbolKindEnum.FUNCTION)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            if (exprFunc == null)
                throw new ArgumentNullException("exprFunc");

            // Search optimization
            if (name.EndsWith("_"))
                name = name.Remove(name.Length - 1);

            IDictionary<string, ExprOp> items = GetOrCreate(symbol);

            ExprOp op;
            if (items.TryGetValue(name, out op))
                throw new InvalidOperationException(String.Format("Functor '{0}' is already defined", name));

            items.Add(name, ExprOp.WrapFunctor(exprFunc));
        }

        public void MakeOptionFunctors(OptionCollection options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            Tuple<OptionCollection,IDictionary<Option,ExprOp>> optionItems = new Tuple<OptionCollection,IDictionary<Option,ExprOp>>
                (options, new Dictionary<Option,ExprOp>());
            foreach (Option option in options.Options)
                optionItems.Item2[option] = ExprOp.WrapFunctor(scope => option.Call((CallScope)scope));

            LookupKindOptionItems.Add(SymbolKindEnum.FUNCTION, optionItems);
        }

        public void MakeOptionHandlers(OptionCollection options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            Tuple<OptionCollection,IDictionary<Option,ExprOp>> optionItems = new Tuple<OptionCollection,IDictionary<Option,ExprOp>>
                (options, new Dictionary<Option,ExprOp>());
            foreach (Option option in options.Options)
                optionItems.Item2[option] = ExprOp.WrapFunctor(scope => option.Handler((CallScope)scope));

            LookupKindOptionItems.Add(SymbolKindEnum.OPTION, optionItems);
        }

        public ExprOp Lookup(SymbolKindEnum kind, string name, object parent)
        {
            IDictionary<string, ExprOp> items;
            if (LookupKindItems.TryGetValue(kind, out items))
            {
                name = name.Replace('-', '_'); // search optimization
                ExprOp op;
                if (items.TryGetValue(name, out op))
                    return op;
            }

            Tuple<OptionCollection,IDictionary<Option,ExprOp>> optionItems;
            if (LookupKindOptionItems.TryGetValue(kind, out optionItems))
            {
                Option option = optionItems.Item1.LookupOption(name, parent);
                if (option != null)
                    return optionItems.Item2[option];
            }

            return null;
        }

        private IDictionary<string, ExprOp> GetOrCreate(SymbolKindEnum kind)
        {
            IDictionary<string, ExprOp> items;
            if (!LookupKindItems.TryGetValue(kind, out items))
            {
                items = new Dictionary<string, ExprOp>();
                LookupKindItems.Add(kind, items);
            }
            return items;
        }

        private readonly IDictionary<SymbolKindEnum, IDictionary<string, ExprOp>> LookupKindItems = new Dictionary<SymbolKindEnum, IDictionary<string, ExprOp>>();
        private readonly IDictionary<SymbolKindEnum, Tuple<OptionCollection,IDictionary<Option,ExprOp>>> LookupKindOptionItems = new Dictionary<SymbolKindEnum, Tuple<OptionCollection,IDictionary<Option,ExprOp>>>();
    }
}

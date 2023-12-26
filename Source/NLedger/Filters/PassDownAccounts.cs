// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Chain;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Filters
{
    public class PassDownAccounts : AccountHandler
    {
        public PassDownAccounts(AccountHandler handler, IEnumerable<Account> iter, Predicate pred = null, Scope context = null)
            : base(handler)
        {
            Pred = pred;
            Context = context;

            foreach(Account account in iter)
            {
                if (Pred == null)
                {
                    Handle(account);
                }
                else
                {
                    BindScope boundScope = new BindScope(context, account);
                    if (Pred.Calc(boundScope).Bool)
                        Handle(account);
                }
            }

            Flush();
        }

        public Predicate Pred { get; private set; }
        public Scope Context { get; private set; }

        public override void Clear()
        {
            if (Pred != null)
                Pred.MarkUncomplited();

            base.Clear();
        }
    }
}

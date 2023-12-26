// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLedger.Utility;
using NLedger.Accounts;

namespace NLedger.Iterators
{
    public class BasicAccountsIterator : IIterator<Account>
    {
        public BasicAccountsIterator(Account account)
        {
            Account = account;
        }

        public Account Account { get; private set; }

        public IEnumerable<Account> Get()
        {
            return Account.RecursiveEnum(a => a.Accounts.Values);
        }
    }
}

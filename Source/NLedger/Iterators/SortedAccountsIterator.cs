// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Iterators
{
    public class SortedAccountsIterator : IIterator<Account>
    {
        public SortedAccountsIterator(Account account, Expr sortCmp, Report report, bool flattenAll)
        {
            AccountsList = new List<List<Account>>();
            SortedAccountsI = new List<BoostIterator<Account>>();

            SortCmp = sortCmp;
            Report = report;
            FlattenAll = flattenAll;

            PushBack(account);
        }

        public Expr SortCmp { get; private set; }
        public Report Report { get; private set; }
        public bool FlattenAll { get; private set; }
        public List<List<Account>> AccountsList { get; private set; }
        public List<BoostIterator<Account>> SortedAccountsI { get; private set; }

        public void PushBack(Account account)
        {
            AccountsList.Add(new List<Account>());

            if (FlattenAll)
            {
                PushAll(account, AccountsList.Last());
                StableSort(AccountsList.Last());

                if (Logger.Current.ShowDebug("account.sorted"))
                {
                    foreach (var acct in AccountsList.Last())
                        Logger.Current.Debug("account.sorted", () => String.Format("Account (flat): {0}", acct.FullName));
                }
            }
            else
            {
                SortAccounts(account, AccountsList.Last());
            }

            SortedAccountsI.Add(AccountsList.Last().Begin());
        }

        public void PushAll(Account account, List<Account> accounts)
        {
            foreach(KeyValuePair<string,Account> pair in account.Accounts)
            {
                accounts.Add(pair.Value);
                PushAll(pair.Value, accounts);
            }
        }

        public void SortAccounts(Account account, List<Account> accounts)
        {
            foreach (KeyValuePair<string, Account> pair in account.Accounts)
                accounts.Add(pair.Value);

            StableSort(accounts);

            if (Logger.Current.ShowDebug("account.sorted"))
            {
                foreach (var acct in accounts)
                    Logger.Current.Debug("account.sorted", () => String.Format("Account (flat): {0}", acct.FullName));
            }
        }

        private void StableSort(List<Account> list)
        {
            // [DM] Enumerable.OrderBy is a stable sort that preserve original positions for equal items
            var orderedList = list.OrderBy(p => p, new CompareAccounts(SortCmp, Report)).ToList();
            list.Clear();
            list.AddRange(orderedList);
        }

        private Account Increment()
        {
            while(SortedAccountsI.Any() && SortedAccountsI.Last().Current == null)
            {
                SortedAccountsI.PopBack();
                if (!AccountsList.Any())
                    throw new InvalidOperationException("accounts list is empty");
                AccountsList.PopBack();
            }

            if (!SortedAccountsI.Any())
            {
                return null;
            }
            else
            {
                Account account = SortedAccountsI.Last().Current;
                SortedAccountsI.Last().Increment();

                // If this account has children, queue them up to be iterated next.
                if (!FlattenAll && account.Accounts.Any())
                    PushBack(account);

                // Make sure the sorting value gets recalculated for this account
                account.XData.SortCalc = false;

                return account;
            }
        }

        public IEnumerable<Account> Get()
        {
            Account account;
            while ((account = Increment()) != null)
                yield return account;
        }
    }
}

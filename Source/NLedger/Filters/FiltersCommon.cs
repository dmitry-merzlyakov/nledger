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
using NLedger.Values;
using NLedger.Xacts;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLedger.Utils;

namespace NLedger.Filters
{
    public static class FiltersCommon
    {
        public static Account CreateTempAccountFromPath(IEnumerable<string> accountNames, Temporaries temps, Account master)
        {
            Account newAccount = null;
            foreach(string name in accountNames)
            {
                if (newAccount != null)
                {
                    newAccount = newAccount.FindAccount(name);
                }
                else
                {
                    newAccount = master.FindAccount(name, false);
                    if (newAccount == null)
                        newAccount = temps.CreateAccount(name, master);
                }
            }

            if (newAccount == null)
                throw new InvalidOperationException("newAccount");

            return newAccount;
        }

        /// <summary>
        /// Ported from handle_value
        /// </summary>
        public static void HandleValue(Value value, Account account, Xact xact, Temporaries temps, PostHandler handler, 
            Date date = default(Date), bool actDateP = true, Value total = null, bool directAmount = false,
            bool markVisited = false, bool bidirLink = true)
        {
            Post post = temps.CreatePost(xact, account, bidirLink);
            post.Flags |= SupportsFlagsEnum.ITEM_GENERATED;

            // If the account for this post is all virtual, then report the post as
            // such.  This allows subtotal reports to show "(Account)" for accounts
            // that contain only virtual posts.
            if (account != null && account.HasXData && account.XData.AutoVirtualize)
            {
                if (!account.XData.HasNonVirtuals)
                {
                    post.Flags |= SupportsFlagsEnum.POST_VIRTUAL;
                    if (!account.XData.HasUnbVirtuals)
                        post.Flags |= SupportsFlagsEnum.POST_MUST_BALANCE;
                }
            }

            PostXData xdata = post.XData;

            if (date.IsValid())
            {
                if (actDateP)
                    xdata.Date = date;
                else
                    xdata.ValueDate = date;
            }

            Value temp = Value.Clone(value);

            switch (value.Type)
            {
                case ValueTypeEnum.Boolean:
                case ValueTypeEnum.Integer:
                    temp.InPlaceCast(ValueTypeEnum.Amount);
                    post.Amount = temp.AsAmount;
                    break;

                case ValueTypeEnum.Amount:
                    post.Amount = temp.AsAmount;
                    break;

                case ValueTypeEnum.Balance:
                case ValueTypeEnum.Sequence:
                    xdata.CompoundValue = temp;
                    xdata.Compound = true;
                    break;

                default:
                    throw new InvalidOperationException();
            }

            if (!Value.IsNullOrEmpty(total))
                xdata.Total = total;

            if (directAmount)
                xdata.DirectAmt = true;

            Logger.Current.Debug("filters.changed_value.rounding", () => String.Format("post.amount = {0}", post.Amount));

            handler.Handle(post);

            if (markVisited)
            {
                post.XData.Visited = true;
                post.Account.XData.Visited = true;
            }
        }
    }
}

// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Chain;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Filters
{
    public class InjectPosts : PostHandler
    {
        public InjectPosts(PostHandler handler, string tagList, Account master)
            : base (handler)
        {
            TagsList = new List<Tuple<string, Account, ISet<Xact>>>();
            Temps = new Temporaries();
            
            foreach(string q in tagList.Split(',').Select(s => s.Trim()))
            {
                string[] accountNames = q.Split(':');
                Account account = FiltersCommon.CreateTempAccountFromPath(accountNames, Temps, master);
                account.IsGeneratedAccount = true;

                TagsList.Add(new Tuple<string, Account, ISet<Xact>>(q, account, new HashSet<Xact>()));
            }
        }

        public Temporaries Temps { get; private set; }
        public IList<Tuple<string, Account, ISet<Xact>>> TagsList { get; private set; }

        public override void Handle(Post post)
        {
            foreach(Tuple<string, Account, ISet<Xact>> pair in TagsList)
            {
                Value tagValue = post.GetTag(pair.Item1, false);
                // When checking if the transaction has the tag, only inject once
                // per transaction.
                if (Value.IsNullOrEmptyOrFalse(tagValue) && !pair.Item3.Contains(post.Xact))
                {
                    tagValue = post.Xact.GetTag(pair.Item1);
                    if (!Value.IsNullOrEmptyOrFalse(tagValue))
                        pair.Item3.Add(post.Xact);
                }                    

                if (!Value.IsNullOrEmptyOrFalse(tagValue))
                {
                    Xact xact = Temps.CopyXact(post.Xact);
                    xact.Date = post.GetDate();
                    xact.Flags |= SupportsFlagsEnum.ITEM_GENERATED;
                    Post temp = Temps.CopyPost(post, xact);

                    temp.Account = pair.Item2;
                    temp.Amount = tagValue.AsAmount;
                    temp.Flags |= SupportsFlagsEnum.ITEM_GENERATED;

                    base.Handle(temp);
                }
            }

            base.Handle(post);
        }
    }
}

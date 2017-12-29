// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Xacts
{
    /// <summary>
    /// Porrted from add_balancing_post (xact.cc)
    /// </summary>
    public struct AddBalancingPost
    {
        public AddBalancingPost(XactBase xact, Post nullPost) : this()
        {
            First = true;
            Xact = xact;
            NullPost = nullPost;
        }

        public AddBalancingPost(AddBalancingPost other) : this()
        {
            First = other.First;
            Xact = other.Xact;
            NullPost = other.NullPost;
        }

        public bool First { get; private set; }
        public XactBase Xact { get; private set; }
        public Post NullPost { get; private set; }

        public void Amount(Amount amount)
        {
            if (First)
            {
                NullPost.Amount = amount.Negated();
                NullPost.Flags = NullPost.Flags | SupportsFlagsEnum.POST_CALCULATED;
                First = false;
            }
            else
            {
                Post p = new Post(NullPost.Account, amount.Negated());
                p.Flags = p.Flags | SupportsFlagsEnum.ITEM_GENERATED | SupportsFlagsEnum.POST_CALCULATED;
                p.State = NullPost.State;
                Xact.AddPost(p);
            }
        }
    }

}

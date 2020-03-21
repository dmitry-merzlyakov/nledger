// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLedger.Utility;
using NLedger.Utility.Rnd;
using NLedger.Amounts;
using NLedger.Accounts;
using NLedger.Commodities;
using NLedger.Xacts;

namespace NLedger.Filters
{
    public class AnonymizePosts : PostHandler
    {
        public AnonymizePosts(PostHandler handler)
            : base(handler)
        { }

        public IDictionary<Commodity, int> CommodityIndexMap
        {
            get { return Comms; }
        }

        public void RenderCommodity(Amount amt)
        {
            Commodity comm = amt.Commodity;

            int id;
            bool newlyAdded = false;

            if (!Comms.TryGetValue(comm, out id))
            {
                id = NextCommID++;
                newlyAdded = true;
                Comms.Add(comm, id);
            }

            StringBuilder buf = new StringBuilder();
            do
            {
                buf.Append((char)('A' + (id % 26)));
                id /= 26;
            } while (id > 0);

            if (amt.HasAnnotation)
                amt.SetCommodity(CommodityPool.Current.FindOrCreate(buf.ToString(), amt.Annotation));
            else
                amt.SetCommodity(CommodityPool.Current.FindOrCreate(buf.ToString()));

            if (newlyAdded)
            {
                amt.Commodity.Flags |= comm.Flags;
                amt.Commodity.Precision = comm.Precision;
            }
        }

        public override void Handle(Post post)
        {
            bool copyXactDetails = false;

            if (LastXact != post.Xact)
            {
                Temps.CopyXact(post.Xact);
                LastXact = post.Xact;
                copyXactDetails = true;
            }
            Xact xact = Temps.LastXact;
            xact.Code = null;

            if (copyXactDetails)
            {
                xact.CopyDetails(post.Xact);

                string buf = String.Format("{0}{1}{0}", post.Xact.Payee, IntegerGen.Value());

                xact.Payee = SHA1.GetHash(buf);
                xact.Note = null;
            }
            else
            {
                xact.Journal = post.Xact.Journal;
            }

            IList<string> accountNames = new List<string>();

            for (Account acct = post.Account; acct != null; acct = acct.Parent)
            {
                string buf = String.Format("{0}{1}{2}", IntegerGen.Value(), acct, acct.FullName);

                accountNames.Add(SHA1.GetHash(buf));
            }

            Account newAccount = FiltersCommon.CreateTempAccountFromPath(accountNames, Temps, xact.Journal.Master);
            Post temp = Temps.CopyPost(post, xact, newAccount);
            temp.Note = null;
            temp.Flags |= SupportsFlagsEnum.POST_ANONYMIZED;

            RenderCommodity(temp.Amount);
            if (temp.Amount.HasAnnotation)
            {
                temp.Amount.Annotation.Tag = null;
                if (temp.Amount.Annotation.Price != null)
                    RenderCommodity(temp.Amount.Annotation.Price);
            }

            if (temp.Cost != null)
                RenderCommodity(temp.Cost);
            if (temp.AssignedAmount != null)
                RenderCommodity(temp.AssignedAmount);

            base.Handle(temp);
        }

        public override void Clear()
        {
            Temps.Clear();
            Comms.Clear();
            LastXact = null;

            base.Clear();
        }

        private int NextCommID { get; set; }
        private Xact LastXact { get; set; }

        private readonly Temporaries Temps = new Temporaries();
        private readonly IDictionary<Commodity, int> Comms = new Dictionary<Commodity, int>();
        private readonly IntegerGenerator IntegerGen = new IntegerGenerator(1, 2000000000);
    }
}

// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Commodities;
using NLedger.Journals;
using NLedger.Xacts;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Iterators
{
    /// <summary>
    /// Ported from posts_commodities_iterator
    /// </summary>
    public class PostCommoditiesIterator : IIterator<Post>
    {
        public PostCommoditiesIterator(Journal journal)
        {
            Posts = new XactPostsIterator();
            XactTemps = new List<Xact>();
            Temps = new Temporaries();

            Reset(journal);
        }

        public JournalPostsIterator JournalPosts { get; private set; }
        public XactsIterator Xacts { get; private set; }
        public XactPostsIterator Posts { get; private set; }
        public IList<Xact> XactTemps { get; private set; }
        public Temporaries Temps { get; private set; }

        public void Reset(Journal journal)
        {
            JournalPosts = new JournalPostsIterator(journal);

            ISet<Commodity> commodities = new HashSet<Commodity>();

            foreach(Post post in JournalPosts.Get())
            {
                Commodity comm = post.Amount.Commodity;
                if (comm.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_NOMARKET))
                    continue;
                commodities.Add(comm.Referent);
            }

            IDictionary<string, Xact> xactsByCommodity = new Dictionary<string, Xact>();
            foreach(Commodity comm in commodities)
            {
                comm.MapPrices((d, a) => CreatePriceXact(d, a, xactsByCommodity, journal, journal.Master.FindAccount(comm.Symbol)));
            }

            Xacts = new XactsIterator();
            Xacts.Reset(XactTemps);
        }

        private void CreatePriceXact(DateTime date, Amount price, IDictionary<string, Xact> xactsByCommodity, Journal journal, Account account)
        {
            Xact xact;
            string symbol = price.Commodity.Symbol;

            if (!xactsByCommodity.TryGetValue(symbol, out xact))
            {
                xact = Temps.CreateXact();
                XactTemps.Add(xact);
                xact.Payee = symbol;
                xact.Date = (Date)date.Date;
                xactsByCommodity.Add(symbol, xact);
                xact.Journal = journal;
            }

            bool postAlreadyExists = false;

            foreach(Post post in xact.Posts)
            {
                if (post.Date == date && post.Amount == price)
                {
                    postAlreadyExists = true;
                    break;
                }
            }

            if (!postAlreadyExists)
            {
                Post temp = Temps.CreatePost(xact, account);
                temp.Date = (Date)date.Date;
                temp.Amount = price;

                temp.XData.Datetime = date;
            }
        }

        public IEnumerable<Post> Get()
        {
            foreach(Xact xact in Xacts.Get())
            {
                Posts.Reset(xact);
                foreach(Post post in Posts.Get())
                {
                    yield return post;
                }
            }
        }
    }
}

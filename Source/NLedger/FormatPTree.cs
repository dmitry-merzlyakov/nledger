// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Chain;
using NLedger.Commodities;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Utility.Xml;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NLedger
{
    /// <summary>
    /// Ported from format_ptree/format_t (ptree.h)
    /// </summary>
    public enum FormatPTreeKind
    {
        FORMAT_XML
    }

    /// <summary>
    /// Ported from format_ptree (ptree.h)
    /// </summary>
    public class FormatPTree : PostHandler
    {
        // account_visited_p
        public static bool IsAccountVisited(Account acct)
        {
            return (acct.HasXData && acct.XData.Visited) || acct.ChildrenWithFlags(false, true) > 0;
        }

        public FormatPTree(Report report, FormatPTreeKind format = FormatPTreeKind.FORMAT_XML)
            : base(null)
        {
            Report = report;
            Format = format;

            Commodities = new Dictionary<string, Commodity>();
            TransactionsSet = new HashSet<Xact>();
        }

        public FormatPTreeKind Format { get; private set; }

        public override void Flush()
        {
            XDocument xDoc = XmlExtensions.CreateLedgerDoc();
            XElement commElement = xDoc.Root.AddElement("commodities");

            foreach (Commodity commodity in Commodities.Values)
                commodity.ToXml(commElement.AddElement("commodity"), true);

            XElement accElement = xDoc.Root.AddElement("accounts");
            Report.Session.Journal.Master.ToXml(accElement.AddElement("account"), a => IsAccountVisited(a));

            XElement tranElement = xDoc.Root.AddElement("transactions");
            foreach (Xact xact in TransactionsSet)
            {
                XElement tran = tranElement.AddElement("transaction");
                xact.ToXml(tran);

                XElement posts = tran.AddElement("postings");
                foreach(Post post in xact.Posts)
                {
                    if (post.HasXData && post.XData.Visited)
                        post.ToXml(posts.AddElement("posting"));
                }
            }

            using (StringWriter sw = new EncodingStringWriter(Encoding.UTF8))
            {
                xDoc.Save(sw);
                Report.OutputStream.Write(sw.ToString());
            }
        }

        public override void Handle(Post post)
        {
            if (!post.XData.Visited)
                throw new InvalidOperationException("assert(post.xdata().has_flags(POST_EXT_VISITED));");

            Commodities[post.Amount.Commodity.Symbol] = post.Amount.Commodity;

            if (!TransactionsSet.Contains(post.Xact))
                TransactionsSet.Add(post.Xact);
        }

        public override void Clear()
        {
            Commodities.Clear();
            TransactionsSet.Clear();
            base.Clear();
        }

        protected Report Report { get; private set; }
        protected IDictionary<string, Commodity> Commodities { get; private set; }
        protected ISet<Xact> TransactionsSet { get; private set; }
    }
}

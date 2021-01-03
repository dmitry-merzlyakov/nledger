// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Items;
using NLedger.Scopus;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Print
{
    public class PrintXacts : PostHandler
    {
        public PrintXacts(Report report, bool printRaw = false)
            : base(null)
        {
            Report = report;
            PrintRaw = printRaw;
            FirstTitle = true;

            XactsPresent = new Dictionary<Xact, bool>();
            Xacts = new List<Xact>();
        }

        public override void Title(string str)
        {
            if (FirstTitle)
                FirstTitle = false;
            else
                Report.OutputStream.WriteLine();
        }

        public override void Flush()
        {
            bool first = true;
            foreach(Xact xact in Xacts)
            {
                if (first)
                    first = false;
                else
                    Report.OutputStream.WriteLine();

                if (PrintRaw)
                {
                    Report.OutputStream.Write(Item.PrintItem(xact));
                    Report.OutputStream.WriteLine();
                }
                else
                {
                    Report.OutputStream.Write(PrintCommon.PrintXact(Report, xact));
                }
            }

            Report.OutputStream.Flush();
        }

        public override void Handle(Post post)
        {
            if (!post.HasXData || !post.XData.Displayed)
            {
                if (!XactsPresent.ContainsKey(post.Xact))
                {
                    XactsPresent[post.Xact] = true;
                    Xacts.Add(post.Xact);
                }
                post.XData.Displayed = true;
            }
        }

        public override void Clear()
        {
            XactsPresent.Clear();
            Xacts.Clear();

            base.Clear();
        }

        protected Report Report { get; private set; }
        protected IDictionary<Xact, bool> XactsPresent { get; private set; }
        protected IList<Xact> Xacts { get; private set; }
        protected bool PrintRaw { get; private set; }
        protected bool FirstTitle { get; private set; }
    }
}

// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Chain;
using NLedger.Commodities;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Output
{
    public class ReportCommodities : PostHandler
    {
        public ReportCommodities(Report report)
            : base(null)
        {
            Report = report;
            Commodities = new SortedDictionary<Commodity, int>();
        }

        public override void Flush()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<Commodity, int> pair in Commodities)
            {
                if (Report.CountHandler.Handled)
                    sb.AppendFormat("{0} ", pair.Value);
                sb.AppendLine(pair.Key.ToString());
            }

            Report.OutputStream.Write(sb.ToString());
        }

        public override void Handle(Post post)
        {
            Amount temp = post.Amount.StripAnnotations(Report.WhatToKeep());
            Commodity comm = temp.Commodity;

            int count;
            if (!Commodities.TryGetValue(comm, out count))
                count = 0;
            Commodities[comm] = count + 1;

            if (comm.IsAnnotated)
            {
                AnnotatedCommodity annComm = (AnnotatedCommodity)comm;
                if (annComm.Details.Price != null)
                {
                    if (!Commodities.TryGetValue(annComm.Details.Price.Commodity, out count))
                        count = 0;
                    Commodities[annComm.Details.Price.Commodity] = count + 1;
                }
            }

            if (post.Cost != null)
            {
                Amount tempCost = post.Cost.StripAnnotations(Report.WhatToKeep());

                if (!Commodities.TryGetValue(tempCost.Commodity, out count))
                    count = 0;
                Commodities[tempCost.Commodity] = count + 1;
            }
        }

        public override void Clear()
        {
            Commodities.Clear();
            base.Clear();
        }

        protected Report Report { get; private set; }
        protected IDictionary<Commodity, int> Commodities { get; private set; }
    }
}

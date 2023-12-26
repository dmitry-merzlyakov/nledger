// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Output
{
    public class ReportPayees : PostHandler
    {
        public ReportPayees(Report report)
            : base(null)
        {
            Report = report;
            Payees = new SortedDictionary<string, int>();
        }

        public override void Flush()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, int> pair in Payees)
            {
                if (Report.CountHandler.Handled)
                    sb.AppendFormat("{0} ", pair.Value);
                sb.AppendLine(pair.Key.ToString());
            }

            Report.OutputStream.Write(sb.ToString());
        }

        public override void Handle(Post post)
        {
            int count;
            if (!Payees.TryGetValue(post.Payee, out count))
                count = 0;

            Payees[post.Payee] = count + 1;
        }

        public override void Clear()
        {
            Payees.Clear();
            base.Clear();
        }

        protected Report Report { get; private set; }
        protected IDictionary<string, int> Payees { get; private set; }
    }
}

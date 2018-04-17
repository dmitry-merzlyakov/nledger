// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Chain;
using NLedger.Formatting;
using NLedger.Scopus;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Output
{
    public class ReportAccounts : PostHandler
    {
        public ReportAccounts(Report report) : base(null)
        {
            Report = report;
            Accounts = new Dictionary<Account, int>();
        }

        public override void Flush()
        {
            StringBuilder sb = new StringBuilder();
            Format prependFormat = null;
            int prependWidth = 0;

            if (Report.PrependFormatHandler.Handled)
            {
                prependFormat = new Format(Report.PrependFormatHandler.Str());
                if (Report.PrependWidthHandler.Handled)
                    prependWidth = Int32.Parse(Report.PrependWidthHandler.Str());
            }

            foreach(KeyValuePair<Account,int> pair in Accounts.OrderBy(p => p.Key.FullName, StringComparer.Ordinal))
            {
                if (prependFormat != null)
                {
                    BindScope boundScope = new BindScope(Report, pair.Key);
                    sb.AppendFormat(StringExtensions.GetWidthAlignFormatString(prependWidth), 
                        prependFormat.Calc(boundScope));
                }

                if (Report.CountHandler.Handled)
                    sb.AppendFormat("{0} ", pair.Value);
                sb.AppendLine(pair.Key.ToString());
            }

            Report.OutputStream.Write(sb.ToString());
        }

        public override void Handle(Post post)
        {
            int count;
            if (!Accounts.TryGetValue(post.Account, out count))
                count = 0;
                
            Accounts[post.Account] = count + 1;
        }

        public override void Clear()
        {
            Accounts.Clear();
            base.Clear();
        }

        protected Report Report { get; private set; }
        protected IDictionary<Account, int> Accounts { get; private set; }
    }
}

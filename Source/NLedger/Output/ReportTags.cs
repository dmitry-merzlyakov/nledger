// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Items;
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Output
{
    public class ReportTags : PostHandler
    {
        public ReportTags(Report report)
            : base(null)
        {
            Report = report;
            Tags = new SortedDictionary<string, int>();
        }

        public override void Flush()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, int> pair in Tags)
            {
                if (Report.CountHandler.Handled)
                    sb.AppendFormat("{0} ", pair.Value);
                sb.AppendLine(pair.Key.ToString());
            }

            Report.OutputStream.Write(sb.ToString());
        }

        public virtual void GatherMetadata(Item item)
        {
            if (item.GetMetadata() == null)
                return;

            foreach (KeyValuePair<string, ItemTag> data in item.GetMetadata())
            {
                string tag = data.Key;
                if (Report.ValuesHandler.Handled && !Value.IsNullOrEmptyOrFalse(data.Value.Value))
                    tag += ": " + data.Value.Value.ToString();

                int count;
                if (Tags.TryGetValue(tag, out count))
                    Tags[tag] = count + 1;
                else
                    Tags[tag] = 1;
            }
        }

        public override void Handle(Post post)
        {
            GatherMetadata(post.Xact);
            GatherMetadata(post);
        }

        public override void Clear()
        {
            Tags.Clear();
            base.Clear();
        }

        protected Report Report { get; private set; }
        protected IDictionary<string, int> Tags { get; private set; }
    }
}

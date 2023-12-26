// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLedger.Utility;
using System.Threading.Tasks;

namespace NLedger.Drafts
{
    // draft_t::xact_template_t
    public class DraftXactTemplate
    {
        public DraftXactTemplate()
        {
            Posts = new List<DraftXactPostTemplate>();
        }

        public Date? Date { get; set; }
        public string Code { get; set; }
        public string Note { get; set; }
        public Mask PayeeMask { get; set; }
        public IList<DraftXactPostTemplate> Posts { get; private set; }

        public string Dump()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format("Date:       {0}", Date.HasValue ? Date.Value.ToString() : "<today>"));

            if (!String.IsNullOrEmpty(Code))
                sb.AppendLine(String.Format("Code:       {0}", Code));
            if (!String.IsNullOrEmpty(Note))
                sb.AppendLine(String.Format("Note:       {0}", Note));

            if (PayeeMask == null)
                sb.AppendLine("Payee mask: INVALID (template expression will cause an error)");
            else
                sb.AppendLine(String.Format("Payee mask: {0}", PayeeMask));

            if (!Posts.Any())
            {
                sb.AppendLine();
                sb.AppendLine("<Posting copied from last related transaction>");
            }
            else
            {
                foreach (DraftXactPostTemplate post in Posts)
                {
                    sb.AppendLine();
                    sb.AppendLine(String.Format("[Posting \"{0}\"]", post.From ? "from" : "to"));

                    if (post.AccountMask != null)
                        sb.AppendLine(String.Format("  Account mask: {0}", post.AccountMask));
                    else if (post.From)
                        sb.AppendLine("  Account mask: <use last of last related accounts>");
                    else
                        sb.AppendLine("  Account mask: <use first of last related accounts>");

                    if (post.Amount != null)
                        sb.AppendLine(String.Format("  Amount:       {0}", post.Amount));

                    if (post.Cost != null)
                        sb.AppendLine(String.Format("  Cost:         {0} {1}", post.CostOperator, post.Cost));
                }
            }

            return sb.ToString();
        }
    }
}

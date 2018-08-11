// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Items;
using NLedger.Xacts;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    /// <summary>
    /// Ported from format_emacs_posts (emacs.h)
    /// </summary>
    public class FormatEmacsPosts : PostHandler
    {
        public FormatEmacsPosts(TextWriter outWriter)
            : base(null)
        {
            OutWriter = outWriter;
            Out = new StringBuilder();
        }

        public virtual void WriteXact(Xact xact)
        {
            if (xact.Pos != null)
                Out.AppendFormat("\"{0}\" {1} ", xact.Pos.PathName, xact.Pos.BegLine);
            else
                Out.Append("\"\" -1 ");

            DateTime when = xact.GetDate().FromLocalToUtc();
            int date = when.ToPosixTime();

            Out.AppendFormat("({0} {1} 0) ", date / 65536, date % 65536);

            if (!String.IsNullOrEmpty(xact.Code))
                Out.AppendFormat("\"{0}\" ", xact.Code);
            else
                Out.Append("nil ");

            if (String.IsNullOrEmpty(xact.Payee))
                Out.Append("nil");
            else
                Out.AppendFormat("\"{0}\"", xact.Payee);

            Out.AppendLine();
        }

        public override void Flush()
        {
            if (LastXact != null)
                Out.AppendLine("))");

            OutWriter.Write(Out.ToString());
            OutWriter.Flush();
        }

        public override void Handle(Post post)
        {
            if (!post.HasXData || !post.XData.Displayed)
            {
                if (LastXact == null)
                {
                    Out.Append("((");
                    WriteXact(post.Xact);
                }
                else if (post.Xact != LastXact)
                {
                    Out.Append(")");
                    Out.AppendLine();
                    Out.Append(" (");
                    WriteXact(post.Xact);
                }
                else
                {
                    Out.AppendLine();
                }

                if (post.Pos != null)
                    Out.AppendFormat("  ({0} ", post.Pos.BegLine);
                else
                    Out.Append("  (-1 ");

                Out.AppendFormat("\"{0}\" \"{1}\"", post.ReportedAccount.FullName, post.Amount);

                switch (post.State)
                {
                    case ItemStateEnum.Uncleared:
                        Out.Append(" nil");
                        break;

                    case ItemStateEnum.Cleared:
                        Out.Append(" t");
                        break;

                    case ItemStateEnum.Pending:
                        Out.Append(" pending");
                        break;
                }

                if (post.Cost != null)
                    Out.AppendFormat(" \"{0}\"", post.Cost);
                if (!String.IsNullOrEmpty(post.Note))
                    Out.AppendFormat(" \"{0}\"", EscapeString(post.Note));
                Out.Append(")");

                LastXact = post.Xact;

                post.XData.Displayed = true;
            }
        }

        public virtual string EscapeString(string raw)
        {
            raw = raw.Replace("\\", "\\\\");
            raw = raw.Replace("\"", "\\\"");
            return raw;
        }

        protected Xact LastXact { get; set; }
        protected StringBuilder Out { get; set; }
        protected TextWriter OutWriter { get; set; }
    }
}

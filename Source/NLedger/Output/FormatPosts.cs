// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Formatting;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Output
{
    /// <summary>
    /// Ported from format_posts
    /// </summary>
    public class FormatPosts : PostHandler
    {
        public FormatPosts(Report report, string format, string prependFormat = null, int prependWidth = 0)
            : base(null)
        {
            Report = report;
            PrependWidth = prependWidth;
            FirstReportTitle = true;

            int pi = format.IndexOf("%/");
            if (pi >= 0)
            {
                FirstLineFormat = new Format(format.Substring(0, pi));
                string n = format.Substring(pi + 2);
                int ppi = n.IndexOf("%/");
                if (ppi >= 0)
                {
                    NextLineFormat = new Format(n.Substring(0, ppi), FirstLineFormat);
                    BetweenFormat = new Format(n.Substring(0, ppi + 2));
                }
                else
                {
                    NextLineFormat = new Format(n, FirstLineFormat);
                }
            }
            else
            {
                FirstLineFormat = new Format(format);
                NextLineFormat = new Format(format);
            }

            if (!String.IsNullOrEmpty(prependFormat))
                PrependFormat = new Format(prependFormat);
        }

        public override void Title(string str)
        {
            ReportTitle = str;
        }

        public override void Flush()
        {
            Report.OutputStream.Flush();
        }

        public override void Handle(Post post)
        {
            if (!post.HasXData || !post.XData.Displayed)
            {
                StringBuilder sb = new StringBuilder();
                BindScope boundScope = new BindScope(Report, post);

                if (!String.IsNullOrEmpty(ReportTitle))
                {
                    if (FirstReportTitle)
                        FirstReportTitle = false;
                    else
                        sb.AppendLine();

                    ValueScope valScope = new ValueScope(boundScope, Value.StringValue(ReportTitle));
                    Format groupTitleFormat = new Format(Report.GroupTitleFormatHandler.Str());

                    sb.Append(groupTitleFormat.Calc(valScope));

                    ReportTitle = string.Empty;
                }

                if (PrependFormat != null)
                {
                    sb.AppendFormat(StringExtensions.GetWidthAlignFormatString(PrependWidth), PrependFormat.Calc(boundScope));
                }

                if (LastXact != post.Xact)
                {
                    if (LastXact != null)
                    {
                        BindScope xactScope = new BindScope(Report, LastXact);
                        if (BetweenFormat != null)      // DM - in the original code, between_format is called w/o instantiating and returns nothing
                            sb.Append(BetweenFormat.Calc(xactScope));
                    }
                    sb.Append(FirstLineFormat.Calc(boundScope));
                    LastXact = post.Xact;
                }
                else if (LastPost != null && LastPost.GetDate() != post.GetDate())
                {
                    sb.Append(FirstLineFormat.Calc(boundScope));
                }
                else
                {
                    sb.Append(NextLineFormat.Calc(boundScope));
                }

                post.XData.Displayed = true;
                LastPost = post;

                Report.OutputStream.Write(sb.ToString());
            }
        }

        public override void Clear()
        {
            LastXact = null;
            LastPost = null;

            ReportTitle = string.Empty;
            base.Clear();
        }

        protected Report Report { get; private set; }
        protected Format FirstLineFormat { get; private set; }
        protected Format NextLineFormat { get; private set; }
        protected Format BetweenFormat { get; private set; }
        protected Format PrependFormat { get; private set; }
        protected int PrependWidth { get; private set; }
        protected Xact LastXact { get; private set; }
        protected Post LastPost { get; private set; }
        protected bool FirstReportTitle { get; private set; }
        protected string ReportTitle { get; private set; }
    }
}

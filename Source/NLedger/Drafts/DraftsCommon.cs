// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Print;
using NLedger.Scopus;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Drafts
{
    public static class DraftsCommon
    {
        public static Value TemplateCommand(CallScope args)
        {
            Report report = args.FindScope<Report>();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--- Input arguments ---");
            sb.Append(args.Value().Dump());
            sb.AppendLine();
            sb.AppendLine();

            Draft draft = new Draft(args.Value());
            sb.AppendLine("--- Transaction template ---");
            sb.Append(draft.Dump());

            report.OutputStream.Write(sb.ToString());

            return Value.True;
        }

        public static Value XactCommand(CallScope args)
        {
            Report report = args.FindScope<Report>();
            Draft draft = new Draft(args.Value());

            Xact newXact = draft.Insert(report.Session.Journal);
            if (newXact != null)
            {
                // Only consider actual postings for the "xact" command
                report.LimitHandler.On("#xact", "actual");

                report.XactReport(new PrintXacts(report), newXact);
            }

            return Value.True;
        }
    }
}

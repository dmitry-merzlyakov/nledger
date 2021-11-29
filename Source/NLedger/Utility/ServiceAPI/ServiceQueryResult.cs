// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLedger.Journals;

namespace NLedger.Utility.ServiceAPI
{
    public class ServiceQueryResult : BaseServiceResponse
    {
        public ServiceQueryResult(ServiceSession serviceSession, string query, CancellationToken token)
            : base(serviceSession, token)
        {
            if (String.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            Query = query;
            Build(token);
        }

        public string Query { get; }
        public IEnumerable<Post> Posts { get; private set; }

        protected override void Workload()
        {
            try
            {
                Posts = ServiceSession.GlobalScope.Session.Journal.Query(Query).ToArray();
                Status = 0;
            }
            catch (Exception err)
            {
                ServiceSession.GlobalScope.ReportError(err);
            }
        }
    }

}

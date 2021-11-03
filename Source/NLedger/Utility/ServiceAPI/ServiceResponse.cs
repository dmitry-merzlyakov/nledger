// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts.Impl;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NLedger.Utility.ServiceAPI
{
    /// <summary>
    /// Service API command response (contains the result of an executed command)
    /// </summary>
    public class ServiceResponse : BaseServiceResponse
    {
        public ServiceResponse(ServiceSession serviceSession, string command, CancellationToken token)
            : base(serviceSession, token)
        {
            if (String.IsNullOrWhiteSpace(command))
                throw new ArgumentNullException(nameof(command));

            Command = command;
            Build(token);
        }

        public string Command { get; }

        protected override void Workload()
        {
            try
            {
                Status = ServiceSession.GlobalScope.ExecuteCommandWrapper(StringExtensions.SplitArguments(Command), true);
            }
            catch (CountError errors)
            {
                Status = errors.Count;
            }
            catch (Exception err)
            {
                ServiceSession.GlobalScope.ReportError(err);
            }
        }
    }
}

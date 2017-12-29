// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Accounts
{
    /// <summary>
    /// Ported from account_t/xdata_t (account.h)
    /// </summary>
    public class AccountXData
    {
        public AccountXData()
        {
            SelfDetails = new AccountXDataDetails();
            FamilyDetails = new AccountXDataDetails();
            ReportedPosts = new List<Post>();
        }

        public bool SortCalc { get; set; }
        public bool HasNonVirtuals { get; set; }
        public bool HasUnbVirtuals { get; set; }
        public bool AutoVirtualize { get; set; }
        public bool Visited { get; set; }
        public bool Matching { get; set; }
        public bool ToDisplay { get; set; }
        public bool Displayed { get; set; }

        public AccountXDataDetails SelfDetails { get; private set; }
        public AccountXDataDetails FamilyDetails { get; private set; }

        public IList<Post> ReportedPosts { get; private set; }

        public IList<Tuple<Value, bool>> SortValues
        {
            get { return _SortValues.Value; }
        }

        public void SetFlags(AccountXData accountXData)
        {
            SortCalc = accountXData.SortCalc;
            HasNonVirtuals = accountXData.HasNonVirtuals;
            HasUnbVirtuals = accountXData.HasUnbVirtuals;
            AutoVirtualize = accountXData.AutoVirtualize;
            Visited = accountXData.Visited;
            Matching = accountXData.Matching;
            ToDisplay = accountXData.ToDisplay;
            Displayed = accountXData.Displayed;
        }

        private Lazy<List<Tuple<Value, bool>>> _SortValues = new Lazy<List<Tuple<Value, bool>>>();
    }
}

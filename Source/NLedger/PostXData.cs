// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Utility;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    public class PostXData
    {
        public PostXData()
        { }

        public PostXData(PostXData postXData)
        {
            if (postXData == null)
                throw new ArgumentNullException("postXData");

            Received = postXData.Received;
            Handled = postXData.Handled;
            Displayed = postXData.Displayed;
            DirectAmt = postXData.DirectAmt;
            SortCalc = postXData.SortCalc;
            Compound = postXData.Compound;
            Visited = postXData.Visited;
            Matches = postXData.Matches;
            Considered = postXData.Considered;

            VisitedValue = Value.Clone(postXData.VisitedValue);
            CompoundValue = Value.Clone(postXData.CompoundValue);
            Total = Value.Clone(postXData.Total);
            Count = postXData.Count;
            Date = postXData.Date;
            ValueDate = postXData.ValueDate;
            Datetime = postXData.Datetime;
            Account = postXData.Account;
        }

        public bool Received { get; set; }
        public bool Handled { get; set; }
        public bool Displayed { get; set; }
        public bool DirectAmt { get; set; }
        public bool SortCalc { get; set; }
        public bool Compound { get; set; }
        public bool Visited { get; set; }
        public bool Matches { get; set; }
        public bool Considered { get; set; }

        public Value VisitedValue { get; set; }
        public Value CompoundValue { get; set; }
        public Value Total { get; set; }
        public int Count { get; set; }
        public Date Date { get; set; }
        public Date ValueDate { get; set; }
        public DateTime Datetime { get; set; }
        public Account Account { get; set; }

        public IList<Tuple<Value, bool>> SortValues 
        {
            get { return _SortValues.Value; }
        }

        private Lazy<List<Tuple<Value, bool>>> _SortValues = new Lazy<List<Tuple<Value, bool>>>();
    }
}

// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.TimeLogging
{
    public class TimeXact
    {
        public TimeXact()
        { }

        public TimeXact(ItemPosition position, DateTime checkin, bool completed = false, Account account = null, string desc = null, string note = null)
        {
            Position = position ?? new ItemPosition();
            Checkin = checkin;
            Completed = completed;
            Account = account;
            Desc = desc ?? String.Empty;
            Note = note ?? String.Empty;
        }

        public TimeXact(ItemPosition position, DateTime checkin, Account account)
            : this(position, checkin, false, account)
        { }

        public TimeXact(TimeXact xact)
        {
            Position = xact.Position;
            Checkin = xact.Checkin;
            Completed = xact.Completed;
            Account = xact.Account;
            Desc = xact.Desc;
            Note = xact.Note;
        }

        public DateTime Checkin { get; set; }
        public bool Completed { get; private set; }
        public Account Account { get; private set; }
        public string Desc { get; set; }
        public string Note { get; set; }
        public ItemPosition Position { get; private set; }
    }
}

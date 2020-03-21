// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Chain;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Filters
{
    /// <summary>
    /// Ported from transfer_details
    /// </summary>
    public class TransferDetails : PostHandler
    {
        public TransferDetails(PostHandler handler, TransferDetailsElementEnum whichElement, 
            Account master, Expr expr, Scope scope)
            : base(handler)
        {
            Master = master;
            Expr = expr;
            Scope = scope;
            WhichElement = whichElement;
            Temps = new Temporaries();
        }

        public Account Master { get; private set; }
        public Expr Expr { get; private set; }
        public Scope Scope { get; private set; }
        public Temporaries Temps { get; private set; }
        public TransferDetailsElementEnum WhichElement { get; private set; }

        /// <summary>
        /// Ported from void transfer_details::operator()(post_t& post)
        /// </summary>
        public override void Handle(Post post)
        {
            Xact xact = Temps.CopyXact(post.Xact);
            xact.Date = post.GetDate();

            Post temp = Temps.CopyPost(post, xact);
            temp.State = post.State;

            BindScope boundScope = new BindScope(Scope, temp);
            Value substitute = Expr.Calc(boundScope);

            if (!Value.IsNullOrEmpty(substitute))
            {
                switch(WhichElement)
                {
                    case TransferDetailsElementEnum.SET_DATE:
                        temp.Date = substitute.AsDate;
                        break;

                    case TransferDetailsElementEnum.SET_ACCOUNT:
                        {
                            string accountName = substitute.AsString;
                            if (!String.IsNullOrEmpty(accountName) && !accountName.EndsWith(":"))
                            {
                                Account prevAccount = temp.Account;
                                temp.Account.RemovePost(temp);

                                accountName += ":" + prevAccount.FullName;

                                string[] accountNames = accountName.Split(':');
                                temp.Account = FiltersCommon.CreateTempAccountFromPath(accountNames, Temps, xact.Journal.Master);
                                temp.Account.AddPost(temp);

                                temp.Account.SetFlags(prevAccount);
                                if (prevAccount.HasXData)
                                    temp.Account.XData.SetFlags(prevAccount.XData);
                            }
                            break;
                        }

                    case TransferDetailsElementEnum.SET_PAYEE:
                        xact.Payee = substitute.AsString;
                        break;
                }
            }

            base.Handle(temp);
        }

        public override void Clear()
        {
            Expr.MarkUncomplited();
            Temps.Clear();

            base.Clear();
        }
    }
}

// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Chain;
using NLedger.Expressions;
using NLedger.Times;
using NLedger.Values;
using NLedger.Xacts;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Filters
{
    /// <summary>
    /// Ported from subtotal_posts
    /// </summary>
    public class SubtotalPosts : PostHandler
    {
        public SubtotalPosts(PostHandler handler, Expr amountExpr, string dateFormat = null)
            : base (handler)
        {
            AmountExpr = amountExpr;
            DateFormat = dateFormat;

            ComponentPosts = new List<Post>();
            Temps = new Temporaries();
            Values = new SortedDictionary<string, AcctValue>();
        }

        public Expr AmountExpr { get; private set; }
        public IDictionary<string, AcctValue> Values { get; private set; }
        public string DateFormat { get; private set; }
        public Temporaries Temps { get; private set; }
        public IList<Post> ComponentPosts { get; private set; }

        /// <summary>
        /// Ported from subtotal_posts::report_subtotal
        /// </summary>
        public void ReportSubtotal(string specFmt = null, DateInterval interval = null)
        {
            if (!ComponentPosts.Any())
                return;

            Date? rangeStart = interval != null ? interval.Start : null;
            Date? rangeFinish = interval != null ? interval.InclusiveEnd : null;

            if (!rangeStart.HasValue || !rangeFinish.HasValue)
            {
                foreach(Post post in ComponentPosts)
                {
                    Date date = post.GetDate();
                    Date valueDate = post.ValueDate;

                    if (!rangeStart.HasValue || date < rangeStart)
                        rangeStart = date;
                    if (!rangeFinish.HasValue || valueDate > rangeFinish)
                        rangeFinish = valueDate;
                }
            }

            ComponentPosts.Clear();

            string outDate;
            if (!String.IsNullOrEmpty(specFmt))
                outDate = TimesCommon.Current.FormatDate(rangeFinish.Value, FormatTypeEnum.FMT_CUSTOM, specFmt);
            else if (!String.IsNullOrEmpty(DateFormat))
                outDate = "- " + TimesCommon.Current.FormatDate(rangeFinish.Value, FormatTypeEnum.FMT_CUSTOM, DateFormat);
            else
                outDate = "- " + TimesCommon.Current.FormatDate(rangeFinish.Value);

            Xact xact = Temps.CreateXact();
            xact.Payee = outDate;
            xact.Date = rangeStart.Value;

            foreach (KeyValuePair<string,AcctValue> pair in Values)
                FiltersCommon.HandleValue(
                    /* value=      */ pair.Value.Value,
                    /* account=    */ pair.Value.Account,
                    /* xact=       */ xact,
                    /* temps=      */ Temps,
                    /* handler=    */ (PostHandler)Handler,
                    /* date=       */ rangeFinish.Value,
                    /* act_date_p= */ false);

            Values.Clear();
        }

        public override void Flush()
        {
            if (Values.Any())
                ReportSubtotal();

            base.Flush();
        }

        public override void Handle(Post post)
        {
            ComponentPosts.Add(post);

            Account acct = post.ReportedAccount;
            if (acct == null)
                throw new InvalidOperationException("acct");

            Value amount = Value.Get(post.Amount);
            post.XData.CompoundValue = amount;
            post.XData.Compound = true;

            AcctValue acctValue;
            if (!Values.TryGetValue(acct.FullName, out acctValue))
            {
                Values.Add(acct.FullName, new AcctValue(acct, amount, 
                    post.Flags.HasFlag(SupportsFlagsEnum.POST_VIRTUAL),
                    post.Flags.HasFlag(SupportsFlagsEnum.POST_MUST_BALANCE)));
            }
            else
            {
                if (post.Flags.HasFlag(SupportsFlagsEnum.POST_VIRTUAL) != acctValue.IsVirtual)
                    throw new LogicError(LogicError.ErrorMessageEquityCannotAcceptVirtualAndNonVirtualPostingsToTheSameAccount);

                acctValue.Value = Value.AddOrSetValue(acctValue.Value, amount);
            }

            // If the account for this post is all virtual, mark it as
            // such, so that `handle_value' can show "(Account)" for accounts
            // that contain only virtual posts.

            post.ReportedAccount.XData.AutoVirtualize = true;

            if (!post.Flags.HasFlag(SupportsFlagsEnum.POST_VIRTUAL))
                post.ReportedAccount.XData.HasNonVirtuals = true;
            else if (!post.Flags.HasFlag(SupportsFlagsEnum.POST_MUST_BALANCE))
                post.ReportedAccount.XData.HasUnbVirtuals = true;
        }

        public override void Clear()
        {
            AmountExpr.MarkUncomplited();
            Values.Clear();
            Temps.Clear();
            ComponentPosts.Clear();

            base.Clear();
        }

        public class AcctValue
        {
            public AcctValue(Account account, bool isVirtual = false, bool mustBalance = false)
            {
                Account = account;
                IsVirtual = isVirtual;
                MustBalance = mustBalance;
            }

            public AcctValue(Account account, Value value, bool isVirtual = false, bool mustBalance = false)
                : this(account, isVirtual, mustBalance)
            {
                Value = value;
            }

            public Account Account { get; private set; }
            public Value Value { get; set; }
            public bool IsVirtual { get; private set; }
            public bool MustBalance { get; private set; }
        }
    }
}

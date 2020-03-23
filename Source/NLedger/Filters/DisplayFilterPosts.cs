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
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLedger.Utils;

namespace NLedger.Filters
{
    // This filter requires that calc_posts be used at some point
    // later in the chain.    
    public class DisplayFilterPosts : PostHandler
    {
        public DisplayFilterPosts(PostHandler handler, Report report, bool showRounding)
            : base(handler)
        {
            Report = report;
            DisplayAmountExpr = report.DisplayAmountHandler.Expr;
            DisplayTotalExpr = report.DisplayTotalHandler.Expr;
            ShowRounding = showRounding;
            Temps = new Temporaries();
            CreateAccounts();
        }

        public Report Report { get; private set; }
        public Expr DisplayAmountExpr { get; private set; }
        public Expr DisplayTotalExpr { get; private set; }
        public bool ShowRounding { get; private set; }
        public Value LastDisplayTotal { get; private set; }
        public Temporaries Temps { get; private set; }
        public Account RoundingAccount { get; private set; }

        public Account RevaluedAccount { get; private set; }

        public void CreateAccounts()
        {
            RoundingAccount = Temps.CreateAccount("<Adjustment>");
            RevaluedAccount = Temps.CreateAccount("<Revalued>");
        }

        public bool OutputRounding(Post post)
        {
            BindScope boundScope = new BindScope(Report, post);
            Value newDisplayTotal = new Value();

            if (ShowRounding)
            {
                newDisplayTotal = DisplayTotalExpr.Calc(boundScope).StripAnnotations(Report.WhatToKeep());
                Logger.Current.Debug("filters.changed_value.rounding", () => String.Format("rounding.new_display_total     = {0}", newDisplayTotal));
            }

            // Allow the posting to be displayed if:
            //  1. Its display_amount would display as non-zero, or
            //  2. The --empty option was specified, or
            //  3. a) The account of the posting is <Revalued>, and
            //     b) the revalued option is specified, and
            //     c) the --no-rounding option is not specified.

            if (post.Account == RevaluedAccount)
            {
                if (ShowRounding)
                    LastDisplayTotal = newDisplayTotal;
                return true;
            }

            Value repricedAmount = DisplayAmountExpr.Calc(boundScope).StripAnnotations(Report.WhatToKeep());
            if (!Value.IsNullOrEmptyOrFalse(repricedAmount))
            {
                if (!Value.IsNullOrEmpty(LastDisplayTotal))
                {
                    Logger.Current.Debug("filters.changed_value.rounding", () => String.Format("rounding.repriced_amount       = {0}", repricedAmount));

                    Value preciseDisplayTotal = newDisplayTotal.Truncated() - repricedAmount.Truncated();

                    Logger.Current.Debug("filters.changed_value.rounding", () => String.Format("rounding.precise_display_total = {0}", preciseDisplayTotal));
                    Logger.Current.Debug("filters.changed_value.rounding", () => String.Format("rounding.last_display_total    = {0}", LastDisplayTotal));

                    Value diff = preciseDisplayTotal - LastDisplayTotal;
                    if (!Value.IsNullOrEmptyOrFalse(diff))
                    {
                        Logger.Current.Debug("filters.changed_value.rounding", () => String.Format("rounding.diff                  = {0}", diff));

                        FiltersCommon.HandleValue(
                            /* value=         */ diff,
                            /* account=       */ RoundingAccount,
                            /* xact=          */ post.Xact,
                            /* temps=         */ Temps,
                            /* handler=       */ (PostHandler)Handler,
                            /* date=          */ default(Date),
                            /* act_date_p=    */ true,
                            /* total=         */ preciseDisplayTotal,
                            /* direct_amount= */ true,
                            /* mark_visited=  */ false,
                            /* bidir_link=    */ false);
                    }
                }
                if (ShowRounding)
                    LastDisplayTotal = newDisplayTotal;
                return true;
            }
            else
            {
                return Report.EmptyHandler.Handled;
            }
        }

        public override void Handle(Post post)
        {
            if (OutputRounding(post))
                base.Handle(post);
        }

        public override void Clear()
        {
            DisplayAmountExpr.MarkUncomplited();
            DisplayTotalExpr.MarkUncomplited();

            LastDisplayTotal = new Value();

            Temps.Clear();
            base.Clear();

            CreateAccounts();

        }
    }
}

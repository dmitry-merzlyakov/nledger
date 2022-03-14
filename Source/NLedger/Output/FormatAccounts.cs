// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Chain;
using NLedger.Formatting;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Output
{
    public class FormatAccounts : AccountHandler
    {
        public FormatAccounts(Report report, string format, string prependFormat = null, int prependWidth = 0) : 
            base(null)
        {
            Report = report;
            PrependWidth = prependWidth;
            FirstReportTitle = true;

            DispPred = new Predicate();
            PostedAccounts = new List<Account>(); 
                        
            int pi = format.IndexOf("%/");
            if (pi >= 0)
            {
                AccountLineFormat = new Format(format.Substring(0, pi));
                string n = format.Substring(pi + 2);
                int ppi = n.IndexOf("%/");
                if (ppi >= 0)
                {
                    TotalLineFormat = new Format(n.Substring(0, ppi), AccountLineFormat);
                    SeparatorFormat = new Format(n.Substring(ppi + 2));
                }
                else
                {
                    TotalLineFormat = new Format(n, AccountLineFormat);
                }
            }
            else
            {
                AccountLineFormat = new Format(format);
                TotalLineFormat = new Format(format, AccountLineFormat);
            }

            if (!String.IsNullOrEmpty(prependFormat))
                PrependFormat = new Format(prependFormat);
        }

        public Tuple<int,int> MarkAccounts(Account account, bool flat)
        {
            int visited = 0;
            int toDisplay = 0;

            foreach(Account acc in account.Accounts.Values)
            {
                Tuple<int, int> i = MarkAccounts(acc, flat);
                visited += i.Item1;
                toDisplay += i.Item2;
            }

            if (Logger.Current.ShowDebug(DebugAccountDisplay))
            {
                Logger.Current.Debug(DebugAccountDisplay, () => "Considering account: " + account.FullName);
                if (account.HasXFlags(d => d.Visited))
                    Logger.Current.Debug(DebugAccountDisplay, () => "  it was visited itself");
                Logger.Current.Debug(DebugAccountDisplay, () => "  it has " + visited + " visited children");
                Logger.Current.Debug(DebugAccountDisplay, () => "  it has " + toDisplay + " children to display");
            }

            if (account.Parent != null && (account.HasXFlags(d => d.Visited) || (!flat && visited > 0)))
            {
                BindScope boundScope = new BindScope(Report, account);
                CallScope callScope = new CallScope(boundScope);

                if ((!flat && toDisplay > 1) || ((flat || toDisplay != 1 || account.HasXFlags(d => d.Visited)) &&
                    (Report.EmptyHandler.Handled || !Value.IsNullOrEmptyOrFalse(Report.DisplayValue(Report.FnDisplayTotal(callScope)))) &&
                    !Value.IsNullOrEmptyOrFalse(DispPred.Calc(boundScope))))
                {
                    account.XData.ToDisplay = true;
                    Logger.Current.Debug(DebugAccountDisplay, () => "Marking account as TO_DISPLAY");
                    toDisplay = 1;
                }
                visited = 1;
            }

            return new Tuple<int, int>(visited, toDisplay);
        }

        public override void Title(string str)
        {
            ReportTitle = str;
        }

        public virtual int PostAccount(Account account, bool flat)
        {
            if (!flat && account.Parent != null)
                PostAccount(account.Parent, flat);

            if (account.XData.ToDisplay && !account.XData.Displayed)
            {
                StringBuilder sb = new StringBuilder();

                Logger.Current.Debug("account.display", () => String.Format("Displaying account: {0}", account.FullName));
                account.XData.Displayed = true;

                BindScope boundScope = new BindScope(Report, account);

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

                sb.Append(AccountLineFormat.Calc(boundScope));

                Report.OutputStream.Write(sb.ToString());

                return 1;
            }
            return 0;
        }

        public override void Flush()
        {
            StringBuilder sb = new StringBuilder();

            if (Report.DisplayHandler.Handled)
            {
                Logger.Current.Debug("account.display", () => String.Format("Account display predicate: {0}", Report.DisplayHandler.Str()));
                DispPred.Parse(Report.DisplayHandler.Str());
            }

            MarkAccounts(Report.Session.Journal.Master, Report.FlatHandler.Handled);

            int displayed = PostedAccounts.Sum(account => PostAccount(account, Report.FlatHandler.Handled));

            if (displayed > 1 && !Report.NoTotalHandler.Handled && !Report.PercentHandler.Handled)
            {
                BindScope boundScope = new BindScope(Report, Report.Session.Journal.Master);
                sb.Append(SeparatorFormat?.Calc(boundScope));

                if (PrependFormat != null)
                {
                    sb.AppendFormat(StringExtensions.GetWidthAlignFormatString(PrependWidth), PrependFormat.Calc(boundScope));
                }

                sb.Append(TotalLineFormat.Calc(boundScope));
            }

            Report.OutputStream.Write(sb.ToString());
        }

        /// <summary>
        /// Ported from void format_accounts::operator()(account_t& account)
        /// </summary>
        public override void Handle(Account account)
        {
            Logger.Current.Debug("account.display", () => String.Format("Posting account: {0}", account.FullName));
            PostedAccounts.Add(account);
        }

        public override void Clear()
        {
            DispPred.MarkUncomplited();
            PostedAccounts.Clear();

            ReportTitle = string.Empty;

            base.Clear();
        }

        protected Report Report { get; private set; }
        protected Format AccountLineFormat { get; private set; }
        protected Format TotalLineFormat { get; private set; }
        protected Format SeparatorFormat { get; private set; }
        protected Format PrependFormat { get; private set; }
        protected int PrependWidth { get; private set; }
        protected Predicate DispPred { get; private set; }
        protected bool FirstReportTitle { get; private set; }
        protected string ReportTitle { get; private set; }
        protected IList<Account> PostedAccounts { get; private set; }

        private static readonly string DebugAccountDisplay = "account.display";

    }
}

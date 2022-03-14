// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Items;
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Print
{
    public static class PrintCommon
    {
        /// <summary>
        /// Ported from post_has_simple_amount
        /// </summary>
        public static bool PostHasSimpleAmount(Post post)
        {
            // Is the amount the result of a computation, i.e., it wasn't
            // explicit specified by the user?
            if (post.Flags.HasFlag(SupportsFlagsEnum.POST_CALCULATED))
                return false;

            // Is the amount still empty?  This shouldn't be true by this point,
            // but we check anyway for safety.
            if (post.Amount.IsEmpty)
                return false;

            // Is the amount a complex expression.  If so, the first 'if' should
            // have triggered.
            if (post.AmountExpr != null)
                return false;

            // Is there a balance assignment?  If so, don't elide the amount as
            // that can change the semantics.
            if (post.AssignedAmount != null)
                return false;

            // Does it have an explicitly specified cost (i.e., one that wasn't
            // calculated for the user)?  If so, don't elide the amount!
            if (post.Cost != null && !post.Flags.HasFlag(SupportsFlagsEnum.POST_COST_CALCULATED))
                return false;

            return true;
        }

        public static string PrintNote(string note, bool noteOnNextLine, int columns, int priorWidth)
        {
            StringBuilder sb = new StringBuilder();

            // The 3 is for two spaces and a semi-colon before the note.
            if (noteOnNextLine || (columns > 0 && (columns <= priorWidth + 3 || note.Length > columns - (priorWidth + 3))))
            {
                sb.AppendLine();
                sb.Append("    ;");
            }
            else
            {
                sb.Append("  ;");
            }

            bool needSeparator = false;
            foreach(char c in note)
            {
                if (c == '\n')
                {
                    needSeparator = true;
                }
                else
                {
                    if (needSeparator)
                    {
                        sb.AppendLine();
                        sb.Append("    ;");

                        needSeparator = false;
                    }
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Ported from print_xact
        /// </summary>
        public static string PrintXact(Report report, Xact xact)
        {
            StringBuilder sbOut = new StringBuilder();

            FormatTypeEnum formatType = FormatTypeEnum.FMT_WRITTEN;
            string format = null;

            if (report.DateFormatHandler.Handled)
            {
                formatType = FormatTypeEnum.FMT_CUSTOM;
                format = report.DateFormatHandler.Str();
            }

            StringBuilder buf = new StringBuilder();

            buf.Append(TimesCommon.Current.FormatDate(Item.UseAuxDate ? xact.GetDate() : xact.PrimaryDate(), formatType, format));
            if (!Item.UseAuxDate && (xact.DateAux.HasValue))
                buf.AppendFormat("={0}", TimesCommon.Current.FormatDate(xact.DateAux.Value, formatType, format));

            buf.Append(" ");

            buf.Append(xact.State == ItemStateEnum.Cleared ? "* " : (xact.State == ItemStateEnum.Pending ? "! " : ""));

            if (!String.IsNullOrEmpty(xact.Code))
                buf.AppendFormat("({0}) ", xact.Code);

            buf.Append(xact.Payee);

            string leader = buf.ToString();
            sbOut.Append(leader);

            int columns = report.ColumnsHandler.Handled ? int.Parse(report.ColumnsHandler.Str()) : 80;

            if (!String.IsNullOrEmpty(xact.Note))
                sbOut.Append(PrintNote(xact.Note, xact.Flags.HasFlag(SupportsFlagsEnum.ITEM_NOTE_ON_NEXT_LINE), columns, leader.Length));
            sbOut.AppendLine();

            if (xact.GetMetadata() != null)
            {
                foreach (KeyValuePair<string,ItemTag> data in xact.GetMetadata())
                {
                    if (!data.Value.IsParsed)
                    {
                        sbOut.Append("    ; ");
                        if (!Value.IsNullOrEmptyOrFalse(data.Value.Value))
                            sbOut.AppendFormat("{0}: {1}", data.Key, data.Value.Value.ToString());
                        else
                            sbOut.AppendFormat(":{0}: ", data.Key);
                        sbOut.AppendLine();
                    }
                }
            }

            int count = xact.Posts.Count;
            int index = 0;

            foreach(Post post in xact.Posts)
            {
                index++;

                if (!report.GeneratedHandler.Handled && (post.Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP) || post.Flags.HasFlag(SupportsFlagsEnum.ITEM_GENERATED))
                    && !post.Flags.HasFlag(SupportsFlagsEnum.POST_ANONYMIZED))
                    continue;

                sbOut.Append("    ");

                StringBuilder pbuf = new StringBuilder();

                if (xact.State == ItemStateEnum.Uncleared)
                    pbuf.Append(post.State == ItemStateEnum.Cleared ? "* " : (post.State == ItemStateEnum.Cleared ? "! " : ""));

                if (post.Flags.HasFlag(SupportsFlagsEnum.POST_VIRTUAL))
                {
                    pbuf.Append(post.Flags.HasFlag(SupportsFlagsEnum.POST_MUST_BALANCE) ? "[" : "(");
                }

                pbuf.Append(post.Account.FullName);

                if (post.Flags.HasFlag(SupportsFlagsEnum.POST_VIRTUAL))
                {
                    pbuf.Append(post.Flags.HasFlag(SupportsFlagsEnum.POST_MUST_BALANCE) ? "]" : ")");
                }

                string name = pbuf.ToString();

                int accountWidth = report.AccountWidthHandler.Handled ? int.Parse(report.AccountWidthHandler.Str()) : 36;

                if (accountWidth < name.Length)
                    accountWidth = name.Length;

                if (!post.Flags.HasFlag(SupportsFlagsEnum.POST_CALCULATED) || report.GeneratedHandler.Handled)
                {
                    sbOut.Append(name);
                    int slip = accountWidth - name.Length;

                    int amountWidth = report.AmountWidthHandler.Handled ? int.Parse(report.AmountWidthHandler.Str()) : 12;

                    string amt = String.Empty;
                    if (post.AmountExpr != null)
                    {
                        amt = UniString.Justify(post.AmountExpr.Text, amountWidth, true);
                    }
                    else if (count == 2 && index == 2 && PostHasSimpleAmount(post) && PostHasSimpleAmount(xact.Posts.First()) &&
                        xact.Posts.First().Amount.Commodity == post.Amount.Commodity)
                    {
                        // If there are two postings and they both simple amount, and
                        // they are both of the same commodity, don't bother printing
                        // the second amount as it's always just an inverse of the
                        // first.
                    }
                    else
                    {
                        amt = Value.Get(post.Amount).Print(amountWidth, -1, AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY | (report.GeneratedHandler.Handled ? 0 : AmountPrintEnum.AMOUNT_PRINT_NO_COMPUTED_ANNOTATIONS));
                    }

                    string trimmedAmt = amt.TrimStart();
                    int amtSlip = amt.Length - trimmedAmt.Length;

                    StringBuilder amtBuf = new StringBuilder();
                    if (slip + amtSlip < 2)
                        amtBuf.Append(new string(' ', 2 - (slip + amtSlip)));
                    amtBuf.Append(amt);

                    if (post.GivenCost != null && !post.Flags.HasFlag(SupportsFlagsEnum.POST_CALCULATED) && !post.Flags.HasFlag(SupportsFlagsEnum.POST_COST_CALCULATED))
                    {
                        string costOp = post.Flags.HasFlag(SupportsFlagsEnum.POST_COST_IN_FULL) ? "@@" : "@";
                        if (post.Flags.HasFlag(SupportsFlagsEnum.POST_COST_VIRTUAL))
                            costOp = "(" + costOp + ")";

                        if (post.Flags.HasFlag(SupportsFlagsEnum.POST_COST_VIRTUAL))
                            amtBuf.AppendFormat(" {0} {1}", costOp, post.GivenCost.Abs());
                        else
                            amtBuf.AppendFormat(" {0} {1}", costOp, (post.GivenCost / post.Amount).Abs());
                    }

                    if (post.AssignedAmount != null)
                        amtBuf.AppendFormat(" = {0}", post.AssignedAmount);

                    string trailer = amtBuf.ToString();
                    if (!String.IsNullOrEmpty(trailer))
                    {
                        if (slip > 0)
                            sbOut.AppendFormat(StringExtensions.GetWidthAlignFormatString(slip), " ");
                        sbOut.Append(trailer);

                        accountWidth += trailer.Length;
                    }
                }
                else
                {
                    sbOut.Append(pbuf.ToString());
                }

                if (!String.IsNullOrEmpty(post.Note))
                    sbOut.Append(PrintNote(post.Note, post.Flags.HasFlag(SupportsFlagsEnum.ITEM_NOTE_ON_NEXT_LINE), columns, 4 + accountWidth));
                sbOut.AppendLine();
            }
            return sbOut.ToString();
        }
    }
}

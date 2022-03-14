// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NLedger.Utility.Xml;
using System.Security.Cryptography;
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Commodities;
using NLedger.Items;
using NLedger.Xacts;
using NLedger.Values;

namespace NLedger.Utility.Xml
{
    public static class XmlExtensions
    {
        public static XDocument CreateLedgerDoc()
        {
            long longver = (VersionInfo.Ledger_VERSION_MAJOR << 16) | (VersionInfo.Ledger_VERSION_MINOR << 8) | VersionInfo.Ledger_VERSION_PATCH;
            return new XDocument(new XDeclaration("1.0", "utf-8", ""),
                new XElement("ledger", new XAttribute("version", longver)));
        }

        public static XElement AddElement(this XContainer container, string name, string value = null)
        {
            XElement element = new XElement(name);
            if (!String.IsNullOrEmpty(value))
                element.Value = value;
            container.Add(element);
            return element;
        }

        // put_commodity
        public static void ToXml(this Commodity commodity, XElement element, bool commodityDetails = false)
        {
            StringBuilder flags = new StringBuilder();
            if (!commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED)) flags.Append("P");
            if (commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_SEPARATED)) flags.Append("S");
            if (commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS)) flags.Append("T");
            if (commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA)) flags.Append("D");
            element.Add(new XAttribute("flags", flags.ToString()));

            element.AddElement("symbol", commodity.Symbol);

            if (commodityDetails && commodity.IsAnnotated)
                ((AnnotatedCommodity)commodity).Details.ToXml(element.AddElement("annotation"));
        }

        // put_annotation
        public static void ToXml(this Annotation details, XElement element)
        {
            if (details.Price != null)
                details.Price.ToXml(element.AddElement("price"));

            if (details.Date != null)
                details.Date.Value.ToXml(element.AddElement("date"));

            if (!String.IsNullOrEmpty(details.Tag))
                element.Add("tag", details.Tag);

            if (details.ValueExpr != null)
                element.Add("value_expr", details.ValueExpr.Text);
        }

        // put_amount
        public static void ToXml(this Amount amt, XElement element, bool commodityDetails = false)
        {
            if (amt.HasCommodity)
                amt.Commodity.ToXml(element.AddElement("commodity"), commodityDetails);

            element.AddElement("quantity", amt.Quantity.ToString());
        }

        // put_datetime
        public static void ToXml(this DateTime when, XElement element)
        {
            element.Value = Times.TimesCommon.Current.FormatDateTime(when, Times.FormatTypeEnum.FMT_WRITTEN);
        }

        // put_date
        public static void ToXml(this Date when, XElement element)
        {
            element.Value = Times.TimesCommon.Current.FormatDate(when, Times.FormatTypeEnum.FMT_WRITTEN);
        }

        /*
        public static void ToXml(this DateTime? dateTime, XElement element)
        {
            if (dateTime != null)
                dateTime.Value.ToXml(element);
            else
                element.Value = String.Empty;
        }
         */

        // put_account
        public static void ToXml(this Account acct, XElement element, Func<Account, bool> pred)
        {
            if (pred(acct))
            {
                element.Add(new XAttribute("id", acct.GetAccountId()));

                element.AddElement("name", acct.Name);
                element.AddElement("fullname", acct.FullName);

                Value total = acct.Amount();
                if (!Value.IsNullOrEmpty(total))
                    total.ToXml(element.AddElement("account-amount"));

                total = acct.Total();
                if (!Value.IsNullOrEmpty(total))
                    total.ToXml(element.AddElement("account-total"));

                foreach (Account account in acct.Accounts.Values)
                    account.ToXml(element.AddElement("account"), pred);
            }           
        }

        public static string GetAccountId(this Account acct)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(acct.FullName));
                return BitConverter.ToString(hash);
            }
        }

        // put_value
        public static void ToXml(this Value value, XElement element)
        {
            switch (value.Type)
            {
                case ValueTypeEnum.Void:
                    element.AddElement("void", String.Empty);
                    break;

                case ValueTypeEnum.Boolean:
                    element.AddElement("bool", value.AsBoolean.ToString().ToLower());
                    break;

                case ValueTypeEnum.Integer:
                    element.AddElement("int", value.AsLong.ToString());
                    break;

                case ValueTypeEnum.Amount:
                    value.AsAmount.ToXml(element.AddElement("amount"));
                    break;

                case ValueTypeEnum.Balance:
                    value.AsBalance.ToXml(element.AddElement("balance"));
                    break;

                case ValueTypeEnum.DateTime:
                    value.AsDateTime.ToXml(element.AddElement("datetime"));
                    break;

                case ValueTypeEnum.String:
                    element.AddElement("string", value.AsString);
                    break;

                case ValueTypeEnum.Mask:
                    value.AsMask.ToXml(element.AddElement("mask"));
                    break;

                case ValueTypeEnum.Sequence:
                    {
                        XElement st = element.AddElement("sequence");
                        foreach (Value member in value.AsSequence)
                            member.ToXml(st);
                        break;
                    }

                default: // ValueTypeEnum.Scope, Any
                    throw new InvalidOperationException("assert(false);");
            }
        }

        // put_balance
        public static void ToXml(this Balance bal, XElement element)
        {
            foreach (Amount amount in bal.Amounts.Values)
                amount.ToXml(element.AddElement("amount"));
        }

        // put_mask
        public static void ToXml(this Mask regex, XElement element)
        {
            element.Value = regex.ToString();
        }

        // put_xact
        public static void ToXml(this Xact xact, XElement element)
        {
            if (xact.State == ItemStateEnum.Cleared)
                element.Add(new XAttribute("state", "cleared"));
            else if (xact.State == ItemStateEnum.Pending)
                element.Add(new XAttribute("state", "pending"));

            if (xact.Flags.HasFlag(SupportsFlagsEnum.ITEM_GENERATED))
                element.Add(new XAttribute("generated", "true"));

            if (xact.Date.HasValue)
                xact.Date.Value.ToXml(element.AddElement("date"));
            if (xact.DateAux.HasValue)
                xact.DateAux.Value.ToXml(element.AddElement("aux-date"));

            if (!String.IsNullOrEmpty(xact.Code))
                element.AddElement("code", xact.Code);

            element.AddElement("payee", xact.Payee);

            if (!String.IsNullOrEmpty(xact.Note))
                element.AddElement("note", xact.Note);

            xact.MetaToXml(element);
        }

        // put_metadata
        public static void MetaToXml(this Item item, XElement element)
        {
            if (item.GetMetadata() != null)
            {
                foreach (KeyValuePair<string, ItemTag> meta in item.GetMetadata())
                {
                    if (meta.Value.Value != null)
                    {
                        XElement vt = element.AddElement("value");
                        vt.Add(new XAttribute("key", meta.Key));
                        meta.Value.Value.ToXml(vt);
                    }
                    else
                    {
                        element.AddElement("tag", meta.Key);
                    }
                }
            }
        }

        // put_post
        public static void ToXml(this Post post, XElement element)
        {
            if (post.State == ItemStateEnum.Cleared)
                element.Add(new XAttribute("state", "cleared"));
            else if (post.State == ItemStateEnum.Pending)
                element.Add(new XAttribute("state", "pending"));

            if (post.Flags.HasFlag(SupportsFlagsEnum.POST_VIRTUAL))
                element.Add(new XAttribute("virtual", "true"));
            if (post.Flags.HasFlag(SupportsFlagsEnum.ITEM_GENERATED))
                element.Add(new XAttribute("generated", "true"));

            if (post.Date.HasValue)
                post.Date.Value.ToXml(element.AddElement("date"));
            if (post.DateAux.HasValue)
                post.DateAux.Value.ToXml(element.AddElement("aux-date"));

            if (post.Account != null)
            {
                XElement acc = element.AddElement("account");
                acc.Add(new XAttribute("ref", post.Account.GetAccountId()));
                acc.AddElement("name", post.Account.FullName);
            }

            XElement amt = element.AddElement("post-amount");
            if (post.HasXData && post.XData.Compound)
                post.XData.CompoundValue.ToXml(amt);
            else
                post.Amount.ToXml(amt.AddElement("amount"));

            if (post.Cost != null)
                post.Cost.ToXml(element.AddElement("cost"));

            if (post.AssignedAmount != null)
            {
                if (post.Flags.HasFlag(SupportsFlagsEnum.POST_CALCULATED))
                    post.AssignedAmount.ToXml(element.AddElement("balance-assertion"));
                else
                    post.AssignedAmount.ToXml(element.AddElement("balance-assignment"));
            }

            if (!String.IsNullOrEmpty(post.Note))
                element.AddElement("note", post.Note);

            if (post.GetMetadata() != null)
                post.MetaToXml(element.AddElement("metadata"));

            if (post.HasXData && !Value.IsNullOrEmpty(post.XData.Total))
                post.XData.Total.ToXml(element.AddElement("total"));
        }
    }
}

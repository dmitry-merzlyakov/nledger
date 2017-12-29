// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Commodities;
using NLedger.Items;
using NLedger.Textual;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Csv
{
    public class CsvReader
    {
        public CsvReader(ParseContext context)
        {
            Context = context;

            DateMask = new Mask("date");
            DateAuxMask = new Mask("posted( ?date)?");
            CodeMask = new Mask("code");
            PayeeMask = new Mask("(payee|desc(ription)?|title)");
            AmountMask = new Mask("amount");
            CostMask = new Mask("cost");
            TotalMask = new Mask("total");
            NoteMask = new Mask("note");

            Index = new List<CsvHeadersEnum>();
            Names = new List<string>();

            ReadIndex(Context.Reader);
        }

        public string PathName
        {
            get { return Context.PathName; }
        }

        public int LineNum
        {
            get { return Context.LineNum; }
        }

        public string LastLine
        {
            get { return Context.LineBuf; }
        }

        public Xact ReadXact(bool richData)
        {
            string line = NextLine(Context.Reader);
            string origLine = line;
            if (String.IsNullOrEmpty(line) || !Index.Any())
                return null;
            Context.LineNum++;

            Xact xact = new Xact();
            Post post = new Post();

            xact.State = ItemStateEnum.Cleared;

            xact.Pos = new ItemPosition()
            {
                PathName = Context.PathName,
                BegPos = (int)Context.Reader.Position,
                BegLine = Context.LineNum,
                Sequence = Context.Sequence++
            };

            post.Xact = xact;

            post.Pos = new ItemPosition()
            {
                PathName = Context.PathName,
                BegPos = (int)Context.Reader.Position,
                BegLine = Context.LineNum,
                Sequence = Context.Sequence++
            };

            post.State = ItemStateEnum.Cleared;
            post.Account = null;

            int n = 0;
            Amount amt = null;
            string total = null;
            string field;

            while (!String.IsNullOrEmpty(line) && n < Index.Count)
            {
                field = ReadField(ref line);

                switch(Index[n])
                {
                    case CsvHeadersEnum.FIELD_DATE:
                        xact.Date = TimesCommon.Current.ParseDate(field);
                        break;

                    case CsvHeadersEnum.FIELD_DATE_AUX:
                        if (!String.IsNullOrEmpty(field))
                            xact.DateAux = TimesCommon.Current.ParseDate(field);
                        break;

                    case CsvHeadersEnum.FIELD_CODE:
                        if (!String.IsNullOrEmpty(field))
                            xact.Code = field;
                        break;

                    case CsvHeadersEnum.FIELD_PAYEE:
                        {
                            bool found = false;
                            foreach (Tuple<Mask,string> value in Context.Journal.PayeeAliasMappings)
                            {
                                if (value.Item1.Match(field))
                                {
                                    xact.Payee = value.Item2;
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                                xact.Payee = field;
                            break;
                        }

                    case CsvHeadersEnum.FIELD_AMOUNT:
                        {
                            amt = new Amount();
                            amt.Parse(ref field, AmountParseFlagsEnum.PARSE_NO_REDUCE);
                            if (!amt.HasCommodity && CommodityPool.Current.DefaultCommodity != null)
                                amt.SetCommodity(CommodityPool.Current.DefaultCommodity);
                            post.Amount = amt;
                            break;
                        }

                    case CsvHeadersEnum.FIELD_COST:
                        {
                            amt = new Amount();
                            amt.Parse(ref field, AmountParseFlagsEnum.PARSE_NO_REDUCE);
                            if (!amt.HasCommodity && CommodityPool.Current.DefaultCommodity != null)
                                amt.SetCommodity(CommodityPool.Current.DefaultCommodity);
                            post.Cost = amt;
                            break;
                        }

                    case CsvHeadersEnum.FIELD_TOTAL:
                        total = field;
                        break;

                    case CsvHeadersEnum.FIELD_NOTE:
                        if (!String.IsNullOrEmpty(field))
                            xact.Note = field;
                        break;

                    case CsvHeadersEnum.FIELD_UNKNOWN:
                        if (!String.IsNullOrEmpty(Names[n]) && !String.IsNullOrEmpty(field))
                            xact.SetTag(Names[n], Value.StringValue(field));
                        break;

                    default:
                        throw new InvalidOperationException();
                }
                n++;
            }

            if (richData)
            {
                xact.SetTag("Imported", Value.StringValue(TimesCommon.Current.FormatDate(TimesCommon.Current.CurrentDate, FormatTypeEnum.FMT_WRITTEN).ToString()));
                xact.SetTag("CSV", Value.StringValue(origLine));
            }

            // Translate the account name, if we have enough information to do so

            foreach(Tuple<Mask,Account> value in Context.Journal.PayeesForUnknownAccounts)
            {
                if (value.Item1.Match(xact.Payee))
                {
                    post.Account = value.Item2;
                    break;
                }
            }

            xact.AddPost(post);

            // Create the "balancing post", which refers to the account for this data

            post = new Post();
            post.Xact = xact;

            post.Pos = new ItemPosition()
            {
                PathName = Context.PathName,
                BegPos = (int)Context.Reader.Position,
                BegLine = Context.LineNum,
                Sequence = Context.Sequence++
            };

            post.State = ItemStateEnum.Cleared;
            post.Account = Context.Master;

            if (amt != null && !amt.IsEmpty)
                post.Amount = amt.Negated();

            if (!String.IsNullOrEmpty(total))
            {
                amt = new Amount();
                amt.Parse(ref total, AmountParseFlagsEnum.PARSE_NO_REDUCE);
                if (!amt.HasCommodity && CommodityPool.Current.DefaultCommodity != null)
                    amt.SetCommodity(CommodityPool.Current.DefaultCommodity);
                post.AssignedAmount = amt;
            }

            xact.AddPost(post);
            return xact;
        }

        protected ParseContext Context { get; private set; }

        protected Mask DateMask { get; private set; }
        protected Mask DateAuxMask { get; private set; }
        protected Mask CodeMask { get; private set; }
        protected Mask PayeeMask { get; private set; }
        protected Mask AmountMask { get; private set; }
        protected Mask CostMask { get; private set; }
        protected Mask TotalMask { get; private set; }
        protected Mask NoteMask { get; private set; }

        protected IList<CsvHeadersEnum> Index { get; private set; }
        protected IList<string> Names { get; private set; }

        protected string ReadField(ref string line)
        {
            StringBuilder field = new StringBuilder();
            InputTextStream inStream = new InputTextStream(line);

            char c;
            if (inStream.Peek == '"' || inStream.Peek == '|')
            {
                c = inStream.Get();
                char x;
                while(!inStream.Eof)
                {
                    x = inStream.Get();
                    if (x == '\\')
                        x = inStream.Get();
                    else if (x == '"' && inStream.Peek == '"')
                        x = inStream.Get();
                    else if (x == c)
                    {
                        if (x == '|')
                            inStream.Unget();
                        else if (inStream.Peek == ',')
                            c = inStream.Get();
                        break;
                    }
                    if (x != default(char))
                        field.Append(x);
                }
            }
            else
            {
                while (!inStream.Eof)
                {
                    c = inStream.Get();

                    if (c == ',')
                        break;

                    if (c != default(char))
                        field.Append(c);
                }
            }

            line = inStream.RemainSource;
            return field.ToString().Trim();
        }

        protected void ReadIndex(ITextualReader reader)
        {
            string line = NextLine(reader);

            while (!String.IsNullOrEmpty(line))
            {
                string field = ReadField(ref line);
                Names.Add(field);

                if (DateMask.Match(field))
                    Index.Add(CsvHeadersEnum.FIELD_DATE);
                else if (DateAuxMask.Match(field))
                    Index.Add(CsvHeadersEnum.FIELD_DATE_AUX);
                else if (CodeMask.Match(field))
                    Index.Add(CsvHeadersEnum.FIELD_CODE);
                else if (PayeeMask.Match(field))
                    Index.Add(CsvHeadersEnum.FIELD_PAYEE);
                else if (AmountMask.Match(field))
                    Index.Add(CsvHeadersEnum.FIELD_AMOUNT);
                else if (CostMask.Match(field))
                    Index.Add(CsvHeadersEnum.FIELD_COST);
                else if (TotalMask.Match(field))
                    Index.Add(CsvHeadersEnum.FIELD_TOTAL);
                else if (NoteMask.Match(field))
                    Index.Add(CsvHeadersEnum.FIELD_NOTE);
                else
                    Index.Add(CsvHeadersEnum.FIELD_UNKNOWN);
            }
        }

        protected string NextLine(ITextualReader reader)
        {
            while(!reader.IsEof())
            {
                Context.LineBuf = reader.ReadLine();
                if (Context.LineBuf.StartsWith("#"))
                    continue;

                return Context.LineBuf;
            }
            return string.Empty;
        }
    }
}

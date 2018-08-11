// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Expressions;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Annotate
{
    /// <summary>
    /// Ported from annotation_t
    /// </summary>
    public class Annotation : IEquatable<Annotation>
    {
        /// <summary>
        /// Equal to operator bool() const
        /// </summary>
        public static bool IsNullOrEmpty(Annotation annotation)
        {
            return annotation == null ||
                (Amount.IsNullOrEmpty(annotation.Price) && (!annotation.Date.HasValue) &&
                String.IsNullOrEmpty(annotation.Tag) && annotation.ValueExpr== null);
        }

        public Annotation(Amount price = null)
        {
            Price = price;
        }

        public Annotation(Amount price, Date? date, string tag)
        {
            Price = price;
            Date = date;
            Tag = tag;
        }

        public Amount Price { get; set; }
        public Date? Date { get; set; }
        public Expr ValueExpr { get; set; }
        public string Tag { get; set; }

        public bool IsPriceNotPerUnit { get; set; }
        public bool IsPriceFixated { get; set; }
        public bool IsPriceCalculated { get; set; }
        public bool IsDateCalculated { get; set; }
        public bool IsTagCalculated { get; set; }
        public bool IsValueExprCalculated { get; set; }

        public void Parse(ref string line)
        {
            if (line == null)
                throw new ArgumentNullException("line");

            while(true)
            {
                string originalLine = line;

                line = line.TrimStart();
                if (line.StartsWith("{"))
                {
                    if (Price != null)
                        throw new AmountError(AmountError.ErrorMessageMoreThanOnePriceForCommodity);

                    line = line.Remove(0, 1);
                    if (line.StartsWith("{"))
                    {
                        line = line.Remove(0, 1);
                        IsPriceNotPerUnit = true;
                    }
                    line = line.TrimStart();
                    if (line.StartsWith("="))
                    {
                        line = line.Remove(0, 1);
                        IsPriceFixated = true;
                    }
                    string buf = StringExtensions.ReadInto(ref line, WhileNotClosedFigureBracket);
                    if (line.StartsWith("}"))
                    {
                        line = line.Remove(0, 1);
                        if (IsPriceNotPerUnit)
                        {
                            if (line.StartsWith("}"))
                                line = line.Remove(0, 1);
                            else
                                throw new AmountError(AmountError.ErrorMessageCommodityLotPriceLacksDoubleClosingBrace);
                        }
                    }
                    else
                    {
                        throw new AmountError(AmountError.ErrorMessageCommodityLotPriceLacksClosingBrace);
                    }

                    Price = new Amount();
                    PriceParse(ref buf);
                    Logger.Current.Debug("commodity.annotations", () => String.Format("Parsed annotation price: {0}", Price));
                }
                else if (line.StartsWith("["))
                {
                    if (Date.HasValue)
                        throw new AmountError(AmountError.ErrorMessageCommoditySpecifiesMoreThanOneDate);
                    line = line.Remove(0, 1);
                    string buf = StringExtensions.ReadInto(ref line, WhileNotClosedSquareBracket);
                    if (line.StartsWith("]"))
                        line = line.Remove(0, 1);
                    else
                        throw new AmountError(AmountError.ErrorMessageCommodityDateLacksClosingBracket);

                    Date = TimesCommon.Current.ParseDate(buf);
                }
                else if (line.StartsWith("("))
                {
                    line = line.Remove(0, 1);
                    if (line.StartsWith("@"))
                    {
                        line = originalLine;
                        break;
                    }
                    else if (line.StartsWith("("))
                    {
                        if (ValueExpr != null)
                            throw new AmountError(AmountError.ErrorMessageCommoditySpecifiesMoreThanOneExpression);

                        line = line.Remove(0, 1);
                        string buf = StringExtensions.ReadInto(ref line, WhileNotClosedBracket);
                        if (line.StartsWith(")"))
                        {
                            line = line.Remove(0, 1);
                            if (line.StartsWith(")"))
                                line = line.Remove(0, 1);
                            else
                                throw new AmountError(AmountError.ErrorMessageCommodityExpressionLacksClosingParentheses);
                        }
                        else
                        {
                            throw new AmountError(AmountError.ErrorMessageCommodityExpressionLacksClosingParentheses);
                        }

                        ValueExpr = CreateExpr(buf);
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(Tag))
                            throw new AmountError(AmountError.ErrorMessageCommoditySpecifiesMoreThanOneTag);
                        string buf = StringExtensions.ReadInto(ref line, WhileNotClosedBracket);
                        if (line.StartsWith(")"))
                            line = line.Remove(0, 1);
                        else
                            throw new AmountError(AmountError.ErrorMessageCommodityTagLacksClosingParenthesis);

                        Tag = buf;
                    }
                } else
                {
                    line = originalLine;
                    break;
                }
            }

            if (Logger.Current.ShowDebug("amount.commodities") && !IsNullOrEmpty(this))
                Logger.Current.Debug("amount.commodities", () => String.Format("Parsed commodity annotations:\r\n{0}", this));
        }

        public bool Equals(Annotation other)
        {
            return other != null &&
                Price == other.Price && Date == other.Date && Tag == other.Tag && ValueExpr == other.ValueExpr;
        }

        public override int GetHashCode()
        {
            return (Price?.GetHashCode() ?? 0) ^ (Date?.GetHashCode() ?? 0) ^ (Tag?.GetHashCode() ?? 0) ^ (ValueExpr?.GetHashCode() ?? 0);
        }

        protected virtual void PriceParse(ref string buf)
        {
            Price.Parse(ref buf, AmountParseFlagsEnum.PARSE_NO_MIGRATE);
        }

        protected virtual Expr CreateExpr(string buf)
        {
            return new Expr(buf);
        }

        /// <summary>
        /// Ported from annotation_t::print
        /// </summary>
        public string Print(bool keepBase = false, bool noComputedAnnotations = false)
        {
            StringBuilder sb = new StringBuilder();

            if (Price != null && (!noComputedAnnotations || !IsPriceCalculated))
                sb.AppendFormat(" {{{0}{1}}}", IsPriceFixated ? "=" : "", keepBase ? Price : Price.Unreduced());

            if (Date.HasValue && (!noComputedAnnotations || !IsDateCalculated))
                sb.AppendFormat(" [{0}]", TimesCommon.Current.FormatDate(Date.Value, FormatTypeEnum.FMT_PRINTED));

            if (!String.IsNullOrEmpty(Tag) && (!noComputedAnnotations || !IsTagCalculated))
                sb.AppendFormat(" ({0})", Tag);

            if (ValueExpr != null && !IsValueExprCalculated)
                sb.AppendFormat(" (({0}))", ValueExpr);

            return sb.ToString();
        }

        private static Func<char, bool> WhileNotClosedFigureBracket = (c) => c != '}';
        private static Func<char, bool> WhileNotClosedSquareBracket = (c) => c != ']';
        private static Func<char, bool> WhileNotClosedBracket = (c) => c != ')';
    }
}

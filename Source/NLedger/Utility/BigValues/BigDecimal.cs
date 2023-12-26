// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.BigValues
{
    /// <summary>
    /// Implementation of IBigValue interface that uses Decimal type for arithmetic operations.
    /// </summary>
    /// <remarks>
    /// The purpose of having IBigValue driven by Decimal is to provide an option for systems 
    /// that do not support BigInteger (and BigRational respectively).
    /// In this case, they may recomplile NLedger with BigDecimal and get the same functionality
    /// but limited with Decimal quantities. See Amount.cs to get more information how to switch arithmetics.
    /// 
    /// By default, NLedger uses BigRational.
    /// </remarks>
    public struct BigDecimal : IBigValue<BigDecimal>
    {
        private readonly decimal Value;
        private BigDecimal(decimal value)
        {
            Value = value;
        }

        public void Abs(out BigDecimal result)
        {
            result = new BigDecimal(Math.Abs(Value));
        }

        public void Add(out BigDecimal result, ref BigDecimal addend)
        {
            result = new BigDecimal(Value + addend.Value);
        }

        public void Ceiling(out BigDecimal result)
        {
            result = new BigDecimal(Math.Ceiling(Value));
        }

        public int CompareTo(ref BigDecimal value)
        {
            return Value.CompareTo(value.Value);
        }

        public bool ConvertibleToLong()
        {
            var integral = Math.Floor(Value);
            return Value - integral == 0 && integral >= long.MinValue && integral <= long.MaxValue;
        }

        public void Divide(out BigDecimal result, ref BigDecimal divisor)
        {
            result = new BigDecimal(Value / divisor.Value);
        }

        public bool Equals(ref BigDecimal value)
        {
            return Value.Equals(value.Value);
        }

        public void Floor(out BigDecimal result)
        {
            result = new BigDecimal(Math.Floor(Value));
        }

        public void FromDouble(out BigDecimal result, double value)
        {
            result = new BigDecimal((decimal)value);
        }

        public void FromLong(out BigDecimal result, long value)
        {
            result = new BigDecimal(value);
        }

        public bool IsZero()
        {
            return Value == 0;
        }

        public void Multiply(out BigDecimal result, ref BigDecimal multiplier)
        {
            result = new BigDecimal(Value * multiplier.Value);
        }

        public void Negate(out BigDecimal result)
        {
            result = new BigDecimal(-Value);
        }

        public void Parse(out BigDecimal result, string s, IFormatProvider provider)
        {
            result = new BigDecimal(Decimal.Parse(s, provider));
        }

        public void Round(out BigDecimal result, int places)
        {
            result = new BigDecimal(Math.Round(Value, places));
        }

        public void Scale(out BigDecimal result, int scale)
        {
            decimal decPow = (decimal)Math.Pow(10, scale);
            result = new BigDecimal(Value / decPow);
        }

        public int Sign()
        {
            return Math.Sign(Value);
        }

        public void Subtract(out BigDecimal result, ref BigDecimal subtractor)
        {
            result = new BigDecimal(Value - subtractor.Value);
        }

        public decimal ToDecimal()
        {
            return Value;
        }

        public long ToLong()
        {
            return (long)Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(format, formatProvider);
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}

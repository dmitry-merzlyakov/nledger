// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    [Serializable]
    public struct Date : IComparable, IFormattable, IConvertible, /* ISerializable, */ IComparable<Date>, IEquatable<Date>
    {
        public static bool HasTimePart(DateTime dateTime)
        {
            return dateTime.TimeOfDay != TimeSpan.Zero;
        }

        public static implicit operator DateTime(Date date)
        {
            return date.Value;
        }

        public static explicit operator Date(DateTime dateTime)
        {
            return new Date(dateTime);
        }

        public static bool TryParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style, out Date result)
        {
            result = default(Date);

            DateTime dateTimeResult;
            if (DateTime.TryParseExact(s, format, provider, style, out dateTimeResult))
            {
                if (HasTimePart(dateTimeResult))
                    return false;

                result = (Date)dateTimeResult;
                return true;
            }

            return false;
        }

        public static bool operator ==(Date d1, Date d2)
        {
            return d1.Value == d2.Value;
        }

        public static bool operator !=(Date d1, Date d2)
        {
            return d1.Value != d2.Value;
        }

        public static bool operator <(Date d1, Date d2)
        {
            return d1.Value < d2.Value;
        }

        public static bool operator >(Date d1, Date d2)
        {
            return d1.Value > d2.Value;
        }

        public static bool operator <=(Date d1, Date d2)
        {
            return d1.Value <= d2.Value;
        }

        public static bool operator >=(Date d1, Date d2)
        {
            return d1.Value >= d2.Value;
        }

        public static Date operator +(Date d, TimeSpan t)
        {
            return (Date)(d.Value + t);
        }

        public static TimeSpan operator -(Date d1, Date d2)
        {
            return d1.Value - d2.Value;
        }

        public static Date operator -(Date d, TimeSpan t)
        {
            return (Date)(d.Value - t);
        }


        public Date(int year, int month, int day) 
            : this(new DateTime(year, month, day))
        { }

        private Date(DateTime dateTime)
        {
            if (HasTimePart(dateTime))
                throw new ArgumentException(String.Format("DateTime {0} cannot be converted to Date because it has time part", dateTime));

            Value = dateTime;
        }

        public int Year
        {
            get { return Value.Year; }
        }

        public int Month
        {
            get { return Value.Month; }
        }

        public int Day
        {
            get { return Value.Day; }
        }

        public long Ticks
        {
            get { return Value.Ticks; }
        }

        public DayOfWeek DayOfWeek
        {
            get { return Value.DayOfWeek; }
        }

        public Date AddDays(int value)
        {
            return (Date)Value.AddDays(value);
        }

        public Date AddMonths(int value)
        {
            return (Date)Value.AddMonths(value);
        }

        public Date AddYears(int value)
        {
            return (Date)Value.AddYears(value);
        }

        public TimeSpan Subtract(Date value)
        {
            return Value.Subtract(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public string ToString(string format)
        {
            return Value.ToString(format);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(format, formatProvider);
        }

        public string ToString(IFormatProvider provider)
        {
            return Value.ToString(provider);
        }

        public int CompareTo(Date other)
        {
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(object obj)
        {
            return Value.CompareTo(obj);
        }

        public bool Equals(Date other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() == typeof(Date))
                return Equals((Date)obj);

            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public TypeCode GetTypeCode()
        {
            return Value.GetTypeCode();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToBoolean(provider);
        }

        public char ToChar(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToChar(provider);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToSByte(provider);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToByte(provider);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToInt16(provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToUInt16(provider);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToInt32(provider);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToUInt32(provider);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToInt64(provider);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToUInt64(provider);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToSingle(provider);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToDouble(provider);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToDecimal(provider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToDateTime(provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return ((IConvertible)Value).ToType(conversionType, provider);
        }

        private readonly DateTime Value;
    }
}

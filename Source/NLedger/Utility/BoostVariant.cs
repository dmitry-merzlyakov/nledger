// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public sealed class BoostVariant : IEquatable<BoostVariant>
    {
        public static readonly BoostVariant Empty = new BoostVariant();

        public static bool operator == (BoostVariant x, BoostVariant y)
        {
            if (Object.ReferenceEquals(x, null))
                return Object.ReferenceEquals(y, null);

            return x.Equals(y);
        }

        public static bool operator !=(BoostVariant x, BoostVariant y)
        {
            if (Object.ReferenceEquals(x, null))
                return Object.ReferenceEquals(y, null);

            return !x.Equals(y);
        }

        public BoostVariant()
        { }

        public BoostVariant(params Type[] allowedTypes)
        {
            AllowedTypes = allowedTypes;
        }

        public BoostVariant(object value)
        {
            data = value;
        }

        public bool IsEmpty
        {
            get { return data == null; }
        }

        public Type Type
        {
            get { return IsEmpty ? null : data.GetType(); }
        }

        public object Value
        {
            get { return data; }
        }

        public T GetValue<T>()
        {
            return (T)data;
        }

        public void SetValue<T>(T value)
        {
            object objValue = value;

            if (objValue != null && AllowedTypes != null)
            {
                Type type = typeof(T);

                if (type == typeof(BoostVariant))
                {
                    BoostVariant boostVariant = (BoostVariant)objValue;
                    type = boostVariant.Type;
                    objValue = boostVariant.Value;
                }

                if (type != null && !AllowedTypes.Contains(type))
                    throw new ArgumentException(String.Format("This type is not allowed: {0}", type));
            }

            data = objValue;
        }

        public override string ToString()
        {
            if (IsEmpty)
                return base.ToString();
            else
                return Value.ToString();
        }

        public bool Equals(BoostVariant other)
        {
            if (Object.ReferenceEquals(other, null))
                return false;

            return Object.Equals(data, other.data);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as BoostVariant);
        }

        public override int GetHashCode()
        {
            return data == null ? 0 : data.GetHashCode();
        }

        private object data = null;
        private readonly Type[] AllowedTypes = null;
    }
}

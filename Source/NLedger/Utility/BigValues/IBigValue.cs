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

namespace NLedger.Utility.BigValues
{
    /// <summary>
    /// This interface specifies all arithmetical operations that
    /// Ledger needs for managing amount quantities.
    /// </summary>
    /// <remarks>
    /// The original Ledger performs all manipulations with quantities
    /// by means of bigint_t that wraps up MPFR arbitrary precision arithmetic.
    /// 
    /// In .Net application, this interface allows to generalize BigInt class and to abstract
    /// the way of performing arithmetical operations so that you can have
    /// different implementation of them.
    /// 
    /// The basic rules applicable to all interface methods are:
    /// - Instances of this interface (structs or classes) are immutable;
    /// - All operations do not change the state of the current instance;
    /// - All operations do not change the state of operands;
    /// - All operations only produce new result instances.
    /// </remarks>
    /// <typeparam name="T">The class or struct that performs arithmetical operations.</typeparam>
    public interface IBigValue<T> : IFormattable
    {
        void Add(out T result, ref T addend);
        void Subtract(out T result, ref T subtractor);
        void Multiply(out T result, ref T multiplier);
        void Divide(out T result, ref T divisor);

        void Negate(out T result);
        void Abs(out T result);
        void Floor(out T result);
        void Ceiling(out T result);
        void Round(out T result, int places);
        int Sign();

        bool IsZero();
        bool ConvertibleToLong();
        void Scale(out T result, int scale);

        long ToLong();
        decimal ToDecimal();

        bool Equals(ref T value);
        int CompareTo(ref T value);

        // Factory methods that make no sense for regular values. They are here by technical reasons.
        void Parse(out T result, string s, IFormatProvider provider);
        void FromLong(out T result, long value);
        void FromDouble(out T result, double value);
    }
}

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

namespace NLedger.Utility
{
    public static class ObjectExtensions
    {
        public static Type SafeGetType(this object obj)
        {
            return obj != null ? obj.GetType() : null;
        }

        // Note: Equals & GetHasCode must be overriden in V class; otherwise comparison will be performed on references instead of actual values
        // See: http://stackoverflow.com/questions/13470335/comparing-two-dictionaries-for-equal-data-in-c
        public static bool Compare<K,V>(this IDictionary<K,V> dic1, IDictionary<K,V> dic2)
        {
            if (dic1 == dic2)
                return true;

            if (dic1 == null || dic2 == null)
                return false;

            return dic1.Count == dic2.Count && !dic1.Except(dic2).Any();
        }

        public static IEnumerable<T> RecursiveEnum<T>(this T instance, Func<T,T> getParent, bool skipCurrent = true)
        {
            if (getParent == null)
                throw new ArgumentNullException("getParent");

            if (instance != null)
            {
                if (!skipCurrent)
                    yield return instance;

                while ((instance = getParent(instance)) != null)
                    yield return instance;
            }
        }

        public static IEnumerable<T> RecursiveEnum<T>(this T instance, Func<T, IEnumerable<T>> getChildren)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            if (getChildren == null)
                throw new ArgumentNullException("getChildren");

            yield return instance;

            foreach (T child in getChildren(instance) ?? Enumerable.Empty<T>())
            {
                foreach(T grandChild in child.RecursiveEnum(getChildren))
                    yield return grandChild;
            }
        }
    }
}

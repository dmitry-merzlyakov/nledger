// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Scopus
{
    public static class ScopeExtensions
    {
        /// <summary>
        /// Ported from T * search_scope(scope_t * ptr, bool prefer_direct_parents = false)
        /// </summary>
        public static T SearchScope<T>(this Scope scope, bool preferDirectParent = false) where T : Scope
        {
            if (scope == null)
                throw new ArgumentNullException("scope");

            Logger.Current.Debug("scope.search", () => String.Format("Searching scope {0}", scope.Description));

            T sought = scope as T;
            if (sought != null)
                return sought;

            BindScope bindScope = scope as BindScope;
            if (bindScope != null)
            {
                sought = SearchScope<T>(preferDirectParent ? bindScope.Parent : bindScope.GrandChild);
                if (sought != null)
                    return sought;
                return SearchScope<T>(preferDirectParent ? bindScope.GrandChild : bindScope.Parent);
            }

            ChildScope childScope = scope as ChildScope;
            if (childScope != null)
                return SearchScope<T>(childScope.Parent);

            return null;
        }

        public static T FindScope<T>(this ChildScope scope, bool skipThis = true, bool preferDirectParent = false) where T : Scope
        {
            if (scope == null)
                throw new ArgumentNullException("scope");

            T sought = SearchScope<T>(skipThis ? scope.Parent : scope, preferDirectParent);
            if (sought == null)
                throw new Exception("Could not find scope");
            return sought;
        }

        public static T FindScope<T>(this Scope scope, bool preferDirectParent = false) where T : Scope
        {
            if (scope == null)
                throw new ArgumentNullException("scope");

            T sought = SearchScope<T>(scope, preferDirectParent);
            if (sought == null)
                throw new Exception("Could not find scope");
            return sought;
        }
    }
}

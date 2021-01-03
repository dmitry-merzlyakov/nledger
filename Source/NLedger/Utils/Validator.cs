// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utils
{
    public class Validator
    {
        public static bool IsVerifyEnabled
        {
            get { return MainApplicationContext.Current.IsVerifyEnabled; }
            set { MainApplicationContext.Current.IsVerifyEnabled = value; }
        }

        /// <summary>
        /// Ported from #define assert(x)
        /// </summary>
        public static void Verify(Func<bool> resultFunc)
        {
            if (resultFunc == null)
                throw new ArgumentNullException("resultFunc");

            if (IsVerifyEnabled && !resultFunc())
            {
                // [DM] Original implementation debug_assert publishes four pieces of information:
                // - reason (the text of an assertion expression);
                // - func (the name of a containing method;
                // - file (the name of source file);
                // - line (line num in the source file).
                // In .Net world, file and line are basically available till debug symbols are available.
                // Otherwise, empty information will be printed.
                // On the other hand, it makes sense to print the namespec, class name and method name 
                // (not only the method name).
                // And the final moment is that "reason" is completely unavailable as long as
                // we use a generic lambda (not expression tree). We will print a useless mnemonic name.
                var stackFrame = new StackTrace().GetFrames().Skip(1).First();
                MethodBase methodBase = stackFrame.GetMethod();
                DebugAssert(resultFunc.ToString(),
                    methodBase.DeclaringType.Namespace, methodBase.DeclaringType.Name, methodBase.Name,
                    stackFrame.GetFileName(), stackFrame.GetFileLineNumber());
            }
        }

        /// <summary>
        /// Ported from debug_assert
        /// </summary>
        public static void DebugAssert(string reason, string nspace, string classname, string methodname, string file, long line)
        {
            var message = String.Format("Assertion failed in {0} [namespace {1}, class {2}, method {3}]; assertion function {4}.", ErrorContext.FileContext(file, line),
                nspace, classname, methodname, reason);
            throw new AssertionFailedError(message);
        }
    }
}

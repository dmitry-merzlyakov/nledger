// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Expressions
{
    /// <summary>
    ///  * This class provides basic behavior for all the domain specific expression
    ///  * languages used in Leger:
    ///  *
    ///  * | Typename    | Description                | result_type     | Derives     |
    ///  * |-------------+----------------------------+-----------------+-------------|
    ///  * | expr_t      | Value expressions          | value_t         |             |
    ///  * | predicate_t | Special form of expr_t     | bool            | expr_t      |
    ///  * | query_t     | Report queries             | bool            | predicate_t |
    ///  * | period_t    | Time periods and durations | date_interval_t |             |
    ///  * | draft_t     | Partially filled xacts     | xact_t *        |             |
    ///  * | format_t    | Format strings             | string          |             |
    /// </summary>
    public abstract class ExprBase<ResultType> where ResultType : class
    {
        const string StreamKey = "<stream>";

        public ExprBase(Scope context = null)
        {
            Context = context;
            IsCompiled = false;
        }

        public ExprBase(ExprBase<ResultType> other)
        {
            Context = other.Context;
            Str = other.Str;
            IsCompiled = other.IsCompiled;
        }

        public virtual bool IsEmpty
        {
            get { return String.IsNullOrWhiteSpace(Str); }
        }

        public virtual string Text
        {
            get { return Str; }
            set
            {
                Str = value;
                IsCompiled = false;
            }
        }

        public virtual void Assign(ExprBase<ResultType> expr)
        {
            if (this != expr)
            {
                Str = expr.Str;
                Context = expr.Context;
                IsCompiled = expr.IsCompiled;
            }
        }

        public void Assign(string expr)
        {
            Parse(expr);
        }

        public void Parse(string exprStr, AmountParseFlagsEnum flags = AmountParseFlagsEnum.PARSE_DEFAULT)
        {
            Parse(new InputTextStream(exprStr), flags, exprStr);
        }

        public virtual void Parse(InputTextStream inStream, AmountParseFlagsEnum flags = AmountParseFlagsEnum.PARSE_DEFAULT, string originalString = null)
        {
            Text = !string.IsNullOrEmpty(originalString) ? originalString : StreamKey;
        }

        public virtual void MarkUncomplited()
        {
            IsCompiled = false;
        }

        public void Recompile(Scope scope)
        {
            IsCompiled = false;
            Compile(scope);
        }

        public virtual void Compile(Scope scope)
        {
            if (!IsCompiled)
            {
                // Derived classes need to do something here.
                Context = scope;
                IsCompiled = true;
            }
        }

        public ResultType Calc(Scope scope)
        {
            if (!IsCompiled)
            {
                Logger.Current.Debug("expr.compile", () => String.Format("Before compilation:{0}", Dump()));
                Logger.Current.Debug("expr.compile", () => String.Format("Compiling: {0}", Str));
                Compile(scope);
                Logger.Current.Debug("expr.compile", () => String.Format("After compilation:{0}", Dump()));
            }

            Logger.Current.Debug("expr.calc", () => String.Format("Calculating: {0}", Str));
            return RealCalc(scope);
        }

        public ResultType Calc()
        {
            if (Context == null)
                throw new InvalidOperationException("No context");

            return Calc(Context);
        }

        public string PrintToStr()
        {
            return Print();
        }

        public virtual string ContextToStr()
        {
            return String.Empty;
        }

        public virtual string Print()
        {
            return String.Empty;
        }

        public override string ToString()
        {
            return Print();
        }

        public virtual string Dump()
        {
            return String.Empty;
        }

        public Scope Context { get; set;}
        protected string Str { get; private set; }
        protected bool IsCompiled { get; private set; }

        protected abstract ResultType RealCalc(Scope scope);
    }
}

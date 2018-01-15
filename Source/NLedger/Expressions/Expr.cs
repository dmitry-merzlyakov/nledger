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
using NLedger.Utility;
using System.IO;
using NLedger.Amounts;
using NLedger.Values;
using NLedger.Scopus;

namespace NLedger.Expressions
{
    /// <summary>
    /// Ported from func_t
    /// </summary>
    public delegate Value ExprFunc(Scope scope);

    public static class ExprFuncExtensions
    {
        public static bool IsNullOrEmpty(this ExprFunc exprFunc)
        {
            return exprFunc == null || exprFunc == Expr.EmptyFunc;
        }
    }

    /// <summary>
    /// Ported from expr_t
    /// </summary>
    public class Expr : ExprBase<Value>, IEquatable<Expr>
    {
        public static Expr Empty = new Expr(String.Empty);
        public static ExprFunc EmptyFunc = s => { return Value.Empty; };

        public static bool operator ==(Expr left, Expr right)
        {
            if (Object.Equals(left, null))
                return Object.Equals(right, null);
            else
                return left.Equals(right);
        }

        public static bool operator !=(Expr left, Expr right)
        {
            if (Object.Equals(left, null))
                return !Object.Equals(right, null);
            else
                return !left.Equals(right);
        }

        /// <summary>
        /// Dealing with expr pointers tucked into value objects.
        /// </summary>
        public static bool IsExpr(Value value)
        {
            return value.Type == ValueTypeEnum.Any && value.AsAny().SafeGetType() == typeof(ExprOp);
        }

        public static ExprOp AsExpr(Value value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Type != ValueTypeEnum.Any)
                throw new ArgumentException("value");

            return value.AsAny<ExprOp>();
        }

        public static Value SourceCommand(CallScope args)
        {
            string pathname;
            TextReader reader;

            if (args.Has(0))
            {
                pathname = args.Get<string>(0);
                reader = FileSystem.GetStreamReader(pathname);
            }
            else
            {
                pathname = "<stdin>";
                reader = FileSystem.ConsoleInput;
            }

            SymbolScope filelocals = new SymbolScope(args);
            int lineNum = 0;
            string line;
            int pos = 0;
            int endpos = 0;

            while ((line = reader.ReadLine()) != null)
            {
                pos = endpos;
                endpos = line.Length + Environment.NewLine.Length;
                lineNum++;

                line = line.TrimStart();
                if (!String.IsNullOrEmpty(line) && !line.StartsWith(";"))
                {
                    try
                    {
                        new Expr(line).Calc(filelocals);
                    }
                    catch
                    {
                        ErrorContext.Current.AddErrorContext(String.Format("While parsing value expression on line {0}:", lineNum));
                        ErrorContext.Current.AddErrorContext(ErrorContext.SourceContext(pathname, pos, endpos, "> "));
                    }
                }
            }

            return Value.True;
        }

        public Expr()
        { }

        public Expr(Expr other) : base(other)
        {
            Op = other.Op;
        }

        public Expr(string str, AmountParseFlagsEnum flags = AmountParseFlagsEnum.PARSE_DEFAULT)
        {
            if (!string.IsNullOrEmpty(str))
                Parse(str, flags);
        }

        public Expr(InputTextStream inStream, AmountParseFlagsEnum flags = AmountParseFlagsEnum.PARSE_DEFAULT)
        {
            Parse(inStream, flags);
        }

        public Expr(ExprOp op, Scope context = null)
            : base(context)
        {
            Op = op;
        }

        /// <summary>
        /// The same as "ptr"
        /// </summary>
        public ExprOp Op { get; private set; }

        /// <summary>
        /// virtual operator bool() const throw();
        /// </summary>
        public override bool IsEmpty
        {
            get { return Op == null; }
        }

        public void Assign(Expr expr)
        {
            if (expr != this)
            {
                base.Assign(expr);
                Op = expr.Op;
            }
        }

        public bool Equals(Expr other)
        {
            if (other == null)
                return false;

            return Str == other.Str;
        }

        public override bool Equals(object obj)
        {
            if (obj is Expr)
                return this.Equals((Expr)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return (Str ?? String.Empty).GetHashCode();
        }

        public override string Print()
        {
            if (Op != null)
            {
                string str = String.Empty;
                Op.Print(ref str);
                return str;
            }
            else
            {
                return base.Print();
            }
        }

        public override void Parse(InputTextStream inStream, AmountParseFlagsEnum flags = AmountParseFlagsEnum.PARSE_DEFAULT, string originalString = null)
        {
            ExprParser parser = new ExprParser();
            int startPos = inStream.Pos;
            Op = parser.Parse(inStream, flags, originalString);
            int endPos = inStream.Pos;

            if (!String.IsNullOrEmpty(originalString))
            {
                Text = originalString;
            }
            else if (endPos > startPos)
            {
                inStream.Pos = startPos;
                Text = inStream.RemainSource.Substring(0, endPos - startPos);
                inStream.Pos = endPos;
            }
            else
            {
                Text = "<stream>";
            }
        }

        public override void Compile(Scope scope)
        {
            if (!IsCompiled && Op != null)
            {
                Op = Op.Compile(scope);
                base.Compile(scope);
            }
        }

        protected override Value RealCalc(Scope scope)
        {
            if (Op != null)
            {
                ExprOp locus = null;
                try
                {
                    return Op.Calc(scope, locus);
                }
                catch
                {
                    if (locus != null)
                    {
                        string currentContext = ErrorContext.Current.GetContext();

                        ErrorContext.Current.AddErrorContext("While evaluating value expression:");
                        ErrorContext.Current.AddErrorContext(ErrorContext.OpContext(Op, locus));

                        if (Logger.Current.LogLevel >= LogLevelEnum.LOG_INFO)
                        {
                            ErrorContext.Current.AddErrorContext("The value expression tree was:");
                            ErrorContext.Current.AddErrorContext(Op.Dump());
                        }

                        if (!String.IsNullOrEmpty(currentContext))
                            ErrorContext.Current.AddErrorContext(currentContext);
                    }
                    throw;
                }
            }
            return new Value();
        }

        public bool IsConstant
        {
            get { return Op != null && Op.IsValue; }
        }

        public Value ConstantValue()
        {
            return Op.AsValue;
        }

        public bool IsFunction
        {
            get { return Op != null && Op.IsFunction; }
        }

        public ExprFunc GetFunction()
        {
            return Op.AsFunction;
        }

        public override string ContextToStr()
        {
            return Op != null ? ErrorContext.OpContext(Op) : "<empty expression>";
        }

        public override string Dump()
        {
            return Op != null ? Op.Dump() : base.Dump();
        }
    }
}

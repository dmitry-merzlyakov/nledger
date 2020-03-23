// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Commodities;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Expressions
{
    /// <summary>
    /// Ported from expr_t::op_t\kind_t
    /// </summary>
    public enum OpKindEnum
    {
        // Constants
        PLUG,
        VALUE,
        IDENT,

        CONSTANTS,

        FUNCTION,
        SCOPE,

        TERMINALS,

        // Binary operators
        O_NOT,
        O_NEG,

        UNARY_OPERATORS,

        O_EQ,
        O_LT,
        O_LTE,
        O_GT,
        O_GTE,

        O_AND,
        O_OR,

        O_ADD,
        O_SUB,
        O_MUL,
        O_DIV,

        O_QUERY,
        O_COLON,

        O_CONS,
        O_SEQ,

        O_DEFINE,
        O_LOOKUP,
        O_LAMBDA,
        O_CALL,
        O_MATCH,

        BINARY_OPERATORS,

        OPERATORS,

        UNKNOWN,

        LAST
    };

    /// <summary>
    /// Ported from expr_t::op_t
    /// </summary>
    public class ExprOp
    {
        public static ExprOp NewNode(OpKindEnum kind, ExprOp left = null, ExprOp right = null)
        {
            ExprOp node = new ExprOp() { Kind = kind };
            if (left != null)
                node.Left = left;
            if (right != null)
                node.Right = right;
            return node;
        }

        public static ExprOp WrapValue(Value value)
        {
            ExprOp node = new ExprOp(OpKindEnum.VALUE);
            node.AsValue = value;
            return node;
        }

        public static ExprOp WrapFunctor(ExprFunc func)
        {
            ExprOp op = new ExprOp(OpKindEnum.FUNCTION);
            op.AsFunction = func;
            return op;
        }

        /// <summary>
        /// Ported from expr_t::ptr_op_t lookup_ident
        /// </summary>
        public static ExprOp LookupIdent(ExprOp op, Scope scope)
        {
            ExprOp def = op.Left;

            // If no definition was pre-compiled for this identifier, look it up
            // in the current scope.
            if (def == null || def.Kind == OpKindEnum.PLUG)
            {
                Logger.Current.Debug("scope.symbols", () => String.Format("Looking for IDENT '{0}'", op.AsIdent));
                def = scope.Lookup(SymbolKindEnum.FUNCTION, op.AsIdent);
            }

            if (def == null)
                throw new CalcError(String.Format(CalcError.ErrorMessageUnknownIdentifier, op.AsIdent));

            return def;
        }

        public static void CheckTypeContext(Scope scope, Value result)
        {
            if (scope.TypeRequired && scope.TypeContext != ValueTypeEnum.Void && result.Type != scope.TypeContext)
                throw new CalcError(String.Format(CalcError.ErrorMessageExpectedReturnOfSmthButReceivedSmth, scope.TypeContext, result.Type));
        }

        public static ExprOp FindDefinition(ExprOp op, Scope scope, ExprOp locus, int depth, int recursionDepth = 0)
        {
            // If the object we are apply call notation to is a FUNCTION value
            // or a O_LAMBDA expression, then this is the object we want to
            // call.
            if (op.IsFunction || op.Kind == OpKindEnum.O_LAMBDA)
                return op;

            if (recursionDepth > 256)
                throw new ValueError(ValueError.ValueFunctionRecursionDepthTooDeep);

            // If it's an identifier, look up its definition and see if it's a
            // function.
            if (op.IsIdent)
                return FindDefinition(LookupIdent(op, scope), scope, locus, depth, recursionDepth + 1);

            // Value objects might be callable if they contain an expression.
            if (op.IsValue)
            {
                Value def = op.AsValue;
                if (Expr.IsExpr(def))
                    return FindDefinition(Expr.AsExpr(def), scope, locus, depth, recursionDepth + 1);
                else
                    throw new ValueError(String.Format(ValueError.CannotCallSmthAsFunction, def));
            }

            // Resolve ordinary expressions.
            return FindDefinition(WrapValue(op.Calc(scope, locus, depth + 1)), scope, locus, depth + 1, recursionDepth + 1);
        }

        public static Value SplitConsExpr(ExprOp op)
        {
            if (op.Kind == OpKindEnum.O_CONS)
            {
                Value seq = new Value();
                seq.PushBack(Value.Get(op.Left));

                ExprOp next = op.Right;
                while(next != null)
                {
                    ExprOp valueOp;
                    if (next.Kind == OpKindEnum.O_CONS)
                    {
                        valueOp = next.Left;
                        next = next.HasRight ? next.Right : null;
                    }
                    else
                    {
                        valueOp = next;
                        next = null;
                    }
                    seq.PushBack(Value.Get(valueOp));
                }
                return seq;
            }
            else
            {
                return Value.Get(op);
            }
        }

        public ExprOp()
        {
            Kind = OpKindEnum.UNKNOWN;
        }

        public ExprOp(OpKindEnum kind) : this()
        {
            Kind = kind;
        }

        public OpKindEnum Kind { get; set; }

        public bool IsValue
        {
            get 
            {
                if (Kind == OpKindEnum.VALUE)
                {
                    if (Data.Type != typeof(Value))
                        throw new InvalidOperationException("Wrong type");
                    return true;
                }
                return false;
            }
        }

        public Value AsValue
        {
            get
            {
                if (!IsValue)
                    throw new InvalidOperationException("Not a Value");
                Value value = Data.GetValue<Value>();
                Validator.Verify(() => value.IsValid);
                return value;
            }
            set
            {
                Validator.Verify(() => value.IsValid);
                Data.SetValue(value);
            }
        }

        public bool IsIdent
        {
            get 
            {
                if (Kind == OpKindEnum.IDENT)
                {
                    if (Data.Type != typeof(string))
                        throw new InvalidOperationException("Wrong type");
                    return true;
                }
                return false;
            }
        }

        public string AsIdent
        {
            get
            {
                if (!IsIdent)
                    throw new InvalidOperationException("Not an Ident");
                return Data.GetValue<string>();
            }
            set
            {
                Data.SetValue(value);
            }
        }

        public bool IsFunction
        {
            get { return Kind == OpKindEnum.FUNCTION; }
        }

        public ExprFunc AsFunction
        {
            get
            {
                if (!IsFunction)
                    throw new InvalidOperationException("Not a Function");
                return Data.GetValue<ExprFunc>();
            }
            set
            {
                Data.SetValue(value);
            }
        }

        public bool IsScope
        {
            get { return Kind == OpKindEnum.SCOPE; }
        }

        public bool IsScopeUnset
        {
            get { return Data.IsEmpty; }
        }

        public Scope AsScope
        {
            get
            {
                if (!IsScope)
                    throw new InvalidOperationException("Not a Scope");
                return Data.GetValue<Scope>();
            }
            set
            {
                Data.SetValue(value);
            }
        }

        // These three functions must use 'kind == IDENT' rather than
        // 'is_ident()', because they are called before the `data' member gets
        // set, which is_ident() tests.

        public ExprOp Left
        {
            get
            {
                if (!(Kind > OpKindEnum.TERMINALS || Kind == OpKindEnum.IDENT || IsScope))
                    throw new InvalidOperationException("Wrong context");

                return _Left;
            }
            set
            {
                if (!(Kind > OpKindEnum.TERMINALS || Kind == OpKindEnum.IDENT || IsScope))
                    throw new InvalidOperationException("Wrong context");

                _Left = value;
            }
        }

        public ExprOp AsOp
        {
            get
            {
                if (Kind <= OpKindEnum.TERMINALS && !IsIdent)
                    throw new InvalidOperationException("Wrong context");

                return Data.GetValue<ExprOp>();

            }
        }

        public ExprOp Right
        {
            get
            {
                if (Kind <= OpKindEnum.TERMINALS)
                    throw new InvalidOperationException("Wrong context");

                return AsOp;
            }
            set
            {
                if (Kind <= OpKindEnum.TERMINALS)
                    throw new InvalidOperationException("Wrong context");

                Data.SetValue(value);
            }
        }

        public bool HasRight
        {
            get 
            {
                if (Kind <= OpKindEnum.TERMINALS)
                    return false;

                return !Data.IsEmpty && AsOp != null;
            }
        }

        /// <summary>
        /// Ported from expr_t::ptr_op_t expr_t::op_t::compile
        /// </summary>
        public ExprOp Compile(Scope scope, int depth = 0, Scope paramScope = null)
        {
            ExprOp result = null;
            Scope boundScope;

            Logger.Current.Debug("expr.compile", () => new String('.', depth));

            if (Kind >= OpKindEnum.LAST)
                throw new InvalidOperationException();

            if (IsIdent)
            {
                Logger.Current.Debug("expr.compile", () => String.Format("Lookup: {0} in {1}", AsIdent, scope));
                ExprOp def = null;
                if (paramScope != null)
                    def = paramScope.Lookup(SymbolKindEnum.FUNCTION, AsIdent);

                if (def == null)
                    def = scope.Lookup(SymbolKindEnum.FUNCTION, AsIdent);

                if (def != null)
                {
                    // Identifier references are first looked up at the point of
                    // definition, and then at the point of every use if they could
                    // not be found there.
                    Logger.Current.Debug("expr.compile", () => String.Format("Found definition:{0}", def.Dump()));
                    result = Copy(def); 
                }
                else if (Left != null)
                {
                    result = Copy();
                }
                else
                {
                    result = this;
                }
            }
            else if (IsScope)
            {
                Scope subScope = new SymbolScope(Scope.EmptyScope);
                AsScope = subScope;
                boundScope = new BindScope(scope, subScope);
                scope = boundScope;
            }
            else if (Kind < OpKindEnum.TERMINALS)
            {
                result = this;
            }
            else if (Kind == OpKindEnum.O_DEFINE)
            {
                switch (Left.Kind)
                {
                    case OpKindEnum.IDENT:
                        {
                            ExprOp node = Right.Compile(scope, depth + 1, paramScope);
                            Logger.Current.Debug("expr.compile", () => String.Format("Defining {0} in {1}", Left.AsIdent, scope));
                            scope.Define(SymbolKindEnum.FUNCTION, Left.AsIdent, node);
                        }
                        break;

                    case OpKindEnum.O_CALL:
                        if (Left.Left.IsIdent)
                        {
                            ExprOp node = new ExprOp(OpKindEnum.O_LAMBDA);
                            node.Left = Left.Right;
                            node.Right = Right;

                            node.Compile(scope, depth + 1, paramScope);
                            Logger.Current.Debug("expr.compile", () => String.Format("Defining {0} in {1}", Left.Left.AsIdent, scope));
                            scope.Define(SymbolKindEnum.FUNCTION, Left.Left.AsIdent, node);
                            break;
                        }
                        throw new CompileError(CompileError.ErrorMessageInvalidFunctionDefinition);

                    default:
                        throw new CompileError(CompileError.ErrorMessageInvalidFunctionDefinition);

                }
                result = WrapValue(Value.Empty);
            }
            else if (Kind == OpKindEnum.O_LAMBDA)
            {
                SymbolScope parms = new SymbolScope(paramScope ?? Scope.EmptyScope);

                for(ExprOp sym = Left; sym != null; sym = sym.HasRight ? sym.Right : null)
                {
                    ExprOp varName = sym.Kind == OpKindEnum.O_CONS ? sym.Left : sym;
                    if (!varName.IsIdent)
                    {
                        string buf = varName.Dump();
                        throw new CalcError(String.Format(CalcError.ErrorMessageInvalidFunctionOrLambdaParameter, buf));
                    }
                    else
                    {
                        Logger.Current.Debug("expr.compile", () => String.Format("Defining function parameter {0}", varName.AsIdent));
                        parms.Define(SymbolKindEnum.FUNCTION, varName.AsIdent, new ExprOp(OpKindEnum.PLUG));
                    }

                }

                ExprOp rhs = Right.Compile(scope, depth + 1, parms);
                if (rhs == Right)
                {
                    result = this;
                }
                else
                {
                    result = Copy(Left, rhs);
                }
            }

            if (result == null)
            {
                if (Left == null)
                    throw new CalcError(CalcError.ErrorMessageSyntaxError);

                ExprOp lhs = Left.Compile(scope, depth + 1, paramScope);
                ExprOp rhs = Kind > OpKindEnum.UNARY_OPERATORS && HasRight
                    ? (Kind == OpKindEnum.O_LOOKUP ? Right : Right.Compile(scope, depth + 1, paramScope))
                    : null;

                if (lhs == Left && (rhs == null || rhs == Right))
                {
                    result = this;
                }
                else
                {
                    ExprOp intermediate = Copy(lhs, rhs);

                    // Reduce constants immediately if possible
                    if ((lhs == null || lhs.IsValue) && (rhs == null || rhs.IsValue))
                        result = WrapValue(intermediate.Calc(scope, null, depth + 1));
                    else
                        result = intermediate;
                }
            }

            Logger.Current.Debug("expr.compile", () => new String('.', depth));

            return result;
        }

        /// <summary>
        /// Ported from value_t expr_t::op_t::calc
        /// </summary>
        public Value Calc(Scope scope, ExprOp locus = null, int depth = 0)
        {
            try
            {
                Value result = Value.Empty;

                Logger.Current.Debug("expr.calc", () => String.Format("{0}{1} => ...", new String('.', depth), ErrorContext.OpContext(this)));

                switch (Kind)
                {
                    case OpKindEnum.VALUE:
                        result = AsValue;
                        break;

                    case OpKindEnum.O_DEFINE:
                        result = Value.Empty;
                        break;

                    case OpKindEnum.IDENT:
                        ExprOp definition = LookupIdent(this, scope);
                        if (definition != null)
                        {
                          // Evaluating an identifier is the same as calling its definition
                          // directly
                            result = definition.Calc(scope, locus, depth + 1);
                            CheckTypeContext(scope, result);
                        }
                        break;

                    case OpKindEnum.FUNCTION:
                        // Evaluating a FUNCTION is the same as calling it directly; this
                        // happens when certain functions-that-look-like-variables (such as
                        // "amount") are resolved.
                        CallScope callArgs = new CallScope(scope, locus, depth + 1);
                        result = AsFunction(callArgs);
                            CheckTypeContext(scope, result);
                        break;

                    case OpKindEnum.SCOPE:
                        // assert(! is_scope_unset()); - does not make sense here
                        if (IsScopeUnset)
                        {
                            SymbolScope subscope = new SymbolScope(scope);
                            result = Left.Calc(subscope, locus, depth + 1);
                        }
                        else
                        {
                            BindScope boundScope = new BindScope(scope, AsScope);
                            result = Left.Calc(boundScope, locus, depth + 1);
                        }
                        break;

                    case OpKindEnum.O_LOOKUP:
                        ContextScope contextScope = new ContextScope(scope, ValueTypeEnum.Scope);
                        bool scopeError = true;
                        Value obj = Left.Calc(contextScope, locus, depth + 1);
                        if (obj != null)
                        {
                            if (obj.Type == ValueTypeEnum.Scope && obj.AsScope != null)
                            {
                                BindScope boundScope = new BindScope(scope, obj.AsScope);
                                result = Right.Calc(boundScope, locus, depth + 1);
                                scopeError = false;
                            }
                        }
                        if (scopeError)
                            throw new CalcError(CalcError.ErrorMessageLeftOperandDoesNotEvaluateToObject);
                        break;

                    case OpKindEnum.O_CALL:
                        result = CalcCall(scope, locus, depth);
                        CheckTypeContext(scope, result);
                        break;

                    case OpKindEnum.O_LAMBDA:
                        result = Value.Get(this);
                        break;

                    case OpKindEnum.O_MATCH:
                        result = Value.Get(Right.Calc(scope, locus, depth + 1).AsMask.Match(Left.Calc(scope, locus, depth + 1).ToString()));
                        break;

                    case OpKindEnum.O_EQ:
                        result = Value.Get(Left.Calc(scope, locus, depth + 1).IsEqualTo(Right.Calc(scope, locus, depth + 1)));
                        break;

                    case OpKindEnum.O_LT:
                        result = Value.Get(Left.Calc(scope, locus, depth + 1).IsLessThan(Right.Calc(scope, locus, depth + 1)));
                        break;

                    case OpKindEnum.O_LTE:
                        result = Value.Get(!Left.Calc(scope, locus, depth + 1).IsGreaterThan(Right.Calc(scope, locus, depth + 1)));
                        break;

                    case OpKindEnum.O_GT:
                        result = Value.Get(Left.Calc(scope, locus, depth + 1).IsGreaterThan(Right.Calc(scope, locus, depth + 1)));
                        break;

                    case OpKindEnum.O_GTE:
                        result = Value.Get(!Left.Calc(scope, locus, depth + 1).IsLessThan(Right.Calc(scope, locus, depth + 1)));
                        break;

                    case OpKindEnum.O_ADD:
                        result = Left.Calc(scope, locus, depth + 1) + Right.Calc(scope, locus, depth + 1);
                        break;

                    case OpKindEnum.O_SUB:
                        result = Left.Calc(scope, locus, depth + 1) - Right.Calc(scope, locus, depth + 1);
                        break;

                    case OpKindEnum.O_MUL:
                        result = Left.Calc(scope, locus, depth + 1) * Right.Calc(scope, locus, depth + 1);
                        break;

                    case OpKindEnum.O_DIV:
                        result = Left.Calc(scope, locus, depth + 1) / Right.Calc(scope, locus, depth + 1);
                        break;

                    case OpKindEnum.O_NEG:
                        result = Left.Calc(scope, locus, depth + 1).Negated();
                        break;

                    case OpKindEnum.O_NOT:
                        result = Value.Get(!Left.Calc(scope, locus, depth + 1).Bool);
                        break;

                    case OpKindEnum.O_AND:
                        if (Left.Calc(scope, locus, depth + 1).Bool)
                            result = Right.Calc(scope, locus, depth + 1);
                        else
                            result = Value.Get(false);
                        break;

                    case OpKindEnum.O_OR:
                        Value temp_O_OR = Left.Calc(scope, locus, depth + 1);
                        if (temp_O_OR.Bool)
                            result = temp_O_OR;
                        else
                            result = Right.Calc(scope, locus, depth + 1);
                        break;

                    case OpKindEnum.O_QUERY:
                        if (Right == null || Right.Kind != OpKindEnum.O_COLON)
                            throw new InvalidOperationException("O_QUERY");

                        Value temp = Left.Calc(scope, locus, depth + 1);
                        if (temp.Bool)
                            result = Right.Left.Calc(scope, locus, depth + 1);
                        else
                            result = Right.Right.Calc(scope, locus, depth + 1);
                        break;

                    case OpKindEnum.O_COLON:
                        throw new InvalidOperationException("We should never calculate an O_COLON operator");

                    case OpKindEnum.O_CONS:
                        result = CalcCons(scope, locus, depth);
                        break;

                    case OpKindEnum.O_SEQ:
                        result = CalcSeq(scope, locus, depth);
                        break;

                    default:
                        throw new CalcError(String.Format(CalcError.ErrorMessageUnexpectedExprNode, this));
                }

                Logger.Current.Debug("expr.calc", () => String.Format("{0}{1} => {2}", new String('.', depth), ErrorContext.OpContext(this), result.Dump(true)));

                return result;
            }
            catch
            {
                if (locus == null)
                    locus = this;
                throw;
            }
        }

        public Value Call(Value args, Scope scope, ExprOp locus = null, int depth = 0)
        {
            CallScope callArgs = new CallScope(scope, locus, depth + 1);
            callArgs.Args = args;

            if (IsFunction)
                return AsFunction(callArgs);
            else if (Kind == OpKindEnum.O_LAMBDA)
                return CallLambda(this, scope, callArgs, locus, depth);
            else
                return FindDefinition(this, scope, locus, depth).Calc(callArgs, locus, depth);
        }

        /// <summary>
        /// Ported from expr_t::op_t::dump
        /// </summary>
        public string Dump(int depth = 0)
        {
            StringBuilder sb = new StringBuilder(new string(' ', depth));
            
            switch(Kind)
            {
                case OpKindEnum.VALUE:
                    sb.AppendFormat("VALUE: {0}", AsValue.Dump());
                    break;
                case OpKindEnum.IDENT:
                    sb.AppendFormat("IDENT: {0}", AsIdent);
                    break;
                case OpKindEnum.SCOPE:
                    sb.AppendFormat("SCOPE: {0}", IsScopeUnset ? "null" : AsScope.Description);
                    break;

                case OpKindEnum.LAST:
                    throw new InvalidOperationException("assert(false)");

                default:
                    sb.Append(Kind.ToString());
                    break;
            }

            sb.AppendLine(String.Format(" ({0})", 0 /* DM - no refc */));

            // An identifier is a special non-terminal, in that its left() can
            // hold the compiled definition of the identifier.
            if (Kind > OpKindEnum.TERMINALS || IsScope || IsIdent)
            {
                if (Left != null)
                {
                    sb.Append(Left.Dump(depth + 1));
                    if (Kind > OpKindEnum.UNARY_OPERATORS && HasRight)
                        sb.Append(Right.Dump(depth + 1));
                }
                else if (Kind > OpKindEnum.UNARY_OPERATORS)
                {
                    if (HasRight)
                        throw new InvalidOperationException("assert(! has_right())");
                }
            }

            return sb.ToString();
        }

        public bool PrintCons(ref string str, ExprOp op, ExprOpContext context)
        {
            bool found = false;

            if (op.Left == null)
                throw new InvalidOperationException();

            if (op.Left.Print(ref str, context))
                found = true;

            if (op.Right != null)
            {
                str += ", ";
                if (op.Right.Kind == OpKindEnum.O_CONS)
                    found = PrintCons(ref str, op.Right, context);
                else if (op.Right.Print(ref str, context))
                    found = true;
            }
            return found;
        }

        public bool PrintSeq(ref string str, ExprOp op, ExprOpContext context)
        {
            bool found = false;

            if (op.Left == null)
                throw new InvalidOperationException();

            if (op.Left.Print(ref str, context))
                found = true;

            if (op.Right != null)
            {
                str += "; ";
                if (op.Right.Kind == OpKindEnum.O_SEQ)
                    found = PrintSeq(ref str, op.Right, context);
                else if (op.Right.Print(ref str, context))
                    found = true;
            }
            return found;
        }

        public bool Print(ref string str, ExprOpContext context = null)
        {
            str = str ?? String.Empty;
            context = context ?? new ExprOpContext();

            bool found = false;

            if (context.StartPos.HasValue && this == context.OpToFind)
            {
                context.StartPos = str.Length - 1;
                found = true;
            }

            string symbol = null;

            if (Kind > OpKindEnum.TERMINALS && (Kind != OpKindEnum.O_CALL && Kind != OpKindEnum.O_DEFINE))
                str += '(';

            switch (Kind)
            {
                case OpKindEnum.VALUE:
                    str += AsValue.Dump(context.Relaxed);
                    break;

                case OpKindEnum.IDENT:
                    str += AsIdent;
                    break;

                case OpKindEnum.FUNCTION:
                    str += "<FUNCTION>";
                    break;

                case OpKindEnum.SCOPE:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_NOT:
                    str += "!";
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_NEG:
                    str += "- ";
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_ADD:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " + ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_SUB:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " - ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_MUL:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " * ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_DIV:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " / ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_EQ:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " == ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_LT:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " < ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_LTE:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " <= ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_GT:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " > ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_GTE:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " >= ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_AND:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " & ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_OR:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " | ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_QUERY:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " ? ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_COLON:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " : ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_CONS:
                    found = PrintCons(ref str, this, context);
                    break;

                case OpKindEnum.O_SEQ:
                    found = PrintSeq(ref str, this, context);
                    break;

                case OpKindEnum.O_DEFINE:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " = ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_LOOKUP:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += ".";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_LAMBDA:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " -> ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                case OpKindEnum.O_CALL:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    if (Right != null)
                    {
                        if (Right.Kind == OpKindEnum.O_CONS)
                        {
                            if (Right != null && Right.Print(ref str, context))
                                found = true;
                        }
                        else
                        {
                            str += "(";
                            if (Right != null && Right.Print(ref str, context))
                                found = true;
                            str += ")";
                        }
                    }
                    else
                        str += "()";
                    break;

                case OpKindEnum.O_MATCH:
                    if (Left != null && Left.Print(ref str, context))
                        found = true;
                    str += " =~ ";
                    if (Right != null && Right.Print(ref str, context))
                        found = true;
                    break;

                default:
                    throw new InvalidOperationException();
            }

            if (Kind > OpKindEnum.TERMINALS && (Kind != OpKindEnum.O_CALL && Kind != OpKindEnum.O_DEFINE))
                str += ')';

            if (!String.IsNullOrEmpty(symbol))
            {
                if (CommodityPool.Current.Find(symbol) != null)
                    str += "@";
                str += symbol;
            }

            if (context.EndPos.HasValue && this == context.OpToFind)
            {
                context.EndPos = str.Length - 1;
            }

            return found;
        }

        private ExprOp Copy(ExprOp left = null, ExprOp right = null)
        {
            ExprOp node = NewNode(Kind, left, right);
            if (Kind < OpKindEnum.TERMINALS)
                if (!Data.IsEmpty)
                    node.Data.SetValue(Data.Value);
            return node;
        }

        private Value CalcCall(Scope scope, ExprOp locus, int depth)
        {
            ExprOp func = Left;
            string name = func.IsIdent ? func.AsIdent : "<value expr>";

            func = FindDefinition(func, scope, locus, depth);

            CallScope callArgs = new CallScope(scope, locus, depth + 1);
            if (HasRight)
                callArgs.Args = SplitConsExpr(Right);

            try
            {
                if (func.IsFunction)
                {
                    return func.AsFunction(callArgs);
                }
                else
                {
                    if (func.Kind != OpKindEnum.O_LAMBDA)
                        throw new InvalidOperationException("Not a lambda");

                    return CallLambda(func, scope, callArgs, locus, depth);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("While calling function '{0} {1}':", name, callArgs.Args), ex);
            }
        }

        private Value CalcCons(Scope scope, ExprOp locus, int depth)
        {
            Value result = Left.Calc(scope, locus, depth + 1);
            if (HasRight)
            {
                Value temp = new Value();
                temp.PushBack(result);

                ExprOp next = Right;
                while(next != null)
                {
                    ExprOp valueOp;
                    if (next.Kind == OpKindEnum.O_CONS)
                    {
                        valueOp = next.Left;
                        next = next.HasRight ? Right : null;
                    }
                    else
                    {
                        valueOp = next;
                        next = null;
                    }
                    temp.PushBack(valueOp.Calc(scope, locus, depth + 1));
                }
                result = temp;
            }
            return result;
        }

        private Value CalcSeq(Scope scope, ExprOp locus, int depth)
        {
            // An O_SEQ is very similar to an O_CONS except that only the last
            // result value in the series is kept.  O_CONS builds up a list.
            //
            // Another feature of O_SEQ is that it pushes a new symbol scope onto
            // the stack.  We evaluate the left side here to catch any
            // side-effects, such as definitions in the case of 'x = 1; x'.
            Value result = Left.Calc(scope, locus, depth + 1);
            if (HasRight)
            {
                ExprOp next = Right;
                while(next != null)
                {
                    ExprOp valueOp;
                    if (next.Kind == OpKindEnum.O_SEQ)
                    {
                        valueOp = next.Left;
                        next = next.Right;
                    }
                    else
                    {
                        valueOp = next;
                        next = null;
                    }
                    result = valueOp.Calc(scope, locus, depth + 1);
                }
            }
            return result;
        }

        private Value CallLambda(ExprOp func, Scope scope, CallScope callArgs, ExprOp locus, int depth)
        {
            int argsIndex = 0;
            long argsCount = callArgs.Size;

            SymbolScope argsScope = new SymbolScope(Scope.EmptyScope);

            for(ExprOp sym = func.Left; sym != null; sym = sym.HasRight ? sym.Right : null)
            {
                ExprOp varName = sym.Kind == OpKindEnum.O_CONS ? sym.Left : sym;
                
                if (!varName.IsIdent)
                    throw new CalcError(CalcError.ErrorMessageInvalidFunctionDefinition);

                if (argsIndex == argsCount)
                {
                    Logger.Current.Debug("expr.calc", () => String.Format("Defining function argument as null: {0}", varName.AsIdent));
                    argsScope.Define(SymbolKindEnum.FUNCTION, varName.AsIdent, WrapValue(Value.Empty));
                }
                else
                {
                    Logger.Current.Debug("expr.calc", () => String.Format("Defining function argument from call_args: {0}", varName.AsIdent));
                    argsScope.Define(SymbolKindEnum.FUNCTION, varName.AsIdent, WrapValue(callArgs[argsIndex++]));
                }
            }

            if (argsIndex < argsCount)
                throw new CalcError(String.Format(CalcError.ErrorMessageTooFewArgumentsInFunctionCall, argsCount, argsIndex));

            if (func.Right.IsScope)
            {
                BindScope outerScope = new BindScope(scope, func.Right.AsScope);
                BindScope boundScope = new BindScope(outerScope, argsScope);
                return func.Right.Left.Calc(boundScope, locus, depth + 1);
            }
            else
            {
                return func.Right.Calc(argsScope, locus, depth + 1);
            }
        }

        private readonly BoostVariant Data = new BoostVariant();
        private ExprOp _Left;
        //private ExprOp _Right;
    }
}

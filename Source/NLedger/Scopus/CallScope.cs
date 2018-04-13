// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Scopus
{
    using NValue = NLedger.Values.Value;

    public class CallScope : ContextScope
    {
        public static CallScope Create(string parm)
        {
            CallScope callScope = new CallScope(new EmptyScope());
            callScope.PushBack(NValue.StringValue(parm));
            return callScope;
        }

        public static string JoinArgs(CallScope args)
        {
            return String.Join(" ", args.ArgsList);
        }

        public readonly int Depth;

        public CallScope(Scope parent, ExprOp locus = null, int depth = 0) : 
            base (parent, parent.TypeContext, parent.TypeRequired)
        {
            Locus = locus;
            Depth = depth;
        }

        public Value Args { get; set; }

        public ExprOp Locus { get; private set; }

        public Value Resolve (int index, ValueTypeEnum context = ValueTypeEnum.Void, bool isRequired = false)
        {
            if (index >= Size)
                throw new CalcError(CalcError.ErrorMessageTooFewArgumentsToFunction);

            Value value = ArgsList[index];
            if (value.Type == ValueTypeEnum.Any)
            {
                ContextScope scope = new ContextScope(this, context, isRequired);
                value = Expr.AsExpr(value).Calc(scope, Locus, Depth);
                if (isRequired && value.Type != context)
                    throw new CalcError(String.Format(CalcError.ErrorMessageExpectedSmthForArgumentSmthButReceivedSmth, context, index, value));

                // DM - update the original value (Args) with expression result. Note the difference between Sequence (List inside) and a regular value
                if (Args.Type == ValueTypeEnum.Sequence)
                    Args.AsSequence[index] = value;
                else
                    Args = value;
            }
            return value;
        }

        public Value Value()
        {
            // Make sure that all of the arguments have been resolved.
            for (int index = 0; index < Size; index++)
                Resolve(index);

            return Args;
        }

        public Value this[int index]
        {
            get { return Resolve(index); }
        }

        public T Context<T>() where T: Scope
        {
            if (ptr == null)
                ptr = this.FindScope<T>();

            return (T)ptr;
        }

        public bool Has(int index)
        {
            return index < Size && !(NValue.IsNullOrEmpty(ArgsList[index]));
        }

        public bool Has<T>(int index)
        {
            if (index < Size)
            {
                Resolve(index, NValue.GetValueType<T>(), false);
                return !NValue.IsNullOrEmpty(ArgsList[index]);
            }
            return false;
        }

        public T Get<T>(int index, bool convert = true)
        {
            // DM - this method was completely rewritten to handle conversion to the expected type
            // #remove-boxing - Consider removing excess boxing in this method.

            if (typeof(T) == typeof(int))
                convert = true; // see - inline int call_scope_t::get<int>(std::size_t index, bool) {

            if (typeof(T) == typeof(ExprOp))
                return (T)(object)Args.AsSequence[index].AsAny<ExprOp>(); // see - call_scope_t::get<expr_t::ptr_op_t>(std::size_t index, bool)

            ValueTypeEnum valType = NValue.GetValueType<T>();
            Value val = Resolve(index, NValue.GetValueType<T>(), !convert);
            switch (valType)
            {
                case ValueTypeEnum.Amount: return (T)(object)val.AsAmount;
                case ValueTypeEnum.Any: return val.AsAny<T>();
                case ValueTypeEnum.Balance: return (T)(object)val.AsBalance;
                case ValueTypeEnum.Boolean: return (T)(object)val.AsBoolean;
                case ValueTypeEnum.Date: return (T)(object)val.AsDate;
                case ValueTypeEnum.DateTime: return (T)(object)val.AsDateTime;
                case ValueTypeEnum.Integer: return (T)Convert.ChangeType(val.AsLong, typeof(T));
                case ValueTypeEnum.Mask: return (T)(object)val.AsMask;
                case ValueTypeEnum.Scope: return (T)(object)val.AsScope;
                case ValueTypeEnum.Sequence: return (T)(object)val.AsSequence;
                case ValueTypeEnum.String: return (T)(object)val.AsString;
                case ValueTypeEnum.Void: return default(T);
                default: return default(T);
            }
        }

        public void PushFront(Value val)
        {
            ArgsList.Insert(0, val);
        }

        public void PushBack(Value val)
        {
            ArgsList.Add(val);
        }

        public void PopBack()
        {
            if (ArgsList.Count > 0)
                ArgsList.RemoveAt(ArgsList.Count - 1);
        }

        public long Size
        {
            get { return ArgsList.Count; }
        }

        public bool IsEmpty
        {
            get { return Size == 0; }
        }

        private IList<Value> ArgsList
        {
            get
            {
                if (NValue.IsNullOrEmpty(Args))
                    Args = NValue.Get(new List<Value>());

                return Args.AsSequence;
            }
        }

        private object ptr;
    }
}

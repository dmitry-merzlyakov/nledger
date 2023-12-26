// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Utility;
using NLedger.Values;
using NLedger.Xacts;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python
{
    public interface IPythonValueConverter
    {
        PyObject GetObject(Value val);
        Value GetValue(PyObject obj);
    }

    public class PythonValueConverter : IPythonValueConverter
    {
        public PythonValueConverter(PythonSession pythonSession)
        {
            PythonSession = pythonSession ?? throw new ArgumentNullException(nameof(pythonSession));
        }

        public PythonSession PythonSession { get; }

        /// <summary>
        /// Composed from object convert_value_to_python(const value_t& val)
        /// </summary>
        public PyObject GetObject(Value val)
        {
            switch (val.Type)
            {
                case ValueTypeEnum.Boolean: return GetPyBool(val.AsBoolean);
                case ValueTypeEnum.DateTime: return GetPyDateTime(val.AsDateTime);
                case ValueTypeEnum.Date: return GetPyDate(val.AsDate);
                case ValueTypeEnum.Integer: return GetPyInt(val.AsLong);
                case ValueTypeEnum.Amount: return GetPyAmount(val.AsAmount);
                case ValueTypeEnum.Balance: return GetPyBalance(val.AsBalance);

                case ValueTypeEnum.String:
                    using (PythonSession.GIL())
                        return val.AsString.ToPython();

                case ValueTypeEnum.Mask: return GetPyValue(val);

                case ValueTypeEnum.Sequence:
                    using (PythonSession.GIL())
                    {
                        var arglist = new PyList();
                        foreach (var elem in val.AsSequence)
                            arglist.Append(GetObject(elem));
                        return arglist;
                    }

                case ValueTypeEnum.Scope:
                    var scope = val.AsScope;
                    if (scope == null)
                        break;
                    if (scope.GetType() == typeof(Post))
                        return GetPyPost((Post)scope);
                    if (scope.GetType() == typeof(Xact))
                        return GetPyXact((Xact)scope);
                    if (scope.GetType() == typeof(Account))
                        return GetPyAccount((Account)scope);
                    if (scope.GetType() == typeof(PeriodXact))
                        return GetPyPeriodXact((PeriodXact)scope);
                    if (scope.GetType() == typeof(AutoXact))
                        return GetPyAutoXact((AutoXact)scope);
                    throw new LogicError("Cannot downcast scoped object to specific type");

                case ValueTypeEnum.Any:
                    using (PythonSession.GIL())
                        return PyObject.FromManagedObject(val.AsAny());
            }
            return null;
        }

        public Value GetValue(PyObject obj)
        {
            if (obj == null)
                return new Value();

            if (obj.IsNone())
                return new Value();

            using (PythonSession.GIL())
            {
                var pythonTypeName = obj.GetPythonTypeName();

                if (pythonTypeName == "bool")
                    return Value.Get(obj.As<bool>());
                if (pythonTypeName == "int")
                    return Value.Get(obj.As<int>());
                if (pythonTypeName == "str")
                    return Value.StringValue(obj.As<string>());

                if (pythonTypeName == "date")
                    return Value.Get(GetDate(obj));
                if (pythonTypeName == "datetime")
                    return Value.Get(GetDateTime(obj));

                if (pythonTypeName == "Balance")
                    return Value.Get(obj.GetAttr("origin").As<Balance>());
                if (pythonTypeName == "Amount")
                    return Value.Get(obj.GetAttr("origin").As<Amount>());
                if (pythonTypeName == "Mask")
                    return Value.Get(obj.GetAttr("origin").As<Mask>());
                if (pythonTypeName == "Posting")
                    return Value.ScopeValue(obj.GetAttr("origin").As<Post>());
                if (pythonTypeName == "Transaction")
                    return Value.ScopeValue(obj.GetAttr("origin").As<Xact>());
                if (pythonTypeName == "PeriodicTransaction")
                    return Value.ScopeValue(obj.GetAttr("origin").As<PeriodXact>());
                if (pythonTypeName == "AutomatedTransaction")
                    return Value.ScopeValue(obj.GetAttr("origin").As<AutoXact>());
                if (pythonTypeName == "Account")
                    return Value.ScopeValue(obj.GetAttr("origin").As<Account>());

                if (pythonTypeName == "Value")
                    return obj.GetAttr("origin").As<Value>();

                if (obj.GetPythonTypeName() == "list" && !(obj is PyList))
                    obj = new PyList(obj);

                if (obj is PyList)
                {
                    var values = new List<Value>();
                    foreach (var elem in (PyList)obj)
                        values.Add(GetValue(elem));
                    return Value.Get(values);
                }

                if (obj.HasAttr("origin"))
                    return Value.Get(obj.GetAttr("origin").AsManagedObject(typeof(object)));

                return Value.Get(obj.AsManagedObject(typeof(object)));
            }
        }

        public PyObject GetPyDate(Date date) => GetPyObject("to_pdate(value)", date);
        public PyObject GetPyDateTime(DateTime dateTime) => GetPyObject("to_pdatetime(value)", dateTime);
        public PyObject GetPyBool(bool val) => GetPyObject("bool(str(value)=='True')", val);
        public PyObject GetPyInt(long val) => GetPyObject("int(str(value))", val);

        public PyObject GetPyAmount(Amounts.Amount amount) => GetPyObject("Amount.from_origin(value)", amount);
        public PyObject GetPyBalance(Balance balance) => GetPyObject("Balance.from_origin(value)", balance);
        public PyObject GetPyPost(Post post) => GetPyObject("Posting.from_origin(value)", post);
        public PyObject GetPyXact(Xact xact) => GetPyObject("Transaction.from_origin(value)", xact);
        public PyObject GetPyPeriodXact(PeriodXact periodXact) => GetPyObject("PeriodicTransaction.from_origin(value)", periodXact);
        public PyObject GetPyAutoXact(AutoXact autoXact) => GetPyObject("AutomatedTransaction.from_origin(value)", autoXact);
        public PyObject GetPyAccount(Account account) => GetPyObject("Account.from_origin(value)", account);
        public PyObject GetPyValue(Value val) => GetPyObject("Value.to_value(value)", val);

        public Date GetDate(PyObject val) => GetPyObject("to_ndate(value)", val).As<Date>();
        public DateTime GetDateTime(PyObject val) => GetPyObject("to_ndatetime(value)", val).As<DateTime>();

        private PyObject GetPyObject(string init_method, object val)
        {
            using (PythonSession.GIL())
            {
                var dict = new PyDict();
                dict["value"] = (val as PyObject) ?? PyObject.FromManagedObject(val);
                return PythonSession.LedgerModule.Eval(init_method, dict);
            }
        }

    }
}

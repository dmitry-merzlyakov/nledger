using NLedger.Utility;
using NLedger.Values;
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

        public PyObject GetObject(Value val)
        {
            switch (val.Type)
            {
                // TODO - Runtime.ToPython()?
                case ValueTypeEnum.Amount: return GetPyAmount(val.AsAmount);
                case ValueTypeEnum.Any: return PyObject.FromManagedObject(val.AsAny());
                case ValueTypeEnum.Balance: return GetPyBalance(val.AsBalance);
                case ValueTypeEnum.Boolean: return GetPyBool(val.AsBoolean);
                case ValueTypeEnum.Date: return GetPyDate(val.AsDate);
                case ValueTypeEnum.DateTime: return PyObject.FromManagedObject(val.AsDateTime);
                case ValueTypeEnum.Integer: return PyObject.FromManagedObject(val.AsLong);
                case ValueTypeEnum.Mask: return GetPyValue(val); //PyObject.FromManagedObject(val.AsMask); // TODO
                case ValueTypeEnum.String: return val.AsString.ToPython();
            }
            return null; // TODO add casting scope to post etc
        }

        public Value GetValue(PyObject obj)
        {
            if (obj == null)
                return new Value();

            if (obj.IsNone())
                return new Value();

            var pythonTypeName = obj.GetPythonTypeName();

            if (pythonTypeName == "bool")
                return Value.Get(obj.As<bool>());

            // TODO    
            return Value.Get(obj);
        }

        public PyObject GetPyDate(Date date) => GetPyObject("to_pdate", date);
        public PyObject GetPyBool(bool val) => GetPyObject("bool", val);

        public PyObject GetPyAmount(Amounts.Amount amount) => GetPyObject("Amount.from_origin", amount);
        public PyObject GetPyBalance(Balance balance) => GetPyObject("Balance.from_origin", balance);
        public PyObject GetPyValue(Value val) => GetPyObject("Value.to_value", val);

        private PyObject GetPyObject(string init_method, object val)
        {
            using (PythonSession.GIL())
            {
                var dict = new PyDict();
                dict["value"] = PyObject.FromManagedObject(val);
                return PythonSession.LedgerModule.Eval($"{init_method}(value)", dict);
            }
        }

    }
}

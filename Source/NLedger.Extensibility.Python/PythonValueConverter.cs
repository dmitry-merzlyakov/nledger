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
                case ValueTypeEnum.Amount: return GetAmount(val.AsAmount);
                case ValueTypeEnum.Any: return PyObject.FromManagedObject(val.AsAny());
                case ValueTypeEnum.Balance: return PyObject.FromManagedObject(val.AsBalance);
                case ValueTypeEnum.Boolean: return PyObject.FromManagedObject(val.AsBoolean);
                case ValueTypeEnum.Date: return PyObject.FromManagedObject(val.AsDate);
                case ValueTypeEnum.DateTime: return PyObject.FromManagedObject(val.AsDateTime);
                case ValueTypeEnum.Integer: return PyObject.FromManagedObject(val.AsLong);
                case ValueTypeEnum.Mask: return PyObject.FromManagedObject(val.AsMask);
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

        public PyObject GetAmount(Amounts.Amount amount)
        {
            using (PythonSession.GIL())
            {
                var dict = new PyDict();
                dict["value"] = PyObject.FromManagedObject((NLedger.Extensibility.Export.Amount)amount);
                return PythonSession.LedgerModule.Eval("Amount(value)", dict);
            }
        }
    }
}

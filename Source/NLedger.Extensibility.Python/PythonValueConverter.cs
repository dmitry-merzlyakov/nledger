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
        public PyObject GetObject(Value val)
        {
            switch (val.Type)
            {
                // TODO - Runtime.ToPython()?
                case ValueTypeEnum.Amount: return PyObject.FromManagedObject(val.AsAmount);
                case ValueTypeEnum.Any: return PyObject.FromManagedObject(val.AsAny());
                case ValueTypeEnum.Balance: return PyObject.FromManagedObject(val.AsBalance);
                case ValueTypeEnum.Boolean: return PyObject.FromManagedObject(val.AsBoolean);
                case ValueTypeEnum.Date: return PyObject.FromManagedObject(val.AsDateTime);
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

            var pythonType = obj.GetPythonType();
            var pythonTypeName = pythonType.GetAttr("__name__").ToString();

            if (pythonTypeName == "bool")
                return Value.Get(obj.As<bool>());

            // TODO    
            return Value.Get(obj);
        }
    }
}

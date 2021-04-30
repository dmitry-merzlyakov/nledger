using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python
{
    public static class Extensions
    {
        public static string GetPythonTypeName(this PyObject obj)
        {
            return obj?.GetPythonType().GetAttr("__name__").ToString();
        }

        public static bool IsModule(this PyObject obj)
        {
            return obj.GetPythonTypeName() == "module";
        }
    }
}

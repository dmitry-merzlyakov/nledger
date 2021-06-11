using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests
{
    public static class Extensions
    {
        public static string NormalizeOutput(this string text)
        {
            return text?.Trim().Replace("\r\n", "\n");
        }
    }
}

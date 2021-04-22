using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    // Helper class that is intended to validate .Net Extension capabilities
    public static class PrintHelper
    {
        public static void Print(string arg)
        {
            MainApplicationContext.Current?.ApplicationServiceProvider.VirtualConsoleProvider.ConsoleOutput.WriteLine(arg);
        }

        public static void Print(string arg0, string arg1)
        {
            MainApplicationContext.Current?.ApplicationServiceProvider.VirtualConsoleProvider.ConsoleOutput.WriteLine($"{arg0} {arg1}");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.ServiceAPI
{
    public sealed class MemoryStreamManager : IDisposable
    {
        public MemoryStreamManager(string inputText = null)
        {
            ConsoleInput = new StringReader(inputText ?? String.Empty);
            ConsoleOutput = new StringWriter();
            ConsoleError = new StringWriter();
        }

        public TextWriter ConsoleError { get; private set; }
        public TextReader ConsoleInput { get; private set; }
        public TextWriter ConsoleOutput { get; private set; }

        public string GetOutputText()
        {
            return ConsoleOutput.ToString();
        }

        public string GetErrorText()
        {
            return ConsoleError.ToString();
        }

        public void Dispose()
        {
            ConsoleInput.Dispose();
            ConsoleOutput.Dispose();
            ConsoleError.Dispose();
        }
    }
}

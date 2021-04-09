using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility
{
    /// <summary>
    /// A basis for custom functional extensions (custom functions and commands, integration with other executables etc)
    /// </summary>
    /// <remarks>
    /// This class basically reflects Ledger's extensibility approach implemented as Python bridge (see python_session and python_interpreter_t).
    /// </remarks>
    public class ExtendedSession : Session
    {
        public static ExtendedSession Current => throw new NotImplementedException();

        public ExtensionModule MainModule { get; }
        public bool IsInitialized => throw new NotImplementedException();

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void ImportOption(string name)
        {
            throw new NotImplementedException();
        }

        public void Eval(string code, ExtensionEvalModeEnum mode)
        {
            throw new NotImplementedException();
        }

    }
}

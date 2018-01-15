using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Abstracts.Impl
{
    public class ManPageProvider : IManPageProvider
    {
        public const string LedgerManFile = "ledger.1.html";

        public bool Show()
        {
            string pathName = FileSystem.Combine(LedgerManFile, FileSystem.CurrentPath());
            return MainApplicationContext.Current.ProcessManager.Start(pathName);
        }
    }
}

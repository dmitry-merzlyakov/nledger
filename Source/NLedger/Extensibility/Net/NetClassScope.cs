using NLedger.Expressions;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public class NetClassScope : Scope
    {
        public NetClassScope(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                throw new ArgumentNullException(nameof(className));

            ClassType = Type.GetType(className) ?? throw new ArgumentException($"Cannot find .Net class '{className}'");
        }

        public Type ClassType { get; }

        public override string Description => ClassType.FullName;

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            throw new NotImplementedException();
        }
    }
}

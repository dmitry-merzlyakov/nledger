using NLedger.Expressions;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public class NetClassScope : Scope
    {
        public NetClassScope(string className, IValueConverter valueConverter)
        {
            if (string.IsNullOrWhiteSpace(className))
                throw new ArgumentNullException(nameof(className));

            ClassType = Type.GetType(className) ?? throw new ArgumentException($"Cannot find .Net class '{className}'");
            ValueConverter = valueConverter ?? throw new ArgumentNullException(nameof(valueConverter));
        }

        public IValueConverter ValueConverter { get; }
        public Type ClassType { get; }

        public override string Description => ClassType.FullName;

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            if (kind == SymbolKindEnum.FUNCTION)
            {
                var methods = ClassType.GetMethods().Where(m => m.Name == name).ToArray();
                if (methods.Any())
                    return ExprOp.WrapFunctor(new MethodFunctor(null, methods, ValueConverter).ExprFunctor);
            }
            return null;
        }
    }
}

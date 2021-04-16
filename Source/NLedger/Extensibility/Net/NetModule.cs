using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public class NetModule : Scope
    {
        public override string Description => "todo";

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            if (name == "Exists")
                return ExprOp.WrapFunctor(ExprFunc);

            //throw new NotImplementedException();
            var module = new NetModule();
            return ExprOp.WrapValue(Value.ScopeValue(new NetModule()));
        }

        public Value ExprFunc(Scope scope)
        {
            return Value.False;
        }
    }
}

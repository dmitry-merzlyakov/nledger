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
                //return ExprOp.WrapFunctor(new Functor().ExprFunc);

            //throw new NotImplementedException();
            if (name == "System" || name == "IO" || name == "File")
            {
                var module = new NetModule();
                return ExprOp.WrapValue(Value.ScopeValue(new NetModule()));
            }

            return null;
        }

        public Value ExprFunc(Scope scope)
        {
            CallScope callScope = scope as CallScope;

            IList<Value> vv = new List<Value>();
            if (callScope.Value().Type == ValueTypeEnum.Sequence)
                foreach (var v in callScope.Value().AsSequence)
                    vv.Add(v);
            var vvv = callScope.Value();


            if (System.IO.File.Exists("kk"))
                return Value.Get(new testobject() { val = "999" });

            return Value.False;
        }
    }

    public class Functor
    {
        public Value ExprFunc(Scope scope)
        {
            CallScope callScope = scope as CallScope;
            var vvv = callScope.Value();

            return Value.False;
        }
    }

    public class testobject
    {
        public string val { get; set; }
    }
}

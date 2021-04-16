using NLedger.Expressions;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public class NetSession : ExtendedSession
    {
        public override void DefineGlobal(string name, object value)
        {
            //throw new NotImplementedException();
        }

        public override void Eval(string code, ExtensionEvalModeEnum mode)
        {
            //throw new NotImplementedException();
        }

        public override void ImportOption(string name)
        {
            //throw new NotImplementedException();
        }

        public override void Initialize()
        {
            //throw new NotImplementedException();
        }

        public override bool IsInitialized()
        {
            return true;
        }

        protected override ExprOp LookupFunction(string name)
        {
            //throw new NotImplementedException();
            return ExprOp.WrapValue(Value.ScopeValue(new NetModule()));
        }
    }
}

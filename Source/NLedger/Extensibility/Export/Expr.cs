using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Expr : BaseExport<Expressions.Expr>
    {
        public static implicit operator Expr(Expressions.Expr expr) => new Expr(expr);
        public static explicit operator bool(Expr expr) => (bool)expr.Origin.IsEmpty;

        public static implicit operator Action(Expr expr) => () => expr.calc();

        protected Expr(Expressions.Expr origin) : base(origin)
        { }

        public Expr(string str): this(new Expressions.Expr(str))
        { }

        public string text() => Origin.Text;
        public string set_text(string text) => Origin.Text = text;

        public Value calc() => Origin.Calc();
        public Value calc(Scope scope) => Origin.Calc(scope.Origin);
        public void compile(Scope scope) => Origin.Compile(scope.Origin);

        public bool is_constant() => Origin.IsConstant;

    }
}

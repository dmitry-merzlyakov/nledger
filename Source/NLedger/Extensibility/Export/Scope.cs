using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public enum SymbolKind
    {
        UNKNOWN = Scopus.SymbolKindEnum.UNKNOWN,
        FUNCTION = Scopus.SymbolKindEnum.FUNCTION,
        OPTION = Scopus.SymbolKindEnum.OPTION,
        PRECOMMAND = Scopus.SymbolKindEnum.PRECOMMAND,
        COMMAND = Scopus.SymbolKindEnum.COMMAND,
        DIRECTIVE = Scopus.SymbolKindEnum.DIRECTIVE,
        FORMAT = Scopus.SymbolKindEnum.FORMAT
    }

    public class Scope : BaseExport<Scopus.Scope>
    {
        public static implicit operator Scope(Scopus.Scope scope) => new Scope(scope);

        protected Scope(Scopus.Scope origin) : base(origin)
        { }

        public string description => Origin.Description;
        public ValueType type_context => (ValueType)Origin.TypeContext;
        public bool type_required => Origin.TypeRequired;
    }
}

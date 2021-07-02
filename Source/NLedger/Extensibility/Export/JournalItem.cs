using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class TagData
    {
        public Value first { get; set; }
        public bool second { get; set; }
    }

    public enum State
    {
        Uncleared = Items.ItemStateEnum.Uncleared,
        Cleared = Items.ItemStateEnum.Cleared,
        Pending = Items.ItemStateEnum.Pending
    }

    public class JournalItem : Scope
    {
        public static readonly uint ITEM_NORMAL = (uint)SupportsFlagsEnum.ITEM_NORMAL;
        public static readonly uint ITEM_GENERATED = (uint)SupportsFlagsEnum.ITEM_GENERATED;
        public static readonly uint ITEM_TEMP = (uint)SupportsFlagsEnum.ITEM_TEMP;

        public static implicit operator JournalItem(Items.Item item) => new JournalItem(item);

        public static bool operator ==(JournalItem left, JournalItem right) => left.Origin == right.Origin;
        public static bool operator !=(JournalItem left, JournalItem right) => left.Origin != right.Origin;

        protected JournalItem(Items.Item origin) : base(origin)
        {
            Origin = origin;
        }

        public new Items.Item Origin { get; }

        public uint flags { get => (uint)Origin.Flags; set => Origin.Flags = (SupportsFlagsEnum)value; }

        public bool has_flags(uint flag) => (((uint)Origin.Flags) & flag) == flag;
        public void clear_flags() => Origin.Flags = default(SupportsFlagsEnum);
        public void add_flags(uint flag) => Origin.Flags = (SupportsFlagsEnum)((uint)Origin.Flags | flag);
        public void drop_flags(uint flag) => Origin.Flags = (SupportsFlagsEnum)((uint)Origin.Flags & ~flag);

        public string note { get => Origin.Note; set => Origin.Note = value; }
        public Position pos { get => Origin.Pos; set => Origin.Pos = value.Origin; }
        public IDictionary<string, TagData> metadata => Origin.GetMetadata().ToDictionary(m => m.Key, m => new TagData() { first = m.Value.Value, second = m.Value.IsParsed }); // [DM] Implemented as R/O property

        public void copy_details(JournalItem item) => Origin.CopyDetails(item.Origin);

        public bool has_tag(string tag) => Origin.HasTag(tag);
        public bool has_tag(Mask tag_mask) => Origin.HasTag(tag_mask.Origin);
        public bool has_tag(Mask tag_mask, Mask value_mask) => Origin.HasTag(tag_mask.Origin, value_mask?.Origin);
        public Value get_tag(string tag) => Origin.GetTag(tag);
        public Value get_tag(Mask tag_mask) => Origin.GetTag(tag_mask.Origin);
        public Value get_tag(Mask tag_mask, Mask value_mask) => Origin.GetTag(tag_mask.Origin, value_mask?.Origin);
        public Value tag(string tag) => Origin.GetTag(tag);
        public Value tag(Mask tag_mask) => Origin.GetTag(tag_mask.Origin);
        public Value tag(Mask tag_mask, Mask value_mask) => Origin.GetTag(tag_mask.Origin, value_mask?.Origin);

        public void set_tag(string tag) => Origin.SetTag(tag);
        public void set_tag(string tag, Value value) => Origin.SetTag(tag, value.Origin);
        public void set_tag(string tag, Value value, bool overwriteExisting) => Origin.SetTag(tag, value?.Origin, overwriteExisting);

        public void parse_tags(string note, Scope scope) => Origin.ParseTags(note, scope?.Origin);
        public void parse_tags(string note, Scope scope, bool overwriteExisting) => Origin.ParseTags(note, scope?.Origin, overwriteExisting);
        public void append_note(string note, Scope scope) => Origin.AppendNote(note, scope?.Origin);
        public void append_note(string note, Scope scope, bool overwriteExisting) => Origin.AppendNote(note, scope?.Origin, overwriteExisting);

        public static bool use_aux_date { get => Items.Item.UseAuxDate; set => Items.Item.UseAuxDate = value; }

        public Date? date { get => Origin.Date; set => Origin.Date = value; }
        public Date? aux_date { get => Origin.DateAux; set => Origin.DateAux = value; }
        public State state { get => (State)Origin.State; set => Origin.State = (Items.ItemStateEnum)value; }

        public Expressions.ExprOp lookup(SymbolKind kind, string name) => Origin.Lookup((Scopus.SymbolKindEnum)kind, name);
        public bool valid() => Origin.Valid();

        public override bool Equals(object obj) => Origin.Equals(obj);
        public override int GetHashCode() => Origin.GetHashCode();
    }
}

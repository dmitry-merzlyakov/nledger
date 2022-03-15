// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Items
{
    /// <summary>
    /// Ported from item_t (item.h)
    /// </summary>
    public abstract class Item : Scope
    {
        public static bool UseAuxDate 
        {
            get => MainApplicationContext.Current.UseAuxDate;
            set => MainApplicationContext.Current.UseAuxDate = value;
        }

        public static string ItemContext(Item item, string desc)
        {
            if (!item.HasPos)
                return String.Empty;

            long len = item.Pos.EndPos - item.Pos.BegPos;
            if (len <= 0)
                return String.Empty;

            if (len >= 1024 * 1024)
                throw new InvalidOperationException("len");

            if (String.IsNullOrEmpty(item.Pos.PathName))
                return desc + " from streamed input:";

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} from \"{1}\"", desc, item.Pos.PathName);

            if (item.Pos.BegLine != item.Pos.EndLine)
                sb.AppendFormat(", lines {0}-{1}:", item.Pos.BegLine, item.Pos.EndLine);
            else
                sb.AppendFormat(", line {0}:", item.Pos.BegLine);
            sb.AppendLine();

            sb.Append(PrintItem(item, "> "));
            return sb.ToString();
        }

        public static string PrintItem(Item item, string prefix = null)
        {
            return ErrorContext.SourceContext(item.Pos.PathName, item.Pos.BegPos, item.Pos.EndPos, prefix ?? String.Empty);
        }

        public Item()
        { }

        public Item(Item item)
        {
            CopyDetails(item);
        }

        public SupportsFlagsEnum Flags { get; set; }
        public ItemStateEnum State { get; set; }
        public Date? Date { get; set; }
        public Date? DateAux { get; set; }
        public string Note { get; set; }
        public ItemPosition Pos
        {
            get { return _Pos ?? (_Pos = new ItemPosition()); }
            set { _Pos = value; }
        }

        public string Id
        {
            get
            {
                Value refUUID = GetTag("UUID");
                if (!Value.IsNullOrEmpty(refUUID))
                    return refUUID.AsString;
                else
                    return Seq.ToString();
            }
        }

        public long Seq
        {
            get { return HasPos ? Pos.Sequence : 0; }
        }

        public void AppendNote(string note, Scope scope, bool overwriteExisting = true)
        {
            if (!String.IsNullOrWhiteSpace(Note))
            {
                Note = Note + Environment.NewLine + note;
            }
            else
            {
                Note = note;
            }

            ParseTags(note, scope, overwriteExisting);
        }

        public bool HasPos
        {
            get { return _Pos != null; }
        }

        /// <summary>
        /// Ported from item.cc - parse_tags()
        /// </summary>
        public virtual void ParseTags(string note, Scope scope, bool overwriteExisting = true)
        {
            if (string.IsNullOrEmpty(note))
                return;

            if (note.IndexOf(':') < 0)
            {
                int pos = note.IndexOf("[");
                if (pos >= 0 && note.Length > pos+1)
                {
                    char firstChar = note[pos + 1];
                    if (Char.IsDigit(firstChar) || firstChar == '=')
                    {
                        int endPos = note.IndexOf("]", pos);
                        if (endPos > 0)
                        {
                            string dates = note.Substring(pos + 1, endPos - pos - 1);

                            string auxDate = StringExtensions.SplitBySeparator(ref dates, '=');
                            if (!String.IsNullOrEmpty(auxDate))
                                DateAux = TimesCommon.Current.ParseDate(auxDate);
                            if (!String.IsNullOrEmpty(dates))
                                Date = TimesCommon.Current.ParseDate(dates);
                        }
                    }
                }
                return;
            }

            bool isFirst = true;
            bool byValue = false;

            string substr = null;
            while (!String.IsNullOrEmpty(substr = StringExtensions.GetWord(ref note)))
            {
                if (substr.Length < 2)
                    continue;

                if (substr.StartsWith(":") && substr.EndsWith(":"))  // a series of tags
                {
                    foreach (string substrTag in substr.Substring(1, substr.Length - 2).Split(':'))
                        SetTag(substrTag, new Value(), overwriteExisting, true);
                }
                else if (isFirst && substr.EndsWith(":")) // a metadata setting
                {
                    int index = 1;
                    if (substr.EndsWith("::"))
                    {
                        byValue = true;
                        index = 2;
                    }

                    string tag = substr.Remove(substr.Length - index);
                    string field = note;

                    if (byValue)
                    {
                        BindScope boundScope = new BindScope(scope, this);
                        SetTag(tag, new Expr(field).Calc(boundScope), overwriteExisting, true);
                    }
                    else
                        SetTag(tag, Value.Get(field), overwriteExisting, true);

                    break;
                }
                isFirst = false;
            }
        }

        public bool HasDate
        {
            get { return Date.HasValue; }
        }

        /// <summary>
        /// Ported from date(). 
        /// Note that Item.Date is equal to the original _date field.
        /// </summary>
        /// <returns></returns>
        public virtual Date GetDate()
        {
            if (!HasDate)
                throw new InvalidOperationException("Date has no value");

            if (UseAuxDate && DateAux.HasValue)
                return DateAux.Value;

            return Date.Value;
        }

        /// <summary>
        /// Ported from aux_date
        /// </summary>
        /// <returns></returns>
        public virtual Date? GetAuxDate()
        {
            return DateAux;
        }

        /// <summary>
        /// Ported from primary_date
        /// </summary>
        public virtual Date PrimaryDate()
        {
            if (!Date.HasValue)
                throw new InvalidOperationException("Date has no value");

            return Date.Value;
        }

        public virtual bool HasTag(string tag, bool inherit = true)
        {
            Logger.Current.Debug("item.meta", () => String.Format("Checking if item has tag: {0}", tag));
            if (Metadata == null)
            {
                Logger.Current.Debug("item.meta", () => "Item has no metadata at all");
                return false;
            }

            var hasTag = Metadata.ContainsKey(tag);
            Logger.Current.Debug("item.meta", () => hasTag ? "Item has the tag!" : "Item does not have this tag");

            return hasTag;
        }

        public virtual bool HasTag(Mask tagMask, Mask valueMask = null, bool inherit = true)
        {
            if (tagMask == null)
                throw new ArgumentNullException("tagMask");

            if (Metadata != null)
            {
                if (valueMask == null)
                {
                    return Metadata.Any(kv => tagMask.Match(kv.Key));
                }
                else
                {
                    return Metadata.Any(kv => tagMask.Match(kv.Key) && valueMask.Match(kv.Value.Value.ToString()));
                }
            }

            return false;
        }

        public virtual Value GetTag(string tag, bool inherit = true)
        {
            Logger.Current.Debug("item.meta", () => String.Format("Getting item tag: {0}", tag));
            if (Metadata != null)
            {
                Logger.Current.Debug("item.meta", () => "Item has metadata");
                ItemTag itemTag;
                if (Metadata.TryGetValue(tag, out itemTag))
                {
                    Logger.Current.Debug("item.meta", () => "Found the item!");
                    return itemTag.Value;
                }
            }

            return Value.Empty;
        }

        public Value GetTag(Mask tagMask, Mask valueMask = null)
        {
            if (tagMask == null)
                throw new ArgumentNullException("tagMask");

            KeyValuePair<string, ItemTag> keyValue = default(KeyValuePair<string, ItemTag>);
            if (Metadata != null)
            {
                if (valueMask == null)
                {
                    keyValue = Metadata.FirstOrDefault(kv => tagMask.Match(kv.Key));
                }
                else
                {
                    keyValue = Metadata.FirstOrDefault(kv => tagMask.Match(kv.Key) && valueMask.Match(kv.Value.Value.ToString()));
                }
            }

            return keyValue.Key != null ? keyValue.Value.Value : new Value();
        }

        public void SetTag(string tag)
        {
            SetTag(tag, new Value());
        }

        public void SetTag(string tag, Value value, bool overwriteExisting = true)
        {
            SetTag(tag, value, overwriteExisting, false);
        }

        public virtual void CopyDetails(Item item)
        {
            Flags = item.Flags;
            State = item.State;

            Date = item.Date;
            DateAux = item.DateAux;
            Note = item.Note;
            if (item.HasPos)
                Pos = new ItemPosition(item.Pos);
            if (item.Metadata != null)
                Metadata = new Dictionary<string, ItemTag>(item.Metadata);
        }

        public IEnumerable<KeyValuePair<string, ItemTag>> GetMetadata()
        {
            return Metadata;
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            return LookupItems.Value.Lookup(kind, name, this);
        }

        /// <summary>
        /// Ported from bool item_t::valid()
        /// </summary>
        public virtual bool Valid()
        {
            if (State != ItemStateEnum.Uncleared && State != ItemStateEnum.Cleared && State != ItemStateEnum.Pending)
            {
                Logger.Current.Debug("ledger.validate", () => "item_t: state is bad");
                return false;
            }

            return true;
        }

        #region Lookup Functions

        private static Value GetWrapper(CallScope scope, Func<Item, Value> func)
        {
            return func(ScopeExtensions.FindScope<Item>(scope));
        }

        private static Value GetActual(Item item)
        {
            return Value.Get(!(item.Flags.HasFlag(SupportsFlagsEnum.ITEM_GENERATED) || item.Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP)));
        }

        private static Value GetPrimaryDate(Item item)
        {
            return Value.Get(item.PrimaryDate());
        }

        /// <summary>
        /// Ported from get_aux_date
        /// </summary>
        private static Value GetAuxDate(Item item)
        {
            var auxDate = item.GetAuxDate();
            if (auxDate.HasValue)
                return Value.Get(auxDate.Value);
            else
                return Value.Empty;
        }

        private static Value GetBegLine(Item item)
        {
            return item.Pos != null ? Value.Get(item.Pos.BegLine) : Value.Zero;
        }

        private static Value GetBegPos(Item item)
        {
            return item.Pos != null ? Value.Get(item.Pos.BegPos) : Value.Zero;
        }

        private static Value GetCleared(Item item)
        {
            return Value.Get(item.State == ItemStateEnum.Cleared);
        }

        private static Value GetPending(Item item)
        {
            return Value.Get(item.State == ItemStateEnum.Pending);
        }

        private static Value GetUncleared(Item item)
        {
            return Value.Get(item.State == ItemStateEnum.Uncleared);
        }

        private static Value GetComment(Item item)
        {
            if (String.IsNullOrEmpty(item.Note))
            {
                return Value.StringValue(String.Empty);
            }
            else
            {
                string buf = item.Note.Length > 15 ? Environment.NewLine + "    ;" : "  ;" +
                    item.Note.Replace(Environment.NewLine, Environment.NewLine + "    ;");
                return Value.StringValue(buf);
            }
        }

        private static Value GetEndLine(Item item)
        {
            return item.Pos != null ? Value.Get(item.Pos.EndLine) : Value.Zero;
        }

        private static Value GetEndPos(Item item)
        {
            return item.Pos != null ? Value.Get(item.Pos.EndPos) : Value.Zero;
        }

        private static Value GetPathName(Item item)
        {
            if (item.Pos != null)
                return Value.StringValue(item.Pos.PathName);
            else
                return Value.Empty;
        }

        private static Value GetFileBase(Item item)
        {
            if (item.Pos != null)
                return Value.StringValue(FileSystem.GetFileName(item.Pos.PathName));
            else
                return Value.Empty;
        }

        private static Value GetFilePath(Item item)
        {
            if (item.Pos != null)
                return Value.StringValue(FileSystem.GetDirectoryName(item.Pos.PathName));
            else
                return Value.Empty;
        }

        private static Value HasTag(CallScope args)
        {
            Item item = ScopeExtensions.FindScope<Item>(args);

            if (args.Size == 1)
            {
                if (args[0].Type == ValueTypeEnum.String)
                    return Value.Get(item.HasTag(args.Get<string>(0)));
                else if (args[0].Type == ValueTypeEnum.Mask)
                    return Value.Get(item.HasTag(args.Get<Mask>(0)));
                else
                    throw new RuntimeError(String.Format(RuntimeError.ErrorMessageExpectedStringOrMaskForArgument1ButReceivedSmth, args[0].ToString()));
            }
            else if (args.Size == 2)
            {
                if (args[0].Type == ValueTypeEnum.Mask && args[1].Type == ValueTypeEnum.Mask)
                    return Value.Get(item.HasTag(args.Get<Mask>(0), args.Get<Mask>(1)));
                else
                    throw new RuntimeError(String.Format(RuntimeError.ErrorMessageExpectedMasksForArguments1and2ButReceivedSmthAndSmth, args[0].ToString(), args[1].ToString()));
            }
            else if (args.Size == 0)
                throw new RuntimeError(RuntimeError.ErrorMessageTooFewArgumentsToFunction);
            else
                throw new RuntimeError(RuntimeError.ErrorMessageTooManyArgumentsToFunction);
        }

        private static Value GetId(Item item)
        {
            return Value.StringValue(item.Id);
        }

        private static Value GetTag(CallScope args)
        {
            Item item = ScopeExtensions.FindScope<Item>(args);
            Value val = null;

            if (args.Size == 1)
            {
                if (args[0].Type == ValueTypeEnum.String)
                    val = item.GetTag(args.Get<string>(0));
                else if (args[0].Type == ValueTypeEnum.Mask)
                    val = item.GetTag(args.Get<Mask>(0));
                else
                    throw new RuntimeError(String.Format(RuntimeError.ErrorMessageExpectedStringOrMaskForArgument1ButReceivedSmth, args[0].ToString()));
            }
            else if (args.Size == 2)
            {
                if (args[0].Type == ValueTypeEnum.Mask && args[1].Type == ValueTypeEnum.Mask)
                    val = item.GetTag(args.Get<Mask>(0), args.Get<Mask>(1));
                else
                    throw new RuntimeError(String.Format(RuntimeError.ErrorMessageExpectedMasksForArguments1and2ButReceivedSmthAndSmth, args[0].ToString(), args[1].ToString()));
            }
            else if (args.Size == 0)
                throw new RuntimeError(RuntimeError.ErrorMessageTooFewArgumentsToFunction);
            else
                throw new RuntimeError(RuntimeError.ErrorMessageTooManyArgumentsToFunction);

            return Value.IsNullOrEmptyOrFalse(val) ? Value.Empty : val;
        }

        private static Value GetNote(Item item)
        {
            return !String.IsNullOrEmpty(item.Note) ? Value.StringValue(item.Note) : Value.Empty;
        }

        private static Value GetStatus(Item item)
        {
            return Value.Get((long)item.State);
        }

        private static ExprOpCollection CreateLookupItems()
        {
            ExprOpCollection lookupItems = new ExprOpCollection();

            // a
            lookupItems.MakeFunctor("actual", scope => GetWrapper((CallScope)scope, p => GetActual(p)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("actual_date", scope => GetWrapper((CallScope)scope, p => GetPrimaryDate(p)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("addr", scope => GetWrapper((CallScope)scope, p => Value.Get(p) /* Not allowed in .Net */ ), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("aux_date", scope => GetWrapper((CallScope)scope, p => GetAuxDate(p)), SymbolKindEnum.FUNCTION);

            // b
            lookupItems.MakeFunctor("beg_line", scope => GetWrapper((CallScope)scope, p => GetBegLine(p)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("beg_pos", scope => GetWrapper((CallScope)scope, p => GetBegPos(p)), SymbolKindEnum.FUNCTION);

            // c
            lookupItems.MakeFunctor("cleared", scope => GetWrapper((CallScope)scope, p => GetCleared(p)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("comment", scope => GetWrapper((CallScope)scope, p => GetComment(p)), SymbolKindEnum.FUNCTION);

            // d
            lookupItems.MakeFunctor("d", scope => GetWrapper((CallScope)scope, p => Value.Get(p.GetDate())), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("date", scope => GetWrapper((CallScope)scope, p => Value.Get(p.GetDate())), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("depth", scope => Value.Zero, SymbolKindEnum.FUNCTION);

            // e
            lookupItems.MakeFunctor("end_line", scope => GetWrapper((CallScope)scope, p => GetEndLine(p)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("end_pos", scope => GetWrapper((CallScope)scope, p => GetEndPos(p)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("effective_date", scope => GetWrapper((CallScope)scope, p => GetAuxDate(p)), SymbolKindEnum.FUNCTION);

            // f
            lookupItems.MakeFunctor("filename", scope => GetWrapper((CallScope)scope, p => GetPathName(p)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("filebase", scope => GetWrapper((CallScope)scope, p => GetFileBase(p)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("filepath", scope => GetWrapper((CallScope)scope, p => GetFilePath(p)), SymbolKindEnum.FUNCTION);

            // h
            lookupItems.MakeFunctor("has_tag", scope => HasTag((CallScope)scope), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("has_meta", scope => HasTag((CallScope)scope), SymbolKindEnum.FUNCTION);

            // i
            lookupItems.MakeFunctor("is_account", scope => Value.False /* ignore */, SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("id", scope => GetWrapper((CallScope)scope, p => GetId(p)), SymbolKindEnum.FUNCTION);

            // m
            lookupItems.MakeFunctor("meta", scope => GetTag((CallScope)scope), SymbolKindEnum.FUNCTION);

            // n
            lookupItems.MakeFunctor("note", scope => GetWrapper((CallScope)scope, p => GetNote(p)), SymbolKindEnum.FUNCTION);

            // p
            lookupItems.MakeFunctor("pending", scope => GetWrapper((CallScope)scope, p => GetPending(p)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("parent", scope => Value.False /* ignore */, SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("primary_date", scope => GetWrapper((CallScope)scope, p => GetPrimaryDate(p)), SymbolKindEnum.FUNCTION);

            // s
            lookupItems.MakeFunctor("status", scope => GetWrapper((CallScope)scope, p => GetStatus(p)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("state", scope => GetWrapper((CallScope)scope, p => GetStatus(p)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("seq", scope => GetWrapper((CallScope)scope, p => Value.Get(p.Seq)), SymbolKindEnum.FUNCTION);

            // t
            lookupItems.MakeFunctor("tag", scope => GetTag((CallScope)scope), SymbolKindEnum.FUNCTION);

            // u
            lookupItems.MakeFunctor("uncleared", scope => GetWrapper((CallScope)scope, p => GetUncleared(p)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("uuid", scope => GetWrapper((CallScope)scope, p => GetId(p)), SymbolKindEnum.FUNCTION);

            // v
            lookupItems.MakeFunctor("value_date", scope => GetWrapper((CallScope)scope, p => Value.Get(p.Date)), SymbolKindEnum.FUNCTION);

            // L
            lookupItems.MakeFunctor("L", scope => GetWrapper((CallScope)scope, p => GetActual(p)), SymbolKindEnum.FUNCTION);

            // X
            lookupItems.MakeFunctor("X", scope => GetWrapper((CallScope)scope, p => GetCleared(p)), SymbolKindEnum.FUNCTION);

            // Y
            lookupItems.MakeFunctor("Y", scope => GetWrapper((CallScope)scope, p => GetPending(p)), SymbolKindEnum.FUNCTION);

            return lookupItems;
        }

        #endregion

        private void SetTag(string tag, Value value, bool overwriteExisting, bool isParsed)
        {
            if (String.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException("tag");
            if (value == null)
                throw new ArgumentNullException("value");

            if (Metadata == null)
                Metadata = new SortedDictionary<string,ItemTag>(StringComparer.InvariantCultureIgnoreCase);

            Logger.Current.Debug("item.meta", () => String.Format("Setting tag '{0}' to value '{1}'", tag, Value.IsNullOrEmpty(value) ? "<none>" : value.ToString()));

            ItemTag itemTag = new ItemTag(value, isParsed);

            if (Metadata.ContainsKey(tag))
            {
                if (overwriteExisting)
                    Metadata[tag] = itemTag;
            }
            else
            {
                Metadata.Add(tag, itemTag);
            }
        }

        private IDictionary<string, ItemTag> Metadata = null;
        private ItemPosition _Pos = null;
        private static readonly Lazy<ExprOpCollection> LookupItems = new Lazy<ExprOpCollection>(() => CreateLookupItems(), true);
    }
}

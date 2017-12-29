// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Scopus
{
    public sealed class OptionCollection
    {
        public T Add<T>(T option) where T : Option
        {
            if (option == null)
                throw new ArgumentNullException("option");

            Items.Add(option);

            return option;
        }

        public IEnumerable<Option> Options
        {
            get { return Items; }
        }

        /// <summary>
        /// OPT(name)
        /// </summary>
        public void AddLookupOpt(string optionName)
        {
            LookupEntries.Add(new LookupEntry(FindOption(optionName), OptComparer));
        }

        /// <summary>
        /// OPT_ALT(name, alt) 
        /// </summary>
        public void AddLookupOptAlt(string optionName, string altOptionName)
        {
            LookupEntries.Add(new LookupEntry(FindOption(optionName), OptAltComparer, altOptionName));
        }

        /// <summary>
        /// OPT_(name) 
        /// </summary>
        public void AddLookupOptArgs(string optionName, string shortParamKey)
        {
            LookupEntries.Add(new LookupEntry(FindOption(optionName), OptArgsComparer, shortParamKey));
        }

        /// <summary>
        /// OPT_CH(name)
        /// </summary>
        public void AddLookupArgs(string optionName, string shortParamKey)
        {
            LookupEntries.Add(new LookupEntry(FindOption(optionName), ArgsComparer, shortParamKey));
        }

        public string Report()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Option option in Items)
                sb.Append(option.Report());
            return sb.ToString();
        }

        public Option LookupOption(string s, object parent = null)
        {
            if (String.IsNullOrEmpty(s))
                return null;

            LookupEntry entry = LookupEntries.FirstOrDefault(item => item.Comparer(s, item));
            if (entry != null)
            {
                entry.Option.Parent = parent;
                return entry.Option;
            }
            return null;
        }

        /// <summary>
        /// Ported from is_eq
        /// </summary>
        public static bool IsEq(string p, string n)
        {
            int idxP = 0;
            int idxN = 0;

            // Test whether p matches n, substituting - in p for _ in n.
            for (; idxP < p.Length && idxN < n.Length; idxP++, idxN++)
            {
                if (!(p[idxP] == '-' && n[idxN] == '_') && p[idxP] != n[idxN])
                    return false;
            }
            // Ignore any trailing underscore
            return (idxP == idxN && p.Length == n.Length) || (!(idxP < p.Length) && n[idxN] == '_' && (idxN == n.Length - 1));
        }

        private Option FindOption(string optionName)
        {
            return Items.FirstOrDefault(item => item.Name == optionName);
        }

        private readonly IList<Option> Items = new List<Option>();
        private readonly IList<LookupEntry> LookupEntries = new List<LookupEntry>();

        private static bool OptComparer(string key, LookupEntry lookupEntry)
        {
            return IsEq(key, lookupEntry.Option.Name);
        }

        private static bool OptAltComparer(string key, LookupEntry lookupEntry)
        {
            return IsEq(key, lookupEntry.Option.Name) || IsEq(key, lookupEntry.AltName);
        }

        private static bool OptArgsComparer(string key, LookupEntry lookupEntry)
        {
            if (key.StartsWith(lookupEntry.AltName))
            {
                if (key.Length == lookupEntry.AltName.Length)
                    return true;

                if (lookupEntry.Option.WantsArg && key.Length == lookupEntry.AltName.Length + 1 && key[lookupEntry.AltName.Length] == '_')
                    return true;
            }
            return OptComparer(key, lookupEntry);
        }

        private static bool ArgsComparer(string key, LookupEntry lookupEntry)
        {
            if (key.StartsWith(lookupEntry.AltName))
            {
                if (key.Length == lookupEntry.AltName.Length)
                    return true;

                if (lookupEntry.Option.WantsArg && key.Length == lookupEntry.AltName.Length + 1 && key[lookupEntry.AltName.Length] == '_')
                    return true;
            }
            return false;
        }

        private sealed class LookupEntry
        {
            public LookupEntry(Option option, Func<string, LookupEntry, bool> comparer, string altName = null)
            {
                if (option == null)
                    throw new ArgumentNullException("option");
                if (comparer == null)
                    throw new ArgumentNullException("comparer");

                Option = option;
                Comparer = comparer;
                AltName = altName;
            }

            public Option Option { get; private set; }
            public Func<string, LookupEntry, bool> Comparer { get; private set; }
            public string AltName { get; private set; }
        }
    }
}

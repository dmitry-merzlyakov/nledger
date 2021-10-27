using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    /// <summary>
    /// This is a generic converter that presents Boolean-property flags in a source data object as numberic bit flags.
    /// Provides basic operations such as GetFlags, SetFlags, HasFlags, ClearFlags, AddFlags and DropFlags
    /// </summary>
    public sealed class FlagsConverter<T>
    {
        public uint GetFlags(T origin)
        {
            if (origin == null)
                throw new ArgumentNullException(nameof(origin));

            uint flags = 0;
            foreach (var flag in Mapping.Values.Where(f => f.Getter(origin)).Select(f => f.Flag))
                flags |= flag;

            return flags;
        }

        public void SetFlags(T origin, uint flags)
        {
            if (origin == null)
                throw new ArgumentNullException(nameof(origin));

            foreach (var f in Mapping.Values)
                f.Setter(origin, f.HasFlag(flags));
        }

        public bool HasFlags(T origin, uint flags)
        {
            if (origin == null)
                throw new ArgumentNullException(nameof(origin));

            return Mapping.Values.Where(f => f.HasFlag(flags)).All(f => f.Getter(origin));
        }

        public void ClearFlags(T origin)
        {
            if (origin == null)
                throw new ArgumentNullException(nameof(origin));

            Mapping.Values.ToList().ForEach(f => f.Setter(origin, false));
        }

        public void AddFlags(T origin, uint flags)
        {
            if (origin == null)
                throw new ArgumentNullException(nameof(origin));

            Mapping.Values.Where(f => f.HasFlag(flags)).ToList().ForEach(f => f.Setter(origin, true));
        }

        public void DropFlags(T origin, uint flags)
        {
            if (origin == null)
                throw new ArgumentNullException(nameof(origin));

            Mapping.Values.Where(f => f.HasFlag(flags)).ToList().ForEach(f => f.Setter(origin, false));
        }

        public FlagsConverter<T> AddMapping(uint flag, Func<T, bool> getter, Action<T, bool> setter)
        {
            if (Mapping.ContainsKey(flag))
                throw new ArgumentException($"Flag {flag} has been already added");

            Mapping.Add(flag, new FlagWrapper(flag, getter, setter));
            return this;
        }

        public static bool IsFlag(uint flag)
        {
            return flag != 0 && (flag & (flag - 1)) == 0;
        }


        private class FlagWrapper
        {
            public FlagWrapper(uint flag, Func<T, bool> getter, Action<T, bool> setter)
            {
                if (!IsFlag(flag))
                    throw new ArgumentException($"Value {flag} is not a bit flag");

                Flag = flag;
                Getter = getter ?? throw new ArgumentNullException(nameof(getter));
                Setter = setter ?? throw new ArgumentNullException(nameof(setter));
            }

            public uint Flag { get; }
            public Func<T, bool> Getter { get; }
            public Action<T, bool> Setter { get; }

            public bool HasFlag(uint flags) => (flags & Flag) != 0;
        }

        private readonly IDictionary<uint, FlagWrapper> Mapping = new Dictionary<uint, FlagWrapper>();
    }
}

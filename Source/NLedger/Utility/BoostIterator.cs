// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public static class BoostIteratorExtensions
    {
        public static BoostIterator<T> Begin<T>(this IList<T> source)
        {
            return new BoostIterator<T>(source, 0);
        }

        public static BoostIterator<T> End<T>(this IList<T> source)
        {
            return new BoostIterator<T>(source, source.Count - 1);
        }

        public static void PopBack<T>(this IList<T> source)
        {
            source.RemoveAt(source.Count - 1);
        }
    }

    public class BoostIterator<T>
    {
        public static bool operator == (BoostIterator<T> x, BoostIterator<T> y)
        {
            return !Object.ReferenceEquals(x, null) ? x.Equals(y) : Object.ReferenceEquals(y, null);
        }

        public static bool operator !=(BoostIterator<T> x, BoostIterator<T> y)
        {
            return !Object.ReferenceEquals(x, null) ? !x.Equals(y) : !Object.ReferenceEquals(y, null);
        }

        public BoostIterator(IList<T> source, int position = -1)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            Source = source;
            Position = position;
        }

        public IList<T> Source { get; private set; }
        public int Position { get; private set; }

        public T Current
        {
            get { return Position >= 0 && Position < Source.Count ? Source[Position] : default(T); }
        }

        public void Increment()
        {
            Position++;
        }

        public override bool Equals(object obj)
        {
            BoostIterator<T> iterator = obj as BoostIterator<T>;
            if (iterator == null)
                return false;

            return Source == iterator.Source && Position == iterator.Position;
        }

        public override int GetHashCode()
        {
            return Source.GetHashCode() ^ Position.GetHashCode();
        }
    }
}

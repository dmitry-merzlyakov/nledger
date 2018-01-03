// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Textual
{
    /// <summary>
    /// Ported from the type std::list&lt;application_t&gt; apply_stack (textual.cc)
    /// </summary>
    public class ApplyStack
    {
        public ApplyStack(ApplyStack parent = null)
        {
            Parent = parent;
            Items = new Stack<Tuple<string, object>>();
        }

        public ApplyStack Parent { get; private set; }

        public int Size
        {
            get { return Items.Count; }
        }

        public IEnumerable<T> GetApplications<T>()
        {
            List<T> items = Items.
                Where(item => item.Item2 != null && item.Item2.GetType() == typeof(T)).
                Select(item => (T)item.Item2).ToList();

            if (Parent != null)
                items.AddRange(Parent.GetApplications<T>());

            return items;
        }

        public T GetApplication<T>()
        {
            Tuple<string,object> result = Items.FirstOrDefault(item => item.Item2 != null && item.Item2.GetType() == typeof(T));

            if (result != null)
                return (T)result.Item2;
            else
                return Parent != null ? Parent.GetApplication<T>() : default(T);
        }

        public void PushFront<T>(string key, T value)
        {
            Items.Push(new Tuple<string, object>(key, value));
        }

        public void PopFront()
        {
            Items.Pop();
        }

        public bool IsFrontType<T>()
        {
            return Items.Any() && Items.Peek().Item2 is T;
        }

        public T Front<T>()
        {
            return (T)Items.Peek().Item2;
        }

        public string FrontLabel()
        {
            return Items.Peek().Item1;
        }

        private readonly Stack<Tuple<string, object>> Items;
    }
}

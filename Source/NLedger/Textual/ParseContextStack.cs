// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Textual
{
    /// <summary>
    /// Ported from parse_context_stack_t (context.h)
    /// </summary>
    public sealed class ParseContextStack
    {
        public int Count
        {
            get { return ParsingContext.Count; }
        }

        public void Push()
        {
            Push(new ParseContext(FileSystem.CurrentPath()));
        }

        public void Push(ITextualReader reader)
        {
            Push(reader, FileSystem.CurrentPath());
        }

        public void Push(ITextualReader reader, string currentPath)
        {
            Push(new ParseContext(reader, FileSystem.CurrentPath()));
        }

        public void Push(string pathName)
        {
            Push(pathName, FileSystem.CurrentPath());
        }

        public void Push(string pathName, string currentPath)
        {
            Push(ParseContext.OpenForReading(pathName, currentPath));
        }

        public void Push(ParseContext context)
        {
            ParsingContext.Push(context);
        }

        public void Pop()
        {
            if (!ParsingContext.Any())
                throw new InvalidOperationException("stack is empty");

            ParsingContext.Pop().Dispose();     // [DM] Calling Dispose cleans up corresponded to the removed context resources (e.g. open stream readers)
        }

        public ParseContext GetCurrent()
        {
            if (!ParsingContext.Any())
                throw new InvalidOperationException("stack is empty");

            return ParsingContext.Peek();
        }

        private readonly Stack<ParseContext> ParsingContext = new Stack<ParseContext>();
    }
}

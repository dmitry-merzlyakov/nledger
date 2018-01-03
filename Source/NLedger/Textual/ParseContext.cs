// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Journals;
using NLedger.Scopus;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Textual
{
    /// <summary>
    /// Ported from parse_context_t (context.h)
    /// </summary>
    public class ParseContext
    {
        public const int MAX_LINE = 4096;

        /// <summary>
        /// Ported from: open_for_reading
        /// </summary>
        public static ParseContext OpenForReading(string pathName, string currentWorkDirectory)
        {
            string fileName = FileSystem.ResolvePath(pathName);
            fileName = FileSystem.Combine(fileName, currentWorkDirectory);

            if (!FileSystem.FileExists(fileName))
                throw new Exception(String.Format("Cannot read journal file \"{0}\"", fileName));

            string parent = FileSystem.GetParentPath(fileName);
            ITextualReader reader = new TextualReader(FileSystem.GetStreamReader(fileName));
            ParseContext parseContext = new ParseContext(reader, parent) { PathName = fileName };
            return parseContext;
        }

        public ParseContext(string currentPath)
        {
            CurrentPath = currentPath;
        }

        public ParseContext(ITextualReader reader, string currentPath)
            : this(currentPath)
        {
            Reader = reader;
        }

        public Journal Journal { get; set; }
        public Account Master { get; set; }
        public Scope Scope { get; set; }

        public ITextualReader Reader { get; set; }
        public string CurrentPath { get; set; }
        public string PathName { get; private set; }
        public int Count { get; set; }
        public long LineBegPos { get; set; }
        public int LineNum { get; set; }
        public long CurrPos { get; set; }
        public int Errors { get; set; }
        public int Sequence { get; set; }
        public string LineBuf { get; set; }

        public string Location
        {
            get { return ErrorContext.FileContext(PathName, LineNum); }
        }

        public void Warning(string what)
        {
            ErrorContext.Current.WriteWarning(Location + " " + what);
        }
    }
}

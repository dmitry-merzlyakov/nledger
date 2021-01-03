// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Journals
{
    /// <summary>
    /// Ported from journal_t/fileinfo_t (journal.h)
    /// </summary>
    public sealed class JournalFileInfo
    {
        public JournalFileInfo()
        {
            FromStream = true;
        }

        public JournalFileInfo(string fileName)
        {
            FileName = fileName;
            Size = FileSystem.FileSize(fileName);
            ModTime = FileSystem.LastWriteTime(fileName);
        }

        public string FileName { get; private set; }
        public long Size { get; private set; }
        public DateTime ModTime { get; private set; }
        public bool FromStream { get; private set; }
    }
}

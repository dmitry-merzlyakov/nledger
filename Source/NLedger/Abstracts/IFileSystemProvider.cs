// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Abstracts
{
    /// <summary>
    /// File System Virtualization
    /// </summary>
    public interface IFileSystemProvider
    {
        string GetCurrentDirectory();

        bool DirectoryExists(string path);
        bool FileExists(string path);

        IEnumerable<string> GetFiles(string path);

        StreamReader OpenText(string path);
        Stream OpenRead(string path);
        TextWriter CreateText(string path);
        void AppendAllText(string path, string contents);

        long GetFileSize(string fileName);
        DateTime GetLastWriteTimeUtc(string path);

        string GetDirectoryName(string path);
        string GetFileName(string path);
        string PathCombine(string path1, string path2);
        string GetFullPath(string path);
    }
}

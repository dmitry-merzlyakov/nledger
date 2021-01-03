// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace NLedger.Abstracts.Impl
{
    /// <summary>
    /// Default implementation of a file system provider. Uses System.IO.
    /// </summary>
    public sealed class FileSystemProvider : IFileSystemProvider
    {
        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public StreamReader OpenText(string path)
        {
            return File.OpenText(path);
        }

        public Stream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public TextWriter CreateText(string path)
        {
            return File.CreateText(path);
        }

        public void AppendAllText(string path, string contents)
        {
            File.AppendAllText(path, contents);
        }

        public long GetFileSize(string fileName)
        {
            return new FileInfo(fileName).Length;
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            return File.GetLastWriteTimeUtc(path);
        }

        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public string PathCombine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        public string GetFolderPath(SpecialFolder folder)
        {
            return Environment.GetFolderPath(folder);
        }

    }
}

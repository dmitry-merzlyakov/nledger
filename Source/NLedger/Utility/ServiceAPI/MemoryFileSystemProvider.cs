// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace NLedger.Utility.ServiceAPI
{
    /// <summary>
    /// Simulates a virtual in-memory linux-style file system.
    /// </summary>
    public class MemoryFileSystemProvider : IFileSystemProvider
    {
        public static readonly string DirectorySeparator = "/";

        public MemoryFileSystemProvider()
        {
            Root = new FileSystemRoot();
        }

        public void AppendAllText(string path, string content)
        {
            var file = Root.GetFile(path);
            if (file == null)
                CreateFile(path, content);
            else
                file.SetContent(file.Content + content);
        }

        public TextWriter CreateText(string path)
        {
            CreateFile(path);
            return new FileSystemEntryTextWriter(Root.GetFile(path));
        }

        public bool DirectoryExists(string path)
        {
            return Root.GetFolder(path) != null;
        }

        public bool FileExists(string path)
        {
            return Root.GetFile(path) != null;
        }

        public string GetCurrentDirectory()
        {
            return Root.CurrentFolder.FullPath;
        }

        public string GetDirectoryName(string path)
        {
            return Root.GetDirectoryName(path);
        }

        public string GetFileName(string path)
        {
            return Root.GetFileName(path);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            var folder = Root.GetFolder(path);
            if (folder == null)
                throw new InvalidOperationException($"Folder not found: {path}");

            return folder.Files.Values.Select(f => f.FullPath).OrderBy(f => f);
        }

        public long GetFileSize(string fileName)
        {
            var file = Root.GetFile(fileName);
            if (file == null)
                throw new InvalidOperationException($"File not found: {fileName}");

            return file.Size;
        }

        public string GetFullPath(string path)
        {
            return Root.GetFullPath(path);
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            var folder = Root.GetFolder(path);
            if (folder != null)
                return folder.LastWriteTimeUtc;

            var file = Root.GetFile(path);
            if (file == null)
                throw new InvalidOperationException($"Path not found: {path}");

            return file.LastWriteTimeUtc;
        }

        public Stream OpenRead(string fileName)
        {
            var file = Root.GetFile(fileName);
            if (file == null)
                throw new InvalidOperationException($"File not found: {fileName}");

            return new MemoryStream(Encoding.UTF8.GetBytes(file.Content ?? String.Empty));
        }

        public StreamReader OpenText(string fileName)
        {
            return new StreamReader(OpenRead(fileName));
        }

        public string PathCombine(string path1, string path2)
        {
            return FileSystemRoot.PathCombine(path1, path2);
        }

        public string GetFolderPath(SpecialFolder folder)
        {
            // In virtual file system, all special folders are associated with the root
            return Root.FullPath;
        }

        // Helper methods

        public void SetCurrentDirectory(string path)
        {
            Root.SetCurrentDirectory(path);
        }

        public void CreateFile(string path, string content = null)
        {
            var parentfolder = Root.GetFolder(Root.GetDirectoryName(path));
            if (parentfolder == null)
                throw new InvalidOperationException($"Parent path not found: {path}");

            parentfolder.CreateFile(Root.GetFileName(path), content);
        }

        public void CreateFolder(string path)
        {
            var parentfolder = Root.GetFolder(Root.GetDirectoryName(path));
            if (parentfolder == null)
                throw new InvalidOperationException($"Parent path not found: {path}");

            parentfolder.CreateFolder(Root.GetFileName(path));
        }

        private abstract class FileSystemEntry
        {
            public FileSystemEntry(string name, FileSystemFolder parent)
            {
                if (HasParent && String.IsNullOrWhiteSpace(name))
                    throw new ArgumentNullException(nameof(name));
                if (HasParent && parent == null)
                    throw new ArgumentNullException(nameof(parent));

                Name = name;
                Parent = parent;
                LastWriteTimeUtc = DateTime.UtcNow;
            }

            public string Name { get; }
            public FileSystemFolder Parent { get; }
            public DateTime LastWriteTimeUtc { get; protected set; }

            public abstract bool IsFolder { get; }
            public virtual FileSystemRoot Root => Parent.Root;
            public virtual string FullPath => Parent != Root ? Parent.FullPath + DirectorySeparator + Name : DirectorySeparator + Name;
            public virtual bool HasParent => true;
        }

        private class FileSystemFolder : FileSystemEntry
        {
            public FileSystemFolder(string name, FileSystemFolder parent) 
                : base (name, parent)
            { }

            public override bool IsFolder => true;

            public IDictionary<string, FileSystemFolder> Folders { get; } = new Dictionary<string, FileSystemFolder>();
            public IDictionary<string, FileSystemFile> Files { get; } = new Dictionary<string, FileSystemFile>();

            public FileSystemFolder CreateFolder(string name)
            {
                if (String.IsNullOrWhiteSpace(name))
                    throw new ArgumentNullException(nameof(name));
                if (Files.ContainsKey(name))
                    throw new InvalidOperationException($"File already exists: {name}");
                if (Folders.ContainsKey(name))
                    throw new InvalidOperationException($"Folder already exists: {name}");

                var folder = new FileSystemFolder(name, this);
                Folders.Add(name, folder);
                return folder;
            }

            public FileSystemFile CreateFile(string name, string content = null)
            {
                if (String.IsNullOrWhiteSpace(name))
                    throw new ArgumentNullException(nameof(name));
                if (Files.ContainsKey(name))
                    throw new InvalidOperationException($"File already exists: {name}");
                if (Folders.ContainsKey(name))
                    throw new InvalidOperationException($"Folder already exists: {name}");

                var file = new FileSystemFile(name, this);
                Files.Add(name, file);

                if (content != null)
                    file.SetContent(content);

                return file;
            }
        }

        private class FileSystemFile : FileSystemEntry
        {
            public FileSystemFile(string name, FileSystemFolder parent)
                : base(name, parent)
            { }

            public override bool IsFolder => false;
            public long Size => Content != null ? Content.Length : 0;

            public string Content { get; private set; }

            public void SetContent(string content)
            {
                Content = content;
                LastWriteTimeUtc = DateTime.UtcNow;
            }
        }

        private class FileSystemRoot : FileSystemFolder
        {
            public FileSystemRoot()
                : base(String.Empty, null)
            {
                CurrentFolder = this;
            }

            public override FileSystemRoot Root => this;
            public override string FullPath => DirectorySeparator;
            public override bool HasParent => false;

            public FileSystemFolder CurrentFolder { get; private set; }

            public static bool IsAbsolutePath(string path)
            {
                return !String.IsNullOrEmpty(path) && path.StartsWith(DirectorySeparator);
            }

            public static string PathCombine(string path1, string path2)
            {
                if (IsAbsolutePath(path2))
                    return path2;

                var items = ((path1 + DirectorySeparator + path2).Split(new string[] { DirectorySeparator }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
                var normalized = new Stack<string>();

                foreach(var item in items)
                {
                    if (item == "..")
                    {
                        if (normalized.Count == 0)
                            throw new InvalidOperationException($"Cannot combine paths '{path1}' and '{path2}'");

                        normalized.Pop();
                    }
                    else
                    {
                        if (item != ".")
                            normalized.Push(item);
                    }
                }

                var root = IsAbsolutePath(path1) ? DirectorySeparator : String.Empty;
                return root + String.Join(DirectorySeparator, normalized.Reverse().ToArray());
            }

            public string GetFullPath(string path)
            {
                if (String.IsNullOrWhiteSpace(path))
                    throw new ArgumentNullException(nameof(path));

                if (IsAbsolutePath(path))
                    return path;

                return PathCombine(CurrentFolder.FullPath, path);
            }

            public string[] GetFullPathArray(string path)
            {
                return GetFullPath(path).Split(new string[] { DirectorySeparator }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            }

            public string GetDirectoryName(string path)
            {
                var pathArray = GetFullPathArray(path);
                return DirectorySeparator + String.Join(DirectorySeparator, pathArray.Take(pathArray.Length - 1).ToArray());
            }

            public string GetFileName(string path)
            {
                var pathArray = GetFullPathArray(path);
                return pathArray.LastOrDefault();
            }

            public FileSystemFolder GetFolder(string path)
            {
                var pathArray = GetFullPathArray(path);
                if (!pathArray.Any())
                    return this;

                FileSystemFolder folder = this;
                foreach (var item in pathArray)
                {
                    if (!folder.Folders.TryGetValue(item, out folder))
                        return null;                    
                }

                return folder;
            }

            public FileSystemFile GetFile(string path)
            {
                var folderName = GetDirectoryName(path);
                var fileName = GetFileName(path);

                var folder = GetFolder(folderName);
                if (folder == null)
                    return null;

                FileSystemFile file;
                folder.Files.TryGetValue(fileName, out file);
                return file;
            }

            public void SetCurrentDirectory(string path)
            {
                var folder = GetFolder(path);
                if (folder == null)
                    throw new InvalidOperationException($"Path not found: {path}");

                CurrentFolder = folder;
            }

        }

        private class FileSystemEntryTextWriter : StringWriter
        {
            public FileSystemEntryTextWriter(FileSystemFile fileSystemFile)
            {
                if (fileSystemFile == null)
                    throw new ArgumentNullException(nameof(fileSystemFile));

                FileSystemFile = fileSystemFile;
            }

            public FileSystemFile FileSystemFile { get; }

            public override void Close()
            {
                base.Close();
                FileSystemFile.SetContent(ToString());
            }
        }

        private readonly FileSystemRoot Root;
    }
}

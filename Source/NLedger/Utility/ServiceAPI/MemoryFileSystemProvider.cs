using NLedger.Abstracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.ServiceAPI
{
    public class MemoryFileSystemProvider : IFileSystemProvider
    {
        public MemoryFileSystemProvider(string rootName = null)
        {
            Root = new FileSystemRoot(rootName ?? "home");
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

            return folder.Files.Keys;
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
                if (String.IsNullOrWhiteSpace(name))
                    throw new ArgumentNullException(nameof(name));
                if (parent == null && HasParent)
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
            public virtual string FullPath => Parent.FullPath + "/" + Name;
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
            public FileSystemRoot(string name)
                : base(name, null)
            {
                CurrentFolder = this;
            }

            public override FileSystemRoot Root => this;
            public override string FullPath => "/" + Name;
            public override bool HasParent => false;

            public FileSystemFolder CurrentFolder { get; private set; }

            public static string PathCombine(string path1, string path2)
            {
                if (path2.StartsWith("/"))
                    return path2;

                var baseUri = path1.Contains(@":\") ? new Uri(path1) : new Uri("/" + path1);      // Expected absolute path
                var fullUri = new Uri(baseUri, "./" + path2);   // Expected relative path
                return fullUri.AbsoluteUri.Substring("file:/".Length);
            }

            public string GetFullPath(string path)
            {
                if (String.IsNullOrWhiteSpace(path))
                    throw new ArgumentNullException(nameof(path));

                if (path.StartsWith("/"))
                    return path;

                return PathCombine(CurrentFolder.FullPath, path);
            }

            public string[] GetFullPathArray(string path)
            {
                return GetFullPath(path).Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            }

            public string GetDirectoryName(string path)
            {
                var pathArray = GetFullPathArray(path);
                return "/" + String.Join("/", pathArray.Take(pathArray.Length - 1).ToArray());
            }

            public string GetFileName(string path)
            {
                var pathArray = GetFullPathArray(path);
                return pathArray.LastOrDefault();
            }

            public FileSystemFolder GetFolder(string path)
            {
                var pathArray = GetFullPathArray(path);
                if (pathArray.Length < 1 || pathArray[0] != Name)
                    return null;

                FileSystemFolder folder = this;
                for (int i=1; i< pathArray.Length; i++)
                {
                    if (!folder.Folders.TryGetValue(pathArray[i], out folder))
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

            public override void Flush()
            {
                base.Flush();
                FileSystemFile.SetContent(ToString());
            }
        }

        private readonly FileSystemRoot Root;
    }
}

// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public static class FileSystem
    {
        public static readonly string DevNull = @"/dev/null";
        public static readonly string DevStdErr = @"/dev/stderr";
        public static readonly string DevStdIn = @"/dev/stdin";

        public static string CurrentPath()
        {
            return FileSystemProvider.GetCurrentDirectory();
        }

        public static string AppPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static bool FileExists(string fileName)
        {
            if (IsDevNull(fileName) || IsStdErr(fileName))
                return true;

            return FileSystemProvider.FileExists(fileName);
        }

        public static string GetParentPath(string fileName)
        {
            return FileSystemProvider.GetDirectoryName(fileName);
        }

        public static bool DirectoryExists(string directoryName)
        {
            return FileSystemProvider.DirectoryExists(directoryName);
        }

        public static IEnumerable<string> GetDirectoryFiles(string directoryName)
        {
            return FileSystemProvider.GetFiles(directoryName);
        }

        public static string GetFileName(string pathAndFileName)
        {
            return FileSystemProvider.GetFileName(pathAndFileName);
        }

        public static string GetDirectoryName(string path)
        {
            return FileSystemProvider.GetDirectoryName(path);
        }

        public static StreamReader GetStreamReader(string fileName)
        {
            if (IsDevNull(fileName))
                return StreamReader.Null;

            return FileSystemProvider.OpenText(fileName);
        }

        public static StreamReader GetStreamReaderFromString(string data)
        {
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            return new StreamReader(stream);
        }

        public static string GetStringFromFile(string fileName, long bytePosition, long byteLength)
        {
            using (var fileStream = FileSystemProvider.OpenRead(fileName))
            {
                fileStream.Position = bytePosition;
                var bytes = new byte[byteLength];
                var count = fileStream.Read(bytes, 0, (int)byteLength);
                if (count < byteLength)
                    throw new InvalidOperationException(String.Format("Error reading file {0}", fileName));

                return System.Text.Encoding.UTF8.GetString(bytes);
            }
        }

        public static void PutStringToFile(string fileName, string str)
        {
            FileSystemProvider.AppendAllText(fileName, str);
        }

        public static StreamReader GetStdInAsStreamReader()
        {
            string input = VirtualConsole.Input.ReadToEnd();
            return GetStreamReaderFromString(input);
        }

        // output_stream_t::initialize(...
        public static TextWriter OutputStreamInitialize(string outputFile, string pagerPath)
        {
            if (IsStdErr(outputFile))
                return VirtualConsole.Error;
            else if (!String.IsNullOrWhiteSpace(outputFile) && outputFile != "-")
                return FileSystemProvider.CreateText(outputFile);
            else if (!String.IsNullOrWhiteSpace(pagerPath))
                return VirtualPager.GetPager(pagerPath);
            else
                return VirtualConsole.Output;
        }

        // output_stream_t::close()
        public static TextWriter OutputStreamClose(TextWriter outputStream)
        {
            if (outputStream != VirtualConsole.Output && outputStream != VirtualConsole.Error)
            {
                outputStream.Close();
            }
            return VirtualConsole.Output;
        }

        public static string Combine(string fileName, string folderName)
        {
            return FileSystemProvider.PathCombine(folderName, fileName);
        }

        public static string ResolvePath(string pathName)
        {
            if (pathName.StartsWith("~"))
                pathName = ExpandPath(pathName);

            if (IsDevNull(pathName) || IsStdErr(pathName))
                return pathName;

            pathName = FileSystemProvider.GetFullPath(pathName);  // #fix-path-normalization - path temp.normalize(); - GetFullPath is not equal replacement
            return pathName;
        }

        public static string HomePath()
        {
            return FileSystemProvider.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        public static string HomePath(string fileName)
        {
            return FileSystemProvider.PathCombine(FileSystemProvider.GetFolderPath(Environment.SpecialFolder.UserProfile), fileName);
        }

        public static string ExpandPath(string pathName)
        {
            if (String.IsNullOrEmpty(pathName))
                return pathName;

            int pos = pathName.IndexOfAny(AnyDirectorySeparatorChar);
            string pfx = null;

            if (pathName.Length == 1 || pos == 1)
            {
                pfx = HomePath();
            }
            else
            {
                string user = pos == -1 ? pathName.Substring(1) : pathName.Substring(1, pos - 1);
                pfx = Path.Combine(FileSystemProvider.GetFolderPath(Environment.SpecialFolder.ApplicationData), user); 
            }

            if (String.IsNullOrEmpty(pfx))
                return pathName;

            if (pos == -1)
                return pfx;

            if (pfx.Length == 0 || !(pfx.EndsWith(DirectorySeparatorChar) || pfx.EndsWith(AltDirectorySeparatorChar)))
                pfx += Path.DirectorySeparatorChar;

            pfx += pathName.Substring(1);

            return pfx;
        }

        public static long FileSize(string fileName)
        {
            return FileSystemProvider.GetFileSize(fileName);
        }

        public static DateTime LastWriteTime(string fileName)
        {
            return FileSystemProvider.GetLastWriteTimeUtc(fileName);
        }

        public static bool IsDevNull(string fileName)
        {
            return String.Equals(fileName, DevNull, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsStdErr(string fileName)
        {
            return String.Equals(fileName, DevStdErr, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsStdIn(string fileName)
        {
            return String.Equals(fileName, DevStdIn, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// This method returns a path to an executable file by its name w/ or w/o extension.
        /// </summary>
        /// <param name="filePath">Executable file name (w/ or w/o extension and relative path)</param>
        /// <returns>Relative or absolute path to an executable file or null if it is not found</returns>
        public static string GetExecutablePath(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
                return null;

            if (FileExists(filePath))
                return filePath;

            var hasPath = Path.IsPathRooted(filePath);
            var fileNames = ComposePossibleExecutableFileNames(filePath);

            var foundFile = FindExistingFile(null, fileNames);
            if (!String.IsNullOrEmpty(foundFile))
                return foundFile;

            if (!hasPath)
            {
                var pathValues = VirtualEnvironment.GetEnvironmentVariable("PATH") ?? String.Empty;
                foreach (var path in pathValues.Split(';'))
                {
                    foreach (var fName in fileNames)
                    {
                        var fullPath = Path.Combine(path, fName);
                        if (File.Exists(fullPath))
                            return fullPath;
                    }
                }
            }

            return null;
        }

        private static string[] ExecutableExtensions = { ".BAT", ".CMD", ".COM", ".EXE" };

        public static IEnumerable<string> ComposePossibleExecutableFileNames(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                return Enumerable.Empty<string>();

            foreach (var ext in ExecutableExtensions)
                if (filePath.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
                    return new string[] { filePath };

            var result = new string[ExecutableExtensions.Length];
            for (int i = 0; i < ExecutableExtensions.Length; i++)
                result[i] = filePath + ExecutableExtensions[i];

            return result;
        }

        public static string FindExistingFile(string folder, IEnumerable<string> files)
        {
            if (files == null)
                throw new ArgumentNullException("files");

            if (!String.IsNullOrEmpty(folder))
                files = files.Select(s => Path.Combine(folder, s));

            return files.FirstOrDefault(s => FileExists(s));
        }

        private static IFileSystemProvider FileSystemProvider
        {
            get { return MainApplicationContext.Current.ApplicationServiceProvider.FileSystemProvider; }
        }

        private static readonly string DirectorySeparatorChar = new string(new char[] { Path.DirectorySeparatorChar });
        private static readonly string AltDirectorySeparatorChar = new string(new char[] { Path.AltDirectorySeparatorChar });
        private static readonly char[] AnyDirectorySeparatorChar = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

    }
}

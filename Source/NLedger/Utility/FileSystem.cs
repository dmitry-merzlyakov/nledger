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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public static class FileSystem
    {
        public static string DevNull = @"/dev/null";
        public static string DevStdErr = @"/dev/stderr";

        public static TextReader ConsoleInput
        {
            get { return _ConsoleInput ?? Console.In; }
        }

        public static TextWriter ConsoleOutput
        {
            get { return _ConsoleOutput ?? Console.Out; }
        }

        public static TextWriter ConsoleError
        {
            get { return _ConsoleError ?? Console.Error; }
        }

        public static string ReadLine(string prompt)
        {
            if (String.IsNullOrWhiteSpace(prompt))
                ConsoleOutput.WriteLine(prompt);
            return ConsoleInput.ReadLine();
        }

        public static string CurrentPath()
        {
            return Directory.GetCurrentDirectory();
        }

        public static bool FileExists(string fileName)
        {
            if (IsDevNull(fileName) || IsStdErr(fileName))
                return true;

            return File.Exists(fileName);
        }

        public static string GetParentPath(string fileName)
        {
            return Path.GetDirectoryName(fileName);
        }

        public static bool DirectoryExists(string directoryName)
        {
            return Directory.Exists(directoryName);
        }

        public static IEnumerable<string> GetDirectoryFiles(string directoryName)
        {
            return Directory.GetFiles(directoryName);
        }

        public static string GetFileName(string pathAndFileName)
        {
            return Path.GetFileName(pathAndFileName);
        }

        public static StreamReader GetStreamReader(string fileName)
        {
            if (IsDevNull(fileName))
                return StreamReader.Null;

            return File.OpenText(fileName);
        }

        public static StreamReader GetStreamReaderFromString(string data)
        {
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            return new StreamReader(stream);
        }

        public static string GetStringFromFile(string fileName, long bytePosition, long byteLength)
        {
            using (var fileStream = File.OpenRead(fileName))
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
            File.AppendAllText(fileName, str);
        }

        public static StreamReader GetStdInAsStreamReader()
        {
            string input = ConsoleInput.ReadToEnd();
            return GetStreamReaderFromString(input);
        }

        // output_stream_t::initialize(...
        public static TextWriter OutputStreamInitialize(string outputFile, string pagerPath)
        {
            if (IsStdErr(outputFile))
                return ConsoleError;
            else if (!String.IsNullOrWhiteSpace(outputFile) && outputFile != "-")
                return File.CreateText(outputFile);
            else if (!String.IsNullOrWhiteSpace(pagerPath))
                throw new NotImplementedException("stream.do_fork does not work on WIN32. Future enhancements...");
            else
                return ConsoleOutput;
        }

        public static string Combine(string fileName, string folderName)
        {
            return Path.Combine(folderName, fileName);
        }

        public static string ResolvePath(string pathName)
        {
            if (pathName.StartsWith("~"))
                pathName = ExpandPath(pathName);

            if (IsDevNull(pathName) || IsStdErr(pathName))
                return pathName;

            pathName = Path.GetFullPath(pathName);  // path temp.normalize(); - TODO check whether it is equalent replacement
            return pathName;
        }

        public static string HomePath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        public static string HomePath(string fileName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), fileName);
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
                pfx = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), user); 
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
            return new FileInfo(fileName).Length;
        }

        public static DateTime LastWriteTime(string fileName)
        {
            return File.GetLastWriteTimeUtc(fileName);
        }

        public static void SetConsoleInput(TextReader consoleInput)
        {
            _ConsoleInput = consoleInput;
        }

        public static void SetConsoleOutput(TextWriter consoleOutput)
        {
            _ConsoleOutput = consoleOutput;
        }

        public static void SetConsoleError(TextWriter consoleError)
        {
            _ConsoleError = consoleError;
        }

        public static bool IsAtty()
        {
            return MainApplicationContext.Current.IsAtty;
        }

        private static bool IsDevNull(string fileName)
        {
            return String.Equals(fileName, DevNull, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsStdErr(string fileName)
        {
            return String.Equals(fileName, DevStdErr, StringComparison.InvariantCultureIgnoreCase);
        }

        private static readonly string DirectorySeparatorChar = new string(new char[] { Path.DirectorySeparatorChar });
        private static readonly string AltDirectorySeparatorChar = new string(new char[] { Path.AltDirectorySeparatorChar });
        private static readonly char[] AnyDirectorySeparatorChar = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        private static TextReader _ConsoleInput = null;
        private static TextWriter _ConsoleOutput = null;
        private static TextWriter _ConsoleError = null;

    }
}

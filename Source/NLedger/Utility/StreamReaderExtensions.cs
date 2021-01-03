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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    /// <summary>
    /// See: https://stackoverflow.com/questions/829568/how-to-know-positionlinenumber-of-a-streamreader-in-a-textfile
    /// </summary>
    public static class StreamReaderExtensions
    {

#if NETFRAMEWORK
        readonly static FieldInfo charPosField = typeof(StreamReader).GetField("charPos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | BindingFlags.DeclaredOnly);
        readonly static FieldInfo byteLenField = typeof(StreamReader).GetField("byteLen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | BindingFlags.DeclaredOnly);
        readonly static FieldInfo charBufferField = typeof(StreamReader).GetField("charBuffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | BindingFlags.DeclaredOnly);
#else
        readonly static FieldInfo charPosField = typeof(StreamReader).GetField("_charPos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | BindingFlags.DeclaredOnly);
        readonly static FieldInfo byteLenField = typeof(StreamReader).GetField("_byteLen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | BindingFlags.DeclaredOnly);
        readonly static FieldInfo charBufferField = typeof(StreamReader).GetField("_charBuffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | BindingFlags.DeclaredOnly);
#endif

        public static long GetPosition(this StreamReader reader)
        {
            //shift position back from BaseStream.Position by the number of bytes read
            //into internal buffer.
            int byteLen = (int)byteLenField.GetValue(reader);
            var position = reader.BaseStream.Position - byteLen;

            //if we have consumed chars from the buffer we need to calculate how many
            //bytes they represent in the current encoding and add that to the position.
            int charPos = (int)charPosField.GetValue(reader);
            if (charPos > 0)
            {
                var charBuffer = (char[])charBufferField.GetValue(reader);
                var encoding = reader.CurrentEncoding;
                var bytesConsumed = encoding.GetBytes(charBuffer, 0, charPos).Length;
                position += bytesConsumed;
            }

            return position;
        }

        public static void SetPosition(this StreamReader reader, long position)
        {
            reader.DiscardBufferedData();
            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class FileInfo : BaseExport<Journals.JournalFileInfo>
    {
        public static implicit operator FileInfo(Journals.JournalFileInfo fileInfo) => new FileInfo(fileInfo);

        protected FileInfo(Journals.JournalFileInfo origin) : base(origin)
        { }

        public string filename => Origin.FileName;
        public long size => Origin.Size;
        public DateTime modtime => Origin.ModTime;
        public bool from_stream => Origin.FromStream;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public abstract class BaseExport<T> where T : class
    {
        protected BaseExport(T origin)
        {
            Origin = origin ?? throw new ArgumentNullException(nameof(origin));
        }

        public T Origin { get; }
    }

    public abstract class BaseStructExport<T> where T : struct
    {
        protected BaseStructExport(T origin)
        {
            Origin = origin;
        }

        public T Origin;
    }

}

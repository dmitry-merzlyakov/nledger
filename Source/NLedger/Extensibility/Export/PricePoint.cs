using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class PricePoint : BaseStructExport<Commodities.PricePoint>
    {
        public static implicit operator PricePoint(Commodities.PricePoint pricePoint) => new PricePoint(pricePoint);

        protected PricePoint(Commodities.PricePoint origin) : base(origin)
        { }

    }
}

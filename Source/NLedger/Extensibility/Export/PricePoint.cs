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
        public static implicit operator PricePoint(Commodities.PricePoint? pricePoint) => pricePoint.HasValue ? new PricePoint(pricePoint.Value) : null;

        protected PricePoint(Commodities.PricePoint origin) : base(origin)
        { }

    }
}

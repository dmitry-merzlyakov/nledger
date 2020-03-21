// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Utility;
using NLedger.Utility.Graph;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Commodities
{
    /// <summary>
    /// Porrted from commodity_history_t
    /// </summary>
    public class CommodityHistory : ICommodityHistory
    {
        public void AddCommodity(Commodity commodity)
        {
            if (!PriceGraph.HasVertex(commodity))
            {
                PriceGraph.AddVertex(commodity);
            }
        }

        public void AddPrice(Commodity source, DateTime when, Amount price)
        {
            if (source == price.Commodity)
                throw new InvalidOperationException("Source commodity should not be equal to price's commodity");

            PriceGraphEdge edge = PriceGraph.FindEdge(source, price.Commodity);
            if (edge == null)
            {
                edge = new PriceGraphEdge();
                PriceGraph.AddEdge(source, price.Commodity, edge);
            }

            edge.Prices[when] = price;
        }

        public void RemovePrice(Commodity source, Commodity target, DateTime date)
        {
            if (source == target)
                throw new InvalidOperationException("Source commodity cannot be equal to target commodity");

            PriceGraphEdge edge = PriceGraph.FindEdge(source, target);
            if (edge != null)
            {
                if (edge.Prices.ContainsKey(date))
                    edge.Prices.Remove(date);

                if (edge.Prices.Count == 0)
                    PriceGraph.RemoveEdge(source, target);
            }
        }

        public void MapPrices(Action<DateTime, Amount> fn, Commodity source, DateTime moment, DateTime oldest = default(DateTime), bool bidirectionally = false)
        {
            Logger.Current.Debug("history.map", () => String.Format("Mapping prices for source commodity: {0}", source));

            RecentEdgeWeight recentEdgeWeight = new RecentEdgeWeight(moment, oldest);
            IGraph<Commodity, PriceGraphEdge> filtered = PriceGraph.Filter((comm1, comm2, edge) => recentEdgeWeight.Filter(comm1, comm2, edge));

            foreach(EdgeDescriptor<Commodity,PriceGraphEdge> edgeDescriptor in filtered.AdjacentVertices(source))
            {
                foreach (KeyValuePair<DateTime, Amount> pricePair in edgeDescriptor.Edge.Prices)
                {
                    DateTime when = pricePair.Key;
                    Amount price = pricePair.Value;

                    Logger.Current.Debug("history.map", () => String.Format("Price {0} on {1}", price, when));

                    if ((oldest.IsNotADateTime() || when >= oldest) && when <= moment)
                    {
                        if (price.Commodity == source)
                        {
                            if (bidirectionally)
                            {
                                price = new Amount(price.GetInvertedQuantity(), edgeDescriptor.GetInvertedVertex(source));
                                Logger.Current.Debug("history.map", () => String.Format("Inverted price is {0}", price));
                                Logger.Current.Debug("history.map", () => String.Format("fn({0}, {1})", when, price));
                                fn(when, price);
                            }
                        }
                        else
                        {
                            Logger.Current.Debug("history.map", () => String.Format("fn({0}, {1})", when, price));
                            fn(when, price);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Ported from commodity_history_impl_t::find_price
        /// </summary>
        public PricePoint? FindPrice(Commodity source, DateTime moment, DateTime oldest = default(DateTime))
        {
            RecentEdgeWeight recentEdgeWeight = new RecentEdgeWeight(moment, oldest);
            IGraph<Commodity, PriceGraphEdge> filtered = PriceGraph.Filter((comm1, comm2, edge) => recentEdgeWeight.Filter(comm1, comm2, edge));

            Logger.Current.Debug("history.find", () => String.Format("sv commodity = {0}", source.Symbol));
            if (source.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_PRIMARY))
                Logger.Current.Debug("history.find", () => "sv commodity is primary");
            Logger.Current.Debug("history.find", () => "tv commodity = none ");

            DateTime mostRecent = moment;
            Amount price = null;

            foreach (EdgeDescriptor<Commodity, PriceGraphEdge> edgeDescriptor in filtered.AdjacentVertices(source))
            {
                Logger.Current.Debug("history.find", () => String.Format("u commodity = {0}", edgeDescriptor.Vertex1.Symbol));
                Logger.Current.Debug("history.find", () => String.Format("v commodity = {0}", edgeDescriptor.Vertex2.Symbol));

                PricePoint point = edgeDescriptor.Edge.PricePoint;
                if (price == null || point.When > mostRecent)
                {
                    mostRecent = point.When;
                    price = point.Price;
                }

                Logger.Current.Debug("history.find", () => String.Format("price was = {0}", price.Unrounded()));

                if (price.Commodity == source)
                    price = new Amount(price.GetInvertedQuantity(), edgeDescriptor.GetInvertedVertex(source));

                Logger.Current.Debug("history.find", () => String.Format("price is  = {0}", price.Unrounded()));
            }

            if (Amount.IsNullOrEmpty(price))
            {
                Logger.Current.Debug("history.find", () => "there is no final price");
                return null;
            }
            else
            {
                Logger.Current.Debug("history.find", () => String.Format("final price is = {0}", price.Unrounded()));
                return new PricePoint(mostRecent, price);
            }
        }

        /// <summary>
        /// Ported from commodity_history_impl_t::find_price
        /// </summary>
        public PricePoint? FindPrice(Commodity source, Commodity target, DateTime moment, DateTime oldest = default(DateTime))
        {
            if (source == target)
                throw new InvalidOperationException("Source commodity is equal to Target");

            RecentEdgeWeight recentEdgeWeight = new RecentEdgeWeight(moment, oldest);
            IGraph<Commodity, PriceGraphEdge> filtered = PriceGraph.Filter((comm1, comm2, edge) => recentEdgeWeight.Filter(comm1, comm2, edge));

            Logger.Current.Debug("history.find", () => String.Format("u commodity = {0}", source.Symbol));
            Logger.Current.Debug("history.find", () => String.Format("v commodity = {0}", target.Symbol));

            IEnumerable<EdgeDescriptor<Commodity, PriceGraphEdge>> shortestPath = filtered.FindShortestPath(source, target, (edge) => edge.Weight.Ticks);

            // Extract the shortest path and performance the calculations
            DateTime leastRecent = moment;
            Amount price = null;

            Commodity lastTarget = target;

            foreach (EdgeDescriptor<Commodity, PriceGraphEdge> edgeDescriptor in shortestPath.Reverse())
            {
                PricePoint point = edgeDescriptor.Edge.PricePoint;

                Commodity uComm = edgeDescriptor.Vertex1;
                Commodity vComm = edgeDescriptor.Vertex2;

                bool firstRun = false;
                if (Amount.IsNullOrEmpty(price))
                {
                    leastRecent = point.When;
                    firstRun = true;
                }
                else if (point.When < leastRecent)
                {
                    leastRecent = point.When;
                }

                Logger.Current.Debug("history.find", () => String.Format("u commodity = {0}", uComm.Symbol));
                Logger.Current.Debug("history.find", () => String.Format("v commodity = {0}", vComm.Symbol));
                Logger.Current.Debug("history.find", () => String.Format("last target = {0}", lastTarget.Symbol));

                // Determine which direction we are converting in
                Amount pprice = new Amount(point.Price);
                Logger.Current.Debug("history.find", () => String.Format("pprice    = {0}", pprice.Unrounded()));

                if (!firstRun)
                {
                    Logger.Current.Debug("history.find", () => String.Format("price was = {0}", price.Unrounded()));
                    if (pprice.Commodity != lastTarget)
                        price *= pprice.Inverted();
                    else
                        price *= pprice;
                }
                else if (pprice.Commodity != lastTarget)
                {
                    price = pprice.Inverted();
                }
                else
                {
                    price = pprice;
                }
                Logger.Current.Debug("history.find", () => String.Format("price is  = {0}", price.Unrounded()));

                if (lastTarget == vComm)
                    lastTarget = uComm;
                else
                    lastTarget = vComm;

                Logger.Current.Debug("history.find", () => String.Format("last target now = {0}", lastTarget.Symbol));
            }

            if (Amount.IsNullOrEmpty(price))
            {
                Logger.Current.Debug("history.find", () => "there is no final price");
                return null;
            }
            else
            {
                price = new Amount(price.Quantity, target);
                Logger.Current.Debug("history.find", () => String.Format("final price is = {0}", price.Unrounded()));
                return new PricePoint(leastRecent, price);
            }
        }

        /// <remarks>ported from print_map</remarks>
        public string PrintMap(DateTime moment = default(DateTime))
        {
            if (moment == default(DateTime))
            {
                return PriceGraph.WriteGraphViz(PriceGraphName, c => String.Format("[label=\"{0}\"]", c.Symbol), null, Commodity.DefaultComparer);
            }
            else
            {
                RecentEdgeWeight recentEdgeWeight = new RecentEdgeWeight(moment);
                IGraph<Commodity, PriceGraphEdge> fg = PriceGraph.Filter((comm1, comm2, edge) => recentEdgeWeight.Filter(comm1, comm2, edge));
                return fg.WriteGraphViz(PriceGraphName, c => String.Format("[label=\"{0}\"]", c.Symbol), null, Commodity.DefaultComparer);
            }
        }

        protected readonly IGraph<Commodity, PriceGraphEdge> PriceGraph = new AdjacencyList<Commodity, PriceGraphEdge>();
        private const string PriceGraphName = "G";
    }

    public class PriceGraphEdge
    {
        public PriceGraphEdge()
        {
            Prices = new SortedDictionary<DateTime, Amount>();
        }

        public IDictionary<DateTime, Amount> Prices { get; set; }
        public TimeSpan Weight { get; set; }
        public PricePoint PricePoint { get; set; }
    }

    /// <summary>
    /// Ported from recent_edge_weight
    /// </summary>
    public class RecentEdgeWeight
    {
        public RecentEdgeWeight(DateTime refTime, DateTime oldest = default(DateTime))
        {
            RefTime = refTime;
            Oldest = oldest;
        }

        public DateTime RefTime { get; private set; }
        public DateTime Oldest { get; private set; }

        /// <summary>
        /// Ported from bool operator()(const Edge& e) const
        /// </summary>
        public bool Filter(Commodity comm1, Commodity comm2, PriceGraphEdge edge)
        {
            Logger.Current.Debug("history.find", () => String.Format("  reftime      = {0}", RefTime));
            if (!Oldest.IsNotADateTime())
                Logger.Current.Debug("history.find", () => String.Format("  oldest       = {0}", Oldest));

            if (edge.Prices.Count == 0)
            {
                Logger.Current.Debug("history.find", () => "  prices map is empty for this edge");
                return false;
            }

            IEnumerable<DateTime> priceDates = edge.Prices.Keys; // [DM] The collection is ordered.
            if (priceDates.First() > RefTime)
            {
                Logger.Current.Debug("history.find", () => "  don't use this edge");
                return false;
            }
            else
            {
                DateTime low = priceDates.Last(pd => pd <= RefTime);
                if (low > RefTime)
                    throw new InvalidOperationException("assert(((*low).first <= reftime));");

                if (!Oldest.IsNotADateTime() && low < Oldest)
                {
                    Logger.Current.Debug("history.find", () => "  edge is out of range");
                    return false;
                }

                var secs = (RefTime - low).TotalSeconds;
                if (secs < 0)
                    throw new InvalidOperationException("assert(secs >= 0);");

                edge.Weight = RefTime - low;
                edge.PricePoint = new PricePoint(low, edge.Prices[low]);

                Logger.Current.Debug("history.find", () => String.Format("  using edge at price point {0} {1}", low, edge.Prices[low]));
                return true;
            }
        }
    }
}

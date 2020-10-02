// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Graph
{
    public class VertexDescriptor<V, E>
    {
        public VertexDescriptor(V vertex)
        {
            Vertex = vertex;
        }

        public V Vertex { get; private set; }

        public IEnumerable<EdgeDescriptor<V,E>> Edges
        {
            get { return EdgeItems == null ? EmptyEdges : EdgeItems.Values; }
        }

        public void AddEdge(EdgeDescriptor<V, E> edgeDesciptor)
        {
            if (EdgeItems == null)
                EdgeItems = new SortedDictionary<Tuple<V, V>, EdgeDescriptor<V, E>>();

            edgeDesciptor.GetInvertedVertex(Vertex); // Validate whether any of edge's vertices are equal to this vertex
            EdgeItems.Add(new Tuple<V, V>(edgeDesciptor.Vertex1, edgeDesciptor.Vertex2), edgeDesciptor);
        }

        public void RemoveEdge(EdgeDescriptor<V, E> edgeDesciptor)
        {
            if (EdgeItems != null)
                EdgeItems.Remove(new Tuple<V, V>(edgeDesciptor.Vertex1, edgeDesciptor.Vertex2));
        }

        public bool TryFindEdge(V vertex1, V vertex2, out EdgeDescriptor<V, E> edgeDesciptor)
        {
            if (EdgeItems != null)
                return EdgeItems.TryGetValue(new Tuple<V, V>(vertex1, vertex2), out edgeDesciptor);
            else
            {
                edgeDesciptor = default(EdgeDescriptor<V, E>);
                return false;
            }
        }

        private IDictionary<Tuple<V,V>, EdgeDescriptor<V, E>> EdgeItems;
        public static readonly IEnumerable<EdgeDescriptor<V, E>> EmptyEdges = Enumerable.Empty<EdgeDescriptor<V, E>>();
    }
}

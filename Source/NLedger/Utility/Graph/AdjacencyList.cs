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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Graph
{
    public class AdjacencyList<V,E> : IGraph<V,E>
    {
        public int NumVertices
        {
            get { return Vertices.Count; }
        }

        IEnumerable<VertexDescriptor<V, E>> IGraph<V, E>.Vertices
        {
            get { return Vertices.Values; }
        }

        public VertexDescriptor<V, E> FindVertexDescriptor(V vertex)
        {
            VertexDescriptor<V, E> vertexDescriptor1;

            if (!Vertices.TryGetValue(vertex, out vertexDescriptor1))
                throw new InvalidOperationException(String.Format("Could not find vertex '{0}'", vertex));

            return vertexDescriptor1;
        }

        public bool HasVertex(V vertex)
        {
            return Vertices.ContainsKey(vertex);
        }

        public void AddVertex(V vertex)
        {
            if (HasVertex(vertex))
                throw new InvalidOperationException(String.Format("Duplicated vertex '{0}'", vertex));

            Vertices.Add(vertex, new VertexDescriptor<V, E>(vertex));
        }

        public void RemoveVertex(V vertex)
        {
            VertexDescriptor<V, E> vertexDescriptor1;

            if (!Vertices.TryGetValue(vertex, out vertexDescriptor1))
                throw new InvalidOperationException(String.Format("Could not find vertex '{0}'", vertex));

            foreach(EdgeDescriptor<V,E> edgeDescriptor in vertexDescriptor1.Edges)
            {
                V vertex2 = edgeDescriptor.GetInvertedVertex(vertex);

                VertexDescriptor<V, E> vertexDescriptor2;
                if (!Vertices.TryGetValue(vertex2, out vertexDescriptor2))
                    throw new InvalidOperationException(String.Format("Could not find vertex '{0}'", vertex2));

                vertexDescriptor2.RemoveEdge(edgeDescriptor);
                Edges.Remove(edgeDescriptor);
            }

            Vertices.Remove(vertex);
        }

        public E FindEdge(V vertex1, V vertex2)
        {
            VertexDescriptor<V, E> vertexDescriptor1;

            if (!Vertices.TryGetValue(vertex1, out vertexDescriptor1))
                throw new InvalidOperationException(String.Format("Could not find vertex '{0}'", vertex1));

            EdgeDescriptor<V, E> edgeDesciptor;
            if (vertexDescriptor1.TryFindEdge(vertex1, vertex2, out edgeDesciptor))
                return edgeDesciptor.Edge;
            else
                return default(E);
        }

        public EdgeDescriptor<V, E> FindEdgeDescriptor(V vertex1, V vertex2)
        {
            VertexDescriptor<V, E> vertexDescriptor1;

            if (!Vertices.TryGetValue(vertex1, out vertexDescriptor1))
                throw new InvalidOperationException(String.Format("Could not find vertex '{0}'", vertex1));

            EdgeDescriptor<V, E> edgeDesciptor;
            vertexDescriptor1.TryFindEdge(vertex1, vertex2, out edgeDesciptor);
            return edgeDesciptor;
        }

        public void AddEdge(V vertex1, V vertex2, E edge)
        {
            VertexDescriptor<V, E> vertexDescriptor1, vertexDescriptor2;

            if (!Vertices.TryGetValue(vertex1, out vertexDescriptor1))
                throw new InvalidOperationException(String.Format("Could not find vertex '{0}'", vertex1));

            EdgeDescriptor<V, E> edgeDesciptor;
            if (vertexDescriptor1.TryFindEdge(vertex1, vertex2, out edgeDesciptor))
                throw new InvalidOperationException(String.Format("The edge for '{0}' and '{1}' is already exists", vertex1, vertex2));

            if (!Vertices.TryGetValue(vertex2, out vertexDescriptor2))
                throw new InvalidOperationException(String.Format("Could not find vertex '{0}'", vertex2));

            edgeDesciptor = new EdgeDescriptor<V, E>(vertex1, vertex2, edge);
            Edges.Add(edgeDesciptor);
            vertexDescriptor1.AddEdge(edgeDesciptor);
            vertexDescriptor2.AddEdge(edgeDesciptor);
        }

        public void RemoveEdge(V vertex1, V vertex2)
        {
            VertexDescriptor<V, E> vertexDescriptor1, vertexDescriptor2;

            if (!Vertices.TryGetValue(vertex1, out vertexDescriptor1))
                throw new InvalidOperationException(String.Format("Could not find vertex '{0}'", vertex1));

            EdgeDescriptor<V, E> edgeDesciptor;
            if (!vertexDescriptor1.TryFindEdge(vertex1, vertex2, out edgeDesciptor))
                throw new InvalidOperationException(String.Format("Could not find an edge for '{0}' and '{1}'", vertex1, vertex2));
            
            if (!Vertices.TryGetValue(vertex2, out vertexDescriptor2))
                throw new InvalidOperationException(String.Format("Could not find vertex '{0}'", vertex2));

            Edges.Remove(edgeDesciptor);
            vertexDescriptor1.RemoveEdge(edgeDesciptor);
            vertexDescriptor2.RemoveEdge(edgeDesciptor);
        }

        public IEnumerable<EdgeDescriptor<V, E>> AdjacentVertices(V vertex)
        {
            VertexDescriptor<V, E> vertexDescriptor;
            if (Vertices.TryGetValue(vertex, out vertexDescriptor))
                return vertexDescriptor.Edges;
            else
                return VertexDescriptor<V, E>.EmptyEdges;
        }

        public IGraph<V, E> Filter(Func<V, V, E, bool> filterEdge)
        {
            if (filterEdge == null)
                throw new ArgumentNullException("filterEdge");

            IEnumerable<EdgeDescriptor<V, E>> filteredEdges = Edges.Where(edge => filterEdge(edge.Vertex1, edge.Vertex2, edge.Edge)).ToList();
            return new ReadOnlyGraph<V, E>(filteredEdges);
        }

        private readonly IDictionary<V, VertexDescriptor<V, E>> Vertices = new SortedDictionary<V, VertexDescriptor<V, E>>();
        private readonly IList<EdgeDescriptor<V, E>> Edges = new List<EdgeDescriptor<V, E>>();
    }
}

// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Graph
{
    public class ReadOnlyGraph<V, E> : IGraph<V, E>
    {
        public ReadOnlyGraph(IEnumerable<EdgeDescriptor<V, E>> edges)
        {
            Edges = edges ?? Enumerable.Empty<EdgeDescriptor<V, E>>();
            Vertices = new SortedDictionary<V, VertexDescriptor<V,E>>();
            foreach (EdgeDescriptor<V, E> edgeDescriptor in Edges)
            {
                AddOrUpdateVertex(edgeDescriptor, edgeDescriptor.Vertex1);
                AddOrUpdateVertex(edgeDescriptor, edgeDescriptor.Vertex2);
            }
        }

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
            throw new InvalidOperationException("The graph is read only");
        }

        public void RemoveVertex(V vertex)
        {
            throw new InvalidOperationException("The graph is read only");
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
            throw new InvalidOperationException("The graph is read only");
        }

        public void RemoveEdge(V vertex1, V vertex2)
        {
            throw new InvalidOperationException("The graph is read only");
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
            throw new InvalidOperationException("The graph is read only");
        }

        private void AddOrUpdateVertex(EdgeDescriptor<V, E> edgeDescriptor, V v)
        {
            VertexDescriptor<V, E> vertexDescriptor;
            if (!Vertices.ContainsKey(v))
            {
                vertexDescriptor = new VertexDescriptor<V, E>(v);
                vertexDescriptor.AddEdge(edgeDescriptor);
                Vertices.Add(v, vertexDescriptor);
            }
            else
            {
                Vertices[v].AddEdge(edgeDescriptor);
            }
        }

        private readonly IEnumerable<EdgeDescriptor<V, E>> Edges;
        private readonly IDictionary<V, VertexDescriptor<V, E>> Vertices;
    }
}

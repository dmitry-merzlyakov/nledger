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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Graph
{
    public static class Extensions
    {
        public static IEnumerable<EdgeDescriptor<V,E>> FindShortestPath<V,E>(this IGraph<V,E> graph, V source, V target, Func<E,long> getWeight)
        {
            if (graph == null)
                throw new ArgumentNullException("graph");
            if (!graph.HasVertex(source))
                return Enumerable.Empty<EdgeDescriptor<V, E>>();
            if (!graph.HasVertex(target))
                return Enumerable.Empty<EdgeDescriptor<V, E>>();

            ShortestPathSearcher<V, E> searcher = new ShortestPathSearcher<V, E>(graph, source, target, getWeight)
            {
                PathWeightCalculation = PathWeightCalculationType.Cumulative
            };
            return searcher.FindShortestPath();
        }

        public static IEnumerable<VertexDescriptor<V, E>> GetOrderedVertices<V, E>(this IGraph<V, E> graph, IComparer<V> vertexComparer = null)
        {
            return vertexComparer == null ? graph.Vertices : graph.Vertices.OrderBy(d => d.Vertex, vertexComparer);
        }

        public static IEnumerable<EdgeDescriptor<V,E>> GetOrderedEdges<V,E>(this VertexDescriptor<V, E> vertexDescriptor, IComparer<V> vertexComparer = null)
        {
            var edges = vertexDescriptor.Edges.Where(d => d.IsVertex1(vertexDescriptor.Vertex));
            if (vertexComparer != null)
                edges = edges.OrderBy(d => d.GetInvertedVertex(vertexDescriptor.Vertex), vertexComparer);
            return edges;
        }

        public static IDictionary<VertexDescriptor<V, E>, int> GetVertexIndex<V, E>(this IGraph<V, E> graph, IComparer<V> vertexComparer = null)
        {
            int indexCounter = 0;
            IDictionary<VertexDescriptor<V,E>,int> vertexIndex = new Dictionary<VertexDescriptor<V,E>,int>();
            foreach (VertexDescriptor<V, E> vertex in graph.GetOrderedVertices(vertexComparer))
                vertexIndex.Add(vertex, indexCounter++);
            return vertexIndex;
        }

        public static string WriteGraphViz<V, E>(this IGraph<V, E> graph, string graphName, Func<V, string> vertexLabel, Func<E, string> edgeLabel, IComparer<V> vertexComparer = null)
        {
            IDictionary<VertexDescriptor<V,E>,int> vertexIndexes = graph.GetVertexIndex(vertexComparer);

            StringBuilder sbOutput = new StringBuilder();
            StringBuilder sbEdges = new StringBuilder();

            sbOutput.AppendFormat("graph {0} {{", graphName);
            sbOutput.AppendLine();
            foreach (VertexDescriptor<V,E> vertex in graph.GetOrderedVertices(vertexComparer))
            {
                int vertexIndex = vertexIndexes[vertex];
                foreach (EdgeDescriptor<V, E> edge in vertex.GetOrderedEdges(vertexComparer))
                {
                    VertexDescriptor<V, E> invertedVertex = graph.FindVertexDescriptor(edge.GetInvertedVertex(vertex.Vertex));
                    sbEdges.AppendFormat("{0}--{1}", vertexIndex, vertexIndexes[invertedVertex]);
                    if (edgeLabel != null)
                        sbEdges.Append(edgeLabel(edge.Edge));
                    sbEdges.AppendLine(" ;");
                }
                sbOutput.AppendFormat("{0}", vertexIndex);
                if (vertexLabel != null)
                    sbOutput.Append(vertexLabel(vertex.Vertex));
                sbOutput.AppendLine(";");
            }

            sbOutput.Append(sbEdges.ToString());
            sbOutput.AppendLine("}");

            return sbOutput.ToString();
        }
    }
}

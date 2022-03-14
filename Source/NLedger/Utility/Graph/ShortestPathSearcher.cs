// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Graph
{
    public enum PathWeightCalculationType
    {
        Aggregative,    // Count all weights in a path
        Cumulative      // Select biggest weight in a path
    }

    public class ShortestPathSearcher<V,E>
    {
        public ShortestPathSearcher(IGraph<V,E> graph, V source, V target, Func<E,long> getWeight)
        {
            if (graph == null)
                throw new ArgumentNullException("graph");
            if (!graph.HasVertex(source))
                throw new ArgumentException("source");
            if (!graph.HasVertex(target))
                throw new ArgumentException("target");
            if (DefaultVertexComparer.Equals(source, target))
                throw new ArgumentException("Source and Target are equal");

            Graph = graph;
            Source = source;
            Target = target;

            GetWeight = getWeight;
        }

        public IGraph<V, E> Graph { get; private set; }
        public V Source { get; private set; }
        public V Target { get; private set; }
        public Func<E, long> GetWeight { get; private set; }
        public PathWeightCalculationType PathWeightCalculation { get; set; }

        public IEnumerable<EdgeDescriptor<V,E>> FindShortestPath()
        {
            RootNode  = new PathNode<V>(Source);
            SucceedNodes = new List<PathNode<V>>();
            InProgressNodes = new List<PathNode<V>>() { RootNode };

            while (InProgressNodes.Any())
                InProgressNodes.ToList().ForEach(node => MakeStep(node));

            // [DM] Enumerable.OrderBy is a stable sort that preserve original positions for equal items
            PathNode<V> foundNode = SucceedNodes.OrderBy(n => n, PathNodesComparisonInstance).FirstOrDefault();

            List<EdgeDescriptor<V,E>> path = new List<EdgeDescriptor<V,E>>();
            while (foundNode != null)
            {
                if (foundNode.Parent != null)
                {
                    EdgeDescriptor<V,E> descriptor = foundNode.IsInverted 
                        ? Graph.FindEdgeDescriptor(foundNode.Vertex, foundNode.Parent.Vertex)
                        : Graph.FindEdgeDescriptor(foundNode.Parent.Vertex, foundNode.Vertex);
                    path.Insert(0, descriptor);
                }

                foundNode = foundNode.Parent;
            }
            return path;
        }

        private class PathNodesComparison : IComparer<PathNode<V>>
        {
            public int Compare(PathNode<V> x, PathNode<V> y)
            {
                return ComparePathNodes(x, y);
            }
        }
        private static PathNodesComparison PathNodesComparisonInstance = new PathNodesComparison();

        protected void MakeStep(PathNode<V> node)
        {
            IEnumerable<EdgeDescriptor<V, E>> edgeDescriptors = Graph.AdjacentVertices(node.Vertex);

            foreach (EdgeDescriptor<V, E> edgeDescriptor in edgeDescriptors)
            {
                V nextVertex = edgeDescriptor.GetInvertedVertex(node.Vertex);
                bool isInverted = !DefaultVertexComparer.Equals(edgeDescriptor.Vertex1, node.Vertex);

                if (node.VertexIsInPath(nextVertex))
                    continue;

                if (DefaultVertexComparer.Equals(nextVertex, Target))
                {
                    SucceedNodes.Add(new PathNode<V>(nextVertex, node, NodeSearchStatus.Success) 
                        {
                            IsInverted = isInverted,
                            Weight = CalctWeight(edgeDescriptor.Edge, node) 
                        });
                    node.Status = NodeSearchStatus.Intermediate;
                }
                else
                {
                    InProgressNodes.Add(new PathNode<V>(nextVertex, node) 
                        {
                            IsInverted = isInverted,
                            Weight = CalctWeight(edgeDescriptor.Edge, node) 
                        });
                    node.Status = NodeSearchStatus.Intermediate;
                }
            }

            if (node.Status != NodeSearchStatus.Intermediate)
                node.Status = NodeSearchStatus.Fault;

            InProgressNodes.Remove(node);
        }

        protected long CalctWeight(E edge, PathNode<V> parentNode)
        {
            long weight = GetWeight == null ? 0 : GetWeight(edge);
            return PathWeightCalculation == PathWeightCalculationType.Aggregative
                ? parentNode.Weight + weight
                : Math.Max(parentNode.Weight, weight);
        }

        protected static int ComparePathNodes(PathNode<V> x, PathNode<V> y)
        {
            return Math.Sign(x.Weight - y.Weight);
        }

        protected PathNode<V> RootNode { get; set; }
        protected List<PathNode<V>> SucceedNodes { get; set; }
        protected List<PathNode<V>> InProgressNodes { get; set; }

        public enum NodeSearchStatus
        {
            Unknown,
            Intermediate,
            Success,
            Fault
        }

        public class PathNode<V1>
        {
            public PathNode(V1 vertex)
            {
                Vertex = vertex;
            }

            public PathNode(V1 vertex, PathNode<V1> parent) : this(vertex)
            {
                Parent = parent;
            }

            public PathNode(V1 vertex, PathNode<V1> parent, NodeSearchStatus status) : this(vertex, parent)
            {
                Status = status;
            }

            public V1 Vertex { get; private set; }
            public PathNode<V1> Parent { get; private set; }
            public NodeSearchStatus Status { get; set; }
            public long Weight { get; set; }
            public bool IsInverted { get; set; }

            public bool VertexIsInPath(V1 nextVertex)
            {
                PathNode<V1> node = this;
                while (node != null)
                {
                    if (DefaultVertexComparer.Equals(nextVertex, node.Vertex))
                        return true;

                    node = node.Parent;
                }
                return false;
            }
            private static readonly EqualityComparer<V1> DefaultVertexComparer = EqualityComparer<V1>.Default;
        }

        private static readonly EqualityComparer<V> DefaultVertexComparer = EqualityComparer<V>.Default;
    }
}

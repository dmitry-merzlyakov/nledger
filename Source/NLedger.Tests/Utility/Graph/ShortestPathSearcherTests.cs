// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Utility.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Utility.Graph
{
    [TestClass]
    public class ShortestPathSearcherTests
    {
        [TestMethod]
        public void ShortestPathSearcher_Integration_DefaultAggregativeWeight()
        {
            string vertexA = "A", vertexB = "B", vertexC = "C", vertexD = "D";

            // Graph A-B, A-C, B-D, B-A
            AdjacencyList<string, int> graph = new AdjacencyList<string, int>();
            graph.AddVertex(vertexA);
            graph.AddVertex(vertexB);
            graph.AddVertex(vertexC);
            graph.AddVertex(vertexD);
            graph.AddEdge(vertexA, vertexB, 10);
            graph.AddEdge(vertexB, vertexA, 5);
            graph.AddEdge(vertexA, vertexC, 20);
            graph.AddEdge(vertexB, vertexD, 10);

            ShortestPathSearcher<string, int> searcher = new ShortestPathSearcher<string, int>(graph, vertexA, vertexD, (i) => i);
            IEnumerable<EdgeDescriptor<string, int>> path = searcher.FindShortestPath();

            Assert.IsNotNull(path);
            Assert.AreEqual(2, path.Count());
            EdgeDescriptor<string, int> firstDesc = path.First();
            Assert.AreEqual("B", firstDesc.Vertex1);
            Assert.AreEqual("A", firstDesc.Vertex2);
            Assert.AreEqual(5, firstDesc.Edge);
            EdgeDescriptor<string, int> lastDesc = path.Last();
            Assert.AreEqual("B", lastDesc.Vertex1);
            Assert.AreEqual("D", lastDesc.Vertex2);
            Assert.AreEqual(10, lastDesc.Edge);            
        }

        [TestMethod]
        public void ShortestPathSearcher_Integration_CumulativeWeight()
        {
            string vertexA = "A", vertexB = "B", vertexC = "C";

            // Graph A-B=2, A-C=3, B-C=4
            AdjacencyList<string, int> graph = new AdjacencyList<string, int>();
            graph.AddVertex(vertexA);
            graph.AddVertex(vertexB);
            graph.AddVertex(vertexC);
            graph.AddEdge(vertexA, vertexB, 2);
            graph.AddEdge(vertexA, vertexC, 3);
            graph.AddEdge(vertexB, vertexC, 4);

            ShortestPathSearcher<string, int> searcher = new ShortestPathSearcher<string, int>(graph, vertexB, vertexC, (i) => i)
            {
                PathWeightCalculation = PathWeightCalculationType.Cumulative
            };
            IEnumerable<EdgeDescriptor<string, int>> path = searcher.FindShortestPath();

            Assert.IsNotNull(path);
            Assert.AreEqual(2, path.Count());
            EdgeDescriptor<string, int> edge1 = path.First();
            Assert.AreEqual("A", edge1.Vertex1);
            Assert.AreEqual("B", edge1.Vertex2);
            Assert.AreEqual(2, edge1.Edge);
            EdgeDescriptor<string, int> edge2 = path.Last();
            Assert.AreEqual("A", edge2.Vertex1);
            Assert.AreEqual("C", edge2.Vertex2);
            Assert.AreEqual(3, edge2.Edge);
        }

    }
}

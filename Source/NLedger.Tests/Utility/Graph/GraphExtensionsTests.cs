// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
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
    public class GraphExtensionsTests
    {
        [TestMethod]
        public void GraphExtensions_GetOrderedVertices_ReturnsOrderedVertixDescriptors()
        {
            IGraph<string, string> graph = new AdjacencyList<string, string>();
            graph.AddVertex("BBB");
            graph.AddVertex("AAA");
            graph.AddVertex("ABA");
            graph.AddVertex("BAB");

            var vertices = graph.GetOrderedVertices(StringComparer.CurrentCulture);
            Assert.AreEqual("AAA", vertices.ElementAt(0).Vertex);
            Assert.AreEqual("ABA", vertices.ElementAt(1).Vertex);
            Assert.AreEqual("BAB", vertices.ElementAt(2).Vertex);
            Assert.AreEqual("BBB", vertices.ElementAt(3).Vertex);
        }

        [TestMethod]
        public void GraphExtensions_GetOrderedEdges_ReturnsOrderedEdgeDescriptors()
        {
            IGraph<string, string> graph = new AdjacencyList<string, string>();
            graph.AddVertex("BBB");
            graph.AddVertex("AAA");
            graph.AddVertex("ABA");
            graph.AddVertex("BAB");

            graph.AddEdge("ABA", "BBB", "aba-bbb");
            graph.AddEdge("ABA", "AAA", "aba-aaa");
            graph.AddEdge("ABA", "BAB", "aba-bab");
            // Those opposite edges will be ignored
            graph.AddEdge("AAA", "ABA", "aaa-aba");
            graph.AddEdge("BBB", "ABA", "bbb-aba");

            var edges = graph.FindVertexDescriptor("ABA").GetOrderedEdges(StringComparer.CurrentCulture);
            Assert.AreEqual(3, edges.Count());
            Assert.AreEqual("AAA", edges.ElementAt(0).Vertex2);
            Assert.AreEqual("BAB", edges.ElementAt(1).Vertex2);
            Assert.AreEqual("BBB", edges.ElementAt(2).Vertex2);
        }
    }
}

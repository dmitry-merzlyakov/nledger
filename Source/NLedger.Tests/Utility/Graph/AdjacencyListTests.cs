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
    public class AdjacencyListTests
    {
        [TestMethod]
        public void AdjacencyList_NumVertices_ReturnsNumberOfVertices()
        {
            int vertex1 = 10;
            int vertex2 = 20;

            AdjacencyList<int, string> adjacencyList = new AdjacencyList<int, string>();
            Assert.AreEqual(0, adjacencyList.NumVertices);

            adjacencyList.AddVertex(vertex1);
            Assert.AreEqual(1, adjacencyList.NumVertices);

            adjacencyList.AddVertex(vertex2);
            Assert.AreEqual(2, adjacencyList.NumVertices);

            adjacencyList.RemoveVertex(vertex2);
            Assert.AreEqual(1, adjacencyList.NumVertices);
        }

        [TestMethod]
        public void AdjacencyList_HasVertex_ChecksWhetherVertexExists()
        {
            int vertex1 = 10;
            int vertex2 = 20;

            AdjacencyList<int, string> adjacencyList = new AdjacencyList<int, string>();
            Assert.IsFalse(adjacencyList.HasVertex(vertex1));
            Assert.IsFalse(adjacencyList.HasVertex(vertex2));

            adjacencyList.AddVertex(vertex1);
            Assert.IsTrue(adjacencyList.HasVertex(vertex1));
            Assert.IsFalse(adjacencyList.HasVertex(vertex2));

            adjacencyList.AddVertex(vertex2);
            Assert.IsTrue(adjacencyList.HasVertex(vertex1));
            Assert.IsTrue(adjacencyList.HasVertex(vertex2));

            adjacencyList.RemoveVertex(vertex2);
            Assert.IsTrue(adjacencyList.HasVertex(vertex1));
            Assert.IsFalse(adjacencyList.HasVertex(vertex2));
        }

        [TestMethod]
        public void AdjacencyList_AddVertex_AddsANewUniqueVertex()
        {
            int vertex1 = 10;
            int vertex2 = 20;

            AdjacencyList<int, string> adjacencyList = new AdjacencyList<int, string>();
            adjacencyList.AddVertex(vertex1);
            adjacencyList.AddVertex(vertex2);
            Assert.AreEqual(2, adjacencyList.NumVertices);
            Assert.IsTrue(adjacencyList.HasVertex(vertex1));
            Assert.IsTrue(adjacencyList.HasVertex(vertex2));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Duplicated vertex '10'")]
        public void AdjacencyList_AddVertex_DeclinesAddingDuplicatedVertices()
        {
            int vertex1 = 10;
            AdjacencyList<int, string> adjacencyList = new AdjacencyList<int, string>();
            adjacencyList.AddVertex(vertex1);
            adjacencyList.AddVertex(vertex1);
        }

        [TestMethod]
        public void AdjacencyList_RemoveVertex_RemovesVerticesWithEdges()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            int vertex3 = 30;
            int vertex4 = 40;
            string edge1 = "20-30";
            string edge2 = "30-40";

            AdjacencyList<int, string> adjacencyList = new AdjacencyList<int, string>();
            adjacencyList.AddVertex(vertex1);
            adjacencyList.AddVertex(vertex2);
            adjacencyList.AddVertex(vertex3);
            adjacencyList.AddVertex(vertex4);
            adjacencyList.AddEdge(vertex2, vertex3, edge1);
            adjacencyList.AddEdge(vertex3, vertex4, edge2);

            Assert.AreEqual(4, adjacencyList.NumVertices);
            Assert.AreEqual(edge1, adjacencyList.FindEdge(vertex2, vertex3));
            Assert.AreEqual(edge2, adjacencyList.FindEdge(vertex3, vertex4));

            adjacencyList.RemoveVertex(vertex2);

            Assert.AreEqual(3, adjacencyList.NumVertices);
            Assert.AreEqual(edge2, adjacencyList.FindEdge(vertex3, vertex4));
        }

        [TestMethod]
        public void AdjacencyList_FindEdge_LooksForEdgesByVertices()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            int vertex3 = 30;
            int vertex4 = 40;
            string edge1 = "20-30";
            string edge2 = "30-40";

            AdjacencyList<int, string> adjacencyList = new AdjacencyList<int, string>();
            adjacencyList.AddVertex(vertex1);
            adjacencyList.AddVertex(vertex2);
            adjacencyList.AddVertex(vertex3);
            adjacencyList.AddVertex(vertex4);
            adjacencyList.AddEdge(vertex2, vertex3, edge1);
            adjacencyList.AddEdge(vertex3, vertex4, edge2);

            Assert.AreEqual(edge1, adjacencyList.FindEdge(vertex2, vertex3));
            Assert.AreEqual(edge2, adjacencyList.FindEdge(vertex3, vertex4));
        }

        [TestMethod]
        public void AdjacencyList_Filter_SelectesPartOfVertices()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            int vertex3 = 30;
            int vertex4 = 40;
            string edge1 = "20-30";
            string edge2 = "30-40";

            AdjacencyList<int, string> adjacencyList = new AdjacencyList<int, string>();
            adjacencyList.AddVertex(vertex1);
            adjacencyList.AddVertex(vertex2);
            adjacencyList.AddVertex(vertex3);
            adjacencyList.AddVertex(vertex4);
            adjacencyList.AddEdge(vertex2, vertex3, edge1);
            adjacencyList.AddEdge(vertex3, vertex4, edge2);

            IGraph<int, string> filtered = adjacencyList.Filter((v1, v2, ed) => v1 == 20);

            Assert.AreEqual(2, filtered.NumVertices);
            Assert.IsTrue(filtered.HasVertex(vertex2));
            Assert.IsTrue(filtered.HasVertex(vertex3));
            Assert.AreEqual(edge1, filtered.FindEdge(vertex2, vertex3));
        }

    }
}

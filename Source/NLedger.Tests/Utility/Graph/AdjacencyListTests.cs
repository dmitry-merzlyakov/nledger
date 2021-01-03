// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility.Graph
{
    public class AdjacencyListTests
    {
        [Fact]
        public void AdjacencyList_NumVertices_ReturnsNumberOfVertices()
        {
            int vertex1 = 10;
            int vertex2 = 20;

            AdjacencyList<int, string> adjacencyList = new AdjacencyList<int, string>();
            Assert.Equal(0, adjacencyList.NumVertices);

            adjacencyList.AddVertex(vertex1);
            Assert.Equal(1, adjacencyList.NumVertices);

            adjacencyList.AddVertex(vertex2);
            Assert.Equal(2, adjacencyList.NumVertices);

            adjacencyList.RemoveVertex(vertex2);
            Assert.Equal(1, adjacencyList.NumVertices);
        }

        [Fact]
        public void AdjacencyList_HasVertex_ChecksWhetherVertexExists()
        {
            int vertex1 = 10;
            int vertex2 = 20;

            AdjacencyList<int, string> adjacencyList = new AdjacencyList<int, string>();
            Assert.False(adjacencyList.HasVertex(vertex1));
            Assert.False(adjacencyList.HasVertex(vertex2));

            adjacencyList.AddVertex(vertex1);
            Assert.True(adjacencyList.HasVertex(vertex1));
            Assert.False(adjacencyList.HasVertex(vertex2));

            adjacencyList.AddVertex(vertex2);
            Assert.True(adjacencyList.HasVertex(vertex1));
            Assert.True(adjacencyList.HasVertex(vertex2));

            adjacencyList.RemoveVertex(vertex2);
            Assert.True(adjacencyList.HasVertex(vertex1));
            Assert.False(adjacencyList.HasVertex(vertex2));
        }

        [Fact]
        public void AdjacencyList_AddVertex_AddsANewUniqueVertex()
        {
            int vertex1 = 10;
            int vertex2 = 20;

            AdjacencyList<int, string> adjacencyList = new AdjacencyList<int, string>();
            adjacencyList.AddVertex(vertex1);
            adjacencyList.AddVertex(vertex2);
            Assert.Equal(2, adjacencyList.NumVertices);
            Assert.True(adjacencyList.HasVertex(vertex1));
            Assert.True(adjacencyList.HasVertex(vertex2));
        }

        [Fact]
        public void AdjacencyList_AddVertex_DeclinesAddingDuplicatedVertices()
        {
            int vertex1 = 10;
            AdjacencyList<int, string> adjacencyList = new AdjacencyList<int, string>();
            adjacencyList.AddVertex(vertex1);
            var exception = Assert.Throws<InvalidOperationException>(() => adjacencyList.AddVertex(vertex1));
            Assert.Equal("Duplicated vertex '10'", exception.Message);
        }

        [Fact]
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

            Assert.Equal(4, adjacencyList.NumVertices);
            Assert.Equal(edge1, adjacencyList.FindEdge(vertex2, vertex3));
            Assert.Equal(edge2, adjacencyList.FindEdge(vertex3, vertex4));

            adjacencyList.RemoveVertex(vertex2);

            Assert.Equal(3, adjacencyList.NumVertices);
            Assert.Equal(edge2, adjacencyList.FindEdge(vertex3, vertex4));
        }

        [Fact]
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

            Assert.Equal(edge1, adjacencyList.FindEdge(vertex2, vertex3));
            Assert.Equal(edge2, adjacencyList.FindEdge(vertex3, vertex4));
        }

        [Fact]
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

            Assert.Equal(2, filtered.NumVertices);
            Assert.True(filtered.HasVertex(vertex2));
            Assert.True(filtered.HasVertex(vertex3));
            Assert.Equal(edge1, filtered.FindEdge(vertex2, vertex3));
        }

    }
}

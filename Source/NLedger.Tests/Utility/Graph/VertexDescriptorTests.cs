// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
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
    public class VertexDescriptorTests
    {
        [Fact]
        public void VertexDescriptor_Constructor_PopulatesPropertries()
        {
            int vertex = 10;
            VertexDescriptor<int, string> vertexDescriptor = new VertexDescriptor<int, string>(vertex);
            Assert.Equal(vertex, vertexDescriptor.Vertex);
        }

        [Fact]
        public void VertexDescriptor_Edges_HaveEmptyListByDefault()
        {
            VertexDescriptor<int, string> vertexDescriptor = new VertexDescriptor<int, string>(10);
            Assert.Empty(vertexDescriptor.Edges);
        }

        [Fact]
        public void VertexDescriptor_AddEdge_AddsDescriptorToEdges()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            VertexDescriptor<int, string> vertexDescriptor = new VertexDescriptor<int, string>(vertex1);
            Assert.Empty(vertexDescriptor.Edges);
            EdgeDescriptor<int, string> edgeDesciptor = new EdgeDescriptor<int, string>(vertex1, vertex2, "edge");
            vertexDescriptor.AddEdge(edgeDesciptor);
            Assert.Contains(edgeDesciptor, vertexDescriptor.Edges);
        }

        [Fact]
        public void VertexDescriptor_AddEdge_FailsByAttemptToAddEdgeWithWrongVertices()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            int vertex3 = 30;

            VertexDescriptor<int, string> vertexDescriptor = new VertexDescriptor<int, string>(vertex3);
            EdgeDescriptor<int, string> edgeDesciptor = new EdgeDescriptor<int, string>(vertex1, vertex2, "edge");
            var exception = Assert.Throws<InvalidOperationException>(() => vertexDescriptor.AddEdge(edgeDesciptor));
            Assert.Equal("Vertex '30' does not belong to the current edge that contains '10' and '20'", exception.Message);
        }

        [Fact]
        public void VertexDescriptor_RemoveEdge_RemovesEdgeFromVertex()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            VertexDescriptor<int, string> vertexDescriptor = new VertexDescriptor<int, string>(vertex1);
            EdgeDescriptor<int, string> edgeDesciptor = new EdgeDescriptor<int, string>(vertex1, vertex2, "edge");
            vertexDescriptor.AddEdge(edgeDesciptor);

            Assert.Contains(edgeDesciptor, vertexDescriptor.Edges);

            vertexDescriptor.RemoveEdge(edgeDesciptor);
            Assert.DoesNotContain(edgeDesciptor, vertexDescriptor.Edges);
        }

        [Fact]
        public void VertexDescriptor_RemoveEdge_NoExceptionIfListIsEmpty()
        {
            VertexDescriptor<int, string> vertexDescriptor = new VertexDescriptor<int, string>(10);

            Assert.Empty(vertexDescriptor.Edges);

            EdgeDescriptor<int, string> edgeDesciptor = new EdgeDescriptor<int, string>(20, 30, "edge");
            vertexDescriptor.RemoveEdge(edgeDesciptor);

            Assert.Empty(vertexDescriptor.Edges);
        }

        [Fact]
        public void VertexDescriptor_RemoveEdge_FailsByAttemptToAddEdgeWithWrongVertices()
        {
            VertexDescriptor<int, string> vertexDescriptor = new VertexDescriptor<int, string>(10);
            EdgeDescriptor<int, string> edgeDesciptor = new EdgeDescriptor<int, string>(10, 20, "edge");
            vertexDescriptor.AddEdge(edgeDesciptor);
            Assert.Single(vertexDescriptor.Edges);

            EdgeDescriptor<int, string> unrelatedEdgeDesciptor = new EdgeDescriptor<int, string>(100, 200, "edge");
            var exception = Assert.Throws<InvalidOperationException>(() => vertexDescriptor.AddEdge(unrelatedEdgeDesciptor));
            Assert.Equal("Vertex '10' does not belong to the current edge that contains '100' and '200'", exception.Message);
        }

        [Fact]
        public void VertexDescriptor_TryFindEdge_ReturnsEdgeByVertex()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            VertexDescriptor<int, string> vertexDescriptor = new VertexDescriptor<int, string>(vertex1);
            EdgeDescriptor<int, string> edgeDesciptor = new EdgeDescriptor<int, string>(vertex1, vertex2, "edge");
            vertexDescriptor.AddEdge(edgeDesciptor);

            EdgeDescriptor<int, string> foundEdge;
            bool result = vertexDescriptor.TryFindEdge(vertex1, vertex2, out foundEdge);

            Assert.True(result);
            Assert.Equal(foundEdge, edgeDesciptor);
        }

        [Fact]
        public void VertexDescriptor_TryFindEdge_ReturnsFalseForWrongVertex()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            VertexDescriptor<int, string> vertexDescriptor = new VertexDescriptor<int, string>(vertex1);
            EdgeDescriptor<int, string> edgeDesciptor = new EdgeDescriptor<int, string>(vertex1, vertex2, "edge");
            vertexDescriptor.AddEdge(edgeDesciptor);

            EdgeDescriptor<int, string> foundEdge;
            bool result = vertexDescriptor.TryFindEdge(vertex2, vertex1, out foundEdge);

            Assert.False(result);
            Assert.Equal(default(EdgeDescriptor<int, string>), foundEdge);
        }

    }
}

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
    public class EdgeDescriptorTests
    {
        [Fact]
        public void EdgeDescriptor_Contructor_Populates_Properties()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            string edge = "edge";

            EdgeDescriptor<int, string> edgeDescriptor = new EdgeDescriptor<int, string>(vertex1, vertex2, edge);

            Assert.Equal(vertex1, edgeDescriptor.Vertex1);
            Assert.Equal(vertex2, edgeDescriptor.Vertex2);
            Assert.Equal(edge, edgeDescriptor.Edge);
        }

        [Fact]
        public void EdgeDescriptor_GetInvertedVertex_ReturnsOppositeVertex()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            string edge = "edge";

            EdgeDescriptor<int, string> edgeDescriptor = new EdgeDescriptor<int, string>(vertex1, vertex2, edge);

            Assert.Equal(vertex2, edgeDescriptor.GetInvertedVertex(vertex1));
            Assert.Equal(vertex1, edgeDescriptor.GetInvertedVertex(vertex2));
        }

        [Fact]
        public void EdgeDescriptor_GetInvertedVertex_RaisesExceptionForUnknownVertex()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            string edge = "edge";

            EdgeDescriptor<int, string> edgeDescriptor = new EdgeDescriptor<int, string>(vertex1, vertex2, edge);
            Assert.Throws<InvalidOperationException>(() => edgeDescriptor.GetInvertedVertex(0));
        }

        [Fact]
        public void EdgeDescriptor_IsVertex1_ReturnsTrueIfParameterIsVertex1()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            string edge = "edge";

            EdgeDescriptor<int, string> edgeDescriptor = new EdgeDescriptor<int, string>(vertex1, vertex2, edge);
            Assert.True(edgeDescriptor.IsVertex1(10));
            Assert.False(edgeDescriptor.IsVertex1(5));
        }

        [Fact]
        public void EdgeDescriptor_IsVertex2_ReturnsTrueIfParameterIsVertex1()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            string edge = "edge";

            EdgeDescriptor<int, string> edgeDescriptor = new EdgeDescriptor<int, string>(vertex1, vertex2, edge);
            Assert.True(edgeDescriptor.IsVertex2(20));
            Assert.False(edgeDescriptor.IsVertex2(5));
        }
    }
}

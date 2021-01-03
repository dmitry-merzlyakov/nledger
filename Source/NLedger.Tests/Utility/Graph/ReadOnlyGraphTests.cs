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
    public class ReadOnlyGraphTests
    {
        [Fact]
        public void ReadOnlyGraph_Coontructor_PopulatesEdgesAndVertices()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            int vertex3 = 30;
            string edge1 = "edge1";
            string edge2 = "edge2";

            EdgeDescriptor<int, string> edgeDescriptor1 = new EdgeDescriptor<int, string>(vertex1, vertex2, edge1);
            EdgeDescriptor<int, string> edgeDescriptor2 = new EdgeDescriptor<int, string>(vertex1, vertex3, edge2);

            IEnumerable<EdgeDescriptor<int, string>> edges = new List<EdgeDescriptor<int, string>>() { edgeDescriptor1, edgeDescriptor2 };
            ReadOnlyGraph<int, string> readOnlyGraph = new ReadOnlyGraph<int, string>(edges);

            Assert.Equal(3, readOnlyGraph.NumVertices);
            Assert.True(readOnlyGraph.HasVertex(vertex1));
            Assert.True(readOnlyGraph.HasVertex(vertex2));
            Assert.True(readOnlyGraph.HasVertex(vertex3));
            Assert.Equal(edge1, readOnlyGraph.FindEdge(vertex1, vertex2));
            Assert.Equal(edge2, readOnlyGraph.FindEdge(vertex1, vertex3));
        }
    }
}

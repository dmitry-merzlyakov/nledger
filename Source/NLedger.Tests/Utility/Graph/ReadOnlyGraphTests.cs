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
    public class ReadOnlyGraphTests
    {
        [TestMethod]
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

            Assert.AreEqual(3, readOnlyGraph.NumVertices);
            Assert.IsTrue(readOnlyGraph.HasVertex(vertex1));
            Assert.IsTrue(readOnlyGraph.HasVertex(vertex2));
            Assert.IsTrue(readOnlyGraph.HasVertex(vertex3));
            Assert.AreEqual(edge1, readOnlyGraph.FindEdge(vertex1, vertex2));
            Assert.AreEqual(edge2, readOnlyGraph.FindEdge(vertex1, vertex3));
        }
    }
}

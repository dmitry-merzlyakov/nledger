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
    public class EdgeDescriptorTests
    {
        [TestMethod]
        public void EdgeDescriptor_Contructor_Populates_Properties()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            string edge = "edge";

            EdgeDescriptor<int, string> edgeDescriptor = new EdgeDescriptor<int, string>(vertex1, vertex2, edge);

            Assert.AreEqual(vertex1, edgeDescriptor.Vertex1);
            Assert.AreEqual(vertex2, edgeDescriptor.Vertex2);
            Assert.AreEqual(edge, edgeDescriptor.Edge);
        }

        [TestMethod]
        public void EdgeDescriptor_GetInvertedVertex_ReturnsOppositeVertex()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            string edge = "edge";

            EdgeDescriptor<int, string> edgeDescriptor = new EdgeDescriptor<int, string>(vertex1, vertex2, edge);

            Assert.AreEqual(vertex2, edgeDescriptor.GetInvertedVertex(vertex1));
            Assert.AreEqual(vertex1, edgeDescriptor.GetInvertedVertex(vertex2));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EdgeDescriptor_GetInvertedVertex_RaisesExceptionForUnknownVertex()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            string edge = "edge";

            EdgeDescriptor<int, string> edgeDescriptor = new EdgeDescriptor<int, string>(vertex1, vertex2, edge);
            edgeDescriptor.GetInvertedVertex(0);
        }

        [TestMethod]
        public void EdgeDescriptor_IsVertex1_ReturnsTrueIfParameterIsVertex1()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            string edge = "edge";

            EdgeDescriptor<int, string> edgeDescriptor = new EdgeDescriptor<int, string>(vertex1, vertex2, edge);
            Assert.IsTrue(edgeDescriptor.IsVertex1(10));
            Assert.IsFalse(edgeDescriptor.IsVertex1(5));
        }

        [TestMethod]
        public void EdgeDescriptor_IsVertex2_ReturnsTrueIfParameterIsVertex1()
        {
            int vertex1 = 10;
            int vertex2 = 20;
            string edge = "edge";

            EdgeDescriptor<int, string> edgeDescriptor = new EdgeDescriptor<int, string>(vertex1, vertex2, edge);
            Assert.IsTrue(edgeDescriptor.IsVertex2(20));
            Assert.IsFalse(edgeDescriptor.IsVertex2(5));
        }
    }
}

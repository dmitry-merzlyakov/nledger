// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Graph
{
    public interface IGraph<V,E>
    {
        int NumVertices { get; }
        IEnumerable<VertexDescriptor<V, E>> Vertices { get; }
        VertexDescriptor<V, E> FindVertexDescriptor(V vertex);
        bool HasVertex(V vertex);
        void AddVertex(V vertex);
        void RemoveVertex(V vertex);
        E FindEdge(V vertex1, V vertex2);
        void AddEdge(V vertex1, V vertex2, E edge);
        void RemoveEdge(V vertex1, V vertex2);
        IGraph<V, E> Filter(Func<V, V, E, bool> filterEdge);
        EdgeDescriptor<V, E> FindEdgeDescriptor(V vertex1, V vertex2);
        IEnumerable<EdgeDescriptor<V, E>> AdjacentVertices(V vertex);
    }
}

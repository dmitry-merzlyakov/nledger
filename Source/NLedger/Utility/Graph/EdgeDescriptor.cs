// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Graph
{
    public class EdgeDescriptor<V, E>
    {
        public EdgeDescriptor(V vertex1, V vertex2, E edge)
        {
            Vertex1 = vertex1;
            Vertex2 = vertex2;
            Edge = edge;
        }

        public V Vertex1 { get; private set; }
        public V Vertex2 { get; private set; }
        public E Edge { get; private set; }

        public bool IsVertex1(V vertex)
        {
            return DefaultVertexComparer.Equals(vertex, Vertex1);
        }

        public bool IsVertex2(V vertex)
        {
            return DefaultVertexComparer.Equals(vertex, Vertex2);
        }

        public V GetInvertedVertex(V vertex)
        {
            if (IsVertex1(vertex))
                return Vertex2;
            else if (IsVertex2(vertex))
                return Vertex1;
            else
                throw new InvalidOperationException(String.Format("Vertex '{0}' does not belong to the current edge that contains '{1}' and '{2}'", vertex, Vertex1, Vertex2));
        }

        private static readonly EqualityComparer<V> DefaultVertexComparer = EqualityComparer<V>.Default;

    }
}

// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python.Platform
{
    /// <summary>
    ///  This class represents a logical connection session to Python
    /// </summary>
    public class PythonConnectionContext : IDisposable
    {
        public PythonConnectionContext(PythonConnector pythonConnector)
        {
            PythonConnector = pythonConnector ?? throw new ArgumentNullException(nameof(pythonConnector));
        }

        public PythonConnector PythonConnector { get; private set; }

        public void Dispose()
        {
            PythonConnector?.Disconnect(this);
            PythonConnector = null;
        }

        /// <summary>
        /// This method helps managing own Python-related objects with a context-bound life cycle (e.g. modules)
        /// PythonConnector guarantees that when this method is called, Python engine is initialized and ready for using
        /// </summary>
        /// <param name="isPlatformInitialization">Flag indicating that Python engine just has been initialized (happens once for AppDomain life cycle)</param>
        public virtual void OnConnected(bool isPlatformInitialization) { }

        /// <summary>
        /// This method helps managing own Python-related objects with a context-bound life cycle (e.g. modules)
        /// It is called when the current session is being removed from the connector's collection.
        /// </summary>
        /// <param name="isPlatformInitialization">Flag indicating that Python engine is going to be stopped (happens once for AppDomain life cycle)</param>
        public virtual void OnDisconnected(bool isPlatformDisposing) { }
    }
}

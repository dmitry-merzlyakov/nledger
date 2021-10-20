using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NLedger.Extensibility.Python.Platform
{
    public class PythonConnector
    {
        public static PythonConnector Current => _Current.Value;

        public static void Configure(IPythonConfigurationReader pythonConfigurationReader)
        {
            lock(SyncRoot)
            {
                if (_Current.IsValueCreated && Current.HasActiveConnections)
                    throw new InvalidOperationException("Python Connector has active connections");

                _Current = new Lazy<PythonConnector>(() => new PythonConnector(pythonConfigurationReader), true);
            }           
        }

        protected PythonConnector(IPythonConfigurationReader pythonConfigurationReader)
        {
            PythonConfigurationReader = pythonConfigurationReader ?? throw new ArgumentNullException(nameof(pythonConfigurationReader));
        }

        public IPythonConfigurationReader PythonConfigurationReader { get; }
        public PythonHost PythonHost { get; private set; }

        public bool IsAvailable => PythonHost != null || PythonConfigurationReader.IsAvailable;
        public bool HasActiveConnections => Connections.Any();
        public bool KeepAlive { get; set; } = true;

        public PythonConnectionContext Connect() => Connect(connector => new PythonConnectionContext(connector));

        public T Connect<T>(Func<PythonConnector, T> contextFactory) where T: PythonConnectionContext
        {
            if (contextFactory == null)
                throw new ArgumentNullException(nameof(contextFactory));

            lock(SyncRoot)
            {
                bool isPlatformInitialization = PythonHost == null;
                if (isPlatformInitialization)
                    PythonHost = new PythonHost(PythonConfigurationReader.Read());

                var context = contextFactory(this);
                context.OnConnected(isPlatformInitialization);

                Connections.Add(context);
                return context;
            }
        }

        public void Disconnect(PythonConnectionContext pythonConnectionContext)
        {
            lock(SyncRoot)
            {
                if (pythonConnectionContext == null || !Connections.Contains(pythonConnectionContext))
                    return;

                Connections.Remove(pythonConnectionContext);
                var isPlatformDisposing = !HasActiveConnections && !KeepAlive;
                pythonConnectionContext.OnDisconnected(isPlatformDisposing);

                if (isPlatformDisposing)
                {
                    PythonHost.Dispose();
                    PythonHost = null;
                }
            }
        }

        private readonly ISet<PythonConnectionContext> Connections = new HashSet<PythonConnectionContext>();
        private static readonly object SyncRoot = new object();
        private static Lazy<PythonConnector> _Current = new Lazy<PythonConnector>(() => 
            new PythonConnector(
                new EnvPythonConfigurationReader(
                new XmlFilePythonConfigurationReader())), 
            true);
    }
}

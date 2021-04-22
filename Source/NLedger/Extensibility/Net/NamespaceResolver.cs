using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NLedger.Extensibility.Net
{
    public class NamespaceResolver : INamespaceResolver
    {
        public NamespaceResolver(bool globalScan = false)
        {
            GlobalScan = globalScan;
        }

        public bool GlobalScan { get; private set; }

        public bool IsClass(string name)
        {
            return Data.ScannedClasses.ContainsKey(name);
        }

        public Type GetClassType(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Type type;
            if (Data.ScannedClasses.TryGetValue(name, out type))
                return type;
            else
                throw new ArgumentException($"Cannot find class '{name}'");
        }

        public bool IsNamespace(string name)
        {
            if (Data.ScannedNamespaces.Contains(name))
                return true;

            name = name + ".";
            return Data.ScannedNamespaces.Any(n => n.StartsWith(name));
        }

        public bool ContainsAssembly(Assembly assembly)
        {
            return Data.ScannedAssemblies.Contains(assembly);
        }

        public void AddAllAssemblies()
        {
            if (!GlobalScan)
            {
                lock(SyncRoot)
                {
                    if (!GlobalScan)
                    {
                        _AppDomainData = new AppDomainData(true);
                        GlobalScan = true;
                    }
                }
            }
        }

        public void AddAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if(!ContainsAssembly(assembly))
            {
                lock(SyncRoot)
                {
                    if (!ContainsAssembly(assembly))
                    {
                        _AppDomainData = new AppDomainData(Data, assembly);
                    }
                }
            }
        }

        private AppDomainData Data
        {
            get
            {
                if (_AppDomainData == null)
                {
                    lock (SyncRoot)
                    {
                        if (_AppDomainData == null)
                            _AppDomainData = new AppDomainData(GlobalScan);
                    }
                }
                return _AppDomainData;
            }
        }

        private class AppDomainData
        {
            public AppDomainData(bool globalScan)
            {
                ScannedAssemblies = new HashSet<Assembly>(globalScan ? AppDomain.CurrentDomain.GetAssemblies() : Enumerable.Empty<Assembly>());
                ScannedClasses = ScannedAssemblies.SelectMany(a => GetAssemblyTypes(a)).GroupBy(t => t.FullName).Where(g => g.Count() == 1).ToDictionary(t => t.Key, t => t.First());
                ScannedNamespaces = new HashSet<string>(ScannedClasses.Select(d => d.Value.Namespace).Distinct().Where(n => !String.IsNullOrEmpty(n)));
            }

            public AppDomainData(AppDomainData priorData, Assembly assembly)
            {
                if (priorData == null)
                    throw new ArgumentNullException(nameof(priorData));
                if (assembly == null)
                    throw new ArgumentNullException(nameof(assembly));

                var classes = GetAssemblyTypes(assembly).ToDictionary(t => t.FullName, t => t);
                var nspaces = classes.Select(d => d.Value.Namespace).Distinct().Where(n => !String.IsNullOrEmpty(n));

                ScannedAssemblies = new HashSet<Assembly>(priorData.ScannedAssemblies);
                ScannedAssemblies.Add(assembly);

                ScannedClasses = priorData.ScannedClasses.Concat(classes).ToDictionary(s => s.Key, s => s.Value);
                ScannedNamespaces = new HashSet<string>(priorData.ScannedNamespaces.Union(nspaces));
            }

            public ISet<Assembly> ScannedAssemblies { get; set; }
            public IDictionary<string, Type> ScannedClasses { get; set; }
            public ISet<string> ScannedNamespaces { get; set; }

            private IEnumerable<Type> GetAssemblyTypes(Assembly assembly)
            {
                try
                {
                    return assembly.GetTypes().Where(t => t.IsClass);
                }
                catch (ReflectionTypeLoadException)
                {
                    // The scanner should ignore any type loading exceptions (related artifacts are simply not available)
                    return Enumerable.Empty<Type>();
                }
            }
        }

        private AppDomainData _AppDomainData = null;
        private readonly object SyncRoot = new object();
    }
}

// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Textual;
using NLedger.Utility;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public class PathFunctor : BaseFunctor
    {
        /// <summary>
        /// Looks for a class name by given path (that includes a namespace, class name and a path to a class member)
        /// and creates a PathFunctor for it.
        /// </summary>
        public static PathFunctor ParsePath(string path, INamespaceResolver namespaceResolver, IValueConverter valueConverter)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));
            if (namespaceResolver == null)
                throw new ArgumentNullException(nameof(namespaceResolver));

            var className = "";
            var items = path.Split('.');
            for(int i=0; i<items.Length; i++)
            {
                className = String.IsNullOrEmpty(className) ? items[i] : className + '.' + items[i];
                if (namespaceResolver.IsClass(className))
                {
                    if (i == items.Length - 1)
                        throw new ParseError($"Path '{path}' contains only class name and cannot be evaluated to get result values");

                    string[] targetItems = new string[items.Length - i - 1];
                    Array.Copy(items, i + 1, targetItems, 0, targetItems.Length);

                    return new PathFunctor(namespaceResolver.GetClassType(className), targetItems, valueConverter);
                }
            }

            throw new ParseError($"Cannot find a class by path '{path}'");
        }

        public PathFunctor(Type classType, string[] path, IValueConverter valueConverter)
            : base(valueConverter)
        {
            if (path == null || !path.Any())
                throw new ArgumentNullException(nameof(path));

            ClassType = classType ?? throw new ArgumentNullException(nameof(classType));
            Path = path;
        }

        public Type ClassType { get; }
        public string[] Path { get; }

        public override Value ExprFunc(Scope scope)
        {
            object context = null;
            var type = ClassType;

            for (int i = 0; i < Path.Length - 1; i++)
            {
                context = ExtractValue(context, type, Path[i]);
                type = context?.GetType();
            }

            var leafName = Path[Path.Length - 1];

            var methods = type.GetMethods().Where(m => m.Name == leafName).ToArray();
            if (methods.Any())
                return new MethodFunctor(context, methods, ValueConverter).ExprFunctor(scope);

            var field = type.GetField(leafName);
            if (field != null)
                return new ValueFunctor(field.GetValue(context), ValueConverter).ExprFunctor(scope);

            var prop = type.GetProperty(leafName);
            if (prop != null)
                return new ValueFunctor(prop.GetValue(context), ValueConverter).ExprFunctor(scope);

            throw new ParseError($"Cannot evaluate path {String.Join(".", Path)}");
        }

        private object ExtractValue(object context, Type type, string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (type == null)
                return null;

            var method = type.GetMethods().Where(m => m.Name == name && m.GetParameters().Length == 0).FirstOrDefault();
            if (method != null)
                return method.Invoke(context, null);

            var field = type.GetField(name);
            if (field != null)
                return field.GetValue(context);

            var prop = type.GetProperty(name);
            if (prop != null)
                return prop.GetValue(context);

            throw new ParseError($"Cannot find an appropriate member with anme '{name}' iin class '{type}'");
        }
    }
}

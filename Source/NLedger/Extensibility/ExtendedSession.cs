// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility
{
    /// <summary>
    /// A basis for custom functional extensions (custom functions and commands, integration with other executables etc)
    /// </summary>
    /// <remarks>
    /// This class basically reflects Ledger's extensibility approach implemented as Python bridge (see python_session and python_interpreter_t).
    /// </remarks>
    public abstract class ExtendedSession : Session, IDisposable
    {
        public const string OptionImport = "import_";

        public static ExtendedSession CreateExtendedSession() => MainApplicationContext.Current?.ApplicationServiceProvider.ExtensionProvider.CreateExtendedSession();

        public static ExtendedSession Current => MainApplicationContext.Current?.ExtendedSession;

        public ExtendedSession()
        {
            CreateOptions();
            CreateLookupItems();
            MainApplicationContext.Current?.SetExtendedSession(this);
        }

        public Option ImportHandler { get; private set; }

        /// <summary>
        /// Depends on implementation. Indicates whether the custom extension requires initialization.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsInitialized();

        /// <summary>
        /// Depends on implementation. Forces initialization actions for a custom extension.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Depends on implementation. Specifies a names value in the global namespace of the custom extension.
        /// </summary>
        public abstract void DefineGlobal(string name, object value);

        /// <summary>
        /// Depends on implementation. Manages 'import' directives or options
        /// </summary>
        /// <param name="name"></param>
        public abstract void ImportOption(string name);

        /// <summary>
        /// Depends on implementation. Manages 'python' directives
        /// </summary>
        /// <param name="name"></param>
        public abstract void Eval(string code, ExtensionEvalModeEnum mode);

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            // Give our superclass first dibs on symbol definitions
            var op = LookupItems.Lookup(kind, name, this) ?? base.Lookup(kind, name);
            if (op != null)
                return op;

            if (IsInitialized())
            {
                if (kind == SymbolKindEnum.FUNCTION)
                    return LookupFunction(name);

                if (kind == SymbolKindEnum.OPTION)
                    return LookupFunction("option_" + name);
            }

            return null;
        }

        /// <summary>
        /// Empty implementation for "value_t python_command(call_scope_t& scope);"
        /// </summary>
        public virtual Value PythonCommand(CallScope scope)
        {
            return Value.Empty;
        }

        /// <summary>
        /// Empty implementation for "value_t server_command(call_scope_t& args);"
        /// </summary>
        public virtual Value ServerCommand(CallScope args)
        {
            return Value.Empty;
        }

        public virtual void Dispose()
        {
            MainApplicationContext.Current?.SetExtendedSession(null);
            StandaloneSessionThreadAcquirer?.Dispose();
        }

        /// <summary>
        /// Depends on implementation. Manages a lookup request for custom members (functions, values, imported modules).
        /// </summary>
        protected abstract ExprOp LookupFunction(string name);

        protected bool IsStandaloneSession => StandaloneSessionThreadAcquirer != null;

        /// <summary>
        /// Creates a "standalone" Ledger session. Standalone session provides a limited scope 
        /// where Ledger domain objects can function under the control of custom code (for example, in Python interpreter console).
        /// This model is the opposite of the usual use of Ledger when sessions are meant to process commands. 
        /// </summary>
        protected static T CreateStandaloneSession<T>(Func<T> sessionFactory, Func<MainApplicationContext> contextFactory = null) where T : ExtendedSession
        {
            if (MainApplicationContext.Current == null)
            {
                var context = contextFactory?.Invoke() ?? new MainApplicationContext();

                var threadAcquirer = context.AcquireCurrentThread();
                var extendedSession = sessionFactory?.Invoke() ?? throw new ArgumentNullException(nameof(sessionFactory));
                extendedSession.StandaloneSessionThreadAcquirer = threadAcquirer;

                context.SetExtendedSession(extendedSession);
                Session.SetSessionContext(extendedSession);
                Scope.DefaultScope = new Report(extendedSession);
            }

            return (T)Current;
        }

        private void CreateOptions()
        {
            ImportHandler = Options.Add(new Option(OptionImport, (o, w, s) => ImportOption(s)));
            Options.AddLookupOpt(OptionImport);
        }

        private void CreateLookupItems()
        {
            LookupItems.MakeFunctor("python", scope => PythonCommand((CallScope)scope), SymbolKindEnum.PRECOMMAND);
            LookupItems.MakeFunctor("server", scope => ServerCommand((CallScope)scope), SymbolKindEnum.PRECOMMAND);

            LookupItems.MakeOptionFunctors(Options);
            LookupItems.MakeOptionHandlers(Options);
        }

        private readonly OptionCollection Options = new OptionCollection();
        private readonly ExprOpCollection LookupItems = new ExprOpCollection();
        private IDisposable StandaloneSessionThreadAcquirer { get; set; }

    }
}

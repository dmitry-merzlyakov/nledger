// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Scopus
{
    using Utils;
    using NValue = NLedger.Values.Value;

    // Ported from #define DO() - virtual void handler_thunk(const optional<string>& whence)
    public delegate void HandlerThunkDelegate (Option sender, string whence);
    // Ported from #define DO_(var) - virtual void handler_thunk(const optional<string>& whence, const string& var)
    public delegate void HandlerThunkStrDelegate (Option sender, string whence, string str);

    public class Option
    {
        // operator bool() const...
        public static implicit operator bool (Option option)
        {
            return option != null && option.Handled;
        }

        // I more like this way rather than the implicit operator above... TBC...
        public static bool IsNotNullAndHandled(Option option)
        {
            return option != null && option.Handled;
        }

        /// <summary>
        /// Ported from: op_bool_tuple find_option(scope_t& scope, const string& name)
        /// </summary>
        public static Tuple<ExprOp,bool> FindOption(Scope scope, string name)
        {
            if (name != null && name.Length > 127)
                throw new OptionError(String.Format(OptionError.ErrorMessage_IllegalOption, name));

            name = name.Replace('-', '_') + '_';
            ExprOp exprOp = scope.Lookup(SymbolKindEnum.OPTION, name);
            if (exprOp != null)
                return new Tuple<ExprOp, bool>(exprOp, true);

            name = name.Remove(name.Length - 1);
            return new Tuple<ExprOp, bool>(scope.Lookup(SymbolKindEnum.OPTION, name), false);
        }

        public static void ProcessEnvironment(IDictionary<string, string> envp, string tag, Scope scope)
        {
            if (String.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException("tag");

            foreach(var keyValue in envp)
            {
                if (keyValue.Key.StartsWith(tag))
                {
                    string name = keyValue.Key.Remove(0, tag.Length).Replace('_', '-').ToLower();
                    if (!String.IsNullOrWhiteSpace(keyValue.Value))
                    {
                        try
                        {
                            ProcessOption("$" + name, name, scope, keyValue.Value, keyValue.Key);
                        }
                        catch(Exception ex)
                        {
                            throw new Exception(String.Format("While parsing environment variable option '{0}':'{1}'", keyValue.Key, keyValue.Value), ex);
                        }
                    }
                }
            }
        }

        public static bool ProcessOption(string whence, string name, Scope scope, string arg, string varname)
        {
            Tuple<ExprOp, bool> opt = FindOption(scope, name);
            if (opt.Item1 != null)
            {
                ProcessOption(whence, opt.Item1.AsFunction, scope, arg, varname);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Ported from void process_option(const string& whence, const expr_t::func_t& opt,
        /// </summary>
        public static void ProcessOption(string whence, ExprFunc opt, Scope scope, string arg, string name)
        {
            try
            {
                CallScope args = new CallScope(scope);

                args.PushBack(NValue.Get(whence));
                if (!String.IsNullOrEmpty(arg))
                    args.PushBack(NValue.Get(arg));

                opt(args);
            }
            catch (CountError)
            {
                throw; // DM - error_count is not std::exception and may pass by "catch" block
            }
            catch (Exception)
            {
                if (!String.IsNullOrEmpty(name) && name.StartsWith("-"))
                    throw new Exception(String.Format("While parsing option '{0}'", name));
                else
                    throw new Exception(String.Format("While parsing environent variable '{0}'", name));
            }
        }

        /// <summary>
        /// Ported from strings_list process_arguments
        /// </summary>
        public static IEnumerable<string> ProcessArguments(IEnumerable<string> args, Scope scope)
        {
            bool anywhere = true;
            IList<string> remaining = new List<string>();

            var argsEnum = args.GetEnumerator();
            while (argsEnum.MoveNext())
            {
                string arg = argsEnum.Current;
                Logger.Current.Debug("option.args", () => String.Format("Examining argument '{0}'", arg));

                if (!anywhere || !arg.StartsWith("-"))
                {
                    Logger.Current.Debug("option.args", () => "  adding to list of real args");
                    remaining.Add(arg);
                    continue;
                }

                // --long-option or -s
                if (arg.Length > 1 && arg[1] == '-')
                {
                    if (arg.Length == 2)
                    {
                        Logger.Current.Debug("option.args", () => "  it's a --, ending options processing");
                        anywhere = false;
                        continue;
                    }

                    Logger.Current.Debug("option.args", () => "  it's an option string");

                    string optName;
                    string name = arg.Substring(2);
                    string value = null;

                    int pos = name.IndexOf("=");
                    if (pos > 0)
                    {
                        optName = name.Substring(0, pos);
                        value = name.Substring(pos + 1);
                        Logger.Current.Debug("option.args", () => String.Format("  read option value from option: {0}", value));
                    }
                    else
                    {
                        optName = name;
                    }

                    Tuple<ExprOp,bool> opt = FindOption(scope, optName);
                    if (opt == null || opt.Item1 == null)
                        throw new OptionError(String.Format(OptionError.ErrorMessage_IllegalOption, name));

                    if (opt.Item2 && value == null && argsEnum.MoveNext())
                    {
                        value = argsEnum.Current;
                        Logger.Current.Debug("option.args", () => String.Format("  read option value from arg: {0}", value));
                        if (String.IsNullOrWhiteSpace(value))
                            throw new OptionError(String.Format(OptionError.ErrorMessage_MissingOptionArgumentFor, name));
                    }
                    ProcessOption("--" + name, opt.Item1.AsFunction, scope, value, "--" + name);
                }
                else if (arg.Length == 1)
                {
                    throw new OptionError(String.Format(OptionError.ErrorMessage_IllegalOption, arg));
                }
                else
                {
                    Logger.Current.Debug("option.args", () => "  single-char option");

                    List<Tuple<ExprOp, bool, char>> optionQueue = new List<Tuple<ExprOp, bool, char>>(); ;

                    int x = 1;
                    for (; x < arg.Length; x++)
                    {
                        char c = arg[x];

                        Tuple<ExprOp, bool> opt = FindOption(scope, c.ToString());
                        if (opt == null || opt.Item1 == null)
                            throw new OptionError(String.Format(OptionError.ErrorMessage_IllegalOption, c.ToString()));

                        optionQueue.Add(new Tuple<ExprOp, bool, char>(opt.Item1, opt.Item2, c));
                    }

                    foreach(Tuple<ExprOp, bool, char> o in optionQueue)
                    {
                        string value = null;
                        if (o.Item2 && argsEnum.MoveNext())
                        {
                            value = argsEnum.Current;
                            Logger.Current.Debug("option.args", () => String.Format("  read option value from arg: {0}", value));
                            if (String.IsNullOrWhiteSpace(value))
                                throw new OptionError(String.Format(OptionError.ErrorMessage_MissingOptionArgumentFor, o.Item3));
                        }
                        ProcessOption("-" + o.Item3, o.Item1.AsFunction, scope, value, "-" + o.Item3);
                    }
                }
            }
            return remaining;
        }

        public Option(string name, char ch = default(char))
        {
            Name = name;
            Ch = ch;

            WantsArg = !String.IsNullOrEmpty(Name) && Name.EndsWith("_");

            Logger.Current.Debug("option.names", () => String.Format("Option: {0}", name));
        }

        public Option(string name, HandlerThunkDelegate onHandlerThunk, char ch = default(char))
            : this (name, ch)
        {
            OnHandlerThunk = onHandlerThunk;
        }

        public Option(string name, HandlerThunkStrDelegate onHandlerThunk, char ch = default(char))
            : this(name, ch)
        {
            OnHandlerThunkStr = onHandlerThunk;
        }

        public string Name { get; private set; }
        public string Value { get; set; }
        public object Parent { get; set; }

        public bool Handled { get; private set; }
        public bool WantsArg { get; private set; }

        public HandlerThunkDelegate OnHandlerThunk { get; private set; }
        public HandlerThunkStrDelegate OnHandlerThunkStr { get; private set; }

        public string Desc
        {
            get 
            {
                string name = ((WantsArg ? Name.Remove(Name.Length - 1) : Name) ?? String.Empty).Replace('_', '-');
                string suffix = Ch == default(char) ? String.Empty : String.Format(" (-{0})", Ch);
                return name + suffix;  
            }
        }

        public string Report()
        {
            if (Handled && !String.IsNullOrEmpty(Source))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0,24}", Desc);
                if (WantsArg)
                    sb.AppendFormat(" = {0,-42}", Value);
                else
                    sb.AppendFormat("{0,45}", " ");
                sb.AppendFormat("{0}", Source);
                sb.AppendLine();
                return sb.ToString();
            }
            else
            {
                return String.Empty;
            }
        }

        public string Str()
        {
            if (!Handled)
                throw new InvalidOperationException("Not handled yet");
            if (String.IsNullOrEmpty(Value))
                throw new InvalidOperationException(String.Format("No argument provided for {0}", Desc));
            return Value;
        }

        public void On(string whence)
        {
            HandlerThunk(whence);
            Handled = true;
            Source = whence;
        }

        public void On(string whence, string str)
        {
            string before = Value;
            HandlerThunkStr(whence, str);
            if (Value == before)
                Value = str;
            Handled = true;
            Source = whence;
        }

        public void Off()
        {
            Handled = false;
            Value = String.Empty;
            Source = null;
        }

        public virtual void HandlerThunk(string whence)
        {
            if (OnHandlerThunk != null)
                OnHandlerThunk(this, whence);
        }

        public virtual void HandlerThunkStr(string whence, string str)
        {
            if (OnHandlerThunkStr != null)
                OnHandlerThunkStr(this, whence, str); 
        }

        public Value Handler(CallScope args)
        {
            if (WantsArg)
            {
                if (args.Size < 2)
                    throw new InvalidOperationException(String.Format("No argument provided for {0}", Desc));
                if (args.Size > 2)
                    throw new InvalidOperationException(String.Format("To many arguments provided for {0}", Desc));
                if (args[0].Type != ValueTypeEnum.String)
                    throw new InvalidOperationException(String.Format("Context argument for {0} not a string", Desc));
                On(args[0].AsString, args[1].AsString);
            }
            else
            {
                if (args.Size < 1)
                        throw new InvalidOperationException(String.Format("No argument provided for {0}", Desc));
                if (args[0].Type != ValueTypeEnum.String)
                        throw new InvalidOperationException(String.Format("Context argument for {0} not a string", Desc));
                On(args[0].AsString);
            }
            return NValue.Get(true);
        }

        // operator()
        public Value Call(CallScope args)
        {
            if (!args.IsEmpty)
            {
                args.PushFront(NValue.StringValue("?expr"));
                return Handler(args);
            }

            if (WantsArg)
                return NValue.StringValue(Value);
            else
                return NValue.Get(Handled);
        }

        protected char Ch { get; private set; }
        protected string Source { get; private set; }
    }
}

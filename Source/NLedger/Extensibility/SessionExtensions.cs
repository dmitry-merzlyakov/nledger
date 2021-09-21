using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility
{
    /// <summary>
    /// Session extensions for integration purposes
    /// </summary>
    public static class SessionExtensions
    {
        public class CommandExecutionResult
        {
            public static readonly CommandExecutionResult Empty = new CommandExecutionResult(null, null);

            public static CommandExecutionResult Success(string output) => new CommandExecutionResult(output, null);
            public static CommandExecutionResult Failure(string error) => new CommandExecutionResult(null, error);

            private CommandExecutionResult(string output, string error)
            {
                Output = output ?? String.Empty;
                Error = error ?? String.Empty;
            }

            public string Output { get; }
            public string Error { get; }
        }

        /// <summary>
        /// Ignored options: OutputHandler, PagerHandler, OptionsHandler
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static CommandExecutionResult ExecuteCommand(this Session session, string arg, bool readJournalFiles = false)
        {
            return ExecuteCommand(session, new string[] { arg }, readJournalFiles);
        }

        /// <summary>
        /// Ignored options: OutputHandler, PagerHandler, OptionsHandler
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static CommandExecutionResult ExecuteCommand(this Session session, IEnumerable<string> args, bool readJournalFiles = false)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            var currentReport = Scope.DefaultScope as Report ?? throw new InvalidOperationException("Global scope has not been initialized with a report object");

            var scopeReport = new Report(currentReport);
            Scope.DefaultScope = scopeReport;

            try
            {
                // Process input arguments
                args = Option.ProcessArguments(args, scopeReport);
                if (!args.Any())
                    return CommandExecutionResult.Empty;

                // Detect verb
                string verb = args.First();
                args = args.Skip(1);

                // Command execution scope
                var boundScope = new BindScope(session, scopeReport);

                // LookForPrecommand
                ExprFunc command = boundScope.Lookup(SymbolKindEnum.PRECOMMAND, verb)?.AsFunction ?? Expr.EmptyFunc;

                // if it is not a pre-command...
                if (command.IsNullOrEmpty())
                {
                    if (readJournalFiles)
                        session.ReadJournalFiles();

                    scopeReport.NormalizeOptions(verb);

                    // LookForCommand
                    command = boundScope.Lookup(SymbolKindEnum.COMMAND, verb)?.AsFunction ?? Expr.EmptyFunc;
                    if (command.IsNullOrEmpty())
                        throw new LogicError(String.Format(LogicError.ErrorMessageUnrecognizedCommand, verb));
                }

                // Specify isolated output stream
                using (scopeReport.OutputStream = new StringWriter())
                {
                    // Compose command args
                    CallScope commandArgs = new CallScope(boundScope);
                    foreach (string arg in args)
                        commandArgs.PushBack(Values.Value.Get(arg));

                    // Execute command
                    command(commandArgs);

                    // Command execution results
                    return CommandExecutionResult.Success(scopeReport.OutputStream.ToString());
                }
            }
            catch (Exception ex)
            {
                // Log errors
                var sb = new StringBuilder();

                string context = ErrorContext.Current.GetContext();
                if (!String.IsNullOrWhiteSpace(context))
                    sb.AppendLine(context);

                sb.AppendLine(String.Format("Error: {0}", ex.Message));

                return CommandExecutionResult.Failure(sb.ToString());
            }

            finally
            {
                scopeReport.QuickClose();
                Scope.DefaultScope = currentReport;
            }
        }
    }
}

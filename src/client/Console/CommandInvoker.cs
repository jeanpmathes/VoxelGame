// <copyright file="CommandInvoker.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.Client.Console
{
    /// <summary>
    ///     Discovers and executes commands using the <see cref="Command" /> class.
    /// </summary>
    public class CommandInvoker
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<CommandInvoker>();

        private readonly Dictionary<string, CommandGroup> commandGroups = new();
        private readonly Dictionary<Type, Parser> parsers = new();

        /// <summary>
        ///     Get the names of all registered commands.
        /// </summary>
        public IEnumerable<string> CommandNames => commandGroups.Keys;

        /// <summary>
        ///     Invoked when new commands are added or discovered.
        /// </summary>
        public event EventHandler CommandsUpdated = delegate {};

        /// <summary>
        ///     Get the help text for a command.
        /// </summary>
        /// <param name="commandName">The name of the command. Must correspond to a discovered command.</param>
        /// <returns>The help text.</returns>
        public string GetCommandHelpText(string commandName)
        {
            return commandGroups.TryGetValue(commandName, out var commandGroup)
                ? commandGroup.Command.HelpText
                : throw new ArgumentException("Command not found.");
        }

        /// <summary>
        ///     Get all signatures for a command.
        /// </summary>
        /// <param name="commandName">The name of the command. Must correspond to a discovered command.</param>
        /// <returns>All signatures for the command.</returns>
        public IEnumerable<string> GetCommandSignatures(string commandName)
        {
            if (!commandGroups.TryGetValue(commandName, out CommandGroup? commandGroup))
                throw new ArgumentException("Command not found.");

            return GetCommandSignatures(commandName, commandGroup);
        }

        private static IEnumerable<string> GetCommandSignatures(string commandName, CommandGroup commandGroup)
        {
            foreach (MethodInfo commandOverload in commandGroup.Overloads)
            {
                StringBuilder signature = new();
                signature.Append(commandName);

                foreach (ParameterInfo parameter in commandOverload.GetParameters())
                {
                    signature.Append(value: ' ');

                    signature.Append(value: '<');
                    signature.Append(parameter.Name);
                    signature.Append(" : ");
                    signature.Append(parameter.ParameterType.Name);
                    signature.Append(value: '>');
                }

                yield return signature.ToString();
            }
        }

        /// <summary>
        ///     Add a parser to parse arguments.
        /// </summary>
        /// <param name="parser">The parser to add. Will replace any existing parser for the same type.</param>
        public void AddParser(Parser parser)
        {
            parsers[parser.ParsedType] = parser;
        }

        /// <summary>
        ///     Search and discover all commands in the calling assembly.
        /// </summary>
        public void SearchCommands()
        {
            logger.LogDebug(Events.Console, "Searching commands");

            var count = 0;

            foreach (Type type in Assembly.GetCallingAssembly().GetTypes()
                         .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Command))))
            {
                ICommand? command = null;

                try
                {
                    command = (ICommand?) Activator.CreateInstance(type);
                }
                catch (Exception e) when (e is MethodAccessException or MemberAccessException)
                {
                    // Commands that have no public constructor are ignored but can be added manually.
                }

                if (command == null) continue;

                List<MethodInfo> overloads = GetOverloads(type);

                commandGroups[command.Name] = new CommandGroup(command, overloads);
                logger.LogDebug(Events.Console, "Found command '{Name}'", command.Name);
                count++;
            }

            logger.LogInformation(Events.Console, "Found {Count} commands", count);
            CommandsUpdated.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Add a command to the list of available commands for this invoker.
        /// </summary>
        /// <param name="command">The command to add.</param>
        public void AddCommand(ICommand command)
        {
            List<MethodInfo> overloads = GetOverloads(command.GetType());
            commandGroups[command.Name] = new CommandGroup(command, overloads);

            logger.LogDebug(Events.Console, "Added command '{Name}'", command.Name);
            CommandsUpdated.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Try to invoke a command using console input. In case of failure, messages are written to the console.
        /// </summary>
        /// <param name="input">The console input.</param>
        /// <param name="context">The command context in which the command should be executed.</param>
        public void InvokeCommand(string input, CommandContext context)
        {
            (string commandName, string[] args) = ParseInput(input);

            if (commandGroups.TryGetValue(commandName, out CommandGroup? commandGroup))
            {
                MethodInfo? method = ResolveOverload(commandGroup.Overloads, args);

                if (method != null) Invoke(commandGroup.Command, method, args, context);
                else
                {
                    context.Console.WriteError($"No overload found, use 'help {commandName}' for more info.");
                    logger.LogWarning(Events.Console, "No overload found for command '{Command}'", commandName);
                }
            }
            else
            {
                context.Console.WriteError($"No command '{commandName}' found, use 'help' for more info.");
                logger.LogWarning(Events.Console, "Command '{Command}' not found", commandName);
            }
        }

        private static (string commandName, string[] args) ParseInput(string input)
        {
            StringBuilder commandName = new();

            foreach (char c in input)
            {
                if (c == ' ') break;
                commandName.Append(c);
            }

            List<StringBuilder> args = new();

            var isNextArg = true;
            var isQuoted = false;
            var isEscaped = false;

            int nextIndex = commandName.Length + 1;
            string remaining = input.Length > nextIndex ? input[nextIndex..] : "";

            foreach (char c in remaining)
                switch (c)
                {
                    case ' ' when !isQuoted:
                        isNextArg = true;

                        break;
                    case '"' when !isEscaped:
                        isQuoted = !isQuoted;

                        break;
                    case '\\':
                        isEscaped = !isEscaped;

                        break;
                    default:
                        if (isNextArg)
                        {
                            args.Add(new StringBuilder());
                            isNextArg = false;
                        }

                        args[^1].Append(c);

                        break;
                }

            return (commandName.ToString(), args.Select(a => a.ToString()).ToArray());
        }

        private MethodInfo? ResolveOverload(List<MethodInfo> overloads, string[] args)
        {
            foreach (MethodInfo method in overloads)
            {
                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length != args.Length) continue;

                var isValid = true;

                for (var i = 0; i < parameters.Length; i++)
                {
                    if (!parsers.TryGetValue(parameters[i].ParameterType, out Parser? parser))
                    {
                        isValid = false;

                        break;
                    }

                    if (!parser.CanParse(args[i]))
                    {
                        isValid = false;

                        break;
                    }
                }

                if (isValid) return method;
            }

            return null;
        }

        private void Invoke(ICommand command, MethodBase method, IReadOnlyList<string> args, CommandContext context)
        {
            ParameterInfo[] parameters = method.GetParameters();

            try
            {
                var parsedArgs = new object[args.Count];

                for (var i = 0; i < args.Count; i++)
                    parsedArgs[i] = parsers[parameters[i].ParameterType].Parse(args[i]);

                command.SetContext(context);
                method.Invoke(command, parsedArgs);

                logger.LogDebug(Events.Console, "Called command '{Command}'", command.Name);
            }
            catch (TargetInvocationException e)
            {
                logger.LogError(
                    Events.Console,
                    e.InnerException,
                    "Error while executing command '{Command}'",
                    method.Name);
            }
        }

        private static List<MethodInfo> GetOverloads(Type type)
        {
            return type.GetMethods()
                .Where(m => m.Name.Equals("Invoke", StringComparison.InvariantCulture) && !m.IsStatic).ToList();
        }

        private sealed record CommandGroup(ICommand Command, List<MethodInfo> Overloads);
    }
}

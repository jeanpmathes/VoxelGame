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
    public class CommandInvoker
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<CommandInvoker>();

        private readonly Dictionary<string, CommandGroup> commandGroups = new();
        private readonly Dictionary<Type, Parser> parsers = new();

        public void AddParser(Parser parser)
        {
            parsers[parser.ParsedType] = parser;
        }

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
                catch (MethodAccessException)
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
        }

        public void AddCommand(ICommand command)
        {
            List<MethodInfo> overloads = GetOverloads(command.GetType());
            commandGroups[command.Name] = new CommandGroup(command, overloads);
        }

        public void InvokeCommand(string input, CommandContext context)
        {
            (string commandName, string[] args) = ParseInput(input);

            if (commandGroups.TryGetValue(commandName, out CommandGroup? commandGroup))
            {
                MethodInfo? method = ResolveOverload(commandGroup.Overloads, args);

                if (method != null) Invoke(commandGroup.Command, method, args, context);
                else
                    logger.LogWarning(Events.Console, "No overload found for command '{Command}'", commandName);
            }
            else
            {
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
                object[] parsedArgs = new object[args.Count];

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

        private record CommandGroup(ICommand Command, List<MethodInfo> Overloads);
    }
}

// <copyright file="CommandInvoker.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Logging;

namespace VoxelGame.Client.Console;

/// <summary>
///     Discovers and executes commands using the <see cref="Command" /> class.
/// </summary>
public sealed partial class CommandInvoker : IResource
{
    private readonly CommandLibrary library = new();
    private readonly ArgumentResolver resolver = new();

    /// <summary>
    ///     Get the names of all registered commands.
    /// </summary>
    public IEnumerable<String> CommandNames => library.Names;

    /// <inheritdoc />
    public RID Identifier { get; } = RID.Named<CommandInvoker>("Default");

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.CommandInvoker;

    #region DISPOSING

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSING

    /// <summary>
    ///     Invoked when new commands are added or discovered.
    /// </summary>
    public event EventHandler? CommandsUpdated;

    /// <inheritdoc cref="CommandLibrary.GetHelpText" />
    public String GetCommandHelpText(String commandName)
    {
        return library.GetHelpText(commandName);
    }

    /// <inheritdoc cref="CommandLibrary.GetSignatures" />
    public IEnumerable<String> GetCommandSignatures(String commandName)
    {
        return library.GetSignatures(commandName);
    }

    /// <summary>
    ///     Add a parser to parse arguments.
    /// </summary>
    /// <param name="parser">The parser to add. Will replace any existing parser for the same type.</param>
    public void AddParser(Parser parser)
    {
        resolver.AddParser(parser);
    }

    /// <summary>
    ///     Search and discover all commands in the calling assembly.
    /// </summary>
    public void SearchCommands()
    {
        LogSearchingCommands(logger);

        var count = 0;

        foreach (Type type in Reflections.GetSubclasses<Command>())
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

            library.AddCommand(command);

            LogFoundCommand(logger, command.Name);
            count++;
        }

        LogFoundCommandsCount(logger, count);
        CommandsUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Add a command to the list of available commands for this invoker.
    /// </summary>
    /// <param name="command">The command to add.</param>
    public void AddCommand(ICommand command)
    {
        library.AddCommand(command);

        LogAddedCommand(logger, command.Name);
        CommandsUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Try to invoke a command using console input. In case of failure, messages are written to the console.
    /// </summary>
    /// <param name="input">The console input.</param>
    /// <param name="context">The command context in which the command should be executed.</param>
    public void InvokeCommand(String input, Context context)
    {
        (String commandName, String[] args) = ParseInput(input);

        if (library.GetCommand(commandName) is {command: var command, overloads: var overloads})
        {
            MethodInfo? method = resolver.ResolveOverload(overloads, args);

            if (method != null)
            {
                Invoke(command, method, args, context);
            }
            else
            {
                context.Console.WriteError($"No overload found, use 'help {commandName}' for more info.");
                LogNoOverloadFound(logger, commandName);
            }
        }
        else
        {
            context.Console.WriteError($"No command '{commandName}' found, use 'help' for more info.");
            LogCommandNotFound(logger, commandName);
        }
    }

    private static (String commandName, String[] args) ParseInput(String input)
    {
        StringBuilder commandName = new();

        foreach (Char c in input)
        {
            if (c == ' ') break;

            commandName.Append(c);
        }

        List<StringBuilder> args = [];

        var isNextArg = true;
        var isQuoted = false;
        var isEscaped = false;

        Int32 nextIndex = commandName.Length + 1;
        String remaining = input.Length > nextIndex ? input[nextIndex..] : "";

        foreach (Char c in remaining)
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

    private void Invoke(ICommand command, MethodBase method, IReadOnlyList<String> args, Context context)
    {
        try
        {
            Object[] parsedArgs = resolver.ParseArguments(method, args);

            command.SetContext(context);
            method.Invoke(command, parsedArgs);

            LogInvokedCommand(logger, command.Name);
        }
        catch (TargetInvocationException e)
        {
            LogErrorInvokingCommand(logger, e.InnerException, method.Name);
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<CommandInvoker>();

    [LoggerMessage(EventId = LogID.CommandInvoker + 0, Level = LogLevel.Debug, Message = "Searching commands")]
    private static partial void LogSearchingCommands(ILogger logger);

    [LoggerMessage(EventId = LogID.CommandInvoker + 1, Level = LogLevel.Debug, Message = "Found command '{Name}'")]
    private static partial void LogFoundCommand(ILogger logger, String name);

    [LoggerMessage(EventId = LogID.CommandInvoker + 2, Level = LogLevel.Information, Message = "Found {Count} commands")]
    private static partial void LogFoundCommandsCount(ILogger logger, Int32 count);

    [LoggerMessage(EventId = LogID.CommandInvoker + 3, Level = LogLevel.Debug, Message = "Added command '{Name}'")]
    private static partial void LogAddedCommand(ILogger logger, String name);

    [LoggerMessage(EventId = LogID.CommandInvoker + 4, Level = LogLevel.Information, Message = "No overload found for command '{Command}'")]
    private static partial void LogNoOverloadFound(ILogger logger, String command);

    [LoggerMessage(EventId = LogID.CommandInvoker + 5, Level = LogLevel.Information, Message = "Command '{Command}' not found")]
    private static partial void LogCommandNotFound(ILogger logger, String command);

    [LoggerMessage(EventId = LogID.CommandInvoker + 6, Level = LogLevel.Debug, Message = "Invoked command '{Command}'")]
    private static partial void LogInvokedCommand(ILogger logger, String command);

    [LoggerMessage(EventId = LogID.CommandInvoker + 7, Level = LogLevel.Error, Message = "Error while invoking command '{Command}'")]
    private static partial void LogErrorInvokingCommand(ILogger logger, Exception? exception, String command);

    #endregion
}

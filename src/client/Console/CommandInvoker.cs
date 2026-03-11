// <copyright file="CommandInvoker.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.UserInterfaces;

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

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE

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
    /// <param name="context">The context in which loading is done.</param>
    public void SearchCommands(IResourceContext context)
    {
        LogSearchingCommands(logger);

        Int32 count = 0;

        foreach (Command command in Reflections.GetSubclassInstances<Command>())
        {
            library.AddCommand(command);

            context.ReportDiscovery(ResourceTypes.Command, RID.Named<Command>(command.Name));

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

        if (String.IsNullOrWhiteSpace(commandName))
        {
            context.Output.WriteError("No command provided, use 'help' for available commands.");

            return;
        }

        if (library.GetCommand(commandName) is {command: var command, overloads: var overloads})
        {
            ArgumentResolver.OverloadResolutionResult resolution = resolver.ResolveOverload(overloads, args);

            if (resolution.IsSuccess)
            {
                Invoke(command, resolution.Method!, args, context);
            }
            else
            {
                WriteOverloadError(context, commandName, resolution.Diagnostics);
                LogNoOverloadFound(logger, commandName);
            }
        }
        else
        {
            String suggestions = String.Join(", ", library.GetCommandSuggestions(commandName));
            String message = suggestions.Length > 0
                ? $"No command '{commandName}' found, use 'help' for more info. Did you mean: {suggestions}?"
                : $"No command '{commandName}' found, use 'help' for more info.";

            context.Output.WriteError(message);
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

        Boolean isNextArg = true;
        Boolean isQuoted = false;
        Boolean isEscaped = false;

        Int32 nextIndex = commandName.Length + 1;
        String remaining = input.Length > nextIndex ? input[nextIndex..] : "";

        foreach (Char c in remaining)
        {
            if (isEscaped)
            {
                if (isNextArg)
                {
                    args.Add(new StringBuilder());
                    isNextArg = false;
                }

                args[^1].Append(c);
                isEscaped = false;

                continue;
            }

            switch (c)
            {
                case ' ' when !isQuoted:
                    isNextArg = true;

                    break;
                case '"':
                    isQuoted = !isQuoted;

                    break;
                case '\\':
                    isEscaped = true;

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
        }

        if (isEscaped)
        {
            if (isNextArg)
            {
                args.Add(new StringBuilder());
            }

            args[^1].Append('\\');
        }

        return (commandName.ToString(), args.Select(a => a.ToString()).ToArray());
    }

    private static void WriteOverloadError(Context context, String commandName, IReadOnlyList<String> diagnostics)
    {
        StringBuilder message = new();
        
        message.Append(CultureInfo.InvariantCulture, $"No overload found for '{commandName}'. Use 'help {commandName}' for more info.");

        if (diagnostics.Count > 0)
        {
            message.AppendLine();

            foreach (String diagnostic in diagnostics.Take(4))
                message.AppendLine(diagnostic);
            
            Int32 remaining = diagnostics.Count - 4;

            if (remaining > 0)
                message.Append(CultureInfo.InvariantCulture, $"... and {remaining} more mismatch(es).");
        }

        context.Output.WriteError(
            message.ToString(),
            [new FollowUp("Show command help", () => { context.Invoker.InvokeCommand($"help {commandName}", context); })]);
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

            context.Output.WriteError($"Error while invoking command '{command.Name}', see log for details");
        }
        catch (Exception e)
        {
            LogErrorExecutingCommand(logger, e, command.Name);

            context.Output.WriteError($"Error while executing command '{command.Name}', see log for details");
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

    [LoggerMessage(EventId = LogID.CommandInvoker + 8, Level = LogLevel.Error, Message = "Error while executing command '{Command}'")]
    private static partial void LogErrorExecutingCommand(ILogger logger, Exception? exception, String command);

    #endregion LOGGING
}

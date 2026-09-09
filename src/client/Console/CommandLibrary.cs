// <copyright file="CommandLibrary.cs" company="VoxelGame">
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
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Console;

/// <summary>
///     Contains all commands that can be executed, and provides access operations.
/// </summary>
public sealed partial class CommandLibrary
{
    private const String MethodName = "Invoke";

    private readonly Dictionary<String, CommandGroup> groups = new();

    /// <summary>
    ///     Get all command names.
    /// </summary>
    public IEnumerable<String> Names => groups.Keys;

    /// <summary>
    ///     Get the help text for a command.
    /// </summary>
    /// <param name="name">The name of the command. Must correspond to a discovered command.</param>
    /// <returns>The help text.</returns>
    public String GetHelpText(String name)
    {
        return groups.TryGetValue(name, out CommandGroup? commandGroup)
            ? commandGroup.Command.HelpText
            : throw Exceptions.ArgumentNotInCollection(nameof(name), nameof(Names), name);
    }

    /// <summary>
    ///     Get all signatures for a command.
    /// </summary>
    /// <param name="name">The name of the command. Must correspond to a discovered command.</param>
    /// <returns>All signatures for the command.</returns>
    public IEnumerable<String> GetSignatures(String name)
    {
        if (!groups.TryGetValue(name, out CommandGroup? commandGroup))
            throw Exceptions.ArgumentNotInCollection(nameof(name), nameof(Names), name);

        return GetCommandSignatures(name, commandGroup);
    }

    /// <summary>
    ///     Get similar command names to a given input.
    /// </summary>
    /// <param name="input">The input to compare against.</param>
    /// <param name="maxSuggestions">The maximum number of suggestions to return.</param>
    /// <returns>The suggested command names.</returns>
    public IEnumerable<String> GetCommandSuggestions(String input, Int32 maxSuggestions = 3)
    {
        String normalizedInput = input.ToLowerInvariant();
        Int32 maxDistance = Math.Max(val1: 2, normalizedInput.Length / 3);

        return groups.Keys
            .Select(name => (name, distance: StringTools.LevenshteinDistance(normalizedInput, name.ToLowerInvariant())))
            .Where(result => result.distance <= maxDistance)
            .OrderBy(result => result.distance)
            .ThenBy(result => result.name)
            .Take(maxSuggestions)
            .Select(result => result.name);
    }

    private static IEnumerable<String> GetCommandSignatures(String commandName, CommandGroup commandGroup)
    {
        foreach (CommandOverload overload in commandGroup.Overloads)
        {
            StringBuilder signature = new();
            signature.Append(commandName);

            foreach (ParameterInfo parameter in overload.CallableParameters)
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
    ///     Try to add a command to the library.
    ///     If the command has any invalid overloads, or the combination of overloads is invalid, it is not added.
    /// </summary>
    /// <param name="command">The command to add.</param>
    /// <returns>Whether the command was added.</returns>
    public Boolean TryAddCommand(ICommand command)
    {
        List<CommandOverload> overloads = [];

        foreach (MethodInfo method in Reflections.GetMethodOverloads(command.GetType(), MethodName))
        {
            if (CommandOverload.TryCreate(method, out CommandOverload? overload))
            {
                overloads.Add(overload);

                continue;
            }

            LogMalformedOverload(logger, command.Name, method.Name);

            return false;
        }

        if (FindAmbiguity(overloads) is {} ambiguity)
        {
            LogAmbiguousOverloads(logger, command.Name, ambiguity.first, ambiguity.second);

            return false;
        }

        groups[command.Name] = new CommandGroup(command, overloads);

        return true;
    }

    private static (CommandOverload first, CommandOverload second)? FindAmbiguity(
        List<CommandOverload> overloads)
    {
        for (Int32 first = 0; first < overloads.Count; first++)
        {
            for (Int32 second = first + 1; second < overloads.Count; second++)
            {
                if (overloads[first].HasSameCallableSignature(overloads[second]))
                    return (overloads[first], overloads[second]);
            }
        }

        return null;
    }

    /// <summary>
    ///     Get a command.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <returns>The command, or <c>null</c> if the command does not exist.</returns>
    public (ICommand command, IReadOnlyList<CommandOverload> overloads)? GetCommand(String name)
    {
        return groups.TryGetValue(name, out CommandGroup? commandGroup)
            ? (commandGroup.Command, commandGroup.Overloads)
            : null;
    }

    private sealed record CommandGroup(ICommand Command, List<CommandOverload> Overloads);

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<CommandLibrary>();

    [LoggerMessage(EventId = LogID.CommandLibrary + 0,
        Level = LogLevel.Warning,
        Message = "Command '{Command}' contains malformed overload '{Overload}': a context parameter must be the final parameter")]
    private static partial void LogMalformedOverload(ILogger logger, String command, String overload);

    [LoggerMessage(EventId = LogID.CommandLibrary + 1,
        Level = LogLevel.Warning,
        Message = "Command '{Command}' contains ambiguous overloads '{FirstOverload}' and '{SecondOverload}': their callable signatures are identical")]
    private static partial void LogAmbiguousOverloads(
        ILogger logger,
        String command,
        CommandOverload firstOverload,
        CommandOverload secondOverload);

    #endregion LOGGING
}

// <copyright file="CommandLibrary.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Console;

/// <summary>
///     Contains all commands that can be executed, and provides access operations.
/// </summary>
public class CommandLibrary
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

    private static IEnumerable<String> GetCommandSignatures(String commandName, CommandGroup commandGroup)
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
    ///     Add a command to the library.
    /// </summary>
    /// <param name="command">The command to add.</param>
    public void AddCommand(ICommand command)
    {
        List<MethodInfo> overloads = Reflections.GetMethodOverloads(command.GetType(), MethodName).ToList();
        groups[command.Name] = new CommandGroup(command, overloads);
    }

    /// <summary>
    ///     Get a command.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <returns>The command, or <c>null</c> if the command does not exist.</returns>
    public (ICommand command, IReadOnlyList<MethodInfo> overloads)? GetCommand(String name)
    {
        return groups.TryGetValue(name, out CommandGroup? commandGroup)
            ? (commandGroup.Command, commandGroup.Overloads)
            : null;
    }

    private sealed record CommandGroup(ICommand Command, List<MethodInfo> Overloads);
}

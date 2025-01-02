// <copyright file="CommandLibrary.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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

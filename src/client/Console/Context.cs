// <copyright file="Context.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Client.Actors;

namespace VoxelGame.Client.Console;

/// <summary>
///     The command execution context.
/// </summary>
/// <param name="Output">The class used to receive output from the command.</param>
/// <param name="Invoker">The invoker used to invoke this command.</param>
/// <param name="Player">The player to execute the command for.</param>
/// <param name="IsScript">Whether this command originates from a script.</param>
public record Context(ConsoleOutput Output, CommandInvoker Invoker, Player Player, Boolean IsScript = false)
{
    /// <summary>
    ///     Get this context as a script context.
    /// </summary>
    /// <returns>The new script context.</returns>
    public Context ToScript()
    {
        return this with {IsScript = true};
    }
}

// <copyright file="Context.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Client.Entities;

namespace VoxelGame.Client.Console;

/// <summary>
///     The command execution context.
/// </summary>
/// <param name="Console">The console in which the command is running.</param>
/// <param name="Invoker">The invoker used to invoke this command.</param>
/// <param name="Player">The player to execute the command for.</param>
/// <param name="IsScript">Whether this command originates from a script.</param>
public record Context(ConsoleWrapper Console, CommandInvoker Invoker, ClientPlayer Player, bool IsScript = false)
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



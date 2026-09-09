// <copyright file="Command.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Client.Actors.Components;

namespace VoxelGame.Client.Console;

/// <summary>
///     The base class of all callable commands. Commands with a zero-parameter constructor are automatically discovered.
///     Every command must have one or more <c>Invoke</c> methods. If a command requires access to the command execution
///     context, it can request the context through the final parameter. Usage of any other parameter to request the
///     context is not valid. The context parameter is not exposed as a call parameter.
/// </summary>
public abstract class Command : ICommand
{
    /// <inheritdoc />
    public abstract String Name { get; }

    /// <inheritdoc />
    public abstract String HelpText { get; }

    /// <summary>
    ///     Get a named position in the world.
    /// </summary>
    /// <param name="name">The name of the position.</param>
    /// <param name="context">The command execution context.</param>
    /// <returns>The position, or null if it does not exist.</returns>
    protected static Vector3d? GetNamedPosition(String name, Context context)
    {
        return name switch
        {
            "origin" => (0, 0, 0),
            "spawn" => context.Player.World.SpawnPosition,
            "min-corner" => -context.Player.World.Extents,
            "max-corner" => context.Player.World.Extents,
            "self" => context.Player.Body.Transform.Position,
            "prev-self" => GetPreviousPlayerPosition(context),
            _ => null
        };
    }

    /// <summary>
    ///     Get the previous position of the player, e.g. before a teleportation.
    ///     This is only set by the command system, so normal movement does not change it.
    /// </summary>
    /// <returns>The previous position, or spawn position if not set.</returns>
    private static Vector3d? GetPreviousPlayerPosition(Context context)
    {
        return context.Player.GetComponent<PreviousPosition>()?.Value ?? context.Player.World.SpawnPosition;
    }

    /// <summary>
    ///     Set the previous position of the player, e.g. before a teleportation.
    /// </summary>
    /// <param name="position">The position to set as previous.</param>
    /// <param name="context">The command execution context.</param>
    protected static void SetPreviousPlayerPosition(Vector3d position, Context context)
    {
        PreviousPosition previousPosition = context.Player.AddComponent<PreviousPosition>();
        previousPosition.Value = position;
    }
}

/// <summary>
///     An interface for all commands.
/// </summary>
public interface ICommand
{
    /// <summary>
    ///     Get the name of this command.
    /// </summary>
    String Name { get; }

    /// <summary>
    ///     Get the help text for this command.
    /// </summary>
    String HelpText { get; }
}

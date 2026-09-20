// <copyright file="NamedPositionRegistry.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Client.Actors.Components;

namespace VoxelGame.Client.Console;

/// <summary>
///     Registry for predefined and player-stored named positions for use for console commands.
/// </summary>
public sealed class NamedPositionRegistry
{
    private readonly Dictionary<String, Func<Context, Vector3d>> predefined = new(StringComparer.Ordinal);

    /// <summary>
    ///     Register a predefined read-only named position.
    /// </summary>
    /// <param name="name">The name of the position without the '@' prefix.</param>
    /// <param name="provider">A function returning the position vector given the command execution context.</param>
    public void Register(String name, Func<Context, Vector3d> provider)
    {
        predefined[name] = provider;
    }

    /// <summary>
    ///     Resolve a named position in the current command execution context.
    ///     Predefined positions are checked first, followed by stored positions on the player.
    /// </summary>
    /// <param name="name">The name of the position without the '@' prefix.</param>
    /// <param name="context">The execution context.</param>
    /// <returns>The resolved position vector, or <c>null</c> if the name is not registered.</returns>
    public Vector3d? Resolve(String name, Context context)
    {
        if (predefined.TryGetValue(name, out Func<Context, Vector3d>? provider))
            return provider(context);

        return context.Player.GetComponent<StoredPositions>()?.Get(name);
    }

    /// <summary>
    ///     Try to store or update a named position.
    ///     Predefined read-only positions cannot be overwritten.
    /// </summary>
    /// <param name="name">The name of the position.</param>
    /// <param name="position">The position vector to store.</param>
    /// <param name="context">The execution context.</param>
    /// <returns><c>true</c> if the position was stored or updated; <c>false</c> if the name is read-only.</returns>
    public Boolean TrySet(String name, Vector3d position, Context context)
    {
        if (predefined.ContainsKey(name)) return false;

        StoredPositions stored = context.Player.AddComponent<StoredPositions>();
        stored.Set(name, position);

        return true;
    }
}

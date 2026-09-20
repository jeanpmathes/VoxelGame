// <copyright file="StoredPositions.cs" company="VoxelGame">
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
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Stores named positions and allows reading and writing them by name.
///     This is used, for example, by console commands to allow a player to store positions.
/// </summary>
public partial class StoredPositions : ActorComponent
{
    private readonly Dictionary<String, Vector3d> positions = new();

    [Constructible]
    private StoredPositions(Actor subject) : base(subject) {}

    /// <summary>
    ///     Get the position associated with the given name.
    /// </summary>
    /// <param name="name">The name of the position.</param>
    /// <returns>The stored position, or <c>null</c> if it does not exist.</returns>
    public Vector3d? Get(String name)
    {
        return positions.TryGetValue(name, out Vector3d position) ? position : null;
    }

    /// <summary>
    ///     Store or update a position with the given name.
    /// </summary>
    /// <param name="name">The name of the position.</param>
    /// <param name="position">The position value to store.</param>
    public void Set(String name, Vector3d position)
    {
        positions[name] = position;
    }

    /// <summary>
    ///     Check whether a position with the given name is stored.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>Whether the position exists.</returns>
    public Boolean Contains(String name)
    {
        return positions.ContainsKey(name);
    }
}

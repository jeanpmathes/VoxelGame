// <copyright file="Player.cs" company="VoxelGame">
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
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Physics;

namespace VoxelGame.Core.Actors;

/// <summary>
///     A player that can interact with the world.
/// </summary>
public abstract class Player : Actor
{
    /// <summary>
    ///     Create a new player.
    /// </summary>
    /// <param name="mass">The mass the player has.</param>
    /// <param name="boundingVolume">The bounding box of the player.</param>
    protected Player(Double mass, BoundingVolume boundingVolume)
    {
        Body = AddComponent<Body, Body.Characteristics>(new Body.Characteristics(mass, boundingVolume));

        AddComponent<Spawning>();
        AddComponent<ChunkLoader>();
    }

    /// <summary>
    ///     The body of the player.
    /// </summary>
    public Body Body { get; }
}

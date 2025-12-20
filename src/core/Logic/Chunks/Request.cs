// <copyright file="Request.cs" company="VoxelGame">
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
using VoxelGame.Core.Actors;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     A request for a chunk (and the surrounding chunks) to be loaded.
/// </summary>
/// <param name="position">The position of the requested chunk.</param>
/// <param name="requester">The actor requesting the chunk.</param>
public class Request(ChunkPosition position, Actor requester)
{
    /// <summary>
    ///     Get the position of the requested chunk.
    /// </summary>
    public ChunkPosition Position { get; } = position;

    /// <summary>
    ///     Get the actor requesting the chunk.
    /// </summary>
    public Actor Requester { get; } = requester;

    /// <summary>
    ///     Internal index used by <see cref="Requests" />.
    /// </summary>
    internal Int32? Index { get; set; }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(Position, Requester);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        if (obj is not Request request)
            return false;

        return Position == request.Position && Requester == request.Requester;
    }
}

// <copyright file="MaximumThroughputStrategy.cs" company="VoxelGame">
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
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     This strategy updates all chunks as soon as possible.
/// </summary>
/// <param name="list">The owning chunk update list.</param>
public class MaximumThroughputStrategy(ChunkStateUpdateList list) : ChunkStateUpdateStrategy(list)
{
    /// <summary>
    ///     Calling remove in update will modify the collection, so a local copy is needed for iteration.
    /// </summary>
    private readonly List<Chunk?> local = [];

    /// <inheritdoc />
    public override Int32 Update(Bag<Chunk> chunks)
    {
        local.Clear();
        chunks.CopyDirectlyTo(local);

        var updated = 0;

        foreach (Chunk? chunk in local)
        {
            if (chunk == null)
                continue;

            Update(chunk);

            updated += 1;
        }

        return updated;
    }
}

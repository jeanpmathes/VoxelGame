// <copyright file="ChunkStateUpdateStrategy.cs" company="VoxelGame">
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
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     Defines how (e.g. order) chunk states are updated.
///     Is used by the <see cref="ChunkStateUpdateList" /> to update chunks.
/// </summary>
public abstract class ChunkStateUpdateStrategy(ChunkStateUpdateList list)
{
    /// <summary>
    ///     Update the chunks in the list.
    ///     The strategy is free to only update a subset of the chunks each call.
    /// </summary>
    /// <param name="chunks">
    ///     The bag of chunks to update.
    ///     The collection is modified by the calling code when adding or removing chunks from the
    ///     <see cref="ChunkStateUpdateList" />.
    /// </param>
    /// <returns>The number of chunks that were updated this call.</returns>
    public abstract Int32 Update(Bag<Chunk> chunks);

    /// <summary>
    ///     Updates a single chunk and removes it from the list if necessary.
    /// </summary>
    /// <param name="chunk">The chunk to update.</param>
    protected void Update(Chunk chunk)
    {
        ChunkState state = chunk.UpdateState();

        if (state.WaitMode != StateWaitModes.None)
            list.Remove(chunk);
    }
}

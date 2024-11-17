// <copyright file="ChunkStateUpdateStrategy.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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

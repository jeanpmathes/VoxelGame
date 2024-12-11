// <copyright file="MaximumThroughputStrategy.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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

// <copyright file="ChunkUpdateDistributor.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Threading.Channels;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     List of chunks that need state updates.
/// </summary>
public class ChunkUpdateList
{
    private readonly List<Chunk?> read = [];
    private readonly Bag<Chunk> write = new(null!);

    private readonly Channel<(Chunk chunk, Action<Chunk> action)> completions = Channel.CreateUnbounded<(Chunk chunk, Action<Chunk> action)>();

    /// <summary>
    ///     Run an update cycle, which will perform chunk state updates.
    /// </summary>
    /// <returns>The number of chunks that were updated this call.</returns>
    public Int32 Update()
    {
        while (completions.Reader.TryRead(out (Chunk chunk, Action<Chunk> action) completion))
        {
            completion.action(completion.chunk);
            Add(completion.chunk);
        }

        read.Clear();
        write.CopyDirectlyTo(read);

        var updated = 0;

        foreach (Chunk? chunk in read)
        {
            if (chunk == null)
                continue;

            ChunkState state = chunk.UpdateState();

            if (state.WaitMode != StateWaitModes.None)
                Remove(chunk);

            updated += 1;
        }

        return updated;
    }

    /// <summary>
    ///     Add a chunk so it will receive updates.
    ///     If the chunk is already added, this method does nothing.
    ///     Must be called on the main thread.
    /// </summary>
    /// <param name="chunk">The chunk to add.</param>
    public void Add(Chunk chunk)
    {
        Throw.IfNotOnMainThread(this);

        if (chunk.UpdateIndex.HasValue)
            return;

        Int32 index = write.Add(chunk);
        chunk.UpdateIndex = index;
    }

    /// <summary>
    ///     Remove a chunk, so it will no longer receive updates.
    ///     If the chunk is not added, this method does nothing.
    ///     Must be called on the main thread.
    /// </summary>
    /// <param name="chunk">The chunk to remove.</param>
    public void Remove(Chunk chunk)
    {
        Throw.IfNotOnMainThread(this);

        if (!chunk.UpdateIndex.HasValue)
            return;

        write.RemoveAt(chunk.UpdateIndex.Value);
        chunk.UpdateIndex = null;
    }

    /// <summary>
    ///     Add a chunk to the list when the update cycle is running.
    ///     This method can be called from any thread.
    /// </summary>
    /// <param name="chunk">The chunk to add.</param>
    /// <param name="onAdd">The action to perform when the chunk is added. Will be called on the main thread.</param>
    public void AddOnUpdate(Chunk chunk, Action<Chunk> onAdd)
    {
        completions.Writer.TryWrite((chunk, onAdd));
    }
}

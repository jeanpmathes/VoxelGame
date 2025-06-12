// <copyright file="ChunkStateUpdateDistributor.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     List of chunks that need state updates.
/// </summary>
public class ChunkStateUpdateList
{
    private readonly Bag<Chunk> chunks = new(null!);

    private readonly Channel<(Chunk chunk, Action<Chunk> action)> completions = Channel.CreateUnbounded<(Chunk chunk, Action<Chunk> action)>();

    private ChunkStateUpdateStrategy strategy;

    /// <summary>
    ///     Create a new chunk update list.
    /// </summary>
    public ChunkStateUpdateList()
    {
        strategy = new MaximumThroughputStrategy(this);
    }

    /// <summary>
    ///     Use the maximum throughput strategy.
    ///     This should be used when only chunk operations but no game logic is running.
    ///     One example is when loading the world.
    /// </summary>
    public void EnterHighThroughputMode()
    {
        strategy = new MaximumThroughputStrategy(this);
    }

    /// <summary>
    ///     Use the low impact strategy.
    ///     This should be used when the game is running and the player is interacting with the world.
    /// </summary>
    public void EnterLowImpactMode()
    {
        strategy = new LowImpactStrategy(this);
    }

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

        return strategy.Update(chunks);
    }

    /// <summary>
    ///     Add a chunk so it will receive updates.
    ///     If the chunk is already added, this method does nothing.
    ///     Must be called on the main thread.
    /// </summary>
    /// <param name="chunk">The chunk to add.</param>
    public void Add(Chunk chunk)
    {
        ApplicationInformation.ThrowIfNotOnMainThread(this);

        if (chunk.HasUpdateIndex())
            return;

        Int32 index = chunks.Add(chunk);
        chunk.SetUpdateIndex(index);
    }

    /// <summary>
    ///     Remove a chunk, so it will no longer receive updates.
    ///     If the chunk is not added, this method does nothing.
    ///     Must be called on the main thread.
    /// </summary>
    /// <param name="chunk">The chunk to remove.</param>
    public void Remove(Chunk chunk)
    {
        ApplicationInformation.ThrowIfNotOnMainThread(this);

        Int32? index = chunk.ClearUpdateIndex();

        if (index == null)
            return;

        chunks.RemoveAt(index.Value);
    }

    /// <summary>
    ///     Add a chunk to the list when the update cycle is running.
    ///     This method can be called from any thread.
    /// </summary>
    /// <param name="chunk">The chunk to add.</param>
    /// <param name="onAdd">The action to perform when the chunk is added. Will be called on the main thread.</param>
    /// <param name="token">The cancellation token.</param>
    public async Task AddToUpdateAsync(Chunk chunk, Action<Chunk> onAdd, CancellationToken token = default)
    {
        await completions.Writer.WriteAsync((chunk, onAdd), token);
    }
}

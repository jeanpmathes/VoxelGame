// <copyright file="ColumnSampleStore.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Chunks;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     A store for column samples for an area the size of a chunk.
/// </summary>
/// <param name="chunkX">The x position in chunk coordinates.</param>
/// <param name="chunkZ">The z position in chunk coordinates.</param>
/// <param name="samples">The samples for the column.</param>
internal class ColumnSampleStore(Int32 chunkX, Int32 chunkZ, Map.Sample[] samples)
{
    private readonly Vector2i anchor = new ChunkPosition(chunkX, y: 0, chunkZ).FirstBlock.Xz;

    /// <summary>
    ///     Get the position of this store.
    /// </summary>
    internal (Int32 X, Int32 Z) Key => (chunkX, chunkZ);

    /// <summary>
    ///     Check if this column store contains the columns for a given chunk position.
    /// </summary>
    internal Boolean Contains(ChunkPosition chunk)
    {
        return chunk.X == chunkX && chunk.Z == chunkZ;
    }

    /// <summary>
    ///     Sample an area of the world and store the samples.
    /// </summary>
    /// <param name="chunkX">The x position in chunk coordinates.</param>
    /// <param name="chunkZ">The z position in chunk coordinates.</param>
    /// <param name="generator">The generator to use.</param>
    /// <returns>The stored samples.</returns>
    internal static ColumnSampleStore Sample(Int32 chunkX, Int32 chunkZ, Generator generator)
    {
        var samples = new Map.Sample[Chunk.BlockSize * Chunk.BlockSize];

        ColumnSampleStore store = new(chunkX, chunkZ, samples);
        SamplingNoiseStore noise = new(store.anchor, generator.Map);

        for (var x = 0; x < Chunk.BlockSize; x++)
        for (var z = 0; z < Chunk.BlockSize; z++)
        {
            Vector2i position = store.anchor + new Vector2i(x, z);
            Map.Sample sample = generator.Map.GetSample(position, noise);

            store.Store(x, z, sample);
        }

        return store;
    }

    /// <summary>
    ///     Get the sample for a position.
    /// </summary>
    /// <param name="position">The position (block coordinates) to get the sample for.</param>
    /// <returns>The sample.</returns>
    internal Map.Sample GetSample(Vector2i position)
    {
        Vector2i offset = position - anchor;

        return samples[offset.X + offset.Y * Chunk.BlockSize];
    }

    internal static Map.Sample GetSample(Vector2i position, ColumnSampleStore? store, Map map)
    {
        return store?.GetSample(position) ?? map.GetSample((position.X, 0, position.Y));
    }

    private void Store(Int32 x, Int32 z, Map.Sample sample)
    {
        samples[x + z * Chunk.BlockSize] = sample;
    }
}

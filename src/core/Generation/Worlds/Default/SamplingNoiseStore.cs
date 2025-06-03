// <copyright file="SamplingNoiseStore.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Toolkit.Collections;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     Stores pre-computed noise values for sampling of chunk-sized regions.
///     By computing all noise values in advance, faster noise grid functions can be used.
/// </summary>
public class SamplingNoiseStore
{
    private const Int32 Size = Chunk.BlockSize;

    private readonly (Array2D<Single> x, Array2D<Single> y) cellSamplingOffsetNoise;

    private SlotCache<Vector2> cellSamplingOffsetNoiseCache;
    private SlotCache<Single> subBiomeDeterminationNoiseCache;
    private SlotCache<Single> oceanicSubBiomeDeterminationNoiseCache;

    /// <summary>
    ///     Create a new noise store for a targeted chunk-sized region of the world.
    ///     This will pre-compute all noise values for the region - a costly operation.
    /// </summary>
    /// <param name="anchor">The start position of the region in block coordinates.</param>
    /// <param name="map">The map to use.</param>
    public SamplingNoiseStore(Vector2i anchor, Map map)
    {
        Anchor = anchor;

        cellSamplingOffsetNoise = map.SamplingNoise.GetCellSamplingOffsetNoiseGrid(anchor, Size);
    }

    /// <summary>
    ///     The anchor position of the region in block coordinates.
    /// </summary>
    public Vector2i Anchor { get; }

    /// <summary>
    ///     Get the pre-computed noise value for the cell sampling offset noise at the given position.
    /// </summary>
    /// <param name="position">The position, must be in the region of the noise store.</param>
    /// <param name="noise">Object to compute noise with, only used on cache misses.</param>
    /// <param name="cachingHint">
    ///     Which cache slot to use, <c>0</c> to not use a cache slot, and <c>1</c> to <c>4</c> to use a
    ///     cache slot.
    /// </param>
    /// <returns>The noise value at the given position.</returns>
    public Vector2 GetCellSamplingOffsetNoise(Vector2i position, SamplingNoise noise, Int32 cachingHint)
    {
        return cachingHint switch
        {
            0 => ReadCellSamplingOffsetNoiseValue(position - Anchor),
            1 => ReadCellSamplingOffsetNoiseCache(position, noise, ref cellSamplingOffsetNoiseCache.key1, ref cellSamplingOffsetNoiseCache.value1),
            2 => ReadCellSamplingOffsetNoiseCache(position, noise, ref cellSamplingOffsetNoiseCache.key2, ref cellSamplingOffsetNoiseCache.value2),
            3 => ReadCellSamplingOffsetNoiseCache(position, noise, ref cellSamplingOffsetNoiseCache.key3, ref cellSamplingOffsetNoiseCache.value3),
            4 => ReadCellSamplingOffsetNoiseCache(position, noise, ref cellSamplingOffsetNoiseCache.key4, ref cellSamplingOffsetNoiseCache.value4),
            _ => throw Exceptions.UnsupportedValue(cachingHint)
        };
    }

    /// <summary>
    ///     Get the cached or computed sub-biome determination noise for a given position.
    ///     This method must be used with a non-zero caching hint.
    /// </summary>
    public Single GetSubBiomeDeterminationNoise(Vector2i position, SamplingNoise noise, Int32 cachingHint)
    {
        return cachingHint switch
        {
            1 => ReadSubBiomeDeterminationNoiseCache(position, noise, ref subBiomeDeterminationNoiseCache.key1, ref subBiomeDeterminationNoiseCache.value1),
            2 => ReadSubBiomeDeterminationNoiseCache(position, noise, ref subBiomeDeterminationNoiseCache.key2, ref subBiomeDeterminationNoiseCache.value2),
            3 => ReadSubBiomeDeterminationNoiseCache(position, noise, ref subBiomeDeterminationNoiseCache.key3, ref subBiomeDeterminationNoiseCache.value3),
            4 => ReadSubBiomeDeterminationNoiseCache(position, noise, ref subBiomeDeterminationNoiseCache.key4, ref subBiomeDeterminationNoiseCache.value4),
            _ => throw Exceptions.UnsupportedValue(cachingHint)
        };
    }

    /// <summary>
    ///     Get the cached or computed oceanic sub-biome determination noise for a given position.
    ///     This method must be used with a non-zero caching hint.
    /// </summary>
    public Single GetOceanicSubBiomeDeterminationNoise(Vector2i position, SamplingNoise samplingNoise, Int32 cachingHint)
    {
        return cachingHint switch
        {
            1 => ReadSubBiomeDeterminationNoiseCache(position, samplingNoise, ref oceanicSubBiomeDeterminationNoiseCache.key1, ref oceanicSubBiomeDeterminationNoiseCache.value1),
            2 => ReadSubBiomeDeterminationNoiseCache(position, samplingNoise, ref oceanicSubBiomeDeterminationNoiseCache.key2, ref oceanicSubBiomeDeterminationNoiseCache.value2),
            3 => ReadSubBiomeDeterminationNoiseCache(position, samplingNoise, ref oceanicSubBiomeDeterminationNoiseCache.key3, ref oceanicSubBiomeDeterminationNoiseCache.value3),
            4 => ReadSubBiomeDeterminationNoiseCache(position, samplingNoise, ref oceanicSubBiomeDeterminationNoiseCache.key4, ref oceanicSubBiomeDeterminationNoiseCache.value4),
            _ => throw Exceptions.UnsupportedValue(cachingHint)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector2 ReadCellSamplingOffsetNoiseValue(Vector2i relative)
    {
        return new Vector2(cellSamplingOffsetNoise.x[relative], cellSamplingOffsetNoise.y[relative]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector2 ReadCellSamplingOffsetNoiseCache(Vector2i readingKey, SamplingNoise generator, ref Vector2i slotKey, ref Vector2 slotValue)
    {
        if (slotKey == readingKey)
            return slotValue;

        Vector2i relative = readingKey - Anchor;

        slotKey = readingKey;

        slotValue = relative.X is >= 0 and < Size && relative.Y is >= 0 and < Size
            ? ReadCellSamplingOffsetNoiseValue(relative)
            : generator.ComputeCellSamplingOffsetNoise(readingKey);

        return slotValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Single ReadSubBiomeDeterminationNoiseCache(Vector2i readingKey, SamplingNoise generator, ref Vector2i slotKey, ref Single slotValue)
    {
        if (slotKey == readingKey)
            return slotValue;

        slotKey = readingKey;
        slotValue = generator.ComputeSubBiomeDeterminationNoise(readingKey);

        return slotValue;
    }

    #pragma warning disable S3898 // Private struct does not need equality operations.
    private struct SlotCache<T> where T : unmanaged
    #pragma warning restore S3898
    {
        [UsedImplicitly]
        public SlotCache() {}

        public Vector2i key1 = (Int32.MaxValue, Int32.MaxValue);
        public T value1 = default;

        public Vector2i key2 = (Int32.MaxValue, Int32.MaxValue);
        public T value2 = default;

        public Vector2i key3 = (Int32.MaxValue, Int32.MaxValue);
        public T value3 = default;

        public Vector2i key4 = (Int32.MaxValue, Int32.MaxValue);
        public T value4 = default;
    }
}

// <copyright file="SamplingNoise.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Toolkit.Collections;
using VoxelGame.Toolkit.Noise;

namespace VoxelGame.Core.Generation.Worlds.Standard;

/// <summary>
///     All noise generators used for the map sampling.
/// </summary>
public sealed class SamplingNoise : IDisposable
{
    private readonly NoiseGenerator cellSamplingOffsetNoiseX;
    private readonly NoiseGenerator cellSamplingOffsetNoiseY;
    private readonly NoiseGenerator oceanicSubBiomeDeterminationNoise;

    private readonly NoiseGenerator stoneSamplingOffsetNoiseX;
    private readonly NoiseGenerator stoneSamplingOffsetNoiseY;

    private readonly NoiseGenerator subBiomeDeterminationNoise;

    /// <summary>
    ///     Create all noise generators used for the map sampling.
    /// </summary>
    /// <param name="factory">The noise factory to use.</param>
    public SamplingNoise(NoiseFactory factory)
    {
        cellSamplingOffsetNoiseX = CreateCellSamplingOffsetNoise();
        cellSamplingOffsetNoiseY = CreateCellSamplingOffsetNoise();

        stoneSamplingOffsetNoiseX = CreateStoneSamplingOffsetNoise();
        stoneSamplingOffsetNoiseY = CreateStoneSamplingOffsetNoise();

        subBiomeDeterminationNoise = CreateSubBiomeDeterminationNoise();
        oceanicSubBiomeDeterminationNoise = CreateSubBiomeDeterminationNoise();

        NoiseGenerator CreateCellSamplingOffsetNoise()
        {
            return factory.CreateNext()
                .WithType(NoiseType.GradientNoise)
                .WithFrequency(frequency: 0.01f)
                .WithFractals()
                .WithOctaves(octaves: 5)
                .WithLacunarity(lacunarity: 2.0f)
                .WithGain(gain: 0.5f)
                .WithWeightedStrength(weightedStrength: 0.0f)
                .Build();
        }

        NoiseGenerator CreateStoneSamplingOffsetNoise()
        {
            return factory.CreateNext()
                .WithType(NoiseType.GradientNoise)
                .WithFrequency(frequency: 0.05f)
                .WithFractals()
                .WithOctaves(octaves: 2)
                .WithLacunarity(lacunarity: 2.0f)
                .WithGain(gain: 0.5f)
                .WithWeightedStrength(weightedStrength: 0.0f)
                .Build();
        }

        NoiseGenerator CreateSubBiomeDeterminationNoise()
        {
            return factory.CreateNext()
                .WithType(NoiseType.CellularNoise)
                .WithFrequency(frequency: 0.03f)
                .Build();
        }
    }

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        cellSamplingOffsetNoiseX.Dispose();
        cellSamplingOffsetNoiseY.Dispose();

        stoneSamplingOffsetNoiseX.Dispose();
        stoneSamplingOffsetNoiseY.Dispose();

        subBiomeDeterminationNoise.Dispose();
        oceanicSubBiomeDeterminationNoise.Dispose();
    }

    #endregion

    /// <summary>
    ///     Get a noise grid filled with cell sampling offset noise.
    /// </summary>
    public (Array2D<Single> x, Array2D<Single> y) GetCellSamplingOffsetNoiseGrid(Vector2i position, Int32 size)
    {
        return (cellSamplingOffsetNoiseX.GetNoiseGrid(position, size), cellSamplingOffsetNoiseY.GetNoiseGrid(position, size));
    }

    /// <summary>
    ///     Get the cell sampling offset noise for a chosen position, in block coordinates.
    ///     If available, the noise will be taken from the store instead of computing it.
    ///     A caching hint can be supplied for accesses outside the stored region.
    /// </summary>
    public Vector2 GetCellSamplingOffsetNoise(Vector2i position, SamplingNoiseStore? store, Int32 cachingHint)
    {
        return store?.GetCellSamplingOffsetNoise(position, this, cachingHint) ??
               ComputeCellSamplingOffsetNoise(position);
    }

    /// <summary>
    ///     Calculate the cell sampling offset noise for a given position.
    ///     This will always compute, therefore prefer using <see cref="GetCellSamplingOffsetNoise" /> if possible.
    /// </summary>
    public Vector2 ComputeCellSamplingOffsetNoise(Vector2i position)
    {
        return new Vector2(
            cellSamplingOffsetNoiseX.GetNoise(position),
            cellSamplingOffsetNoiseY.GetNoise(position));
    }

    /// <summary>
    ///     Get the stone sampling offset noise for a chosen position, in block coordinates.
    /// </summary>
    public Vector2 GetStoneSamplingOffsetNoise(Vector2i position)
    {
        return ComputeStoneSamplingOffsetNoise(position);
    }

    /// <summary>
    ///     Calculate the stone sampling offset noise for a given position.
    /// </summary>
    public Vector2 ComputeStoneSamplingOffsetNoise(Vector2i position)
    {
        return new Vector2(
            stoneSamplingOffsetNoiseX.GetNoise(position),
            stoneSamplingOffsetNoiseY.GetNoise(position));
    }

    /// <summary>
    ///     Get the sub-biome determination noise for a chosen position, in block coordinates.
    ///     If available, the noise will be taken from the store instead of computing it.
    ///     A caching hint must be supplied for all accesses.
    /// </summary>
    public Single GetSubBiomeDeterminationNoise(Vector2i position, SamplingNoiseStore? store, Int32 cachingHint)
    {
        return store?.GetSubBiomeDeterminationNoise(position, this, cachingHint) ??
               ComputeSubBiomeDeterminationNoise(position);
    }

    /// <summary>
    ///     Get the oceanic sub-biome determination noise for a chosen position, in block coordinates.
    ///     If available, the noise will be taken from the store instead of computing it.
    ///     A caching hint must be supplied for all accesses.
    /// </summary>
    public Single GetOceanicSubBiomeDeterminationNoise(Vector2i position, SamplingNoiseStore? store, Int32 cachingHint)
    {
        return store?.GetOceanicSubBiomeDeterminationNoise(position, this, cachingHint) ??
               ComputeOceanicSubBiomeDeterminationNoise(position);
    }

    /// <summary>
    ///     Calculate the sub-biome determination noise for a given position.
    ///     This will always compute, therefore, prefer using <see cref="GetSubBiomeDeterminationNoise" /> if possible.
    /// </summary>
    public Single ComputeSubBiomeDeterminationNoise(Vector2i position)
    {
        return subBiomeDeterminationNoise.GetNoise(position);
    }

    /// <summary>
    ///     Calculate the oceanic sub-biome determination noise for a given position.
    ///     This will always compute, therefore, prefer using <see cref="GetOceanicSubBiomeDeterminationNoise" /> if possible.
    /// </summary>
    public Single ComputeOceanicSubBiomeDeterminationNoise(Vector2i position)
    {
        return oceanicSubBiomeDeterminationNoise.GetNoise(position);
    }
}

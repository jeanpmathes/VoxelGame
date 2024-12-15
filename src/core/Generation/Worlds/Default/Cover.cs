// <copyright file="Cover.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Toolkit.Noise;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     Cover is generated on top of the terrain. It can be used for one-block sized elements.
/// </summary>
public sealed class Cover : IDisposable
{
    private const Double FlowerFactor = 0.05;
    private readonly Boolean hasPlants;

    private NoiseGenerator? noise;

    /// <summary>
    ///     Create a new cover generator.
    /// </summary>
    /// <param name="hasPlants">Whether the cover should generate plants.</param>
    public Cover(Boolean hasPlants)
    {
        this.hasPlants = hasPlants;
    }

    /// <summary>
    ///     Set up used noise with the generation seed.
    /// </summary>
    public void SetUpNoise(NoiseBuilder builder)
    {
        noise = builder
            .WithType(NoiseType.GradientNoise)
            .WithFrequency(frequency: 0.5f)
            .Build();
    }

    /// <summary>
    ///     Get the cover for a given block.
    /// </summary>
    public Content GetContent(Vector3i position, Boolean isFilled, in Map.Sample sample)
    {
        Debug.Assert(noise != null);

        if (isFilled) return Content.Default;

        Temperature temperature = sample.GetRealTemperature(position.Y);

        if (temperature.IsFreezing)
            return new Content(Blocks.Instance.Specials.Snow.GetInstance(height: 1), FluidInstance.Default);

        if (!hasPlants) return Content.Default;

        // No grid noise is used here because this method is only called for a single block per column.

        Single value = noise.GetNoise(position);
        value = value > 0 ? value : value + 1;

        if (value < sample.Humidity) return value < sample.Humidity * FlowerFactor ? new Content(Blocks.Instance.Flower) : new Content(Blocks.Instance.TallGrass);

        return Content.Default;
    }

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) noise?.Dispose();
        else Throw.ForMissedDispose(this);

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Cover()
    {
        Dispose(disposing: false);
    }

    #endregion
}

// <copyright file="GeneratingNoise.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Toolkit.Noise;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     The noise generators used for generating the map.
/// </summary>
public sealed class GeneratingNoise : IDisposable
{
    /// <summary>
    ///     Create all noise generators used for the map generation.
    /// </summary>
    /// <param name="factory">The noise factory to use.</param>
    public GeneratingNoise(NoiseFactory factory)
    {
        Pieces = factory.CreateNext()
            .WithType(NoiseType.CellularNoise)
            .WithFrequency(frequency: 0.025f)
            .Build();

        Stone = factory.CreateNext()
            .WithType(NoiseType.GradientNoise)
            .WithFrequency(frequency: 0.03f)
            .Build();
    }

    /// <summary>
    ///     The noise used for creating map pieces, which are the parts that make up continents.
    /// </summary>
    public NoiseGenerator Pieces { get; }

    /// <summary>
    ///     The noise used to determine the stone types of the cells.
    /// </summary>
    public NoiseGenerator Stone { get; }

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        Pieces.Dispose();
        Stone.Dispose();
    }

    #endregion
}

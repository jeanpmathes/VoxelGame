// <copyright file="NoiseFactory.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Generation;

/// <summary>
///     Creates noise generators.
/// </summary>
public class NoiseFactory
{
    private readonly Random random;

    /// <summary>
    ///     Creates a new noise factory with an initial seed.
    /// </summary>
    /// <param name="seed">The seed to use.</param>
    public NoiseFactory(int seed)
    {
        random = new Random(seed);
    }


    /// <summary>
    ///     Get the next noise generator.
    /// </summary>
    /// <returns>A new noise generator, using a different seed.</returns>
    #pragma warning disable S4049 // Changes state, should be a method
    public FastNoiseLite GetNextNoise() => new(random.Next(int.MinValue, int.MaxValue));
    #pragma warning restore S4049 // Changes state, should be a method
}


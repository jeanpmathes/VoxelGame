﻿// <copyright file="NoiseFactory.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Toolkit.Noise;

namespace VoxelGame.Core.Generation.Worlds;

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
    public NoiseFactory(Int32 seed)
    {
        random = new Random(seed);
    }


    /// <summary>
    ///     Get the next noise builder.
    /// </summary>
    /// <returns>A new noise builder, using a different seed.</returns>
    public NoiseBuilder CreateNext()
    {
        return NoiseBuilder.Create(random.Next(Int32.MinValue, Int32.MaxValue));
    }
}

// <copyright file="NoiseGeneratorTests.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Collections;
using VoxelGame.Toolkit.Noise;
using Xunit;

namespace VoxelGame.Toolkit.Tests.Noise;

[TestSubject(typeof(NoiseGenerator))]
public class NoiseGeneratorTests
{
    private const Int32 Seed = 12345;
    private const Single Frequency = 0.025f;

    [Fact]
    public void NoiseGenerator_GradientNoise_ShouldProduceExpectedValues()
    {
        using NoiseGenerator generator = NoiseBuilder.Create(Seed)
            .WithType(NoiseType.GradientNoise)
            .WithFrequency(Frequency)
            .Build();

        AssertExpectedValues(generator, expected2D: 0.10147238f, expected3D: -0.049963772f);
    }

    [Fact]
    public void NoiseGenerator_CellularNoise_ShouldProduceExpectedValues()
    {
        using NoiseGenerator generator = NoiseBuilder.Create(Seed)
            .WithType(NoiseType.CellularNoise)
            .WithFrequency(Frequency)
            .Build();

        AssertExpectedValues(generator, expected2D: -0.63761735f, expected3D: -0.5763204f);
    }

    [Fact]
    public void NoiseGenerator_FractalGradientNoise_ShouldProduceExpectedValues()
    {
        using NoiseGenerator generator = NoiseBuilder.Create(Seed)
            .WithType(NoiseType.GradientNoise)
            .WithFrequency(Frequency)
            .WithFractals()
            .WithOctaves(octaves: 5)
            .WithLacunarity(lacunarity: 2.0f)
            .WithGain(gain: 0.5f)
            .WithWeightedStrength(weightedStrength: 0.0f)
            .Build();

        AssertExpectedValues(generator, expected2D: 0.55294716f, expected3D: -0.1272859f);
    }

    [Fact]
    public void NoiseGenerator_Noise2D_ShouldBeInRangeAndSingleNoiseEqualToGridNoise()
    {
        using NoiseGenerator generator = NoiseBuilder.Create(seed: 0).Build();

        Vector2i from = (-10, -25);
        const Int32 size = 50;

        Array2D<Single> noise = generator.GetNoiseGrid(from, size);

        foreach ((Int32 x, Int32 y) in MathTools.Range2(size, size))
        {
            Single gridValue = noise[x, y];
            Assert.InRange(gridValue, low: -1, high: 1);

            Single singleValue = generator.GetNoise(from + (x, y));
            Assert.Equal(gridValue, singleValue, precision: 5);
        }
    }

    [Fact]
    public void NoiseGenerator_Noise3D_ShouldBeInRangeAndSingleNoiseEqualToGridNoise()
    {
        using NoiseGenerator generator = NoiseBuilder.Create(seed: 0).Build();

        Vector3i from = (-10, -25, -10);
        const Int32 size = 50;

        Array3D<Single> noise = generator.GetNoiseGrid(from, size);

        foreach ((Int32 x, Int32 y, Int32 z) in MathTools.Range3(size, size, size))
        {
            Single gridValue = noise[x, y, z];
            Assert.InRange(gridValue, low: -1, high: 1);

            Single singleValue = generator.GetNoise(from + (x, y, z));
            Assert.Equal(gridValue, singleValue, precision: 5);
        }
    }

    private static void AssertExpectedValues(NoiseGenerator generator, Single expected2D, Single expected3D)
    {
        Assert.Equal(expected2D, generator.GetNoise(new Vector2i(x: 37, y: -19)), precision: 7);
        Assert.Equal(expected3D, generator.GetNoise(new Vector3i(x: 37, y: -19, z: 11)), precision: 7);
    }
}

// <copyright file="NoiseGeneratorTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
    [Fact]
    public void NoiseGenerator_Noise2D_ShouldBeInRangeAndSingleNoiseEqualToGridNoise()
    {
        using NoiseGenerator generator = NoiseBuilder.Create(seed: 0).Build();

        Vector2i from = (-10, -25);
        const Int32 size = 50;

        Array2D<Single> noise = generator.GetNoiseGrid(from, size);

        foreach ((Int32 x, Int32 y) in MathTool.Range2(size, size))
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

        foreach ((Int32 x, Int32 y, Int32 z) in MathTool.Range3(size, size, size))
        {
            Single gridValue = noise[x, y, z];
            Assert.InRange(gridValue, low: -1, high: 1);

            Single singleValue = generator.GetNoise(from + (x, y, z));
            Assert.Equal(gridValue, singleValue, precision: 5);
        }
    }
}

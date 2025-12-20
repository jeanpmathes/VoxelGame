// <copyright file="NoiseGeneratorTests.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
}

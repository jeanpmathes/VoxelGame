// <copyright file="RadiusTests.cs" company="VoxelGame">
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

using JetBrains.Annotations;
using VoxelGame.GUI.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Utilities;

[TestSubject(typeof(Radius))]
public class RadiusTests
{
    [Fact]
    public void Radius_Constructor_ShouldSetRadiusUniformly()
    {
        Radius radius = new(5f);

        Assert.Equal(expected: 5f, radius.X);
        Assert.Equal(expected: 5f, radius.Y);
    }

    [Fact]
    public void Radius_ToString_ShouldReturnZeroFormatForZeroRadius()
    {
        Assert.Equal("Radius.Zero", Radius.Zero.ToString());
    }

    [Fact]
    public void Radius_ToString_ShouldReturnNonZeroFormatForNonZeroRadius()
    {
        Assert.Equal("Radius(X: 2, Y: 3)", new Radius(X: 2f, Y: 3f).ToString());
    }
}

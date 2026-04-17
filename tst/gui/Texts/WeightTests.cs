// <copyright file="WeightTests.cs" company="VoxelGame">
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
using VoxelGame.GUI.Texts;
using Xunit;

namespace VoxelGame.GUI.Tests.Texts;

public class WeightTests
{
    [Theory]
    [InlineData((Int16) 0)]
    [InlineData((Int16) (-1))]
    [InlineData((Int16) 1000)]
    public void Weight_Constructor_ShouldThrowForOutOfRangeValues(Int16 value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new Weight(value));
    }

    [Theory]
    [InlineData((Int16) 1)]
    [InlineData((Int16) 400)]
    [InlineData((Int16) 999)]
    public void Weight_Constructor_ShouldAllowInRangeValues(Int16 value)
    {
        Weight weight = new(value);

        Assert.Equal(value, weight.Value);
    }

    [Fact]
    public void Weight_GreaterThan_ShouldWork()
    {
        Assert.True(Weight.Bold > Weight.Normal);
        Assert.False(Weight.Normal > Weight.Bold);
    }

    [Fact]
    public void Weight_LessThan_ShouldWork()
    {
        Assert.True(Weight.Normal < Weight.Bold);
        Assert.False(Weight.Bold < Weight.Normal);
    }

    [Fact]
    public void Weight_GreaterThanOrEqual_ShouldWork()
    {
        Assert.True(Weight.Bold >= Weight.Normal);
        Assert.False(Weight.Normal >= Weight.Bold);
    }

    [Fact]
    public void Weight_LessThanOrEqual_ShouldWork()
    {
        Assert.True(Weight.Normal <= Weight.Bold);
        Assert.False(Weight.Bold <= Weight.Normal);
    }
}

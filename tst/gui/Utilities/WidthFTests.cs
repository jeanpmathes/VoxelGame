// <copyright file="WidthFTests.cs" company="VoxelGame">
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

using VoxelGame.GUI.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Utilities;

public class WidthFTests
{
    [Fact]
    public void WidthF_Constructor_ShouldSetValue()
    {
        WidthF width = new(5f);

        Assert.Equal(expected: 5f, width.Value);
    }

    [Fact]
    public void WidthF_ToString_ShouldReturnZeroFormatForZeroRadius()
    {
        Assert.Equal("WidthF.Zero", WidthF.Zero.ToString());
    }

    [Fact]
    public void WidthF_ToString_ShouldReturnNonZeroFormatForNonZeroRadius()
    {
        Assert.Equal("WidthF(Value: 5)", new WidthF(5.0f).ToString());
    }
}

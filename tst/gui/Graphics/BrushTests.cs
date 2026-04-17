// <copyright file="BrushTests.cs" company="VoxelGame">
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

using System.Drawing;
using VoxelGame.GUI.Graphics;
using Xunit;

namespace VoxelGame.GUI.Tests.Graphics;

public class BrushTests
{
    [Fact]
    public void Brush_TransparentBrush_ShouldEqualAllInstances()
    {
        TransparentBrush brush1 = new();
        TransparentBrush brush2 = new();

        Assert.True(brush1.Equals(brush2));
    }

    [Fact]
    public void Brush_TransparentBrush_ShouldReturnSameHashCodeForAllInstances()
    {
        TransparentBrush brush1 = new();
        TransparentBrush brush2 = new();

        Assert.Equal(brush1.GetHashCode(), brush2.GetHashCode());
    }

    [Fact]
    public void Brush_SolidColorBrush_ShouldBeEqualIfSameColor()
    {
        SolidColorBrush first = new(Color.Red);
        SolidColorBrush second = new(Color.Red);

        Assert.True(first.Equals(second));
    }
}

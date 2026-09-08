// <copyright file="ThicknessTests.cs" company="VoxelGame">
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

[TestSubject(typeof(Thickness))]
public class ThicknessTests
{
    [Fact]
    public void Thickness_Constructor_ShouldSetAllSidesUniformly()
    {
        Thickness thickness = new(5f);

        Assert.Equal(expected: 5f, thickness.Left);
        Assert.Equal(expected: 5f, thickness.Top);
        Assert.Equal(expected: 5f, thickness.Right);
        Assert.Equal(expected: 5f, thickness.Bottom);
    }

    [Fact]
    public void Thickness_Constructor_ShouldSetEachSideToSpecifiedValue()
    {
        Thickness thickness = new(left: 1f, top: 2f, right: 3f, bottom: 4f);

        Assert.Equal(expected: 1f, thickness.Left);
        Assert.Equal(expected: 2f, thickness.Top);
        Assert.Equal(expected: 3f, thickness.Right);
        Assert.Equal(expected: 4f, thickness.Bottom);
    }

    [Fact]
    public void Thickness_Addition_Size_ShouldIncreaseSizeByThicknessOfBothSidesOfDimension()
    {
        Size size = new(Width: 100f, Height: 50f);
        Thickness thickness = new(left: 1f, top: 2f, right: 3f, bottom: 4f);

        Size result = size + thickness;

        Assert.Equal(new Size(Width: 104f, Height: 56f), result);
    }

    [Fact]
    public void Thickness_Subtraction_Size_ShouldDecreaseSizeByThicknessOfBothSidesOfDimension()
    {
        Size size = new(Width: 100f, Height: 50f);
        Thickness thickness = new(left: 1f, top: 2f, right: 3f, bottom: 4f);

        Size result = size - thickness;

        Assert.Equal(new Size(Width: 96f, Height: 44f), result);
    }

    [Fact]
    public void Thickness_Addition_Rectangle_ShouldChangeBothPositionAndSizeAccordingly()
    {
        Rectangle rect = new(X: 10f, Y: 20f, Width: 100f, Height: 50f);
        Thickness thickness = new(left: 1f, top: 2f, right: 3f, bottom: 4f);

        Rectangle result = rect + thickness;

        Assert.Equal(expected: 9f, result.X);
        Assert.Equal(expected: 18f, result.Y);
        Assert.Equal(expected: 104f, result.Width);
        Assert.Equal(expected: 56f, result.Height);
    }

    [Fact]
    public void Thickness_Subtraction_Rectangle_ShouldChangeBothPositionAndSizeAccordingly()
    {
        Rectangle rect = new(X: 10f, Y: 20f, Width: 100f, Height: 50f);
        Thickness thickness = new(left: 1f, top: 2f, right: 3f, bottom: 4f);

        Rectangle result = rect - thickness;

        Assert.Equal(expected: 11f, result.X);
        Assert.Equal(expected: 22f, result.Y);
        Assert.Equal(expected: 96f, result.Width);
        Assert.Equal(expected: 44f, result.Height);
    }

    [Fact]
    public void Thickness_ToString_ShouldReturnZeroFormatForZeroThickness()
    {
        Assert.Equal("Thickness.Zero", Thickness.Zero.ToString());
    }

    [Fact]
    public void Thickness_ToString_ShouldReturnNonZeroFormatForNonZeroThickness()
    {
        Assert.Equal("Thickness(Left: 1, Top: 2, Right: 3, Bottom: 4)", new Thickness(left: 1f, top: 2f, right: 3f, bottom: 4f).ToString());
    }
}

// <copyright file="ThicknessFTests.cs" company="VoxelGame">
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
using VoxelGame.GUI.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Utilities;

public class ThicknessFTests
{
    [Fact]
    public void ThicknessF_Constructor_ShouldSetAllSidesUniformly()
    {
        ThicknessF thickness = new(5f);

        Assert.Equal(expected: 5f, thickness.Left);
        Assert.Equal(expected: 5f, thickness.Top);
        Assert.Equal(expected: 5f, thickness.Right);
        Assert.Equal(expected: 5f, thickness.Bottom);
    }

    [Fact]
    public void ThicknessF_Constructor_ShouldSetEachSideToSpecifiedValue()
    {
        ThicknessF thickness = new(left: 1f, top: 2f, right: 3f, bottom: 4f);

        Assert.Equal(expected: 1f, thickness.Left);
        Assert.Equal(expected: 2f, thickness.Top);
        Assert.Equal(expected: 3f, thickness.Right);
        Assert.Equal(expected: 4f, thickness.Bottom);
    }

    [Fact]
    public void ThicknessF_Addition_SizeF_ShouldIncreaseSizeByThicknessOfBothSidesOfDimension()
    {
        SizeF size = new(width: 100f, height: 50f);
        ThicknessF thickness = new(left: 1f, top: 2f, right: 3f, bottom: 4f);

        SizeF result = size + thickness;

        Assert.Equal(new SizeF(width: 104f, height: 56f), result);
    }

    [Fact]
    public void ThicknessF_Subtraction_SizeF_ShouldDecreaseSizeByThicknessOfBothSidesOfDimension()
    {
        SizeF size = new(width: 100f, height: 50f);
        ThicknessF thickness = new(left: 1f, top: 2f, right: 3f, bottom: 4f);

        SizeF result = size - thickness;

        Assert.Equal(new SizeF(width: 96f, height: 44f), result);
    }

    [Fact]
    public void ThicknessF_Addition_RectangleF_ShouldChangeBothPositionAndSizeAccordingly()
    {
        RectangleF rect = new(x: 10f, y: 20f, width: 100f, height: 50f);
        ThicknessF thickness = new(left: 1f, top: 2f, right: 3f, bottom: 4f);

        RectangleF result = rect + thickness;

        Assert.Equal(expected: 9f, result.X);
        Assert.Equal(expected: 18f, result.Y);
        Assert.Equal(expected: 104f, result.Width);
        Assert.Equal(expected: 56f, result.Height);
    }

    [Fact]
    public void ThicknessF_Subtraction_RactangleF_ShouldChangeBothPositionAndSizeAccordingly()
    {
        RectangleF rect = new(x: 10f, y: 20f, width: 100f, height: 50f);
        ThicknessF thickness = new(left: 1f, top: 2f, right: 3f, bottom: 4f);

        RectangleF result = rect - thickness;

        Assert.Equal(expected: 11f, result.X);
        Assert.Equal(expected: 22f, result.Y);
        Assert.Equal(expected: 96f, result.Width);
        Assert.Equal(expected: 44f, result.Height);
    }

    [Fact]
    public void ThicknessF_ToString_ShouldReturnZeroFormatForZeroThickness()
    {
        Assert.Equal("ThicknessF.Zero", ThicknessF.Zero.ToString());
    }

    [Fact]
    public void ThicknessF_ToString_ShouldReturnNonZeroFormatForNonZeroThickness()
    {
        Assert.Equal("ThicknessF(Left: 1, Top: 2, Right: 3, Bottom: 4)", new ThicknessF(left: 1f, top: 2f, right: 3f, bottom: 4f).ToString());
    }
}

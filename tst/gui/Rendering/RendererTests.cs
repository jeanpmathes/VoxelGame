// <copyright file="RendererTests.cs" company="VoxelGame">
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
using VoxelGame.GUI.Rendering;
using VoxelGame.GUI.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Rendering;

[TestSubject(typeof(Renderer))]
public class RendererTests
{
    private readonly MockRenderer renderer = new();

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_Scale_Point_ShouldMultiplyAllComponents(Single scale)
    {
        renderer.OnScale(scale);

        Point point = new(X: 5.0f, Y: 10.0f);
        Point result = renderer.ApplyScale(point);

        Assert.Equal(point.X * scale, result.X);
        Assert.Equal(point.Y * scale, result.Y);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_InverseScale_Point_ShouldDivideAllComponents(Single scale)
    {
        renderer.OnScale(scale);

        Point point = new(X: 5.0f, Y: 10.0f);
        Point result = renderer.ApplyInverseScale(point);

        Assert.Equal(point.X / scale, result.X);
        Assert.Equal(point.Y / scale, result.Y);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_Scale_Size_ShouldMultiplyAllComponents(Single scale)
    {
        renderer.OnScale(scale);

        Size size = new(Width: 3.0f, Height: 5.0f);
        Size result = renderer.ApplyScale(size);

        Assert.Equal(size.Width * scale, result.Width);
        Assert.Equal(size.Height * scale, result.Height);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_InverseScale_Size_ShouldDivideAllComponents(Single scale)
    {
        renderer.OnScale(scale);

        Size size = new(Width: 6.0f, Height: 10.0f);
        Size result = renderer.ApplyInverseScale(size);

        Assert.Equal(size.Width / scale, result.Width);
        Assert.Equal(size.Height / scale, result.Height);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_Scale_Rectangle_ShouldMultiplyAllComponents(Single scale)
    {
        renderer.OnScale(scale);

        Rectangle rectangle = new(X: 3.0f, Y: 5.0f, Width: 10.0f, Height: 20.0f);
        Rectangle result = renderer.ApplyScale(rectangle);

        Assert.Equal(rectangle.X * scale, result.X);
        Assert.Equal(rectangle.Y * scale, result.Y);
        Assert.Equal(rectangle.Width * scale, result.Width);
        Assert.Equal(rectangle.Height * scale, result.Height);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_InverseScale_Rectangle_ShouldDivideAllComponents(Single scale)
    {
        renderer.OnScale(scale);

        Rectangle rectangle = new(X: 6.0f, Y: 10.0f, Width: 20.0f, Height: 40.0f);
        Rectangle result = renderer.ApplyInverseScale(rectangle);

        Assert.Equal(rectangle.X / scale, result.X);
        Assert.Equal(rectangle.Y / scale, result.Y);
        Assert.Equal(rectangle.Width / scale, result.Width);
        Assert.Equal(rectangle.Height / scale, result.Height);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_Scale_ThicknessF_ShouldMultiplyAllComponents(Single scale)
    {
        renderer.OnScale(scale);

        Thickness thickness = new(left: 1.0f, top: 2.0f, right: 3.0f, bottom: 4.0f);
        Thickness result = renderer.ApplyScale(thickness);

        Assert.Equal(thickness.Left * scale, result.Left);
        Assert.Equal(thickness.Top * scale, result.Top);
        Assert.Equal(thickness.Right * scale, result.Right);
        Assert.Equal(thickness.Bottom * scale, result.Bottom);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_InverseScale_ThicknessF_ShouldDivideAllComponents(Single scale)
    {
        renderer.OnScale(scale);

        Thickness thickness = new(left: 2.0f, top: 4.0f, right: 6.0f, bottom: 8.0f);
        Thickness result = renderer.ApplyInverseScale(thickness);

        Assert.Equal(thickness.Left / scale, result.Left);
        Assert.Equal(thickness.Top / scale, result.Top);
        Assert.Equal(thickness.Right / scale, result.Right);
        Assert.Equal(thickness.Bottom / scale, result.Bottom);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_Scale_RadiusF_ShouldMultiplyAllComponents(Single scale)
    {
        renderer.OnScale(scale);

        Radius radius = new(X: 3.0f, Y: 5.0f);
        Radius result = renderer.ApplyScale(radius);

        Assert.Equal(radius.X * scale, result.X);
        Assert.Equal(radius.Y * scale, result.Y);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_InverseScale_RadiusF_ShouldDivideAllComponents(Single scale)
    {
        renderer.OnScale(scale);

        Radius radius = new(X: 6.0f, Y: 10.0f);
        Radius result = renderer.ApplyInverseScale(radius);

        Assert.Equal(radius.X / scale, result.X);
        Assert.Equal(radius.Y / scale, result.Y);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_Scale_WidthF_ShouldMultiplyValue(Single scale)
    {
        renderer.OnScale(scale);

        Width width = new(Value: 3.0f);
        Width result = renderer.ApplyScale(width);

        Assert.Equal(width.Value * scale, result.Value);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_InverseScale_WidthF_ShouldDivideValue(Single scale)
    {
        renderer.OnScale(scale);

        Width width = new(Value: 6.0f);
        Width result = renderer.ApplyInverseScale(width);

        Assert.Equal(width.Value / scale, result.Value);
    }
}

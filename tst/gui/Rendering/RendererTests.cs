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
using System.Drawing;
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
    public void Renderer_Scale_PointF_ShouldMultiplyAllComponents(Single scale)
    {
        renderer.Scale(scale);

        PointF point = new(x: 5.0f, y: 10.0f);
        PointF result = renderer.ApplyScale(point);

        Assert.Equal(point.X * scale, result.X);
        Assert.Equal(point.Y * scale, result.Y);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_InverseScale_PointF_ShouldDivideAllComponents(Single scale)
    {
        renderer.Scale(scale);

        PointF point = new(x: 5.0f, y: 10.0f);
        PointF result = renderer.ApplyInverseScale(point);

        Assert.Equal(point.X / scale, result.X);
        Assert.Equal(point.Y / scale, result.Y);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_Scale_SizeF_ShouldMultiplyAllComponents(Single scale)
    {
        renderer.Scale(scale);

        SizeF size = new(width: 3.0f, height: 5.0f);
        SizeF result = renderer.ApplyScale(size);

        Assert.Equal(size.Width * scale, result.Width);
        Assert.Equal(size.Height * scale, result.Height);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_InverseScale_SizeF_ShouldDivideAllComponents(Single scale)
    {
        renderer.Scale(scale);

        SizeF size = new(width: 6.0f, height: 10.0f);
        SizeF result = renderer.ApplyInverseScale(size);

        Assert.Equal(size.Width / scale, result.Width);
        Assert.Equal(size.Height / scale, result.Height);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_Scale_RectangleF_ShouldMultiplyAllComponents(Single scale)
    {
        renderer.Scale(scale);

        RectangleF rectangle = new(x: 3.0f, y: 5.0f, width: 10.0f, height: 20.0f);
        RectangleF result = renderer.ApplyScale(rectangle);

        Assert.Equal(rectangle.X * scale, result.X);
        Assert.Equal(rectangle.Y * scale, result.Y);
        Assert.Equal(rectangle.Width * scale, result.Width);
        Assert.Equal(rectangle.Height * scale, result.Height);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_InverseScale_RectangleF_ShouldDivideAllComponents(Single scale)
    {
        renderer.Scale(scale);

        RectangleF rectangle = new(x: 6.0f, y: 10.0f, width: 20.0f, height: 40.0f);
        RectangleF result = renderer.ApplyInverseScale(rectangle);

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
        renderer.Scale(scale);

        ThicknessF thickness = new(left: 1.0f, top: 2.0f, right: 3.0f, bottom: 4.0f);
        ThicknessF result = renderer.ApplyScale(thickness);

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
        renderer.Scale(scale);

        ThicknessF thickness = new(left: 2.0f, top: 4.0f, right: 6.0f, bottom: 8.0f);
        ThicknessF result = renderer.ApplyInverseScale(thickness);

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
        renderer.Scale(scale);

        RadiusF radius = new(x: 3.0f, y: 5.0f);
        RadiusF result = renderer.ApplyScale(radius);

        Assert.Equal(radius.X * scale, result.X);
        Assert.Equal(radius.Y * scale, result.Y);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_InverseScale_RadiusF_ShouldDivideAllComponents(Single scale)
    {
        renderer.Scale(scale);

        RadiusF radius = new(x: 6.0f, y: 10.0f);
        RadiusF result = renderer.ApplyInverseScale(radius);

        Assert.Equal(radius.X / scale, result.X);
        Assert.Equal(radius.Y / scale, result.Y);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_Scale_WidthF_ShouldMultiplyValue(Single scale)
    {
        renderer.Scale(scale);

        WidthF width = new(value: 3.0f);
        WidthF result = renderer.ApplyScale(width);

        Assert.Equal(width.Value * scale, result.Value);
    }

    [Theory]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void Renderer_InverseScale_WidthF_ShouldDivideValue(Single scale)
    {
        renderer.Scale(scale);

        WidthF width = new(value: 6.0f);
        WidthF result = renderer.ApplyInverseScale(width);

        Assert.Equal(width.Value / scale, result.Value);
    }
}

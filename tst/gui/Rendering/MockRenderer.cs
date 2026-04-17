// <copyright file="MockRenderer.cs" company="VoxelGame">
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
using VoxelGame.GUI.Graphics;
using VoxelGame.GUI.Rendering;
using VoxelGame.GUI.Texts;
using VoxelGame.GUI.Utilities;
using Brush = VoxelGame.GUI.Graphics.Brush;
using Font = VoxelGame.GUI.Texts.Font;

namespace VoxelGame.GUI.Tests.Rendering;

public class MockRenderer : Renderer
{
    public override void Begin() {}

    public override void End() {}

    public override void PushOffset(PointF offset) {}

    public override void PopOffset() {}

    public override void PushClip(RectangleF rectangle) {}

    public override void PopClip() {}

    public override void BeginClip() {}

    public override void EndClip() {}

    public override Boolean IsClipEmpty()
    {
        return false;
    }

    public override void PushOpacity(Single opacity) {}

    public override void PopOpacity() {}

    public override IFormattedText CreateFormattedText(String text, Font font, TextOptions options)
    {
        return new MockFormattedText();
    }

    public override void DrawFilledRectangle(RectangleF rectangle, RadiusF corners, Brush brush) {}

    public override void DrawLinedRectangle(RectangleF rectangle, WidthF width, RadiusF corners, StrokeStyle stroke, Brush brush) {}

    public override void Resize(Size size) {}

    public new PointF ApplyScale(PointF point)
    {
        return base.ApplyScale(point);
    }

    public new SizeF ApplyScale(SizeF size)
    {
        return base.ApplyScale(size);
    }

    public new RectangleF ApplyScale(RectangleF rectangle)
    {
        return base.ApplyScale(rectangle);
    }

    public new ThicknessF ApplyScale(ThicknessF thickness)
    {
        return base.ApplyScale(thickness);
    }

    public new RadiusF ApplyScale(RadiusF radius)
    {
        return base.ApplyScale(radius);
    }

    public new WidthF ApplyScale(WidthF width)
    {
        return base.ApplyScale(width);
    }

    public new PointF ApplyInverseScale(PointF point)
    {
        return base.ApplyInverseScale(point);
    }

    public new SizeF ApplyInverseScale(SizeF size)
    {
        return base.ApplyInverseScale(size);
    }

    public new RectangleF ApplyInverseScale(RectangleF rectangle)
    {
        return base.ApplyInverseScale(rectangle);
    }

    public new ThicknessF ApplyInverseScale(ThicknessF thickness)
    {
        return base.ApplyInverseScale(thickness);
    }

    public new RadiusF ApplyInverseScale(RadiusF radius)
    {
        return base.ApplyInverseScale(radius);
    }

    public new WidthF ApplyInverseScale(WidthF width)
    {
        return base.ApplyInverseScale(width);
    }
}

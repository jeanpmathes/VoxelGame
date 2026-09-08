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
using VoxelGame.GUI.Drawing;
using VoxelGame.GUI.Drawing.Brushes;
using VoxelGame.GUI.Rendering;
using VoxelGame.GUI.Texts;
using VoxelGame.GUI.Utilities;

namespace VoxelGame.GUI.Tests.Rendering;

public class MockRenderer : Renderer
{
    public override void Reset() {}

    public override void Submit() {}

    public override void PushOffset(Point offset) {}

    public override void PopOffset() {}

    public override void PushClip(Rectangle rectangle) {}

    public override void PopClip() {}

    public override void PushOpacity(Single opacity) {}

    public override void PopOpacity() {}

    public override IFormattedText CreateFormattedText(String text, TextOptions options)
    {
        return new MockFormattedText();
    }

    public override void DrawFilledRectangle(Rectangle rectangle, Radius corners, Brush brush) {}

    public override void DrawLinedRectangle(Rectangle rectangle, Width width, Radius corners, StrokeStyle stroke, Brush brush) {}

    public new Point ApplyScale(Point point)
    {
        return base.ApplyScale(point);
    }

    public new Size ApplyScale(Size size)
    {
        return base.ApplyScale(size);
    }

    public new Rectangle ApplyScale(Rectangle rectangle)
    {
        return base.ApplyScale(rectangle);
    }

    public new Thickness ApplyScale(Thickness thickness)
    {
        return base.ApplyScale(thickness);
    }

    public new Radius ApplyScale(Radius radius)
    {
        return base.ApplyScale(radius);
    }

    public new Width ApplyScale(Width width)
    {
        return base.ApplyScale(width);
    }

    public new Point ApplyInverseScale(Point point)
    {
        return base.ApplyInverseScale(point);
    }

    public new Size ApplyInverseScale(Size size)
    {
        return base.ApplyInverseScale(size);
    }

    public new Rectangle ApplyInverseScale(Rectangle rectangle)
    {
        return base.ApplyInverseScale(rectangle);
    }

    public new Thickness ApplyInverseScale(Thickness thickness)
    {
        return base.ApplyInverseScale(thickness);
    }

    public new Radius ApplyInverseScale(Radius radius)
    {
        return base.ApplyInverseScale(radius);
    }

    public new Width ApplyInverseScale(Width width)
    {
        return base.ApplyInverseScale(width);
    }
}

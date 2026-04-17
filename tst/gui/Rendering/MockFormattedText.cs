// <copyright file="MockFormattedText.cs" company="VoxelGame">
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
using VoxelGame.GUI.Texts;
using Brush = VoxelGame.GUI.Graphics.Brush;

namespace VoxelGame.GUI.Tests.Rendering;

public sealed class MockFormattedText : IFormattedText
{
    public SizeF Measure(SizeF availableSize)
    {
        return new SizeF(width: 42, height: 24);
    }

    public void Draw(RectangleF rectangle, Brush brush) {}

    public void Dispose() {}
}

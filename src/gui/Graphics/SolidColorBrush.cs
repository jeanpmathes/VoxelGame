// <copyright file="SolidColorBrush.cs" company="VoxelGame">
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

namespace VoxelGame.GUI.Graphics;

/// <summary>
///     A brush that fills with a solid color.
/// </summary>
public class SolidColorBrush(Color color) : Brush
{
    /// <summary>
    ///     The color of the brush.
    /// </summary>
    public Color Color { get; } = color;

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is SolidColorBrush solidColorBrush && Color.Equals(solidColorBrush.Color);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return Color.GetHashCode();
    }
}

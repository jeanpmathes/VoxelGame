// <copyright file="HorizontalAlignment.cs" company="VoxelGame">
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

namespace VoxelGame.GUI;

/// <summary>
///     Defines the horizontal alignment of a visual within its parent.
/// </summary>
public enum HorizontalAlignment
{
    /// <summary>
    ///     The visual is stretched horizontally to fill the available space within its parent.
    /// </summary>
    Stretch,

    /// <summary>
    ///     The visual is aligned to the left edge of its parent.
    /// </summary>
    Left,

    /// <summary>
    ///     The visual is centered horizontally within its parent.
    /// </summary>
    Center,

    /// <summary>
    ///     The visual is aligned to the right edge of its parent.
    /// </summary>
    Right
}

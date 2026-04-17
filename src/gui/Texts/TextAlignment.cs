// <copyright file="TextAlignment.cs" company="VoxelGame">
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

namespace VoxelGame.GUI.Texts;

/// <summary>
///     Specifies the horizontal alignment of text within its layout bounds.
/// </summary>
public enum TextAlignment : Byte
{
    /// <summary>
    ///     Text is aligned to the leading edge (left in left-to-right layouts).
    /// </summary>
    Leading,

    /// <summary>
    ///     Text is centered within the layout bounds.
    /// </summary>
    Center,

    /// <summary>
    ///     Text is aligned to the trailing edge (right in left-to-right layouts).
    /// </summary>
    Trailing,

    /// <summary>
    ///     Text is justified to fill the full width of the layout bounds.
    /// </summary>
    Justify
}

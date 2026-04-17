// <copyright file="TextWrapping.cs" company="VoxelGame">
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
///     Specifies how text wraps when it exceeds the width of its layout bounds.
/// </summary>
public enum TextWrapping : Byte
{
    /// <summary>
    ///     Text does not wrap; it overflows beyond the layout bounds.
    /// </summary>
    NoWrap,

    /// <summary>
    ///     Text wraps at word boundaries when it exceeds the layout width.
    /// </summary>
    Wrap
}

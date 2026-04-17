// <copyright file="TextTrimming.cs" company="VoxelGame">
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
///     Specifies how text is trimmed when it overflows its layout bounds.
/// </summary>
public enum TextTrimming : Byte
{
    /// <summary>
    ///     Text is not trimmed; it may overflow the layout bounds.
    /// </summary>
    None,

    /// <summary>
    ///     Text is clipped at a character boundary with no ellipsis.
    /// </summary>
    Character,

    /// <summary>
    ///     Text is clipped at a word boundary with no ellipsis.
    /// </summary>
    Word,

    /// <summary>
    ///     Text is trimmed at a character boundary and an ellipsis is appended.
    /// </summary>
    CharacterEllipsis,

    /// <summary>
    ///     Text is trimmed at a word boundary and an ellipsis is appended.
    /// </summary>
    WordEllipsis,

    /// <summary>
    ///     Text is trimmed in the middle and an ellipsis is inserted, preserving the end of the string.
    ///     Useful for displaying file paths or similar strings where the tail is important.
    /// </summary>
    PathEllipsis
}

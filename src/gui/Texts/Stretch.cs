// <copyright file="Stretch.cs" company="VoxelGame">
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
///     The stretch degree of a font.
/// </summary>
public enum Stretch : Byte
{
    /// <summary>
    ///     The font is ultra-condensed.
    /// </summary>
    UltraCondensed,

    /// <summary>
    ///     The font is extra-condensed.
    /// </summary>
    ExtraCondensed,

    /// <summary>
    ///     The font is condensed.
    /// </summary>
    Condensed,

    /// <summary>
    ///     The font is semi-condensed.
    /// </summary>
    SemiCondensed,

    /// <summary>
    ///     The font is normal (not condensed or expanded).
    /// </summary>
    Normal,

    /// <summary>
    ///     The font is semi-expanded.
    /// </summary>
    SemiExpanded,

    /// <summary>
    ///     The font is expanded.
    /// </summary>
    Expanded,

    /// <summary>
    ///     The font is extra-expanded.
    /// </summary>
    ExtraExpanded,

    /// <summary>
    ///     The font is ultra-expanded.
    /// </summary>
    UltraExpanded
}

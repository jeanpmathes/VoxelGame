// <copyright file="StrokeStyle.cs" company="VoxelGame">
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

namespace VoxelGame.GUI.Graphics;

/// <summary>
///     The style of a stroke, for example, the line around a border.
/// </summary>
public enum StrokeStyle : Byte
{
    /// <summary>
    ///     A solid stroke. The default stroke.
    /// </summary>
    Solid = 0,

    /// <summary>
    ///     A dashed stroke.
    /// </summary>
    Dashes = 1,

    /// <summary>
    ///     A squared stroke, consisting of many squares.
    ///     Essentially like <see cref="Dotted" />, but not round.
    /// </summary>
    Squared = 2,

    /// <summary>
    ///     A dotted stroke, using round markers.
    /// </summary>
    Dotted = 3
}

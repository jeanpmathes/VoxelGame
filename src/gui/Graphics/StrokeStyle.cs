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

namespace VoxelGame.GUI.Graphics;

/// <summary>
///     The style of a stroke, for example the line around a border.
/// </summary>
public enum StrokeStyle
{
    /// <summary>
    ///     A solid stroke. The default stroke.
    /// </summary>
    Solid,

    /// <summary>
    ///     A dashed stroke.
    /// </summary>
    Dashes,

    /// <summary>
    ///     A squared stroke, consisting of many squares.
    ///     Essentially like <see cref="Dotted" />, but not round.
    /// </summary>
    Squared,

    /// <summary>
    ///     A dotted stroke, using round markers.
    /// </summary>
    Dotted
}

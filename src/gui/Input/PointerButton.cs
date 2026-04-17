// <copyright file="PointerButton.cs" company="VoxelGame">
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

namespace VoxelGame.GUI.Input;

/// <summary>
///     The buttons of the pointer device (e.g. mouse buttons).
/// </summary>
public enum PointerButton
{
    /// <summary>
    ///     The left mouse button.
    /// </summary>
    Left,

    /// <summary>
    ///     The right mouse button.
    /// </summary>
    Right,

    /// <summary>
    ///     The middle mouse button (usually the scroll wheel button).
    /// </summary>
    Middle,

    /// <summary>
    ///     The first additional mouse button (e.g. "back" button on some mice).
    /// </summary>
    Button4,

    /// <summary>
    ///     The second additional mouse button (e.g. "forward" button on some mice).
    /// </summary>
    Button5,

    /// <summary>
    ///     Represents an invalid or unrecognized pointer button.
    /// </summary>
    Invalid
}

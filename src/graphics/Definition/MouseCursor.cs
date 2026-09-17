// <copyright file="MouseCursor.cs" company="VoxelGame">
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
using System.Diagnostics.CodeAnalysis;

namespace VoxelGame.Graphics.Definition;

/// <summary>
///     Mouse cursor types.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum MouseCursor : Byte
{
    /// <summary>
    ///     The standard arrow cursor.
    /// </summary>
    Arrow,

    /// <summary>
    ///     An I-like cursor, which shows where the text cursor will appear when clicking.
    /// </summary>
    IBeam,

    /// <summary>
    ///     A cursor for sizing along the north-south direction.
    /// </summary>
    SizeNS,

    /// <summary>
    ///     A cursor for sizing along the east-west direction.
    /// </summary>
    SizeWE,

    /// <summary>
    ///     A cursor for sizing along the diagonal direction, from the north-west corner to the south-east corner.
    /// </summary>
    SizeNWSE,

    /// <summary>
    ///     A cursor for sizing along the diagonal direction, from the south-east corner to the north-west corner.
    /// </summary>
    SizeNESW,

    /// <summary>
    ///     A cursor for sizing along all directions.
    /// </summary>
    SizeAll,

    /// <summary>
    ///     A cursor to show above an invalid area.
    /// </summary>
    No,

    /// <summary>
    ///     A cursor to show when waiting for something.
    /// </summary>
    Wait,

    /// <summary>
    ///     A cursor that looks like a hand.
    /// </summary>
    Hand
}

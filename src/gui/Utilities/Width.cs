// <copyright file="Width.cs" company="VoxelGame">
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

namespace VoxelGame.GUI.Utilities;

/// <summary>
///     A width of lines.
/// </summary>
public readonly record struct Width(Single Value)
{
    /// <summary>
    ///     A width of size zero.
    /// </summary>
    public static Width Zero { get; } = new(0);

    /// <summary>
    ///     A width of size one.
    /// </summary>
    public static Width One { get; } = new(1);

    /// <summary>
    ///     Create a <see cref="Thickness" /> from this width.
    /// </summary>
    /// <returns>A <see cref="Thickness" /> with the same width for all sides.</returns>
    public Thickness ToThickness()
    {
        return new Thickness(Value);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return this == Zero
            ? "Width.Zero"
            : $"Width(Value: {Value})";
    }
}

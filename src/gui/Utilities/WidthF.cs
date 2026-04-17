// <copyright file="WidthF.cs" company="VoxelGame">
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
public readonly record struct WidthF
{
    /// <summary>
    ///     Creates a new width with the specified width.
    /// </summary>
    /// <param name="value">The width of the lines.</param>
    public WidthF(Single value)
    {
        Value = value;
    }

    /// <summary>
    ///     A width of size zero.
    /// </summary>
    public static WidthF Zero { get; } = new(0);

    /// <summary>
    ///     A width of size one.
    /// </summary>
    public static WidthF One { get; } = new(1);

    /// <summary>
    ///     The width value.
    /// </summary>
    public Single Value { get; init; }

    /// <summary>
    ///     Create a <see cref="ThicknessF" /> from this width.
    /// </summary>
    /// <returns>A <see cref="ThicknessF" /> with the same width for all sides.</returns>
    public ThicknessF ToThicknessF()
    {
        return new ThicknessF(Value);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return this == Zero
            ? "WidthF.Zero"
            : $"WidthF(Value: {Value})";
    }
}

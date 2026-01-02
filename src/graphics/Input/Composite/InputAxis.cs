// <copyright file="Axis.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Input.Actions;

namespace VoxelGame.Graphics.Input.Composite;

/// <summary>
///     An input axis consisting of two <see cref="Button" />s.
/// </summary>
public class InputAxis
{
    private readonly Button negative;
    private readonly Button positive;

    /// <summary>
    ///     Create a new input axis.
    /// </summary>
    /// <param name="positive">The positive button.</param>
    /// <param name="negative">The negative button.</param>
    public InputAxis(Button positive, Button negative)
    {
        this.positive = positive;
        this.negative = negative;
    }

    /// <summary>
    ///     Get the value of the axis.
    /// </summary>
    public Single Value
    {
        get
        {
            var value = 0f;

            if (positive.IsDown) value++;
            if (negative.IsDown) value--;

            return value;
        }
    }
}

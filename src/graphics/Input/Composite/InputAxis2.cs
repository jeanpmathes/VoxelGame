// <copyright file="Axis2.cs" company="VoxelGame">
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

using OpenTK.Mathematics;

namespace VoxelGame.Graphics.Input.Composite;

/// <summary>
///     A two-dimensional axis.
/// </summary>
public class InputAxis2
{
    private readonly InputAxis x;
    private readonly InputAxis y;

    /// <summary>
    ///     Create a new axis.
    /// </summary>
    /// <param name="x">The x axis.</param>
    /// <param name="y">The y axis.</param>
    public InputAxis2(InputAxis x, InputAxis y)
    {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    ///     The current value of the axis.
    /// </summary>
    public Vector2 Value => new(x.Value, y.Value);
}

// <copyright file="Axis.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     An axis.
/// </summary>
public enum Axis
{
    /// <summary>
    ///     The x-axis.
    /// </summary>
    X = 0b00,

    /// <summary>
    ///     The y-axis.
    /// </summary>
    Y = 0b01,

    /// <summary>
    ///     The z-axis.
    /// </summary>
    Z = 0b10
}

/// <summary>
///     Extension methods for <see cref="Axis" />.
/// </summary>
public static class AxisExtensions
{
    /// <summary>
    ///     Rotate an axis around the y-axis.
    /// </summary>
    public static Axis Rotate(this Axis axis)
    {
        return axis switch
        {
            Axis.X => Axis.Z,
            Axis.Y => Axis.Y,
            Axis.Z => Axis.X,
            _ => throw Exceptions.UnsupportedEnumValue(axis)
        };
    }

    /// <summary>
    ///     Get the sides associated with the given axis.
    ///     These are the sides the axis passes through.
    /// </summary>
    public static Sides Sides(this Axis axis)
    {
        return axis switch
        {
            Axis.X => Logic.Voxels.Sides.Left | Logic.Voxels.Sides.Right,
            Axis.Y => Logic.Voxels.Sides.Top | Logic.Voxels.Sides.Bottom,
            Axis.Z => Logic.Voxels.Sides.Front | Logic.Voxels.Sides.Back,
            _ => throw Exceptions.UnsupportedEnumValue(axis)
        };
    }

    /// <summary>
    ///     Get the axis as a <see cref="Vector3" />.
    /// </summary>
    public static Vector3d Vector3(this Axis axis, Double onAxis, Double other)
    {
        return axis switch
        {
            Axis.X => new Vector3d(onAxis, other, other),
            Axis.Y => new Vector3d(other, onAxis, other),
            Axis.Z => new Vector3d(other, other, onAxis),
            _ => throw Exceptions.UnsupportedEnumValue(axis)
        };
    }

    /// <summary>
    ///     Get the unit vector of the axis as a <see cref="Vector3d" />.
    /// </summary>
    public static Vector3d ToVector3d(this Axis axis)
    {
        return axis switch
        {
            Axis.X => Vector3d.UnitX,
            Axis.Y => Vector3d.UnitY,
            Axis.Z => Vector3d.UnitZ,
            _ => throw Exceptions.UnsupportedEnumValue(axis)
        };
    }
}

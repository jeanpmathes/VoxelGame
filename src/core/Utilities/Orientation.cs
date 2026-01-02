// <copyright file="Orientation.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     An orientation in 3D space.
/// </summary>
public enum Orientation
{
    /// <summary>
    ///     The north orientation.
    /// </summary>
    North = 0b00,

    /// <summary>
    ///     The east orientation.
    /// </summary>
    East = 0b01,

    /// <summary>
    ///     The south orientation.
    /// </summary>
    South = 0b10,

    /// <summary>
    ///     The west orientation.
    /// </summary>
    West = 0b11
}

/// <summary>
///     Utility methods for orientations.
/// </summary>
public static class Orientations
{
    private static readonly ReadOnlyCollection<Orientation> orientations = new List<Orientation>
        {Orientation.North, Orientation.East, Orientation.South, Orientation.West}.AsReadOnly();

    /// <summary>
    ///     Get all orientations.
    /// </summary>
    public static IEnumerable<Orientation> All => orientations;

    /// <summary>
    ///     Loop through all orientations, starting depending on a position.
    /// </summary>
    /// <param name="position">The position to calculate the first orientation.</param>
    /// <returns>All orientations.</returns>
    public static IEnumerable<Orientation> ShuffledStart(Vector3i position)
    {
        Int32 start = NumberGenerator.GetPositionDependentNumber(position, mod: 4);

        for (Int32 i = start; i < start + 4; i++) yield return orientations[i % 4];
    }
}

/// <summary>
///     Extension methods for <see cref="Orientation" />.
/// </summary>
public static class OrientationExtensions
{
    /// <summary>
    ///     Convert a vector to an orientation.
    /// </summary>
    public static Orientation ToOrientation(this Vector3d vector)
    {
        if (Math.Abs(vector.Z) > Math.Abs(vector.X)) return vector.Z > 0 ? Orientation.South : Orientation.North;

        return vector.X > 0 ? Orientation.East : Orientation.West;
    }

    /// <summary>
    ///     Convert an orientation to a vector.
    /// </summary>
    public static Vector3d ToVector3(this Orientation orientation)
    {
        return orientation.ToVector3i().ToVector3();
    }

    /// <summary>
    ///     Convert an orientation to an integer vector.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static Vector3i ToVector3i(this Orientation orientation)
    {
        return orientation switch
        {
            Orientation.North => -Vector3i.UnitZ,
            Orientation.East => Vector3i.UnitX,
            Orientation.South => Vector3i.UnitZ,
            Orientation.West => -Vector3i.UnitX,
            _ => -Vector3i.UnitZ
        };
    }

    /// <summary>
    ///     Convert an orientation to a <see cref="Side" />.
    /// </summary>
    public static Side ToSide(this Orientation orientation)
    {
        return orientation switch
        {
            Orientation.North => Side.Back,
            Orientation.East => Side.Right,
            Orientation.South => Side.Front,
            Orientation.West => Side.Left,
            _ => Side.Back
        };
    }

    /// <summary>
    ///     Get the opposite orientation.
    /// </summary>
    public static Orientation Opposite(this Orientation orientation)
    {
        return orientation switch
        {
            Orientation.North => Orientation.South,
            Orientation.East => Orientation.West,
            Orientation.South => Orientation.North,
            Orientation.West => Orientation.East,
            _ => Orientation.North
        };
    }

    /// <summary>
    ///     Rotate an orientation clockwise.
    /// </summary>
    public static Orientation Rotate(this Orientation orientation)
    {
        return orientation switch
        {
            Orientation.East => Orientation.South,
            Orientation.West => Orientation.North,
            Orientation.North => Orientation.East,
            Orientation.South => Orientation.West,
            _ => Orientation.North
        };
    }

    /// <summary>
    ///     Offset a vector along an orientation.
    /// </summary>
    public static Vector3i Offset(this Vector3i vector, Orientation orientation)
    {
        return vector + orientation.ToVector3i();
    }

    /// <summary>
    ///     Check if this orientation is on the x-axis.
    /// </summary>
    public static Boolean IsX(this Orientation orientation)
    {
        return orientation.Axis() == Utilities.Axis.X;
    }

    /// <summary>
    ///     Check if this orientation is on the z axis.
    /// </summary>
    public static Boolean IsZ(this Orientation orientation)
    {
        return orientation.Axis() == Utilities.Axis.Z;
    }

    /// <summary>
    ///     Get the axis of this orientation.
    /// </summary>
    public static Axis Axis(this Orientation orientation)
    {
        return orientation switch
        {
            Orientation.North => Utilities.Axis.Z,
            Orientation.East => Utilities.Axis.X,
            Orientation.South => Utilities.Axis.Z,
            Orientation.West => Utilities.Axis.X,
            _ => throw Exceptions.UnsupportedEnumValue(orientation)
        };
    }

    /// <summary>
    ///     Select a random orientation.
    /// </summary>
    public static Orientation NextOrientation(this Random random)
    {
        Int32 value = random.Next(minValue: 0, maxValue: 4);

        return random.Next(minValue: 0, maxValue: 4) switch
        {
            0 => Orientation.North,
            1 => Orientation.East,
            2 => Orientation.South,
            3 => Orientation.West,
            _ => throw Exceptions.UnsupportedValue(value)
        };
    }
}

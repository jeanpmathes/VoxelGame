// <copyright file="Orientation.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

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
    private static readonly IReadOnlyList<Orientation> orientations = new List<Orientation>
        {Orientation.North, Orientation.East, Orientation.South, Orientation.West}.AsReadOnly();

    /// <summary>
    ///     Get all orientations.
    /// </summary>
    public static IEnumerable<Orientation> All => orientations;

    /// <summary>
    ///     Loop trough all orientations, starting depending on a position.
    /// </summary>
    /// <param name="position">The position to calculate the first orientation.</param>
    /// <returns>All orientations.</returns>
    public static IEnumerable<Orientation> ShuffledStart(Vector3i position)
    {
        int start = BlockUtilities.GetPositionDependentNumber(position, mod: 4);

        for (int i = start; i < start + 4; i++) yield return orientations[i % 4];
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
    ///     Convert an orientation to a <see cref="BlockSide" />.
    /// </summary>
    public static BlockSide ToBlockSide(this Orientation orientation)
    {
        return orientation switch
        {
            Orientation.North => BlockSide.Back,
            Orientation.East => BlockSide.Right,
            Orientation.South => BlockSide.Front,
            Orientation.West => BlockSide.Left,
            _ => BlockSide.Back
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
            Orientation.North => Orientation.East,
            Orientation.East => Orientation.South,
            Orientation.South => Orientation.West,
            Orientation.West => Orientation.North,
            _ => Orientation.North
        };
    }

    /// <summary>
    ///     Offset a vector along an orientation.
    /// </summary>
    public static Vector3i Offset(this Orientation orientation, Vector3i vector)
    {
        return vector + orientation.ToVector3i();
    }

    /// <summary>
    ///     Pick an element from a tuple based on an orientation.
    /// </summary>
    public static T Pick<T>(this Orientation orientation, (T north, T east, T south, T west) tuple)
    {
        return orientation switch
        {
            Orientation.North => tuple.north,
            Orientation.East => tuple.east,
            Orientation.South => tuple.south,
            Orientation.West => tuple.west,
            _ => tuple.north
        };
    }

    /// <summary>
    ///     Convert an orientation to an integer flag.
    /// </summary>
    public static uint ToFlag(this Orientation orientation)
    {
        return orientation switch
        {
            Orientation.North => 0b1000,
            Orientation.East => 0b0100,
            Orientation.South => 0b0010,
            Orientation.West => 0b0001,
            _ => 0b1000
        };
    }

    /// <summary>
    ///     Check if this orientation is on the x axis.
    /// </summary>
    public static bool IsX(this Orientation orientation)
    {
        return orientation.Axis() == Utilities.Axis.X;
    }

    /// <summary>
    ///     Check if this orientation is on the z axis.
    /// </summary>
    public static bool IsZ(this Orientation orientation)
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
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, message: null)
        };
    }
}

// <copyright file="Side.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     The side of a block or other cube-like object.
/// </summary>
public enum Side
{
    /// <summary>
    ///     All sides. Only allowed for special cases.
    /// </summary>
    All = -1,

    /// <summary>
    ///     The front side.
    /// </summary>
    Front = 0,

    /// <summary>
    ///     The back side.
    /// </summary>
    Back = 1,

    /// <summary>
    ///     The left side.
    /// </summary>
    Left = 2,

    /// <summary>
    ///     The right side.
    /// </summary>
    Right = 3,

    /// <summary>
    ///     The bottom side.
    /// </summary>
    Bottom = 4,

    /// <summary>
    ///     The top side.
    /// </summary>
    Top = 5
}

/// <summary>
///     Flags to select multiple sides.
/// </summary>
[Flags]
public enum Sides
{
    /// <summary>
    ///     No sides.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The front side.
    /// </summary>
    Front = 1 << 0,

    /// <summary>
    ///     The back side.
    /// </summary>
    Back = 1 << 1,

    /// <summary>
    ///     The left side.
    /// </summary>
    Left = 1 << 2,

    /// <summary>
    ///     The right side.
    /// </summary>
    Right = 1 << 3,

    /// <summary>
    ///     The bottom side.
    /// </summary>
    Bottom = 1 << 4,

    /// <summary>
    ///     The top side.
    /// </summary>
    Top = 1 << 5,

    /// <summary>
    ///     The lateral sides, meaning the front, back, left, and right sides.
    /// </summary>
    Lateral = Front | Back | Left | Right,

    /// <summary>
    ///     All sides.
    /// </summary>
    All = Front | Back | Left | Right | Bottom | Top
}

/// <summary>
///     Extension methods for <see cref="Side" />.
/// </summary>
public static class SideExtensions
{
    // Corners of a block.

    private static readonly Int32[] c001 = [0, 0, 1];
    private static readonly Int32[] c011 = [0, 1, 1];
    private static readonly Int32[] c111 = [1, 1, 1];
    private static readonly Int32[] c101 = [1, 0, 1];
    private static readonly Int32[] c000 = [0, 0, 0];
    private static readonly Int32[] c010 = [0, 1, 0];
    private static readonly Int32[] c110 = [1, 1, 0];
    private static readonly Int32[] c100 = [1, 0, 0];

    private static readonly Vector3i[] directions =
    [
        (0, 0, 0),
        (0, 0, 1),
        (0, 0, -1),
        (-1, 0, 0),
        (1, 0, 0),
        (0, -1, 0),
        (0, 1, 0)
    ];

    private static readonly IReadOnlyCollection<Side> sides = new List<Side>
            {Side.Front, Side.Back, Side.Left, Side.Right, Side.Bottom, Side.Top}
        .AsReadOnly();

    /// <summary>
    ///     Get a compact string representation of all set sides.
    /// </summary>
    /// <param name="side">The side flags.</param>
    /// <returns>A string representation.</returns>
    public static String ToCompactString(this Sides side)
    {
        StringBuilder builder = new(capacity: 6);
        builder.Append(value: '-', repeatCount: 6);

        if (side.HasFlag(Voxels.Sides.Front)) builder[index: 0] = 'F';
        if (side.HasFlag(Voxels.Sides.Back)) builder[index: 1] = 'B';
        if (side.HasFlag(Voxels.Sides.Left)) builder[index: 2] = 'L';
        if (side.HasFlag(Voxels.Sides.Right)) builder[index: 3] = 'R';
        if (side.HasFlag(Voxels.Sides.Bottom)) builder[index: 4] = 'D';
        if (side.HasFlag(Voxels.Sides.Top)) builder[index: 5] = 'U';

        return builder.ToString();
    }

    /// <summary>
    ///     Get a compact string representation of a single side.
    /// </summary>
    /// <param name="side">The side.</param>
    /// <returns>The string representation.</returns>
    public static String ToCompactString(this Side side)
    {
        return side switch
        {
            Side.All => "A",
            Side.Front => "F",
            Side.Back => "B",
            Side.Left => "L",
            Side.Right => "R",
            Side.Bottom => "D",
            Side.Top => "U",
            _ => throw Exceptions.UnsupportedEnumValue(side)
        };
    }

    /// <summary>
    ///     Get the number of sides set in the flags.
    /// </summary>
    public static Int32 Count(this Sides side)
    {
        return BitTools.CountSetBits((UInt32) side);
    }

    /// <summary>
    ///     Get the block side flags as a single side.
    /// </summary>
    /// <param name="side">The block side flags, only one bit should be set.</param>
    /// <returns>The single side.</returns>
    public static Side Single(this Sides side)
    {
        return side.SingleOrDefault() ?? throw Exceptions.UnsupportedEnumValue(side);
    }

    /// <summary>
    /// Get the block side flags as a single side, or null if no or multiple sides are set.
    /// </summary>
    /// <param name="side">The block side flags, only one bit should be set.</param>
    /// <returns>The single side, or null.</returns>
    public static Side? SingleOrDefault(this Sides side)
    {
        return side switch
        {
            Voxels.Sides.Front => Side.Front,
            Voxels.Sides.Back => Side.Back,
            Voxels.Sides.Left => Side.Left,
            Voxels.Sides.Right => Side.Right,
            Voxels.Sides.Bottom => Side.Bottom,
            Voxels.Sides.Top => Side.Top,
            Voxels.Sides.None => null,
            Voxels.Sides.All => Side.All,
            _ => null
        };
    }

    /// <summary>
    ///     Get the opposite sides of the given sides.
    ///     Note that this is not simply a bitwise negation, as <c>None</c> and <c>All</c> remain unchanged.
    ///     Additionally, if two opposite sides are set, they remain set.
    /// </summary>
    /// <param name="side">The sides to get the opposite of.</param>
    /// <returns>The opposite sides.</returns>
    public static Sides Opposite(this Sides side)
    {
        if (side is Voxels.Sides.None or Voxels.Sides.All) return side;

        var result = Voxels.Sides.None;

        if (side.HasFlag(Voxels.Sides.Front)) result |= Voxels.Sides.Back;
        if (side.HasFlag(Voxels.Sides.Back)) result |= Voxels.Sides.Front;

        if (side.HasFlag(Voxels.Sides.Left)) result |= Voxels.Sides.Right;
        if (side.HasFlag(Voxels.Sides.Right)) result |= Voxels.Sides.Left;

        if (side.HasFlag(Voxels.Sides.Bottom)) result |= Voxels.Sides.Top;
        if (side.HasFlag(Voxels.Sides.Top)) result |= Voxels.Sides.Bottom;

        return result;
    }

    /// <summary>
    ///     Get the flag for a side.
    /// </summary>
    public static Sides ToFlag(this Side side)
    {
        return side switch
        {
            Side.All => Voxels.Sides.All,
            Side.Front => Voxels.Sides.Front,
            Side.Back => Voxels.Sides.Back,
            Side.Left => Voxels.Sides.Left,
            Side.Right => Voxels.Sides.Right,
            Side.Bottom => Voxels.Sides.Bottom,
            Side.Top => Voxels.Sides.Top,
            _ => throw Exceptions.UnsupportedEnumValue(side)
        };
    }

    /// <summary>
    ///     Provides an enumerable that contains all actual blocks sides, meaning not the side <c>All</c>.
    /// </summary>
    /// <param name="side">Must be the block side <c>All</c>.</param>
    /// <returns>The block side enumerable.</returns>
    public static IEnumerable<Side> Sides(this Side side)
    {
        Debug.Assert(side == Side.All);

        return sides;
    }

    /// <summary>
    ///     Get the opposite side of a block.
    /// </summary>
    public static Side Opposite(this Side side)
    {
        return side switch
        {
            Side.All => Side.All,
            Side.Front => Side.Back,
            Side.Back => Side.Front,
            Side.Left => Side.Right,
            Side.Right => Side.Left,
            Side.Bottom => Side.Top,
            Side.Top => Side.Bottom,
            _ => throw Exceptions.UnsupportedEnumValue(side)
        };
    }

    /// <summary>
    ///     Get the side as <see cref="Orientation" />.
    /// </summary>
    public static Orientation ToOrientation(this Side side)
    {
        return side switch
        {
            Side.Front => Orientation.South,
            Side.Back => Orientation.North,
            Side.Left => Orientation.West,
            Side.Right => Orientation.East,
            _ => throw Exceptions.UnsupportedEnumValue(side)
        };
    }

    /// <summary>
    ///     Check whether this side is a lateral side, meaning not at the top or bottom.
    /// </summary>
    public static Boolean IsLateral(this Side side)
    {
        return side switch
        {
            Side.All => false,
            Side.Front => true,
            Side.Back => true,
            Side.Left => true,
            Side.Right => true,
            Side.Bottom => false,
            Side.Top => false,
            _ => throw Exceptions.UnsupportedEnumValue(side)
        };
    }

    /// <summary>
    ///     Get the side as a direction vector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i Direction(this Side side)
    {
        Int32 index = (Int32) side + 1;

        if (index > 6) throw Exceptions.UnsupportedEnumValue(side);

        return directions[index];
    }

    /// <summary>
    ///     Get the side corresponding to the given direction.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <returns>The side, or <c>Side.All</c> if the direction is not a valid side direction.</returns>
    public static Side ToSide(this Vector3i direction)
    {
        for (var i = 0; i < directions.Length; i++)
            if (directions[i] == direction)
                return (Side) (i - 1);

        return Side.All;
    }

    /// <summary>
    ///     Offset a vector by the direction of this side.
    /// </summary>
    public static Vector3i Offset(this Vector3i vector, Side side)
    {
        return vector + side.Direction();
    }

    /// <summary>
    ///     Offset a section position by the direction of this side.
    /// </summary>
    public static SectionPosition Offset(this SectionPosition position, Side side)
    {
        (Int32 x, Int32 y, Int32 z) = side.Direction();

        return new SectionPosition(position.X + x, position.Y + y, position.Z + z);
    }

    /// <summary>
    ///     Offset a chunk position by the direction of this side.
    /// </summary>
    public static ChunkPosition Offset(this ChunkPosition position, Side side)
    {
        (Int32 x, Int32 y, Int32 z) = side.Direction();

        return new ChunkPosition(position.X + x, position.Y + y, position.Z + z);
    }

    /// <summary>
    ///     Convert this side to the axis it is on.
    /// </summary>
    public static Axis Axis(this Side side)
    {
        return side switch
        {
            Side.All => throw Exceptions.UnsupportedEnumValue(side),
            Side.Front => Utilities.Axis.Z,
            Side.Back => Utilities.Axis.Z,
            Side.Left => Utilities.Axis.X,
            Side.Right => Utilities.Axis.X,
            Side.Bottom => Utilities.Axis.Y,
            Side.Top => Utilities.Axis.Y,
            _ => throw Exceptions.UnsupportedEnumValue(side)
        };
    }

    /// <summary>
    ///     Rotate a side around the given axis by one step clockwise (right hand rule).
    /// </summary>
    public static Side Rotate(this Side side, Axis axis)
    {
        if (side == Side.All)
            throw Exceptions.UnsupportedEnumValue(side);

        if (side.Axis() == axis)
            return side;

        return axis switch
        {
            Utilities.Axis.X => RotateAroundX(side),
            Utilities.Axis.Y => RotateAroundY(side),
            Utilities.Axis.Z => RotateAroundZ(side),
            _ => throw Exceptions.UnsupportedEnumValue(axis)
        };
    }

    private static Side RotateAroundX(Side side)
    {
        return side switch
        {
            Side.Front => Side.Bottom,
            Side.Bottom => Side.Back,
            Side.Back => Side.Top,
            Side.Top => Side.Front,
            _ => throw Exceptions.UnsupportedEnumValue(side)
        };
    }

    private static Side RotateAroundY(Side side)
    {
        return side switch
        {
            Side.Front => Side.Right,
            Side.Right => Side.Back,
            Side.Back => Side.Left,
            Side.Left => Side.Front,
            Side.Bottom => Side.Bottom,
            _ => throw Exceptions.UnsupportedEnumValue(side)
        };
    }

    private static Side RotateAroundZ(Side side)
    {
        return side switch
        {
            Side.Top => Side.Right,
            Side.Right => Side.Bottom,
            Side.Bottom => Side.Left,
            Side.Left => Side.Top,
            _ => throw Exceptions.UnsupportedEnumValue(side)
        };
    }

    /// <summary>
    ///     Get the corners of this side of a block.
    ///     Every of the four corners is represented by an integer array with three elements.
    /// </summary>
    public static void Corners(this Side side, out Int32[] a, out Int32[] b, out Int32[] c, out Int32[] d)
    {
        switch (side)
        {
            case Side.Front:
                a = c001;
                b = c011;
                c = c111;
                d = c101;

                break;

            case Side.Back:
                a = c100;
                b = c110;
                c = c010;
                d = c000;

                break;

            case Side.Left:
                a = c000;
                b = c010;
                c = c011;
                d = c001;

                break;

            case Side.Right:
                a = c101;
                b = c111;
                c = c110;
                d = c100;

                break;

            case Side.Bottom:
                a = c000;
                b = c001;
                c = c101;
                d = c100;

                break;

            case Side.Top:
                a = c011;
                b = c010;
                c = c110;
                d = c111;

                break;

            default:
                throw Exceptions.UnsupportedEnumValue(side);
        }
    }
}

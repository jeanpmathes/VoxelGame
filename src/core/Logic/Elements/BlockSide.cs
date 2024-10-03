// <copyright file="BlockSide.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
///     The side of a block.
/// </summary>
public enum BlockSide
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
public enum BlockSides
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
    ///     All sides.
    /// </summary>
    All = Front | Back | Left | Right | Bottom | Top
}

/// <summary>
///     Extension methods for <see cref="BlockSide" />.
/// </summary>
public static class BlockSideExtensions
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

    private static readonly IReadOnlyCollection<BlockSide> sides = new List<BlockSide>
            {BlockSide.Front, BlockSide.Back, BlockSide.Left, BlockSide.Right, BlockSide.Bottom, BlockSide.Top}
        .AsReadOnly();

    /// <summary>
    ///     Get a compact string representation of all set sides.
    /// </summary>
    /// <param name="side">The side flags.</param>
    /// <returns>A string representation.</returns>
    public static String ToCompactString(this BlockSides side)
    {
        StringBuilder builder = new(capacity: 6);
        builder.Append(value: '-', repeatCount: 6);

        if (side.HasFlag(BlockSides.Front)) builder[index: 0] = 'F';
        if (side.HasFlag(BlockSides.Back)) builder[index: 1] = 'B';
        if (side.HasFlag(BlockSides.Left)) builder[index: 2] = 'L';
        if (side.HasFlag(BlockSides.Right)) builder[index: 3] = 'R';
        if (side.HasFlag(BlockSides.Bottom)) builder[index: 4] = 'D';
        if (side.HasFlag(BlockSides.Top)) builder[index: 5] = 'U';

        return builder.ToString();
    }

    /// <summary>
    ///     Get the number of sides set in the flags.
    /// </summary>
    public static Int32 Count(this BlockSides side)
    {
        return BitHelper.CountSetBits((UInt32) side);
    }

    /// <summary>
    ///     Get the block side flags as a single side.
    /// </summary>
    /// <param name="side">The block side flags, only one bit should be set.</param>
    /// <returns>The single side.</returns>
    public static BlockSide Single(this BlockSides side)
    {
        return side switch
        {
            BlockSides.Front => BlockSide.Front,
            BlockSides.Back => BlockSide.Back,
            BlockSides.Left => BlockSide.Left,
            BlockSides.Right => BlockSide.Right,
            BlockSides.Bottom => BlockSide.Bottom,
            BlockSides.Top => BlockSide.Top,
            BlockSides.None => throw new ArgumentOutOfRangeException(nameof(side), side, message: null),
            BlockSides.All => BlockSide.All,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, message: null)
        };
    }

    /// <summary>
    ///     Get the flag for a side.
    /// </summary>
    public static BlockSides ToFlag(this BlockSide side)
    {
        return side switch
        {
            BlockSide.All => BlockSides.All,
            BlockSide.Front => BlockSides.Front,
            BlockSide.Back => BlockSides.Back,
            BlockSide.Left => BlockSides.Left,
            BlockSide.Right => BlockSides.Right,
            BlockSide.Bottom => BlockSides.Bottom,
            BlockSide.Top => BlockSides.Top,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, message: null)
        };
    }

    /// <summary>
    ///     Provides an enumerable that contains all actual blocks sides, meaning not the side <c>All</c>.
    /// </summary>
    /// <param name="side">Must be the block side <c>All</c>.</param>
    /// <returns>The block side enumerable.</returns>
    public static IEnumerable<BlockSide> Sides(this BlockSide side)
    {
        Debug.Assert(side == BlockSide.All);

        return sides;
    }

    /// <summary>
    ///     Get the opposite side of a block.
    /// </summary>
    public static BlockSide Opposite(this BlockSide side)
    {
        return side switch
        {
            BlockSide.All => BlockSide.All,
            BlockSide.Front => BlockSide.Back,
            BlockSide.Back => BlockSide.Front,
            BlockSide.Left => BlockSide.Right,
            BlockSide.Right => BlockSide.Left,
            BlockSide.Bottom => BlockSide.Top,
            BlockSide.Top => BlockSide.Bottom,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, message: null)
        };
    }

    /// <summary>
    ///     Get the side as <see cref="Orientation" />.
    /// </summary>
    public static Orientation ToOrientation(this BlockSide side)
    {
        return side switch
        {
            BlockSide.Front => Orientation.South,
            BlockSide.Back => Orientation.North,
            BlockSide.Left => Orientation.West,
            BlockSide.Right => Orientation.East,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, message: null)
        };
    }

    /// <summary>
    ///     Check whether this side is a lateral side, meaning not at the top or bottom.
    /// </summary>
    public static Boolean IsLateral(this BlockSide side)
    {
        return side switch
        {
            BlockSide.All => false,
            BlockSide.Front => true,
            BlockSide.Back => true,
            BlockSide.Left => true,
            BlockSide.Right => true,
            BlockSide.Bottom => false,
            BlockSide.Top => false,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, message: null)
        };
    }

    /// <summary>
    ///     Get the side as a direction vector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i Direction(this BlockSide side)
    {
        Int32 index = (Int32) side + 1;

        if (index > 6) throw new ArgumentOutOfRangeException(nameof(side), side, message: null);

        return directions[index];
    }

    /// <summary>
    ///     Get the side corresponding to the given direction.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <returns>The side, or <c>BlockSide.All</c> if the direction is not a valid side direction.</returns>
    public static BlockSide ToBlockSide(this Vector3i direction)
    {
        for (var i = 0; i < directions.Length; i++)
            if (directions[i] == direction)
                return (BlockSide) (i - 1);

        return BlockSide.All;
    }

    /// <summary>
    ///     Offset a vector by the direction of this side.
    /// </summary>
    public static Vector3i Offset(this BlockSide side, Vector3i v)
    {
        return v + side.Direction();
    }

    /// <summary>
    ///     Offset a section position by the direction of this side.
    /// </summary>
    public static SectionPosition Offset(this BlockSide side, SectionPosition pos)
    {
        (Int32 x, Int32 y, Int32 z) = side.Direction();

        return new SectionPosition(pos.X + x, pos.Y + y, pos.Z + z);
    }

    /// <summary>
    ///     Offset a chunk position by the direction of this side.
    /// </summary>
    public static ChunkPosition Offset(this BlockSide side, ChunkPosition pos)
    {
        (Int32 x, Int32 y, Int32 z) = side.Direction();

        return new ChunkPosition(pos.X + x, pos.Y + y, pos.Z + z);
    }

    /// <summary>
    ///     Convert this side to the axis it is on.
    /// </summary>
    public static Axis Axis(this BlockSide side)
    {
        return side switch
        {
            BlockSide.All => throw new ArgumentOutOfRangeException(nameof(side), side, message: null),
            BlockSide.Front => Utilities.Axis.Z,
            BlockSide.Back => Utilities.Axis.Z,
            BlockSide.Left => Utilities.Axis.X,
            BlockSide.Right => Utilities.Axis.X,
            BlockSide.Bottom => Utilities.Axis.Y,
            BlockSide.Top => Utilities.Axis.Y,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, message: null)
        };
    }

    /// <summary>
    ///     Rotate a side around the given axis by one step clockwise (right hand rule).
    /// </summary>
    public static BlockSide Rotate(this BlockSide side, Axis axis)
    {
        if (side == BlockSide.All)
            throw new ArgumentOutOfRangeException(nameof(side), side, message: null);

        if (side.Axis() == axis)
            return side;

        return axis switch
        {
            Utilities.Axis.X => RotateAroundX(side),
            Utilities.Axis.Y => RotateAroundY(side),
            Utilities.Axis.Z => RotateAroundZ(side),
            _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, message: null)
        };
    }

    private static BlockSide RotateAroundX(BlockSide side)
    {
        return side switch
        {
            BlockSide.Front => BlockSide.Bottom,
            BlockSide.Bottom => BlockSide.Back,
            BlockSide.Back => BlockSide.Top,
            BlockSide.Top => BlockSide.Front,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, message: null)
        };
    }

    private static BlockSide RotateAroundY(BlockSide side)
    {
        return side switch
        {
            BlockSide.Front => BlockSide.Right,
            BlockSide.Right => BlockSide.Back,
            BlockSide.Back => BlockSide.Left,
            BlockSide.Left => BlockSide.Front,
            BlockSide.Bottom => BlockSide.Bottom,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, message: null)
        };
    }

    private static BlockSide RotateAroundZ(BlockSide side)
    {
        return side switch
        {
            BlockSide.Top => BlockSide.Right,
            BlockSide.Right => BlockSide.Bottom,
            BlockSide.Bottom => BlockSide.Left,
            BlockSide.Left => BlockSide.Top,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, message: null)
        };
    }

    /// <summary>
    ///     Check if this side is contained in the given side flags.
    /// </summary>
    public static Boolean IsSet(this BlockSide side, BlockSides flags)
    {
        return flags.HasFlag(side.ToFlag());
    }

    /// <summary>
    ///     Get the corners of this side of a block.
    ///     Every of the four corners is represented by an integer array with three elements.
    /// </summary>
    public static void Corners(this BlockSide side, out Int32[] a, out Int32[] b, out Int32[] c, out Int32[] d)
    {
        switch (side)
        {
            case BlockSide.Front:
                a = c001;
                b = c011;
                c = c111;
                d = c101;

                break;

            case BlockSide.Back:
                a = c100;
                b = c110;
                c = c010;
                d = c000;

                break;

            case BlockSide.Left:
                a = c000;
                b = c010;
                c = c011;
                d = c001;

                break;

            case BlockSide.Right:
                a = c101;
                b = c111;
                c = c110;
                d = c100;

                break;

            case BlockSide.Bottom:
                a = c000;
                b = c001;
                c = c101;
                d = c100;

                break;

            case BlockSide.Top:
                a = c011;
                b = c010;
                c = c110;
                d = c111;

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(side), side, message: null);
        }
    }
}

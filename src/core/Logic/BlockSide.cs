// <copyright file="BlockSide.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic
{
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
    ///     Extension methods for <see cref="BlockSide" />.
    /// </summary>
    public static class BlockSideExtensions
    {
        // Corners of a block.

        private static readonly int[] c001 = { 0, 0, 1 };
        private static readonly int[] c011 = { 0, 1, 1 };
        private static readonly int[] c111 = { 1, 1, 1 };
        private static readonly int[] c101 = { 1, 0, 1 };
        private static readonly int[] c000 = { 0, 0, 0 };
        private static readonly int[] c010 = { 0, 1, 0 };
        private static readonly int[] c110 = { 1, 1, 0 };
        private static readonly int[] c100 = { 1, 0, 0 };

        private static readonly Vector3i[] directions =
        {
            (0, 0, 0),
            (0, 0, 1),
            (0, 0, -1),
            (-1, 0, 0),
            (1, 0, 0),
            (0, -1, 0),
            (0, 1, 0)
        };

        private static readonly ReadOnlyCollection<BlockSide> sides = new List<BlockSide>
                { BlockSide.Front, BlockSide.Back, BlockSide.Left, BlockSide.Right, BlockSide.Bottom, BlockSide.Top }
            .AsReadOnly();

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
        public static bool IsLateral(this BlockSide side)
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
        public static Vector3i Direction(this BlockSide side)
        {
            int index = (int) side + 1;

            if (index > 6) throw new ArgumentOutOfRangeException(nameof(side), side, message: null);

            return directions[index];
        }

        /// <summary>
        ///     Offset a vector by the direction of this side.
        /// </summary>
        public static Vector3i Offset(this BlockSide side, Vector3i v)
        {
            return v + side.Direction();
        }

        /// <summary>
        ///     Convert this side to the axis it is on.
        /// </summary>
        public static Axis Axis(this BlockSide side)
        {
            return side switch
            {
                BlockSide.All => throw new InvalidEnumArgumentException(),
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
        ///     Get a bit flag representing this side.
        /// </summary>
        public static uint ToFlag(this BlockSide side)
        {
            return side switch
            {
                BlockSide.All => 0b11_1111,
                BlockSide.Front => 0b10_0000,
                BlockSide.Back => 0b01_0000,
                BlockSide.Left => 0b00_1000,
                BlockSide.Right => 0b00_0100,
                BlockSide.Bottom => 0b00_0010,
                BlockSide.Top => 0b00_0001,
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, message: null)
            };
        }

        /// <summary>
        ///     Check if the bit flag of a side is set.
        /// </summary>
        public static bool IsSet(this BlockSide side, uint flags)
        {
            return (flags & side.ToFlag()) != 0;
        }

        /// <summary>
        ///     Get the corners of this side of a block.
        ///     Every of the four corners is represented by an integer array with three elements.
        /// </summary>
        public static void Corners(this BlockSide side, out int[] a, out int[] b, out int[] c, out int[] d)
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
}

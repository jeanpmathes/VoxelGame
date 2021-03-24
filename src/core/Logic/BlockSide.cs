// <copyright file="BlockSide.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic
{
    /// <summary>
    /// The side of a block.
    /// </summary>
    public enum BlockSide
    {
        All = -1,
        Front = 0,
        Back = 1,
        Left = 2,
        Right = 3,
        Bottom = 4,
        Top = 5
    }

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
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }

        public static Vector3i Direction(this BlockSide side)
        {
            return side switch
            {
                BlockSide.All => (0, 0, 0),
                BlockSide.Front => (0, 0, 1),
                BlockSide.Back => (0, 0, -1),
                BlockSide.Left => (-1, 0, 0),
                BlockSide.Right => (1, 0, 0),
                BlockSide.Bottom => (0, -1, 0),
                BlockSide.Top => (0, 1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }

        public static Vector3i Offset(this BlockSide side, Vector3i v)
        {
            return v + side.Direction();
        }

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
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }
    }
}
// <copyright file="BlockSide.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using System;

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

        private static readonly int[] C001 = { 0, 0, 1 };
        private static readonly int[] C011 = { 0, 1, 1 };
        private static readonly int[] C111 = { 1, 1, 1 };
        private static readonly int[] C101 = { 1, 0, 1 };
        private static readonly int[] C000 = { 0, 0, 0 };
        private static readonly int[] C010 = { 0, 1, 0 };
        private static readonly int[] C110 = { 1, 1, 0 };
        private static readonly int[] C100 = { 1, 0, 0 };

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

        private static readonly Vector3i[] Directions = new Vector3i[]
        {
            (0, 0, 0),
            (0, 0, 1),
            (0, 0, -1),
            (-1, 0, 0),
            (1, 0, 0),
            (0, -1, 0),
            (0, 1, 0)
        };

        public static Vector3i Direction(this BlockSide side)
        {
            int index = (int) side + 1;

            if (index > 6) throw new ArgumentOutOfRangeException(nameof(side), side, null);

            return Directions[index];
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
                    a = C001;
                    b = C011;
                    c = C111;
                    d = C101;
                    break;

                case BlockSide.Back:
                    a = C100;
                    b = C110;
                    c = C010;
                    d = C000;
                    break;

                case BlockSide.Left:
                    a = C000;
                    b = C010;
                    c = C011;
                    d = C001;
                    break;

                case BlockSide.Right:
                    a = C101;
                    b = C111;
                    c = C110;
                    d = C100;
                    break;

                case BlockSide.Bottom:
                    a = C000;
                    b = C001;
                    c = C101;
                    d = C100;
                    break;

                case BlockSide.Top:
                    a = C011;
                    b = C010;
                    c = C110;
                    d = C111;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }
    }
}
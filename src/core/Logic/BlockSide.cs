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
    }
}
// <copyright file="BlockSide.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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
    }
}
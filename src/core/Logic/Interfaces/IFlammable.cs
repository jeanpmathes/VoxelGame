﻿// <copyright file="IFlammable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic.Interfaces
{
    /// <summary>
    ///     Marks a block as able to be burned.
    /// </summary>
    public interface IFlammable : IBlockBase
    {
        /// <summary>
        ///     Try to burn a block at a given position.
        /// </summary>
        /// <param name="world">The world this block is in.</param>
        /// <param name="position">The position of the block.</param>
        /// <param name="fire">The fire block that caused the burning.</param>
        /// <returns>true if the block was destroyed, false if not.</returns>
        public bool Burn(World world, Vector3i position, Block fire)
        {
            return Destroy(world, position);
        }
    }
}
﻿// <copyright file="AirBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     AirBlocks are blocks that have no collision and are not rendered. They are used for the air block that stands for
    ///     the absence of other blocks.
    ///     Data bit usage: <c>------</c>
    /// </summary>
    public class AirBlock : Block, IFillable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AirBlock" /> class.
        /// </summary>
        /// <param name="name">The name of this block</param>
        /// <param name="namedId">The unique and unlocalized name of this block.</param>
        internal AirBlock(string name, string namedId) :
            base(
                name,
                namedId,
                BlockFlags.Empty,
                BoundingBox.Block,
                TargetBuffer.NotRendered) {}

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return BlockMeshData.Empty();
        }

        internal override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            return false;
        }

        protected override bool CanDestroy(World world, Vector3i position, uint data, PhysicsEntity? entity)
        {
            return false;
        }
    }
}
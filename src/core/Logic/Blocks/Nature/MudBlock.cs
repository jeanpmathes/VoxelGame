﻿// <copyright file="MudBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A block that slows down entities.
    /// </summary>
    public class MudBlock : BasicBlock, IFillable
    {
        private readonly float maxVelocity;

        internal MudBlock(string name, string namedId, TextureLayout layout, float maxVelocity) :
            base(
                name,
                namedId,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                receiveCollisions: true,
                isTrigger: false,
                isInteractable: false)
        {
            this.maxVelocity = maxVelocity;
        }

        public bool AllowInflow(World world, Vector3i position, BlockSide side, Liquid liquid)
        {
            return liquid.Viscosity < 200;
        }

        protected override void EntityCollision(PhysicsEntity entity, Vector3i position, uint data)
        {
            entity.Velocity = VMath.Clamp(entity.Velocity, min: -1f, maxVelocity);
        }
    }
}
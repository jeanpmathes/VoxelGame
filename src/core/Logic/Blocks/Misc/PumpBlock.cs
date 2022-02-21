﻿// <copyright file="PumpBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     Pumps water upwards when interacted with.
    ///     Data bit usage: <c>------</c>
    /// </summary>
    internal class PumpBlock : BasicBlock, IIndustrialPipeConnectable, IFillable
    {
        private readonly int pumpDistance;

        internal PumpBlock(string name, string namedId, int pumpDistance, TextureLayout layout) :
            base(
                name,
                namedId,
                BlockFlags.Basic with { IsInteractable = true },
                layout)
        {
            this.pumpDistance = pumpDistance;
        }

        public bool AllowInflow(World world, Vector3i position, BlockSide side, Liquid liquid)
        {
            return side != BlockSide.Top;
        }

        public bool AllowOutflow(World world, Vector3i position, BlockSide side)
        {
            return side == BlockSide.Top;
        }

        protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
        {
            Liquid.Elevate(entity.World, position, pumpDistance);
        }
    }
}

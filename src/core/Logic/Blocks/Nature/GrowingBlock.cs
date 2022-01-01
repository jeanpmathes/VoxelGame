﻿// <copyright file="GrowingBlock.cs" company="VoxelGame">
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
    ///     A block that grows upwards and is destroyed if a certain ground block is not given.
    ///     Data bit usage: <c>---aaa</c>
    /// </summary>
    // a: age
    public class GrowingBlock : BasicBlock, IFlammable
    {
        private readonly int maxHeight;
        private readonly Block requiredGround;

        internal GrowingBlock(string name, string namedId, TextureLayout layout, Block ground, int maxHeight) :
            base(
                name,
                namedId,
                BlockFlags.Basic,
                layout)
        {
            requiredGround = ground;
            this.maxHeight = maxHeight;
        }

        /// <inheritdoc />
        internal override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            Block down = world.GetBlock(position.Below())?.Block ?? Air;

            return down == requiredGround || down == this;
        }

        /// <inheritdoc />
        internal override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom)
            {
                Block below = world.GetBlock(position.Below())?.Block ?? Air;

                if (below != requiredGround && below != this) ScheduleDestroy(world, position);
            }
        }

        /// <inheritdoc />
        internal override void RandomUpdate(World world, Vector3i position, uint data)
        {
            var age = (int) (data & 0b00_0111);

            if (age < 7)
            {
                world.SetBlock(this.AsInstance((uint) (age + 1)), position);
            }
            else
            {
                if (!(world.GetBlock(position.Above())?.Block.IsReplaceable ?? false)) return;

                var height = 0;

                for (var offset = 0; offset < maxHeight; offset++)
                    if (world.GetBlock(position.Below(offset))?.Block == this) height++;
                    else break;

                if (height < maxHeight) Place(world, position.Above());
            }
        }
    }
}
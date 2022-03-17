﻿// <copyright file="HotLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Liquids
{
    /// <summary>
    ///     A liquid that can burn it's surroundings.
    /// </summary>
    public class HotLiquid : BasicLiquid
    {
        /// <summary>
        ///     Create a new <see cref="HotLiquid" />.
        /// </summary>
        public HotLiquid(string name, string namedId, float density, int viscosity, bool neutralTint,
            TextureLayout movingLayout, TextureLayout staticLayout,
            RenderType renderType = RenderType.Opaque) :
            base(
                name,
                namedId,
                density,
                viscosity,
                neutralTint,
                movingLayout,
                staticLayout,
                renderType) {}

        /// <inheritdoc />
        protected override void ScheduledUpdate(World world, Vector3i position, LiquidLevel level, bool isStatic)
        {
            if (world.GetBlock(position)?.Block is IFlammable block) block.Burn(world, position, Block.Fire);

            BurnAround(world, position);

            base.ScheduledUpdate(world, position, level, isStatic);
        }

        /// <inheritdoc />
        internal override void RandomUpdate(World world, Vector3i position, LiquidLevel level, bool isStatic)
        {
            BurnAround(world, position);
        }

        private static void BurnAround(World world, Vector3i position)
        {
            foreach (BlockSide side in BlockSide.All.Sides())
            {
                Vector3i offsetPosition = side.Offset(position);

                if (world.GetBlock(offsetPosition)?.Block is IFlammable block &&
                    block.Burn(world, offsetPosition, Block.Fire))
                    Block.Fire.Place(world, offsetPosition);

            }
        }
    }
}

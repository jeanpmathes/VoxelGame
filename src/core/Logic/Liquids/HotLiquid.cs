// <copyright file="HotLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Liquids
{
    /// <summary>
    ///     A liquid that can burn it's surroundings.
    /// </summary>
    public class HotLiquid : BasicLiquid
    {
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

        protected override void ScheduledUpdate(World world, Vector3i position, LiquidLevel level, bool isStatic)
        {
            if (world.GetBlock(position, out _) is IFlammable block) block.Burn(world, position, Block.Fire);

            BurnAround(world, position);

            base.ScheduledUpdate(world, position, level, isStatic);
        }

        internal override void RandomUpdate(World world, Vector3i position, LiquidLevel level, bool isStatic)
        {
            BurnAround(world, position);
        }

        private static void BurnAround(World world, Vector3i position)
        {
            for (var side = BlockSide.Front; side <= BlockSide.Top; side++)
            {
                Vector3i offsetPosition = side.Offset(position);

                if (world.GetBlock(offsetPosition, out _) is IFlammable block &&
                    block.Burn(world, offsetPosition, Block.Fire))
                    Block.Fire.Place(world, offsetPosition);

            }
        }
    }
}
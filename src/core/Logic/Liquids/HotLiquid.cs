// <copyright file="HotLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Liquids
{
    /// <summary>
    /// A liquid that can burn it's surroundings.
    /// </summary>
    public class HotLiquid : BasicLiquid
    {
        public HotLiquid(string name, string namedId, float density, int viscosity, bool neutralTint, TextureLayout movingLayout, TextureLayout staticLayout, Visuals.RenderType renderType = Visuals.RenderType.Opaque) :
            base(
                name,
                namedId,
                density,
                viscosity,
                neutralTint,
                movingLayout,
                staticLayout,
                renderType)
        {
        }

        protected override void ScheduledUpdate(World world, int x, int y, int z, LiquidLevel level, bool isStatic)
        {
            if (world.GetBlock(x, y, z, out _) is IFlammable block) block.Burn(world, x, y, z, Block.Fire);

            BurnAround(world, x, y, z);

            base.ScheduledUpdate(world, x, y, z, level, isStatic);
        }

        internal override void RandomUpdate(World world, int x, int y, int z, LiquidLevel level, bool isStatic)
        {
            BurnAround(world, x, y, z);
        }

        private static void BurnAround(World world, int x, int y, int z)
        {
            BurnAndPlaceFire(x, y, z + 1); // Front.
            BurnAndPlaceFire(x, y, z - 1); // Back.
            BurnAndPlaceFire(x - 1, y, z); // Left.
            BurnAndPlaceFire(x + 1, y, z); // Right.
            BurnAndPlaceFire(x, y - 1, z); // Bottom.
            BurnAndPlaceFire(x, y + 1, z); // Top.

            void BurnAndPlaceFire(int x, int y, int z)
            {
                if (world.GetBlock(x, y, z, out _) is IFlammable block && block.Burn(world, x, y, z, Block.Fire))
                {
                    Block.Fire.Place(world, x, y, z);
                }
            }
        }
    }
}
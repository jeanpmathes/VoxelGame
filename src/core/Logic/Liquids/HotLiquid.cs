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
        public HotLiquid(string name, string namedId, float density, int viscosity, bool neutralTint, TextureLayout movingLayout, TextureLayout staticLayout, Visuals.RenderType renderType = Visuals.RenderType.Transparent) :
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

        protected override void ScheduledUpdate(int x, int y, int z, LiquidLevel level, bool isStatic)
        {
            BurnAround(x, y, z);

            base.ScheduledUpdate(x, y, z, level, isStatic);
        }

        internal override void RandomUpdate(int x, int y, int z, LiquidLevel level, bool isStatic)
        {
            BurnAround(x, y, z);
        }

        protected static void BurnAround(int x, int y, int z)
        {
            BurnAt(x, y, z);

            BurnAt(x, y, z + 1); // Front.
            BurnAt(x, y, z - 1); // Back.
            BurnAt(x - 1, y, z); // Left.
            BurnAt(x + 1, y, z); // Right.
            BurnAt(x, y - 1, z); // Bottom.
            BurnAt(x, y + 1, z); // Top.

            void BurnAt(int x, int y, int z)
            {
                if (Game.World.GetBlock(x, y, z, out _) is IFlammable block && block.Burn(x, y, z, Block.Fire))
                {
                    Block.Fire.Place(x, y, z);
                }
            }
        }
    }
}
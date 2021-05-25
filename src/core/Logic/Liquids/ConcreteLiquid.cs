// <copyright file="ConcreteLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic.Blocks;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Liquids
{
    public class ConcreteLiquid : BasicLiquid
    {
        public ConcreteLiquid(string name, string namedId, float density, int viscosity, TextureLayout movingLayout, TextureLayout staticLayout) :
            base(
                name,
                namedId,
                density,
                viscosity,
                neutralTint: false,
                movingLayout,
                staticLayout,
                RenderType.Opaque)
        {
        }

        internal override void RandomUpdate(World world, int x, int y, int z, LiquidLevel level, bool isStatic)
        {
            if (isStatic)
            {
                world.SetDefaultLiquid(x, y, z);
                Block.Specials.Concrete.Place(world, level, x, y, z);
            }
        }
    }
}
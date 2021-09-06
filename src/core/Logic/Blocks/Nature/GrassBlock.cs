// <copyright file="GrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// Dirt covered with flammable grass.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class GrassBlock : CoveredDirtBlock, IFlammable
    {
        internal GrassBlock(string name, string namedId, TextureLayout normal, TextureLayout wet) :
            base(
                name,
                namedId,
                normal,
                wet,
                hasNeutralTint: true,
                supportsFullGrowth: false) {}

        internal override void RandomUpdate(World world, int x, int y, int z, uint data)
        {
            Liquid? liquid = world.GetLiquid(x, y, z, out LiquidLevel level, out _);

            if (liquid == Liquid.Water && level == LiquidLevel.Eight)
            {
                world.SetBlock(Block.Mud, 0, x, y, z);
            }

            if (world.GetBlock(x + 1, y, z, out _) is IGrassSpreadable a) a.SpreadGrass(world, x + 1, y, z, this);
            if (world.GetBlock(x - 1, y, z, out _) is IGrassSpreadable b) b.SpreadGrass(world, x - 1, y, z, this);
            if (world.GetBlock(x, y, z + 1, out _) is IGrassSpreadable c) c.SpreadGrass(world, x, y, z + 1, this);
            if (world.GetBlock(x, y, z - 1, out _) is IGrassSpreadable d) d.SpreadGrass(world, x, y, z - 1, this);

            if (world.GetBlock(x + 1, y + 1, z, out _) is IGrassSpreadable e)
                e.SpreadGrass(world, x + 1, y + 1, z, this);

            if (world.GetBlock(x - 1, y + 1, z, out _) is IGrassSpreadable f)
                f.SpreadGrass(world, x - 1, y + 1, z, this);

            if (world.GetBlock(x, y + 1, z + 1, out _) is IGrassSpreadable g)
                g.SpreadGrass(world, x, y + 1, z + 1, this);

            if (world.GetBlock(x, y + 1, z - 1, out _) is IGrassSpreadable h)
                h.SpreadGrass(world, x, y + 1, z - 1, this);

            if (world.GetBlock(x + 1, y - 1, z, out _) is IGrassSpreadable i)
                i.SpreadGrass(world, x + 1, y - 1, z, this);

            if (world.GetBlock(x - 1, y - 1, z, out _) is IGrassSpreadable j)
                j.SpreadGrass(world, x - 1, y - 1, z, this);

            if (world.GetBlock(x, y - 1, z + 1, out _) is IGrassSpreadable k)
                k.SpreadGrass(world, x, y - 1, z + 1, this);

            if (world.GetBlock(x, y - 1, z - 1, out _) is IGrassSpreadable l)
                l.SpreadGrass(world, x, y - 1, z - 1, this);
        }

        public virtual bool Burn(World world, int x, int y, int z, Block fire)
        {
            world.SetBlock(Block.GrassBurned, 0, x, y, z);
            fire.Place(world, x, y + 1, z);

            return false;
        }
    }
}
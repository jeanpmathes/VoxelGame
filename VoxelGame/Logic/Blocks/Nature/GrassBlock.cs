// <copyright file="GrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Logic.Interfaces;

namespace VoxelGame.Logic.Blocks.Nature
{
    /// <summary>
    /// Dirt covered with flammable grass.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class GrassBlock : CoveredDirtBlock, IFlammable
    {
        public GrassBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                layout,
                hasNeutralTint: true)
        {
        }

        internal override void RandomUpdate(int x, int y, int z, uint data)
        {
            if (Game.World.GetBlock(x + 1, y, z, out _) is IGrassSpreadable a) a.SpreadGrass(x + 1, y, z, this);
            if (Game.World.GetBlock(x - 1, y, z, out _) is IGrassSpreadable b) b.SpreadGrass(x - 1, y, z, this);
            if (Game.World.GetBlock(x, y, z + 1, out _) is IGrassSpreadable c) c.SpreadGrass(x, y, z + 1, this);
            if (Game.World.GetBlock(x, y, z - 1, out _) is IGrassSpreadable d) d.SpreadGrass(x, y, z - 1, this);

            if (Game.World.GetBlock(x + 1, y + 1, z, out _) is IGrassSpreadable e) e.SpreadGrass(x + 1, y + 1, z, this);
            if (Game.World.GetBlock(x - 1, y + 1, z, out _) is IGrassSpreadable f) f.SpreadGrass(x - 1, y + 1, z, this);
            if (Game.World.GetBlock(x, y + 1, z + 1, out _) is IGrassSpreadable g) g.SpreadGrass(x, y + 1, z + 1, this);
            if (Game.World.GetBlock(x, y + 1, z - 1, out _) is IGrassSpreadable h) h.SpreadGrass(x, y + 1, z - 1, this);

            if (Game.World.GetBlock(x + 1, y - 1, z, out _) is IGrassSpreadable i) i.SpreadGrass(x + 1, y - 1, z, this);
            if (Game.World.GetBlock(x - 1, y - 1, z, out _) is IGrassSpreadable j) j.SpreadGrass(x - 1, y - 1, z, this);
            if (Game.World.GetBlock(x, y - 1, z + 1, out _) is IGrassSpreadable k) k.SpreadGrass(x, y - 1, z + 1, this);
            if (Game.World.GetBlock(x, y - 1, z - 1, out _) is IGrassSpreadable l) l.SpreadGrass(x, y - 1, z - 1, this);
        }

        public virtual bool Burn(int x, int y, int z, Block fire)
        {
            Game.World.SetBlock(Block.GrassBurned, 0, x, y, z);
            fire.Place(x, y + 1, z);

            return false;
        }
    }
}
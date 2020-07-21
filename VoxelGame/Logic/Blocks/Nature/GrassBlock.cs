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
    /// Data bit usage: <c>-----</c>
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

        public virtual bool Burn(int x, int y, int z, Block fire)
        {
            Game.World.SetBlock(Block.GRASS_BURNED, 0, x, y, z);
            fire.Place(x, y + 1, z, null);

            return false;
        }
    }
}
// <copyright file="BurnedGrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A <see cref="CoveredDirtBlock"/> on that grass can spread. It models a dirt block with something on it that can be washed away.
    /// </summary>
    internal class CoveredGrassSpreadableBlock : CoveredDirtBlock, IGrassSpreadable, IFillable
    {
        public CoveredGrassSpreadableBlock(string name, string namedId, TextureLayout normal, bool hasNeutralTint) :
            base(
                name,
                namedId,
                normal,
                normal,
                hasNeutralTint)
        {
        }

        public void LiquidChange(int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0) Game.World.SetBlock(Block.Dirt, 0, x, y, z);
        }
    }
}
// <copyright file="BurnedGrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A <see cref="CoveredDirtBlock"/> on that grass can spread.
    /// </summary>
    internal class CoveredGrassSpreadableBlock : CoveredDirtBlock, IGrassSpreadable
    {
        public CoveredGrassSpreadableBlock(string name, string namedId, TextureLayout layout, bool hasNeutralTint) :
            base(
                name,
                namedId,
                layout,
                hasNeutralTint)
        {
        }
    }
}
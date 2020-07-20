// <copyright file="OrganicTintedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using System.Collections.Generic;
using System.Text;
using VoxelGame.Logic.Interfaces;

namespace VoxelGame.Logic.Blocks.Misc
{
    /// <summary>
    /// A <see cref="TintedBlock"/> made out of organic, flammable materials.
    /// </summary>
    public class OrganicTintedBlock : TintedBlock, IFlammable
    {
        public OrganicTintedBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                layout)
        {
        }
    }
}
﻿// <copyright file="GrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     Dirt covered with flammable grass.
    ///     Data bit usage: <c>------</c>
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

        /// <inheritdoc />
        public bool Burn(World world, Vector3i position, Block fire)
        {
            world.SetBlock(GrassBurned.AsInstance(), position);
            fire.Place(world, position.Above());

            return false;
        }

        /// <inheritdoc />
        public override void RandomUpdate(World world, Vector3i position, uint data)
        {
            LiquidInstance? liquid = world.GetLiquid(position);

            if (liquid?.Liquid == Liquid.Water && liquid.Level == LiquidLevel.Eight)
                world.SetBlock(Mud.AsInstance(), position);

            for (int yOffset = -1; yOffset <= 1; yOffset++)
                foreach (Orientation orientation in Orientations.All)
                {
                    Vector3i otherPosition = orientation.Offset(position) + Vector3i.UnitY * yOffset;

                    if (world.GetBlock(otherPosition)?.Block is IGrassSpreadable grassSpreadable)
                        grassSpreadable.SpreadGrass(world, otherPosition, this);
                }
        }
    }
}

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

        public bool Burn(World world, Vector3i position, Block fire)
        {
            world.SetBlock(GrassBurned, data: 0, position);
            fire.Place(world, position.Above());

            return false;
        }

        internal override void RandomUpdate(World world, Vector3i position, uint data)
        {
            Liquid? liquid = world.GetLiquid(position, out LiquidLevel level, out _);

            if (liquid == Liquid.Water && level == LiquidLevel.Eight) world.SetBlock(Mud, data: 0, position);

            for (int yOffset = -1; yOffset <= 1; yOffset++)
            for (var orientation = Orientation.North; orientation <= Orientation.West; orientation++)
            {
                Vector3i otherPosition = orientation.Offset(position) + Vector3i.UnitY * yOffset;

                if (world.GetBlock(otherPosition, out _) is IGrassSpreadable grassSpreadable)
                    grassSpreadable.SpreadGrass(world, otherPosition, this);
            }
        }
    }
}
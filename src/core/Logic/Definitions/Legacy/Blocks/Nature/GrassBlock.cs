// <copyright file="GrassBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

/// <summary>
///     Dirt covered with flammable grass.
///     Data bit usage: <c>------</c>
/// </summary>
public class GrassBlock : CoveredDirtBlock, ICombustible
{
    internal GrassBlock(String name, String namedID, TextureLayout normal, TextureLayout wet) :
        base(
            name,
            namedID,
            normal,
            wet,
            hasNeutralTint: true,
            supportsFullGrowth: false) {}

    /// <inheritdoc />
    public Boolean Burn(World world, Vector3i position, Block fire)
    {
        world.SetBlock(Elements.Legacy.Blocks.Instance.GrassBurned.AsInstance(), position);
        fire.Place(world, position.Above());

        return false;
    }

    /// <inheritdoc />
    public override void RandomUpdate(World world, Vector3i position, UInt32 data)
    {
        FluidInstance? fluid = world.GetFluid(position);

        if (fluid is {IsAnyWater: true, Level: FluidLevel.Eight})
            world.SetBlock(Elements.Legacy.Blocks.Instance.Mud.AsInstance(), position);

        for (Int32 yOffset = -1; yOffset <= 1; yOffset++)
            foreach (Orientation orientation in Orientations.All)
            {
                Vector3i otherPosition = orientation.Offset(position) + Vector3i.UnitY * yOffset;

                if (world.GetBlock(otherPosition)?.Block is IGrassSpreadable grassSpreadable)
                    grassSpreadable.SpreadGrass(world, otherPosition, this);
            }
    }
}

// <copyright file="GrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     Dirt covered with flammable grass.
///     Data bit usage: <c>------</c>
/// </summary>
public class GrassBlock : CoveredDirtBlock, ICombustible
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
        world.SetBlock(Logic.Blocks.Instance.GrassBurned.AsInstance(), position);
        fire.Place(world, position.Above());

        return false;
    }

    /// <inheritdoc />
    public override void RandomUpdate(World world, Vector3i position, uint data)
    {
        FluidInstance? fluid = world.GetFluid(position);

        if (fluid?.Fluid == Logic.Fluids.Instance.Water && fluid.Value.Level == FluidLevel.Eight)
            world.SetBlock(Logic.Blocks.Instance.Mud.AsInstance(), position);

        for (int yOffset = -1; yOffset <= 1; yOffset++)
            foreach (Orientation orientation in Orientations.All)
            {
                Vector3i otherPosition = orientation.Offset(position) + Vector3i.UnitY * yOffset;

                if (world.GetBlock(otherPosition)?.Block is IGrassSpreadable grassSpreadable)
                    grassSpreadable.SpreadGrass(world, otherPosition, this);
            }
    }
}


// <copyright file="GroundedModifiableHeightBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that allows to change its height by interacting and has to be placed on solid ground.
///     Data bit usage: <c>--hhhh</c>
/// </summary>
public class GroundedModifiableHeightBlock : ModifiableHeightBlock
{
    internal GroundedModifiableHeightBlock(string name, string namedID, TextureLayout layout) : base(name, namedID, layout) {}

    /// <inheritdoc />
    public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        return world.HasFullAndSolidGround(position, solidify: true);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        if (side != BlockSide.Bottom || world.HasFullAndSolidGround(position)) return;

        if (GetHeight(data) == IHeightVariable.MaximumHeight)
            ScheduleDestroy(world, position);
        else
            Destroy(world, position);
    }
}

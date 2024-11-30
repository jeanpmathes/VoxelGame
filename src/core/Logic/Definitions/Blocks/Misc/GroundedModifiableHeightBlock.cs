// <copyright file="GroundedModifiableHeightBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
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
    internal GroundedModifiableHeightBlock(String name, String namedID, TextureLayout layout) : base(name, namedID, layout) {}

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        return world.HasFullAndSolidGround(position, solidify: true);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        if (side != Side.Bottom || world.HasFullAndSolidGround(position)) return;

        if (GetHeight(data) == IHeightVariable.MaximumHeight)
            ScheduleDestroy(world, position);
        else
            Destroy(world, position);
    }
}

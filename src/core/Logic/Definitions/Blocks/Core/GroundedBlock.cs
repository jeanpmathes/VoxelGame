﻿// <copyright file="GroundedBlock.cs" company="VoxelGame">
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
///     A BasicBlock that can only be placed on top of blocks that are both solid and full or will become such blocks.
///     These blocks are also flammable.
///     Data bit usage: <c>------</c>
/// </summary>
public class GroundedBlock : BasicBlock, ICombustible
{
    internal GroundedBlock(String name, String namedID, BlockFlags flags, TextureLayout layout) :
        base(
            name,
            namedID,
            flags,
            layout) {}

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        return world.HasFullAndSolidGround(position, solidify: true);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        if (side == Side.Bottom && !world.HasFullAndSolidGround(position)) ScheduleDestroy(world, position);
    }
}

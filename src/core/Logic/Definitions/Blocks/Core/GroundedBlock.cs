﻿// <copyright file="GroundedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
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
    internal GroundedBlock(string name, string namedId, BlockFlags flags, TextureLayout layout) :
        base(
            name,
            namedId,
            flags,
            layout) {}

    /// <inheritdoc />
    public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        return world.HasFullAndSolidGround(position, solidify: true);
    }

    /// <inheritdoc />
    public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        if (side == BlockSide.Bottom && !world.HasFullAndSolidGround(position)) ScheduleDestroy(world, position);
    }
}
